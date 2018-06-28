using System.Collections.Generic;

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

    public class ConnectionManager : IConnectionManager, IConnectionReceiver
    {
        private readonly List<IConnectionReceiver> _receivers = new List<IConnectionReceiver>();

        public void AddConnectionReceiver(IConnectionReceiver receiver)
        {
            _receivers.Add(receiver);
        }

        public void AttachConnection(IConnection c)
        {
            foreach (var receiver in _receivers)
            {
                receiver.AttachConnection(c);
            }
        }

        public void DetachConnection(IConnection c)
        {
            foreach (var receiver in _receivers)
            {
                receiver.DetachConnection(c);
            }
        }
    }
}
