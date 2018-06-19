using QuickGraph;

namespace Improbable.Enterprise.Sandbox.Pathfinding.Api
{
    public class PathEdge : IEdge<PathNode>
    {
        public PathNode Source { get; set; }

        public PathNode Target { get; set; }

        public double Weight { get; set; }
    }
}