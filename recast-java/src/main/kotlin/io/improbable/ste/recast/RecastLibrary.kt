package io.improbable.ste.recast

import com.sun.jna.Library

interface RecastLibrary : Library {
    fun rcContext_create(): RcContext?
    fun rcContext_delete(context: RcContext)
    fun load_mesh(rcContext: RcContext, path: String): InputGeom?
    fun compact_heightfield_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, inputGeom: InputGeom): RcCompactHeightfield?
    fun polymesh_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcCompactHeightfield: RcCompactHeightfield): RcPolyMesh?
    fun polymesh_detail_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcPolyMesh: RcPolyMesh, rcCompactHeightfield: RcCompactHeightfield): RcPolyMeshDetail?
    fun navmesh_data_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcPolyMeshDetail: RcPolyMeshDetail, rcPolyMesh: RcPolyMesh, inputGeom: InputGeom, tx: Int, ty: Int, agentHeight: Float, agentRadius: Float, agentMaxClimb: Float): NavMeshDataResult.ByReference?
}