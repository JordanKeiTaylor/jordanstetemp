using System.Threading.Tasks;

namespace Improbable.Sandbox.Pathfinding.Api
{
    public class IMeshNavigator
    {
        Task<PathResult> GetMeshPath(PathNode start, PathNode stop, Mobility mobility = null);
    }
}