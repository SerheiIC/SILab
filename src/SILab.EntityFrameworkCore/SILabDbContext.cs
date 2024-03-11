using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging; 
using Microsoft.EntityFrameworkCore.Metadata;
using SILab.EntityFrameworkCore.ValueConverters;
using SILab.EntityFrameworkCore.Utils;
using SILab.Domain.Entities;
using SILab.Domain.UnitOfWork;
using SILab.Linq.Expressions;
using SILab.Runtime.Session;
using SILab.Domain.Entities.Auditing;
using SILab.Collections.Extensions;
using SILab.EntityFrameworkCore.Extensions;
using SILab.Domain.Repositories;
using SILab.Common;
using SILab.Dependency;
using SILab.EntityFramework;
using SILab.Extensions;
using SILab.Events.Bus;
using SILab.Events.Bus.Entities;


namespace SILab.EntityFrameworkCore
{
    public abstract class SILabDbContext : DbContext, ITransientDependency, IShouldInitializeDcontext
    {
        /// <summary>
        /// Used to get current session values.
        /// </summary>
        public ISession Session { get; set; }
        /// <summary>
        /// Reference to the logger.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Reference to GUID generator.
        /// </summary>
        public IGuidGenerator GuidGenerator { get; set; }

        /// <summary>
        /// Reference to the event bus.
        /// </summary>
        public IEventBus EventBus { get; set; }

        /// <summary>
        /// Used to trigger entity change events.
        /// </summary>
        public IEntityChangeEventHelper EntityChangeEventHelper { get; set; }

        /// <summary>
        /// Reference to the current UOW provider.
        /// </summary>
        public ICurrentUnitOfWorkProvider CurrentUnitOfWorkProvider { get; set; }


        private static MethodInfo ConfigureGlobalFiltersMethodInfo = typeof(SILabDbContext).GetMethod(nameof(ConfigureGlobalFilters), BindingFlags.Instance | BindingFlags.NonPublic);

        private static MethodInfo ConfigureGlobalValueConverterMethodInfo = typeof(SILabDbContext).GetMethod(nameof(ConfigureGlobalValueConverter), BindingFlags.Instance | BindingFlags.NonPublic);

        protected virtual bool IsSoftDeleteFilterEnabled => CurrentUnitOfWorkProvider?.Current?.IsFilterEnabled(DataFilters.SoftDelete) == true;



        public virtual void Initialize(SILabEfDbContextInitializationContext initializationContext)
        {
            var uowOptions = initializationContext.UnitOfWork.Options;
            if (uowOptions.Timeout.HasValue &&
                Database.IsRelational() &&
                !Database.GetCommandTimeout().HasValue)
            {
                Database.SetCommandTimeout(uowOptions.Timeout.Value.TotalSeconds.To<int>());
            }

            ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
        }

        protected void ConfigureGlobalValueConverter<TEntity>(ModelBuilder modelBuilder, IMutableEntityType entityType)
           where TEntity : class
        {
            if (entityType.BaseType == null && 
                !typeof(TEntity).IsDefined(typeof(OwnedAttribute), true) &&
                !entityType.IsOwned())
            {
                var dateTimeValueConverter = new DateTimeValueConverter();
                var dateTimePropertyInfos = DateTimePropertyInfoHelper.GetDatePropertyInfos(typeof(TEntity));
                dateTimePropertyInfos.DateTimePropertyInfos.ForEach(property =>
                {
                    modelBuilder
                        .Entity<TEntity>()
                        .Property(property.Name)
                        .HasConversion(dateTimeValueConverter);
                });
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        protected SILabDbContext(DbContextOptions options)
            : base(options)
        {
            InitializeDbContext();
        }

        private void InitializeDbContext()
        {
            SetNullsForInjectedProperties();
        }

        private void SetNullsForInjectedProperties()
        {
            Logger = NullLogger.Instance; 
            EntityChangeEventHelper = NullEntityChangeEventHelper.Instance;
            GuidGenerator = SequentialGuidGenerator.Instance;
            EventBus = NullEventBus.Instance;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                ConfigureGlobalFiltersMethodInfo
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, new object[] { modelBuilder, entityType });

                ConfigureGlobalValueConverterMethodInfo
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, new object[] { modelBuilder, entityType });
            }
        }

        protected virtual Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>()
           where TEntity : class
        {
            Expression<Func<TEntity, bool>> expression = null;

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> softDeleteFilter = e => !IsSoftDeleteFilterEnabled || !((ISoftDelete)e).IsDeleted;
                expression = expression == null ? softDeleteFilter : CombineExpressions(expression, softDeleteFilter);
            }
             

