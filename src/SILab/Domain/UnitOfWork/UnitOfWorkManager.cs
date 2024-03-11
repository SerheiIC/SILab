using SILab.Dependency;
using SILab.Domain.Repositories;
using SILab.Domain.UnitOfWork.Handlers;
using System.Transactions;

namespace SILab.Domain.UnitOfWork
{
    internal class UnitOfWorkManager : IUnitOfWorkManager, ITransientDependency
    {
        private readonly IIocResolver _iocResolver;
        private readonly ICurrentUnitOfWorkProvider _currentUnitOfWorkProvider;
        private readonly IUnitOfWorkDefaultOptions _defaultOptions;

        public UnitOfWorkManager(
           IIocResolver iocResolver,
           ICurrentUnitOfWorkProvider currentUnitOfWorkProvider,
           IUnitOfWorkDefaultOptions defaultOptions)
        {
            _iocResolver = iocResolver;
            _currentUnitOfWorkProvider = currentUnitOfWorkProvider;
            _defaultOptions = defaultOptions;
        }

        public IActiveUnitOfWork Current
        {
            get
            {
                return _currentUnitOfWorkProvider.Current;
            }
        }

        public IUnitOfWorkCompleteHandle Begin()
        {
            return Begin(new UnitOfWorkOptions());
        }

        public IUnitOfWorkCompleteHandle Begin(TransactionScopeOption scope)
        {
            return Begin(new UnitOfWorkOptions { Scope = scope });
        }

        public IUnitOfWorkCompleteHandle Begin(UnitOfWorkOptions options)
        {
            options.FillDefaultsForNonProvidedOptions(_defaultOptions);

            var outerUow = _currentUnitOfWorkProvider.Current;

            if (options.Scope == TransactionScopeOption.Required && outerUow != null)
            {
                return outerUow.Options?.Scope == TransactionScopeOption.Suppress
                    ? new InnerUnitOfWorkCompleteHandle(outerUow)
                    : new InnerUnitOfWorkCompleteHandle();
            }

            var unitOfWork = _iocResolver.Resolve<IUnitOfWork>();

            unitOfWork.Completed += (sender, args) =>
            {
                _currentUnitOfWorkProvider.Current = null!;
            };

            unitOfWork.Failed += (sender, args) =>
            {
                _currentUnitOfWorkProvider.Current = null!;
            };

            unitOfWork.Disposed += (sender, args) =>
            {
                _iocResolver.Release(unitOfWork);
            };

            //Inherit filters from outer UOW
            if (outerUow != null)
            {
                options.FillOuterUowFiltersForNonProvidedOptions(outerUow.Filters.ToList());
            }

            unitOfWork.Begin(options);

            //Inherit tenant from outer UOW
            if (outerUow != null)
            {
                unitOfWork.SetTenantId(outerUow.GetTenantId(), false);
            }

            _currentUnitOfWorkProvider.Current = unitOfWork;

            return unitOfWork;
        }
    }
}
