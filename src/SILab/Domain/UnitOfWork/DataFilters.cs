namespace SILab.Domain.UnitOfWork
{
    public static class DataFilters
    {
        /// <summary>
        /// "SoftDelete".
        /// Soft delete filter.
        /// Prevents getting deleted data from database.
        /// See <see cref="ISoftDelete"/> interface.
        /// </summary>
        public const string SoftDelete = "SoftDelete";
 
        /// <summary>
        /// Standard parameters of SILab.
        /// </summary>
        public static class Parameters
        { 
            /// <summary>
            /// "isDeleted".
            /// </summary>
            public const string IsDeleted = "isDeleted";
        }
    }
}
