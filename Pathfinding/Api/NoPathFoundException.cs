using System;

namespace Shared.Pathfinding.Api
{
    public class NoPathFoundException : Exception
    {
        public NoPathFoundException(string message)
            : base(message)
        {
        }
    }
}