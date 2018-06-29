using System.Collections.Generic;

namespace Improbable.Navigation.Api
{
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