using System.Threading.Tasks;

namespace Improbable.Navigation.Api
{
    public interface IMeshNavigator
    {
        /// <summary>
        /// Executes a Task that asynchronously retrieves a <see cref="PathResult"/> between <see cref="PathNode"/>
        /// start and stop if one exists.
        /// </summary>
        /// <param name="start">Start node</param>
        /// <param name="stop">Destination node</param>
        /// <returns>PathResult</returns>
        Task<PathResult> GetMeshPath(PathNode start, PathNode stop);

        /// <summary>
        /// Executes a Task that asynchronously samples a <<see cref="PathNode"/> from the navigation mesh.
        /// </summary>
        /// <returns></returns>
        Task<PathNode> GetRandomPoint();

        /// <summary>
        /// Execute a Task that asynchronously finds the nearest polygon to a given position within the supplied
        /// half extents.
        /// </summary>
        /// <param name="position">Position to search from</param>
        /// <param name="halfExtents">Half extents to search</param>
        /// <returns></returns>
        Task<PathNode> GetNearestPoly(Coordinates position, Vector3d halfExtents);
    }
}