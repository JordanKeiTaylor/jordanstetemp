using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Improbable.Extensions;
using Improbable.Navigation.Api;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace Improbable.Navigation
{
    public class DefaultGraphNavigator : IGraphNavigator
    {
        private readonly AdjacencyGraph<PathNode, PathEdge> _graph;

        public DefaultGraphNavigator(Dictionary<EntityId, PathNode> nodes, List<PathEdge> edges)
        {
            _graph = BuildRoutingGraph(nodes.Values, edges);
        }

        public Task<PathResult> GetGraphPath(PathNode start, PathNode stop)
        {
            return Task.Factory.StartNew(() => CalculatePath(start, stop));
        }

        private AdjacencyGraph<PathNode, PathEdge> BuildRoutingGraph(
            IEnumerable<PathNode> nodes,
            IEnumerable<PathEdge> edges
        ) {
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
        
        private PathResult CalculatePath(PathNode start, PathNode stop)
        {
            var result = new PathResult();
            var astar = new AStarShortestPathAlgorithm<PathNode, PathEdge>(
                _graph,
                edge => edge.Weight,
                node => node.Coords.DistanceTo(stop.Coords)
            );
            astar.SetRootVertex(start);
            astar.FinishVertex += vertex =>
            {
                if (Equals(vertex, stop))
                {
                    astar.Abort();
                }
            };
            
            var recorderObserver = new VertexPredecessorRecorderObserver<PathNode, PathEdge>();
            recorderObserver.Attach(astar);
            astar.Compute();
            var predecessors = recorderObserver.VertexPredecessors;
            if (predecessors.TryGetPath(stop, out var edges))
            {
                result.Status = PathStatus.Success;
                result.Path = new List<PathEdge>(edges as PathEdge[] ?? edges.ToArray());
                return result;
            }

            result.Status = PathStatus.NotFound;
            result.Message = $"No path found between {start.Id} and {stop.Id}";
            return result;
        }
    }
}