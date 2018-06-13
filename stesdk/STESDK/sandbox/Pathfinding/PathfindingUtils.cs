using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using stesdk.sandbox.Extensions;

namespace stesdk.sandbox.Pathfinding
{
    public static class PathfindingUtils<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Calculates the relative time it takes to traverse from start to end given the speed limit.
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <param name="speedLimit">Speed</param>
        /// <returns>Relative time to travel from start to end</returns>
        public static double CalculateEdgeWeight(Coordinate start, Coordinate end, float speedLimit)
        {
            return start.DistanceTo(end) / speedLimit;
        }

        /// <summary>
        /// Calculates the Strongly Connected Components of a graph. An SCC is known as sub-graph where every node
        /// in that sub-graph can traverse to any other node in that sub-graph. This method groups TVertex elements
        /// into a strongly connected component dictionary.
        /// </summary>
        /// <param name="graph">Graph to compute SCCs</param>
        /// <returns>Dictionary of SCCs</returns>
        public static Dictionary<int, List<TVertex>> CalculateStronglyConnectedComponents(IVertexListGraph<TVertex, TEdge> graph)
        {
            var cc = new QuickGraph.Algorithms.ConnectedComponents.StronglyConnectedComponentsAlgorithm<TVertex, TEdge>(graph);

            cc.Compute();

            var groups = cc.Components.GroupBy(kv => kv.Value);

            var sccs = new Dictionary<int, List<TVertex>>();

            foreach (var grouping in groups)
            {
                if (sccs.ContainsKey(grouping.Key) == false)
                {
                    sccs[grouping.Key] = new List<TVertex>();
                }

                foreach (var nodeKv in grouping)
                {
                    sccs[grouping.Key].Add(nodeKv.Key);
                }
            }

            // Removing single node SSCs
            var removals = new List<int>();
            foreach (var key in sccs.Keys)
            {
                if (sccs[key].Count < 2)
                {
                    removals.Add(key);
                }
            }

            foreach (var key in removals)
            {
                sccs.Remove(key);
            }

            return sccs;
        }
    }
}