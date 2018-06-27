using System.Threading.Tasks;

namespace Improbable.Sandbox.Navigation.Api
{
    public interface IMeshNavigator
    {
        Task<PathResult> GetMeshPath(PathNode start, PathNode stop);
    }
}