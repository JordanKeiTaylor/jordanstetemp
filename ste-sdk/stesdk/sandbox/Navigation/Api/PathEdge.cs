using QuickGraph;

namespace Improbable.sandbox.Navigation.Api
{
    public class PathEdge : IEdge<PathNode>
    {
        public PathNode Source { get; set; }

        public PathNode Target { get; set; }

        public double Weight { get; set; }
    }
}