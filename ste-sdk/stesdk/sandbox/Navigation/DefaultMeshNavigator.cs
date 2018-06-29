using System;
using System.Threading.Tasks;
using Improbable.Collections;
using Improbable.Recast;
using Improbable.Recast.Types;
using Improbable.Sandbox.Extensions;
using Improbable.Sandbox.Navigation.Api;

namespace Improbable.Sandbox.Navigation
{
    public class DefaultMeshNavigator : IMeshNavigator
    {
        const uint DT_SUCCESS = 1u << 30;

        readonly NavMesh _navMesh;
        readonly NavMeshQuery _navMeshQuery;
        readonly RecastContext _ctx;

        readonly float[] _halfExtents;
        
        public DefaultMeshNavigator(string navMeshFile) : 
            this(navMeshFile, new[] { 10.0f, 10.0f, 10.0f }) { }

        public DefaultMeshNavigator(string navMeshFile, float[] halfExtents)
        {
            _ctx = new RecastContext();
            _navMesh = _ctx.LoadTiledNavMeshBinFile(navMeshFile);

            if (_navMesh.IsInvalid)
            {
                throw new Exception("Failed to load nav mesh");
            }

            _navMeshQuery = _ctx.CreateNavMeshQuery(_navMesh);
            _halfExtents = halfExtents;
        }
        
        public Task<PathResult> GetMeshPath(PathNode start, PathNode stop)
        {
            return Task.Factory.StartNew(() => CalculatePath(start, stop));
        }

        PathResult CalculatePath(PathNode start, PathNode stop)
        {
            var result = new PathResult();
            var startPoint = _ctx.FindNearestPoly(_navMeshQuery, start.Coords.ToFloat(), _halfExtents);
            if (!HasSucceed(startPoint.status))
            {
                result.Status = PathStatus.Error;
                result.Message = "Failed to find nearest poly for start node";
                return result;
            }
            
            var stopPoint = _ctx.FindNearestPoly(_navMeshQuery, stop.Coords.ToFloat(), _halfExtents);
            if (!HasSucceed(stopPoint.status))
            {
                result.Status = PathStatus.Error;
                result.Message = "Failed to find nearest poly for stop node";
                return result;
            }
            
            var polyPath = _ctx.FindPath(_navMeshQuery, startPoint, stopPoint);
            if (!HasSucceed(polyPath.status))
            {
                result.Status = PathStatus.NotFound;
                result.Message = "Failed to find path between start and stop nodes";
                return result;
            }
            
            var smoothPath = _ctx.FindSmoothPath(_navMeshQuery, _navMesh, polyPath, startPoint, stopPoint);
            if (smoothPath.pathCount <= 0)
            {
                result.Status = PathStatus.NotFound;
                result.Message = "Failed to find smooth path between start and stop nodes";
                return result;
            }

            result.Status = PathStatus.Success;
            result.Path = ToPathEdges(smoothPath.path, smoothPath.pathCount);
            return result;
        }

        List<PathEdge> ToPathEdges(float[] paths, int size)
        {
            var prev = new PathNode {Coords = ToCoord(paths, 0)};
            var edges = new List<PathEdge>();
            for (var i = 3; i < size; i += 3)
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
        
        bool HasSucceed(uint status) 
        {
            return (status & DT_SUCCESS) != 0;
        }
    }
}