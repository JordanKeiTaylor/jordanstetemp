using System.Threading.Tasks;

namespace Improbable.Sandbox.Pathfinding.Api
{
    public interface IGraphNavigator
    {
        /// <summary>
        /// Retrieve a <see cref="PathResult"/> between <see cref="PathNode"/> start and stop
        /// if one exists. If a path does not exist, a <see cref="NoPathFoundException"/> is
        /// thrown.
        /// </summary>
        /// <param name="start">Start node</param>
        /// <param name="stop">Destination node</param>
        /// <returns>PathResult</returns>
        /// <exception cref="NoPathFoundException">Viable path does not exist.</exception>
        Task<PathResult> GetGraphPath(PathNode start, PathNode stop);
    }
}