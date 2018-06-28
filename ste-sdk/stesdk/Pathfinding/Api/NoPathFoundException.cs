using System;

namespace Improbable.Pathfinding.Api
{
    public class NoPathFoundException : Exception
    {
        public NoPathFoundException(string message)
            : base(message)
        {
        }
    }
}