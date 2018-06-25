using System.Threading.Tasks;

namespace Improbable.Sandbox.Pathfinding.Api
{
    public interface IGraphNavigator
    {
        Task<PathResult> GetGraphPath(PathNode start, PathNode stop, Mobility mobility = null);
    }
}