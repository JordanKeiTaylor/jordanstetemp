using System.Collections.Generic;

namespace Shared
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
        private List<IConnectionReceiver> receivers = new List<IConnectionReceiver>();


        public void AddConnectionReceiver(IConnectionReceiver receiver)
        {
            receivers.Add(receiver);
        }

        public void AttachConnection(IConnection c)
        {
            foreach (var receiver in receivers)
            {
                receiver.AttachConnection(c);
            }
        }

        public void DetachConnection(IConnection c)
        {
            foreach (var receiver in receivers)
            {
                receiver.DetachConnection(c);
            }
        }
    }
}
