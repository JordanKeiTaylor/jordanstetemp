using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Recast
{
    public class RecastContext : IDisposable
    {
        private readonly RcContext _context;

        public RecastContext()
        {
            _context = new RcContext(RecastLibrary.rcContext_create());
        }

        public InputGeom LoadInputGeom(string path, bool invertYZ)
        {
            var handle = RecastLibrary.InputGeom_load(_context.DangerousGetHandle(), path, invertYZ);
            return new InputGeom(handle);
        }

        public void CalcGridSize(ref RcConfig config, InputGeom geom)
        {
            RecastLibrary.rcConfig_calc_grid_size(ref config, geom.DangerousGetHandle());
        }

        public CompactHeightfield CreateCompactHeightfield(RcConfig config, InputGeom geom)
        {
            var handle = RecastLibrary.compact_heightfield_create(_context.DangerousGetHandle(), ref config, geom.DangerousGetHandle());
            return new CompactHeightfield(handle);
        }

        public PolyMesh CreatePolyMesh(RcConfig config, CompactHeightfield chf)
        {
            var handle = RecastLibrary.polymesh_create(_context.DangerousGetHandle(), ref config, chf.DangerousGetHandle());
            return new PolyMesh(handle);
        }

        public PolyMeshDetail CreatePolyMeshDetail(RcConfig config, PolyMesh polyMesh, CompactHeightfield chf)
        {
            var handle = RecastLibrary.polymesh_detail_create(_context.DangerousGetHandle(), ref config, polyMesh.DangerousGetHandle(), chf.DangerousGetHandle());
            return new PolyMeshDetail(handle);
        }

        public NavMeshDataResult CreateNavMeshData(RcConfig config, PolyMeshDetail polyMeshDetail, PolyMesh polyMesh,
            InputGeom geom, int tx, int ty, float agentHeight, float agentRadius, float agentMaxClimb)
        {
            return RecastLibrary.navmesh_data_create(
                _context.DangerousGetHandle(),
                ref config,
                polyMeshDetail.DangerousGetHandle(),
                polyMesh.DangerousGetHandle(),
                geom.DangerousGetHandle(),
                tx,
                ty,
                agentHeight,
                agentRadius,
                agentMaxClimb);
        }

        public NavMesh CreateNavMesh(NavMeshDataResult navMeshDataResult)
        {
            var handle = RecastLibrary.navmesh_create(_context.DangerousGetHandle(), ref navMeshDataResult);
            return new NavMesh(handle);
        }

        public NavMeshQuery CreateNavMeshQuery(NavMesh navMesh)
        {
            var handle = RecastLibrary.navmesh_query_create(navMesh.DangerousGetHandle());
            return new NavMeshQuery(handle);
        }

        public PolyPointResult FindRandomPoint(NavMeshQuery navMeshQuery)
        {
            var polyPointResultPointer = RecastLibrary.navmesh_query_find_random_point(navMeshQuery.DangerousGetHandle());
            var polyPointResult = Marshal.PtrToStructure(polyPointResultPointer, typeof(PolyPointResult));
            
            RecastLibrary.poly_point_result_delete(polyPointResultPointer);

            return (PolyPointResult) polyPointResult;
        }

        public FindPathResult FindPath(NavMeshQuery navMeshQuery, PolyPointResult a, PolyPointResult b)
        {
            var filter = RecastLibrary.dtQueryFilter_create();
            var aPointer = Marshal.AllocHGlobal(3 * 4);
            Marshal.Copy(a.point, 0, aPointer, 3);
            
            var bPointer = Marshal.AllocHGlobal(3 * 4);
            Marshal.Copy(b.point, 0, bPointer, 3);
 
            var pathResultPointer = RecastLibrary.navmesh_query_find_path(navMeshQuery.DangerousGetHandle(), a.polyRef, b.polyRef, aPointer, bPointer, filter);
            Marshal.FreeHGlobal(aPointer);
            Marshal.FreeHGlobal(bPointer);
            RecastLibrary.dtQueryFilter_delete(filter);

            var pathResult = Marshal.PtrToStructure(pathResultPointer, typeof(FindPathResult));
            RecastLibrary.find_path_result_delete(pathResultPointer);
            return (FindPathResult) pathResult;
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}