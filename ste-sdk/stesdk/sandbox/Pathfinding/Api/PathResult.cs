using System;
using System.Collections.Generic;

namespace Improbable.Sandbox.Pathfinding.Api
{
    public class PathResult
    {
        private readonly List<PathEdge> _path;

        private readonly Exception _exception;

        public PathResult(List<PathEdge> path = null, Exception exception = null)
        {
            _path = path ?? new List<PathEdge>();
            _exception = exception;
        }

        public List<PathEdge> GetPath()
        {
            return _path;
        }

        public Exception GetException()
        {
            return _exception;
        }
    }
}