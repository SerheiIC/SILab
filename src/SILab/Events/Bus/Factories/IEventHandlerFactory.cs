using SILab.Events.Bus.Handlers;

namespace SILab.Events.Bus.Factories
{
    public interface IEventHandlerFactory
    {
        /// <summary>
        /// Gets an event handler.
        /// </summary>
        /// <returns>The event handler</returns>
        IEventHandler GetHandler();

        /// <summary>
        /// Gets type of the handler (without creating an instance).
        /// </summary>
        /// <returns></returns>
        Type GetHandlerType();

        /// <summary>
        /// Releases an event handler.
        /// </summary>
        /// <param name="handler">Handle to be released</param>
        void ReleaseHandler(IEventHandler handler);
    }
}
