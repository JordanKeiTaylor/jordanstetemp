using System;
using System.Runtime.InteropServices;

namespace Recast
{
    using DtPolyRef = UInt64;
    
    // NOTE: There is possibly a .NET bug when returning struct values larger than 8 bytes from native code.
    // See: https://stackoverflow.com/questions/30363629/marshalling-of-c-struct-as-return-value-of-c-sharp-delegate
    internal static class RecastLibrary
    {
        const string Library = "librecastwrapper";

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void rcConfig_calc_grid_size(ref RcConfig config, IntPtr geom);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr rcContext_create();
        
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void rcContext_delete(IntPtr context);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr InputGeom_load(IntPtr context, string path, bool invertYZ);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr InputGeom_delete(IntPtr inputGeom);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr compact_heightfield_create(IntPtr context, ref RcConfig config, IntPtr geom);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr compact_heightfield_delete(IntPtr chf);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr polymesh_create(IntPtr context, ref RcConfig config, IntPtr chf);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr polymesh_delete(IntPtr polyMesh);
        
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr polymesh_detail_create(IntPtr context, ref RcConfig config, IntPtr polyMesh, IntPtr chf);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr polymesh_detail_delete(IntPtr polyMeshDetail);
        
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern ref NavMeshDataResult navmesh_data_create(IntPtr context, ref RcConfig config, IntPtr polyMeshDetail, IntPtr polyMesh, IntPtr geom, int tx, int ty, float agentHeight, float agentRadius, float agentMaxClimb);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr navmesh_create(IntPtr context, ref NavMeshDataResult navMeshDataResult);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr navmesh_load_tiled_bin(char[] path);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void navmesh_delete(IntPtr navmesh);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr navmesh_query_create(IntPtr navMesh);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void navmesh_query_delete(IntPtr navMeshQuery);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr navmesh_query_find_random_point(IntPtr navMeshQuery);
        
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void poly_point_result_delete(IntPtr polyPointResult);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dtQueryFilter_create();
        
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void dtQueryFilter_delete(IntPtr filter);

        // AS: Returning this struct by ref just doesn't seem to work. Possibly due to it's size.
        // I have no idea why not.
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr navmesh_query_find_path(IntPtr navMeshQuery, DtPolyRef startRef, DtPolyRef endRef,
            IntPtr startPos, IntPtr endPos, IntPtr filter);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void find_path_result_delete(IntPtr findPathResult);
        
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr navmesh_query_get_smooth_path(IntPtr startPos, DtPolyRef startRef, IntPtr endPos, ref FindPathResult path, IntPtr filter, IntPtr navMesh, IntPtr navQuery);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr smooth_path_result_delete(IntPtr smoothPathResult);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool dtPolyRef_is_64bit();

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void random_set_seed(int seed);

    }
}