using System;

namespace Improbable.sandbox.Pathfinding.Api
{
    public class NoPathFoundException : Exception
    {
        public NoPathFoundException(string message)
            : base(message)
        {
        }
    }
}