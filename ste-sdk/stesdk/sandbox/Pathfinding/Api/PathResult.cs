using System;
using System.Collections.Generic;

namespace Improbable.Sandbox.Pathfinding.Api
{
    public class PathResult
    {
        private List<PathEdge> _path;

        private Exception _exception; //TODO: Caller should handle exception

        public PathResult(List<PathEdge> path = null, Exception exception = null)
        {
            _path = path ?? new List<PathEdge>();
            _exception = exception;
        }

        public List<PathEdge> GetPath()
        {
            return _path;
        }

        internal void SetPath(List<PathEdge> path)
        {
            _path = path;
        }

        public Exception GetException()
        {
            return _exception;
        }

        internal void SetException(Exception ex)
        {
            _exception = ex;
        }
    }
}