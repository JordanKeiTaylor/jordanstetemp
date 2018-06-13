using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;
using stesdk.sandbox.Extensions;
using stesdk.sandbox.Pathfinding.Api;

namespace stesdk.sandbox.Pathfinding
{
    public class DefaultNavGraphPathingAlgorithm
    {
        private readonly IVertexListGraph<PathNode, PathEdge> _graph;

        public DefaultNavGraphPathingAlgorithm(IVertexListGraph<PathNode, PathEdge> graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Find a path from PathNode start to PathNode stop. This method returns an
        /// asynchronous task that must be checked for completion. If a path does not
        /// exists, the Task will return a null value.
        /// </summary>
        /// <param name="start">Begining location of pathfind</param>
        /// <param name="stop">Ending location of pathfind</param>
        /// <returns>Task that contains a Path</returns>
        public async Task<PathResult> GetPath(PathNode start, PathNode stop)
        {
            var astar = new AStarShortestPathAlgorithm<PathNode, PathEdge>(
                _graph,
                Weight,
                node => node.Coords.DistanceTo(stop.Coords));
            astar.SetRootVertex(start);
            astar.FinishVertex += vertex =>
            {
                if (Equals(vertex, stop))
                {
                    astar.Abort();
                }
            };

            var wrapper = new QuickGraphPathfindingWrapper<PathEdge>(astar, start, stop);

            return await Task.Factory.StartNew(() =>
            {
                PathResult result;

                try
                {
                    result = new PathResult(wrapper.FindPath());
                }
                catch (Exception e)
                {
                    result = new PathResult(null, e);
                }

                return result;
            });
        }

        /// <summary>
        /// Passed as a Func to the algorithm to determine edge weights.
        /// </summary>
        /// <param name="edge">Current end</param>
        /// <returns>Edge weight</returns>
        private static double Weight(PathEdge edge)
        {
            return edge.Weight;
        }

        /// <summary>
        /// Small wrapper for QuickGraph ShortestPathAlgorithmBase that enables clients to
        /// retrieve a shortest path as an IEnumerable<TEdge>
        /// </summary>
        /// <typeparam name="TEdge">Generic Edge Type that implements QuikcGraph IEdge<PathNode></typeparam>
        private class QuickGraphPathfindingWrapper<TEdge>
            where TEdge : IEdge<PathNode>
        {
            private readonly ShortestPathAlgorithmBase<PathNode, TEdge, IVertexListGraph<PathNode, TEdge>> _algorithm;
            private readonly PathNode _start;
            private readonly PathNode _stop;

            public QuickGraphPathfindingWrapper(
                ShortestPathAlgorithmBase<PathNode, TEdge, IVertexListGraph<PathNode, TEdge>> algorithm,
                PathNode start,
                PathNode stop)
            {
                _algorithm = algorithm;
                _start = start;
                _stop = stop;
            }

            /// <summary>
            /// Computes the algorithm with an attached Vertex Predecessor Recorder to enable pathfinding
            /// </summary>
            /// <returns>TryFunc that queries the recorderObserver for a path</returns>
            public List<TEdge> FindPath()
            {
                var recorderObserver = new VertexPredecessorRecorderObserver<PathNode, TEdge>();

                recorderObserver.Attach(_algorithm);

                _algorithm.Compute();

                var predecessors = recorderObserver.VertexPredecessors;

                if (predecessors.TryGetPath(_stop, out var edges))
                {
                    return new List<TEdge>(edges as TEdge[] ?? edges.ToArray());
                }

                throw new NoPathFoundException($"No path found between {_start.EntityId.Id} and {_stop.EntityId.Id}");
            }
        }
    }
}