            return expression;
        }

        protected void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder, IMutableEntityType entityType)
           where TEntity : class
        {
            if (entityType.BaseType == null && ShouldFilterEntity<TEntity>(entityType))
            {
                var filterExpression = CreateFilterExpression<TEntity>();
                if (filterExpression != null)
                {
                    modelBuilder.Entity<TEntity>().HasQueryFilter(filterExpression);
                }
            }
        }
         

        protected virtual bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType) where TEntity : class
        {
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
             
            return false;
        }

        protected virtual Expression<Func<T, bool>> CombineExpressions<T>(Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            return ExpressionCombiner.Combine(expression1, expression2);
        }

        public override int SaveChanges()
        {
            try
            {
                var changeReport = ApplyMainConcepts();
                var result = base.SaveChanges();
                EntityChangeEventHelper.TriggerEvents(changeReport);
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new DbConcurrencyException(ex.Message, ex);
            }
        }

        protected virtual EntityChangeReport ApplyMainConcepts()
        {
            var changeReport = new EntityChangeReport();

            var userId = GetAuditUserId();

            foreach (var entry in ChangeTracker.Entries().ToList())
            {
                if (entry.State != EntityState.Modified && entry.CheckOwnedEntityChange())
                {
                    Entry(entry.Entity).State = EntityState.Modified;
                }

                ApplyEntityStateConceptThings(entry, userId, changeReport);
            }

            return changeReport;
        }

        protected virtual void ApplyEntityStateConceptThings(EntityEntry entry, long? userId, EntityChangeReport changeReport)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    ApplyConceptsForAddedEntity(entry, userId, changeReport);
                    break;
                case EntityState.Modified:
                    ApplyConceptsForModifiedEntity(entry, userId, changeReport);
                    break;
                case EntityState.Deleted:
                    ApplyConceptsForDeletedEntity(entry, userId, changeReport);
                    break;
            }

            AddDomainEvents(changeReport.DomainEvents, entry.Entity);
        }

       
        protected virtual long? GetAuditUserId()
        {
            if (Session.UserId.HasValue &&
                CurrentUnitOfWorkProvider != null &&
                CurrentUnitOfWorkProvider.Current != null)
            {
                return Session.UserId;
            }

            return null;
        }

        protected virtual void SetDeletionAuditProperties(object entityAsObj, long? userId)
        {
            EntityAuditingHelper.SetDeletionAuditProperties(              
                entityAsObj,              
                userId,
                CurrentUnitOfWorkProvider?.Current?.AuditFieldConfiguration
            );
        }

        protected virtual void ApplyConceptsForDeletedEntity(EntityEntry entry, long? userId, EntityChangeReport changeReport)
        {
            if (IsHardDeleteEntity(entry))
            {
                changeReport.ChangedEntities.Add(new EntityChangeEntry(entry.Entity, EntityChangeType.Deleted));
                return;
            }

            CancelDeletionForSoftDelete(entry);
            SetDeletionAuditProperties(entry.Entity, userId);
            changeReport.ChangedEntities.Add(new EntityChangeEntry(entry.Entity, EntityChangeType.Deleted));
        }

        protected virtual bool IsHardDeleteEntity(EntityEntry entry)
        {
            if (!EntityHelper.IsEntity(entry.Entity.GetType()))
            {
                return false;
            }

            if (CurrentUnitOfWorkProvider?.Current?.Items == null)
            {
                return false;
            }

            if (!CurrentUnitOfWorkProvider.Current.Items.ContainsKey(UnitOfWorkExtensionDataTypes.HardDelete))
            {
                return false;
            }

            var hardDeleteItems = CurrentUnitOfWorkProvider.Current.Items[UnitOfWorkExtensionDataTypes.HardDelete];
            if (!(hardDeleteItems is HashSet<string> objects))
            {
                return false;
            }

          
            var hardDeleteKey = EntityHelper.GetHardDeleteKey(entry.Entity);
            return objects.Contains(hardDeleteKey);
        }

        protected virtual void CancelDeletionForSoftDelete(EntityEntry entry)
        {
            if (!(entry.Entity is ISoftDelete))
            {
                return;
            }

            entry.Reload();
            entry.State = EntityState.Modified;
            entry.Entity.As<ISoftDelete>().IsDeleted = true;
        }


        protected virtual void ApplyConceptsForAddedEntity(EntityEntry entry, long? userId, EntityChangeReport changeReport)
        {
            CheckAndSetId(entry);
            SetCreationAuditProperties(entry.Entity, userId);
            changeReport.ChangedEntities.Add(new EntityChangeEntry(entry.Entity, EntityChangeType.Created));
        }

        protected virtual void ApplyConceptsForModifiedEntity(EntityEntry entry, long? userId, EntityChangeReport changeReport)
        {
            SetModificationAuditProperties(entry.Entity, userId);
            if (entry.Entity is ISoftDelete && entry.Entity.As<ISoftDelete>().IsDeleted)
            {
                SetDeletionAuditProperties(entry.Entity, userId);
                changeReport.ChangedEntities.Add(new EntityChangeEntry(entry.Entity, EntityChangeType.Deleted));
            }
            else
            {
                changeReport.ChangedEntities.Add(new EntityChangeEntry(entry.Entity, EntityChangeType.Updated));
            }
        }

        protected virtual void SetModificationAuditProperties(object entityAsObj, long? userId)
        {
            EntityAuditingHelper.SetModificationAuditProperties(               
                entityAsObj,              
                userId,
                CurrentUnitOfWorkProvider?.Current?.AuditFieldConfiguration
            );
        }

        protected virtual void CheckAndSetId(EntityEntry entry)
        {
            //Set GUID Ids
            var entity = entry.Entity as IEntity<Guid>;
            if (entity != null && entity.Id == Guid.Empty)
            {
                var idPropertyEntry = entry.Property("Id");

                if (idPropertyEntry != null && idPropertyEntry.Metadata.ValueGenerated == ValueGenerated.Never)
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
                CurrentUnitOfWorkProvider?.Current?.AuditFieldConfiguration
            );
        }

        protected virtual void AddDomainEvents(List<DomainEventEntry> domainEvents, object entityAsObj)
        {
            var generatesDomainEventsEntity = entityAsObj as IGeneratesDomainEvents;
            if (generatesDomainEventsEntity == null)
            {
                return;
            }

            if (generatesDomainEventsEntity.DomainEvents.IsNullOrEmpty())
            {
                return;
            }

            domainEvents.AddRange(generatesDomainEventsEntity.DomainEvents.Select(eventData => new DomainEventEntry(entityAsObj, eventData)));
            generatesDomainEventsEntity.DomainEvents.Clear();
        } 
    }
}