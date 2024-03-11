using SILab.EntityFramework.Repositories;
using SILab.EntityFramework.Tests.Domain;

namespace SILab.EntityFramework.Tests.Ef
{
    public class SaleRepository : EfRepositoryBase<AutoPartsDbContext, Sale, Guid>
    {
        public SaleRepository(IDbContextProvider<AutoPartsDbContext> dbContextProvider) 
            : base(dbContextProvider)
        { 

        }
    }
}
