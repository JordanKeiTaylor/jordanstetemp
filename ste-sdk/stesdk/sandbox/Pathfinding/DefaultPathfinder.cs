using System.Collections.Generic;
using System.Threading.Tasks;
using Improbable.Sandbox.Pathfinding.Api;
using QuickGraph;

namespace Improbable.Sandbox.Pathfinding
{
    public class DefaultPathfinder : IPathfinder
    {
        private readonly List<PathEdge> _edges;
        private readonly Dictionary<EntityId, PathNode> _nodes;
        private readonly AdjacencyGraph<PathNode, PathEdge> _graph;
        private readonly DefaultNavGraphPathingAlgorithm _navGraphAlgorithm;

        public DefaultPathfinder(Dictionary<EntityId, PathNode> nodes, List<PathEdge> edges)
        {
            _edges = edges;
            _nodes = nodes;
            _graph = BuildRoutingGraph(_nodes.Values, _edges);
            _navGraphAlgorithm = new DefaultNavGraphPathingAlgorithm(_graph);
        }

        public Task<PathResult> GetNavGraphPath(PathNode start, PathNode stop, Mobility mobility = null)
        {
            return _navGraphAlgorithm.GetPath(start, stop);
        }

        public Task<PathResult> GetNavMeshPath(PathNode start, PathNode stop, Mobility mobility = null)
        {
            throw new System.NotImplementedException();
        }

        private AdjacencyGraph<PathNode, PathEdge> BuildRoutingGraph(
            IEnumerable<PathNode> nodes,
            IEnumerable<PathEdge> edges)
        {
            var graph = new AdjacencyGraph<PathNode, PathEdge>();

            foreach (var node in nodes)
            {
                graph.AddVertex(node);
            }

            foreach (var edge in edges)
            {
                graph.AddEdge(edge);
            }

            return graph;
        }
    }
}