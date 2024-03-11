using System.Data.Entity;

namespace SILab.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public interface IDbContextProvider<TDbContext>
        where TDbContext : DbContext
    {
        TDbContext GetDbContext();

        Task<TDbContext> GetDbContextAsync();

    }
}
