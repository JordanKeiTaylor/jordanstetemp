﻿using System;
using System.Runtime.InteropServices;

namespace Recast
{
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
        public static extern void navmesh_delete(IntPtr navmesh);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr navmesh_query_create(IntPtr navMesh);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void navmesh_query_delete(IntPtr navMeshQuery);
    }
}