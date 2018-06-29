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
    }
}