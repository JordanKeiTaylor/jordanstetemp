using System.Collections.Generic;

namespace Improbable.Navigation.Api
{
    /// <summary>
    /// A PathResult is returned when retrieving a path from the Navigation API. Prior to processing the path, one
    /// should check the result's <see cref="PathStatus"/>. The path is represented as a list of
    /// <see cref="PathNode{T}"/>. 
    /// </summary>
    /// <seealso cref="IGraphNavigator"/>
    /// <seealso cref="IMeshNavigator"/>
    public class PathResult
    {
        private PathStatus _status;
        private List<PathEdge> _path;
        private string _msg;

        public PathResult(PathStatus status = PathStatus.NotFound)
        {
            _status = status;
            _path = new List<PathEdge>();
        }

        public List<PathEdge> Path
        {
            get => _path;
            internal set => _path = value;
        }

        public PathStatus Status
        {
            get => _status;
            internal set => _status = value;
        }

        public string Message
        {
            get => _msg;
            internal set => _msg = value;
        }
    }
}