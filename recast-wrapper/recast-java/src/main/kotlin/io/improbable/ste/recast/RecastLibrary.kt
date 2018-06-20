package io.improbable.ste.recast

import com.sun.jna.Library
import com.sun.jna.Native
import com.sun.jna.Pointer

interface RecastLibrary : Library {
    fun rcContext_create(): RcContext?
    fun rcContext_delete(context: RcContext)
    fun InputGeom_load(rcContext: RcContext, path: String, invertYZ: Boolean): InputGeom?
    fun compact_heightfield_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, inputGeom: InputGeom): RcCompactHeightfield?
    fun polymesh_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcCompactHeightfield: RcCompactHeightfield): RcPolyMesh.ByReference?
    fun polymesh_detail_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcPolyMesh: RcPolyMesh, rcCompactHeightfield: RcCompactHeightfield): RcPolyMeshDetail?
    fun navmesh_data_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcPolyMeshDetail: RcPolyMeshDetail, rcPolyMesh: RcPolyMesh, inputGeom: InputGeom, tx: Int, ty: Int, agentHeight: Float, agentRadius: Float, agentMaxClimb: Float): NavMeshDataResult.ByReference?
    fun rcConfig_calc_grid_size(config: RcConfig.ByReference, inputGeom: InputGeom)
    fun navmesh_create(rcContext: RcContext, data: NavMeshDataResult.ByReference): DtNavMesh
    fun navmesh_load_tiled_bin(path: String): DtNavMesh
    fun navmesh_delete(navMesh: DtNavMesh)
    fun navmesh_query_create(navMesh: DtNavMesh): DtNavMeshQuery
    fun navmesh_query_delete(navQuery: DtNavMeshQuery)
    fun navmesh_query_find_nearest_poly(navMeshQuery: DtNavMeshQuery, point: Pointer, halfExtents: Pointer): PolyPointResult.ByReference
    fun navmesh_query_find_path(navMeshQuery: DtNavMeshQuery, startRef: DtPolyRef, endRef: DtPolyRef, startPos: Pointer, endPos: Pointer, filter: DtQueryFilter): FindPathResult.ByReference
    fun navmesh_query_find_random_point(navMeshQuery: DtNavMeshQuery): PolyPointResult.ByReference
    fun dtQueryFilter_create(): DtQueryFilter
    fun dtQueryFilter_delete(filter: DtQueryFilter)
    fun navmesh_query_get_smooth_path(startPos: Pointer, startRef: DtPolyRef, endPos: Pointer, path: FindPathResult, filter: DtQueryFilter, navMesh: DtNavMesh, navMeshQuery: DtNavMeshQuery): SmoothPathResult.ByReference
    fun dtStatus_failed(dtStatus: DtStatus): Boolean

    companion object RecastLibrary {
        fun load() = Native.loadLibrary("recastwrapper", io.improbable.ste.recast.RecastLibrary::class.java) as io.improbable.ste.recast.RecastLibrary
    }
}
