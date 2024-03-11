using SILab.Domain.UnitOfWork;

namespace SILab.EntityFramework
{
    public class SILabEfDbContextInitializationContext
    {
        public IUnitOfWork UnitOfWork { get; }

        public SILabEfDbContextInitializationContext(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }
    }
}
