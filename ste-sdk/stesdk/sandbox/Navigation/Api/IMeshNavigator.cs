using System.Threading.Tasks;

namespace Improbable.sandbox.Navigation.Api
{
    public interface IMeshNavigator
    {
        Task<PathResult> GetMeshPath(PathNode start, PathNode stop);
    }
}