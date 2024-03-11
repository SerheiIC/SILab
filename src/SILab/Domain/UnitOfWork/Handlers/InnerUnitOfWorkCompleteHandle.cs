using System.Runtime.InteropServices;

namespace SILab.Domain.UnitOfWork.Handlers
{
    /// <summary>
    /// This handle is used for inner unit of work scopes.
    /// A inner unit of work scope actually uses outer unit of work scope
    /// and has no effect on <see cref="IUnitOfWorkCompleteHandle.Complete"/> call.
    /// But if it's not called, an exception is thrown at end of the UOW to rollback the UOW.
    /// </summary>
    internal class InnerUnitOfWorkCompleteHandle : IUnitOfWorkCompleteHandle
    { 
        private volatile bool _isCompleteCalled;
        private volatile bool _isDisposed;
        private readonly IUnitOfWork _parentUnitOfWork;

        public InnerUnitOfWorkCompleteHandle()
        {

        }


        public InnerUnitOfWorkCompleteHandle(IUnitOfWork parentUnitOfWork)
        {
            _parentUnitOfWork = parentUnitOfWork;
        }

        public void Complete()
        {
            _isCompleteCalled = true;
            _parentUnitOfWork.SaveChanges();
        }

        public async Task CompleteAsync()
        {
            _isCompleteCalled = true;
            await _parentUnitOfWork.SaveChangesAsync();
            await Task.FromResult(0);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (!_isCompleteCalled)
            {
                if (HasException())
                {
                    return;
                }

                throw new SILabException("Did not call Complete method of a unit of work.");
            }
        }

        private static bool HasException()
        {
            try
            {
                return Marshal.GetExceptionPointers() != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
