using System.Data.Entity;

namespace SILab.EntityFramework.Repositories
{
    public interface IRepositoryWithDbContext
    {
        DbContext GetDbContext();
    }
}
