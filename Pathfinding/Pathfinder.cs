using System;
using System.Collections.Generic;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace Shared.Pathfinding
{
    /// <summary>
    /// Small wrapper for QuickGraph ShortestPathAlgorithmBase that enables clients to
    /// retrieve a shortest path as an IEnumerable<TEdge>
    /// </summary>
    /// <typeparam name="TVertex">Generic Node Type</typeparam>
    /// <typeparam name="TEdge">Generic Edge Type that implements QuikcGraph IEdge<TVertex></typeparam>
    public class Pathfinder<TVertex, TEdge> where TEdge : IEdge<TVertex>
    {
        private readonly ShortestPathAlgorithmBase<TVertex, TEdge, IVertexListGraph<TVertex, TEdge>> _algorithm;

        public Pathfinder(ShortestPathAlgorithmBase<TVertex, TEdge, IVertexListGraph<TVertex, TEdge>> algorithm)
        {
            _algorithm = algorithm;
        }

        /// <summary>
        /// Computes the algorithm with an attached Vertex Predecessor Recorder to enable pathfinding
        /// </summary>
        /// <returns>TryFunc that queries the recorderObserver for a path</returns>
        public IEnumerable<TEdge> FindPath(TVertex destination)
        {
            var recorderObserver = new VertexPredecessorRecorderObserver<TVertex, TEdge>();

            recorderObserver.Attach(_algorithm);

            _algorithm.Compute();

            var predecessors = recorderObserver.VertexPredecessors;

            if (predecessors.TryGetPath(destination, out var edges))
            {
                return edges;
            }
            throw new NoPathFoundException("No path found.");
        }
    }
}