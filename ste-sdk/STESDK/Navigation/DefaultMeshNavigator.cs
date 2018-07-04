using System;
using System.Threading.Tasks;
using Improbable.Collections;
using Improbable.Extensions;
using Improbable.Navigation.Api;
using Improbable.Recast;
using Improbable.Recast.Types;

namespace Improbable.Navigation
{
    /// <summary>
    /// Default implementation of Mesh navigation. This navigator must be provided the complete mesh file at
    /// initialization.
    /// </summary>
    public class DefaultMeshNavigator : IMeshNavigator
    {
        private const uint DtSuccess = 1u << 30;

        private readonly NavMesh _navMesh;
        private readonly NavMeshQuery _navMeshQuery;
        private readonly RecastContext _ctx;

        private readonly float[] _halfExtents;
        
        /// <inheritdoc />
        /// <summary>
        /// Initialize a new DefaultMeshNavigator given a mesh file.
        /// </summary>
        /// <param name="navMeshFile">Mesh file</param>
        public DefaultMeshNavigator(string navMeshFile) : 
            this(navMeshFile, new[] { 10.0f, 10.0f, 10.0f }) { }

        /// <summary>
        /// Initialize a new DefaultMeshNavigator given a mesh file and halfExtents parameters.
        /// </summary>
        /// <param name="navMeshFile">Mesh file</param>
        /// <param name="halfExtents">Half extents</param>
        /// <exception cref="Exception">Throws an exception if the mesh file cannot be loaded.</exception>
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

        public Task<PathNode> GetRandomPoint()
        {
            return Task.Factory.StartNew(FindRandomPoint);
        }

        public Task<PathNode> GetNearestPoly(Coordinates position, Coordinates halfExtents)
        {
            return Task.Factory.StartNew(() => FindNearestPoly(position, halfExtents));
        }

        public PathNode FindNearestPoly(Coordinates position, Coordinates halfExtents)
        {
            var result = _ctx.FindNearestPoly(_navMeshQuery, new[] {(float) position.x, (float) position.y, (float) position.z},
                new[] {(float) halfExtents.x, (float) halfExtents.y, (float) halfExtents.z});

            if (HasSucceed(result.status))
            {
                return new PathNode
                {
                    Coords = new Coordinates(result.point[0], result.point[1], result.point[2]),
                    Id = (long) result.polyRef,
                    Node = result.polyRef
                };
            }

            return null;
        }

        private PathNode FindRandomPoint()
        {
            var randomPoint = _ctx.FindRandomPoint(_navMeshQuery);

            if (!HasSucceed(randomPoint.status))
            {
                throw new NavigationException("Failure while sampling random point from navigation mesh");
            }

            return new PathNode
            {
                Id = (long) randomPoint.polyRef,
                Coords = new Coordinates(randomPoint.point[0], randomPoint.point[1], randomPoint.point[2]),
                Node = randomPoint.polyRef
            };
        }
        
        private PathResult CalculatePath(PathNode start, PathNode stop)
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

        private List<PathEdge> ToPathEdges(float[] paths, int size)
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

        private Coordinates ToCoord(float[] point, int offset) 
        {
            return new Coordinates(point[offset], point[offset + 1], point[offset + 2]);
        }

        private bool HasSucceed(uint status) 
        {
            return (status & DtSuccess) != 0;
        }
    }
}