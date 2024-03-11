namespace SILab.Domain.UnitOfWork
{
    public interface IUnitOfWorkManagerAccessor
    {
        public IUnitOfWorkManager UnitOfWorkManager { get; }
    }
}
