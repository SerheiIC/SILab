using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SILab.Common;
using SILab.Domain.Entities;
using SILab.Domain.Entities.Auditing;
using SILab.Domain.UnitOfWork;
using SILab.Runtime.Session;
using SILab.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace SILab.EntityFramework
{
    public abstract class SILabDbContext : DbContext, IShouldInitializeDcontext
    {
        /// <summary>
        /// Reference to the logger.
        /// </summary>
        public required ILogger Logger { get; set; }

        /// <summary>
        /// Reference to GUID generator.
        /// </summary>
        public required IGuidGenerator GuidGenerator { get; set; }

        /// <summary>
        /// Used to get current session values.
        /// </summary>
        public ISession SILabSession { get; set; }

        /// <summary>
        /// Reference to the current UOW provider.
        /// </summary>
        public required ICurrentUnitOfWorkProvider CurrentUnitOfWorkProvider { get; set; }

        protected SILabDbContext() 
        {
            InitializeDbContext();
        }

        protected virtual void ObjectStateManager_ObjectStateManagerChanged(object sender, CollectionChangeEventArgs e)
        {
            var contextAdapter = (IObjectContextAdapter)this;
            if (e.Action != CollectionChangeAction.Add)
            {
                return;
            }

            var entry = contextAdapter.ObjectContext.ObjectStateManager.GetObjectStateEntry(e.Element);
            switch (entry.State)
            {
                case EntityState.Added:
                    CheckAndSetId(entry.Entity); 
                    SetCreationAuditProperties(entry.Entity, GetAuditUserId());
                    break;
                  
            }
        }

        protected virtual void CheckAndSetId(object entityAsObj)
        {
            //Set GUID Ids
            var entity = entityAsObj as IEntity<Guid>;
            if (entity != null && entity.Id == Guid.Empty)
            {
                var entityType = ObjectContext.GetObjectType(entityAsObj.GetType());
                var idIdPropertyName = GetIdPropertyName(entityType);
                var edmProperty = GetEdmProperty(entityType, idIdPropertyName);

                if (edmProperty != null && edmProperty.StoreGeneratedPattern == StoreGeneratedPattern.None)
                {
                    entity.Id = GuidGenerator.Create();
                }
            }
        }

        protected virtual void SetCreationAuditProperties(object entityAsObj, long? userId)
        {
            EntityAuditingHelper.SetCreationAuditProperties(              
                entityAsObj,            
                userId,
                CurrentUnitOfWorkProvider.Current.AuditFieldConfiguration
            );
        }

        protected virtual long? GetAuditUserId()
        {
            if (SILabSession.UserId.HasValue &&
                CurrentUnitOfWorkProvider != null &&
                CurrentUnitOfWorkProvider.Current != null)
            {
                return SILabSession.UserId;
            }

            return null;
        }


        private void InitializeDbContext()
        {
            SetNullsForInjectedProperties();
            RegisterToChanges();
        }

        private void SetNullsForInjectedProperties()
        {
            Logger = NullLogger.Instance; 
            GuidGenerator = SequentialGuidGenerator.Instance; 
        }


        private void RegisterToChanges()
        {
            ((IObjectContextAdapter)this)
                .ObjectContext
                .ObjectStateManager
                .ObjectStateManagerChanged += ObjectStateManager_ObjectStateManagerChanged;
        }

        string GetIdPropertyName(Type type)
        {
            var metadata = ((IObjectContextAdapter)this).ObjectContext.MetadataWorkspace;

            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            var entityType = metadata.GetItems<EntityType>(DataSpace.OSpace)
                .Single(t => objectItemCollection.GetClrType(t) == type);

            var entitySetCSpace = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                .Single()
                .EntitySetMappings
                .Single(s => s.EntitySet == entitySetCSpace);

            return mapping
                .EntityTypeMappings.Single()
                .Fragments.Single()
                .PropertyMappings
                .OfType<ScalarPropertyMapping>()
                .Single(m => m.Property.Name == nameof(Entity.Id))
                .Column
                .Name;
        }

        EdmProperty GetEdmProperty(Type type, string propertyName)
        {
            var metadata = ((IObjectContextAdapter)this).ObjectContext.MetadataWorkspace;

            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            var entityType = metadata.GetItems<EntityType>(DataSpace.OSpace)
                .Single(t => objectItemCollection.GetClrType(t) == type);

            var entitySet = metadata.GetItems<EntityContainer>(DataSpace.SSpace).Single().EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            return entitySet.ElementType.Properties.Single(e =>
                string.Equals(e.Name, propertyName, StringComparison.OrdinalIgnoreCase));
        }

        public virtual void Initialize(SILabEfDbContextInitializationContext initializationContext)
        {
            var uowOptions = initializationContext.UnitOfWork.Options;
            if (uowOptions.Timeout.HasValue && !Database.CommandTimeout.HasValue)
            {
                Database.CommandTimeout = uowOptions.Timeout.Value.TotalSeconds.To<int>();
            } 
        }
    }
}
