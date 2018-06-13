using System;

namespace stesdk.sandbox.Pathfinding.Api
{
    public class NoPathFoundException : Exception
    {
        public NoPathFoundException(string message)
            : base(message)
        {
        }
    }
}