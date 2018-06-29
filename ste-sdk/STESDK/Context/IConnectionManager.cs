namespace Improbable.Context
{
    /// <summary>
    /// A connection manager is a class that handles connections (for example the SpatialOS thread in the UDPClient).
    /// It's primary function is to maintain a set of IConnectionReceiver's which are notified of connetion state changes
    /// via the AttachConnection and DetachConnection functions.
    /// </summary>
    public interface IConnectionManager
    {
        void AddConnectionReceiver(IConnectionReceiver receiver);
    }
}
