using SILab.Domain.UnitOfWork;
using SILab.Extensions;

namespace SILab.Domain.Entities.Auditing
{
    public static class EntityAuditingHelper
    {
        public static void SetCreationAuditProperties( 
            object entityAsObj, 
            long? userId,
            IReadOnlyList<AuditFieldConfiguration> auditFields)
        {
            var entityWithCreationTime = entityAsObj as IHasCreationTime;
            if (entityWithCreationTime == null)
            {
                //Object does not implement IHasCreationTime
                return;
            }

            if (entityWithCreationTime.CreationTime == default)
            {
                entityWithCreationTime.CreationTime = DateTime.Now;
            }

            if (!(entityAsObj is ICreationAudited))
            {
                //Object does not implement ICreationAudited
                return;
            }

            if (!userId.HasValue)
            {
                //Unknown user
                return;
            }

            var entity = entityAsObj as ICreationAudited;
            if (entity.CreatorUserId != null)
            {
                //CreatorUserId is already set
                return;
            }
  

            var creationUserIdFilter = auditFields?.FirstOrDefault(e => e.FieldName == SILabAuditFields.CreatorUserId);
            if (creationUserIdFilter != null && !creationUserIdFilter.IsSavingEnabled)
            {
                return;
            }
             
            entity.CreatorUserId = userId;
        }

        public static void SetDeletionAuditProperties(
            object entityAsObj,
            long? userId,
            IReadOnlyList<AuditFieldConfiguration> auditFields)
        {
            if (entityAsObj is IHasDeletionTime)
            {
                var entity = entityAsObj.As<IHasDeletionTime>();

                if (entity.DeletionTime == null)
                {
                    var deletionTimeFilter = auditFields?.FirstOrDefault(e => e.FieldName == AuditFields.DeletionTime);
                    if (deletionTimeFilter == null || deletionTimeFilter.IsSavingEnabled)
                    {
                        entityAsObj.As<IHasDeletionTime>().DeletionTime = DateTime.Now;
                    }
                }
            }

            if (entityAsObj is IDeletionAudited)
            {
                var entity = entityAsObj.As<IDeletionAudited>();

                if (entity.DeleterUserId != null)
                {
                    return;
                }

                if (userId == null)
                {
                    entity.DeleterUserId = null;
                    return;
                }

                var deleterUserIdFilter = auditFields?.FirstOrDefault(e => e.FieldName == AuditFields.DeleterUserId);
                if (deleterUserIdFilter != null && !deleterUserIdFilter.IsSavingEnabled)
                {
                    return;
                }
                else
                {
                    entity.DeleterUserId = userId;
                }
            }
        }

        public static void SetModificationAuditProperties(
           object entityAsObj,
           long? userId,
           IReadOnlyList<AuditFieldConfiguration> auditFields)
        {
            if (entityAsObj is IHasModificationTime)
            {
                var lastModificationTimeFilter = auditFields?.FirstOrDefault(e => e.FieldName == AuditFields.LastModificationTime);
                if (lastModificationTimeFilter == null || lastModificationTimeFilter.IsSavingEnabled)
                {
                    entityAsObj.As<IHasModificationTime>().LastModificationTime = DateTime.Now;
                }
            }

            if (!(entityAsObj is IModificationAudited))
            {
                //Entity does not implement IModificationAudited
                return;
            }

            var entity = entityAsObj.As<IModificationAudited>();

            if (userId == null)
            {
                //Unknown user
                entity.LastModifierUserId = null;
                return;
            }
  
            var lastModifierUserIdFilter = auditFields?.FirstOrDefault(e => e.FieldName == AbpAuditFields.LastModifierUserId);
            if (lastModifierUserIdFilter != null && !lastModifierUserIdFilter.IsSavingEnabled)
            {
                return;
            }

            //Finally, set LastModifierUserId!
            entity.LastModifierUserId = userId;
        }
    } 
}