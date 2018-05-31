using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace Recast
{
    using System.Runtime.InteropServices;

    public class RcContext : SafeHandleZeroOrMinusOneIsInvalid
    {
        public RcContext(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }
        
        protected override bool ReleaseHandle()
        {
            RecastLibrary.rcContext_delete(handle);
            return true;
        }
    }

    public class InputGeom : SafeHandleZeroOrMinusOneIsInvalid
    {
        public InputGeom(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            RecastLibrary.InputGeom_delete(handle);
            return true;
        }
    }

    public class CompactHeightfield : SafeHandleZeroOrMinusOneIsInvalid
    {
        public CompactHeightfield(IntPtr handle) : base(true)
        {
            SetHandle(handle);    
        }

        protected override bool ReleaseHandle()
        {
            RecastLibrary.compact_heightfield_delete(handle);
            return true;
        }
    }

    public class PolyMesh : SafeHandleZeroOrMinusOneIsInvalid
    {
        public PolyMesh(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            RecastLibrary.polymesh_delete(handle);
            return true;
        }
    }
    
    public class PolyMeshDetail : SafeHandleZeroOrMinusOneIsInvalid
    {
        public PolyMeshDetail(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            RecastLibrary.polymesh_detail_delete(handle);
            return true;
        }
    }

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
            var geom = new InputGeom(handle);

            if (geom.IsInvalid)
            {
                throw new IOException("Unable to load geometry");
            }

            return geom;
        }

        public void CalcGridSize(ref RcConfig config, InputGeom geom)
        {
            RecastLibrary.rcConfig_calc_grid_size(ref config, geom.DangerousGetHandle());
        }

        public CompactHeightfield CreateCompactHeightfield(RcConfig config, InputGeom geom)
        {
            var handle = RecastLibrary.compact_heightfield_create(_context.DangerousGetHandle(), ref config, geom.DangerousGetHandle());
            var chf = new CompactHeightfield(handle);

            if (chf.IsInvalid)
            {
                throw new ArgumentException("Exception while building CompactHeightfield.");
            }

            return chf;
        }

        public PolyMesh CreatePolyMesh(RcConfig config, CompactHeightfield chf)
        {
            var handle = RecastLibrary.polymesh_create(_context.DangerousGetHandle(), ref config, chf.DangerousGetHandle());
            var polyMesh = new PolyMesh(handle);

            if (polyMesh.IsInvalid)
            {
                throw new ArgumentException("Exception creating polymesh");
            }

            return polyMesh;
        }

        public PolyMeshDetail CreatePolyMeshDetail(RcConfig config, PolyMesh polyMesh, CompactHeightfield chf)
        {
            var handle = RecastLibrary.polymesh_detail_create(_context.DangerousGetHandle(), ref config, polyMesh.DangerousGetHandle(), chf.DangerousGetHandle());
            var polyMeshDetail = new PolyMeshDetail(handle);

            if (polyMeshDetail.IsInvalid)
            {
                throw new ArgumentException("Exception creating polymesh");
            }

            return polyMeshDetail;
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
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}