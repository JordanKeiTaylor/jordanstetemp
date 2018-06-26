using Improbable.Context;

namespace Improbable.sandbox
{
    /// <summary>
    /// Indicates that the class can handle changes to the Connection state - used in conjunction with the IConnectionManager.
    /// </summary>
    public interface IConnectionReceiver
    {

        /// <summary>
        /// Called when a connection is established.
        /// </summary>
        void AttachConnection(IConnection c);

        /// <summary>
        /// Called when a connection is lost.
        /// </summary>
        void DetachConnection(IConnection c);

    }
}