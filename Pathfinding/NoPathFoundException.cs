using System;

namespace Shared.Pathfinding
{
    public class NoPathFoundException : Exception
    {
        public NoPathFoundException(string message) : base(message)
        {
        }
    }
}