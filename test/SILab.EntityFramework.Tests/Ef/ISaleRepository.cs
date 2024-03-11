using SILab.Domain.Repositories;
using SILab.EntityFramework.Tests.Domain;

namespace SILab.EntityFramework.Tests.Ef
{
    internal interface ISaleRepository : IRepository<Sale, Guid>
    {
    }
}
