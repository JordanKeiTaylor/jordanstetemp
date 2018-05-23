package io.improbable.ste.recast

import com.sun.jna.Library
import com.sun.jna.Native

interface RecastLibrary : Library {
    fun rcContext_create(): RcContext?
    fun rcContext_delete(context: RcContext)
    fun load_mesh(rcContext: RcContext, path: String, invertYZ: Boolean): InputGeom?
    fun compact_heightfield_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, inputGeom: InputGeom): RcCompactHeightfield?
    fun polymesh_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcCompactHeightfield: RcCompactHeightfield): RcPolyMesh.ByReference?
    fun polymesh_detail_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcPolyMesh: RcPolyMesh, rcCompactHeightfield: RcCompactHeightfield): RcPolyMeshDetail?
    fun navmesh_data_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcPolyMeshDetail: RcPolyMeshDetail, rcPolyMesh: RcPolyMesh, inputGeom: InputGeom, tx: Int, ty: Int, agentHeight: Float, agentRadius: Float, agentMaxClimb: Float): NavMeshDataResult.ByReference?
    fun rcConfig_calc_grid_size(config: RcConfig.ByReference, inputGeom: InputGeom)

    companion object RecastLibrary {
        fun load() = Native.loadLibrary("recastwrapper", io.improbable.ste.recast.RecastLibrary::class.java) as io.improbable.ste.recast.RecastLibrary
    }
}

