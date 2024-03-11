namespace SILab.Domain.UnitOfWork
{
    /// <summary>
    /// Standard filters of ABP.
    /// </summary>
    public static class AuditFields
    {
        public const string CreatorUserId = "CreatorUserId";

        public const string LastModifierUserId = "LastModifierUserId";

        public const string DeleterUserId = "DeleterUserId";

        public const string LastModificationTime = "LastModificationTime";

        public const string DeletionTime = "DeletionTime";
    }
}
