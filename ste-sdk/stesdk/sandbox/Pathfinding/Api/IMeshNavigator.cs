using System.Threading.Tasks;

namespace Improbable.Sandbox.Pathfinding.Api
{
    public interface IMeshNavigator
    {
        Task<PathResult> GetMeshPath(PathNode start, PathNode stop, Mobility mobility = null);
    }
}