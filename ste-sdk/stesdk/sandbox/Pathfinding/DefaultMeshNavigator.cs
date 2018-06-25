using System;
using System.Threading.Tasks;
using Improbable.Collections;
using Improbable.Sandbox.Pathfinding.Api;
using Improbable.Recast;
using Improbable.Recast.Types;
using Improbable.Sandbox.Extensions;

namespace Improbable.Sandbox.Pathfinding
{
    public class DefaultMeshNavigator : IMeshNavigator
    {
        const uint DT_SUCCESS = 1u << 30;
        
        NavMesh _navMesh;
        NavMeshQuery _navMeshQuery;
        RecastContext _ctx;

        float[] _halfExtents = { 10.0f, 10.0f, 10.0f };

        public DefaultMeshNavigator(string navMeshFile)
        {
            _ctx = new RecastContext();
            _navMesh = _ctx.LoadTiledNavMeshBinFile(navMeshFile);

            if (_navMesh.IsInvalid)
            {
                throw new Exception("Failed to load nav mesh");
            }

            _navMeshQuery = _ctx.CreateNavMeshQuery(_navMesh);
        }
        
        public Task<PathResult> GetMeshPath(PathNode start, PathNode stop)
        {
            return Task.Factory.StartNew(() =>
            {
                var result = new PathResult();
                try
                {
                    var paths = CalculatePath(start, stop);
                    result.SetPath(ToPathEdges(paths));
                }
                catch (Exception ex)
                {
                    result.SetException(ex);
                }
                return result;
            });
        }

        float[] CalculatePath(PathNode start, PathNode stop)
        {
            var startPoint = _ctx.FindNearestPoly(_navMeshQuery, start.Coords.ToFloat(), _halfExtents);
            if (HasFailed(startPoint.status))
            {
                throw new Exception("Failed to find nearest poly for start node");
            }
            
            var stopPoint = _ctx.FindNearestPoly(_navMeshQuery, stop.Coords.ToFloat(), _halfExtents);
            if (HasFailed(stopPoint.status))
            {
                throw new Exception("Failed to find nearest poly for stop node");
            }
            
            var polyPath = _ctx.FindPath(_navMeshQuery, startPoint, stopPoint);
            if (HasFailed(polyPath.status))
            {
                throw new NoPathFoundException("Failed to find path between start and stop nodes");
            }
            
            var smoothPath = _ctx.FindSmoothPath(_navMeshQuery, _navMesh, polyPath, startPoint, stopPoint);
            if (smoothPath.pathCount <= 0)
            {
                throw new NoPathFoundException("Failed to find smooth path between start and stop nodes");
            }
            
            return smoothPath.path;
        }

        List<PathEdge> ToPathEdges(float[] paths)
        {
            var prev = new PathNode {Coords = ToCoord(paths, 0)};
            var edges = new List<PathEdge>();
            for (var i = 3; i < paths.Length; i += 3)
            {
                var node = new PathNode {Coords = ToCoord(paths, i)};
                edges.Add(new PathEdge {Source = prev, Target = node});
                prev = node;
            }
            return edges;
        }
        
        Coordinates ToCoord(float[] point, int offset) 
        {
            return new Coordinates(point[offset], point[offset + 1], point[offset + 2]);
        }
        
        bool HasFailed(uint status) 
        {
            return (status & DT_SUCCESS) == 0;
        }
    }
}