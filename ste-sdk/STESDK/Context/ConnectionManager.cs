using System.Collections.Generic;

namespace Improbable.Context
{
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