using System;

namespace Improbable.Enterprise.Sandbox.Pathfinding.Api
{
    public class NoPathFoundException : Exception
    {
        public NoPathFoundException(string message)
            : base(message)
        {
        }
    }
}