﻿using System.Threading.Tasks;

namespace Shared.Pathfinding.Api
{
    public interface IPathfinder
    {
        Task<PathResult> GetNavGraphPath(PathNode start, PathNode stop, Mobility mobility = null);

        Task<PathResult> GetNavMeshPath(PathNode start, PathNode stop, Mobility mobility = null);
    }
}