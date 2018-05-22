
import com.sun.jna.*


typealias RcContext = Pointer
typealias InputGeom = Pointer
typealias RcCompactHeightfield = Pointer
typealias RcPolyMesh = Pointer
typealias RcPolyMeshDetail = Pointer

interface CLibrary : Library {
	fun rcContext_create(): RcContext?
	fun rcContext_delete(context: RcContext)
	fun load_mesh(rcContext: RcContext, path: String): InputGeom?
	fun compact_heightfield_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, inputGeom: InputGeom): RcCompactHeightfield?
	fun polymesh_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcCompactHeightfield: RcCompactHeightfield): RcPolyMesh?
	fun polymesh_detail_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcPolyMesh: RcPolyMesh, rcCompactHeightfield: RcCompactHeightfield): RcPolyMeshDetail?
	fun navmesh_data_create(rcContext: RcContext, rcConfig: RcConfig.ByReference, rcPolyMeshDetail: RcPolyMeshDetail, rcPolyMesh: RcPolyMesh, inputGeom: InputGeom, tx: Int, ty: Int, agentHeight: Float, agentRadius: Float, agentMaxClimb: Float): NavMeshDataResult.ByReference?
}

fun main(args: Array<String>) {
	val INSTANCE = Native.loadLibrary("recastwrapper", CLibrary::class.java) as CLibrary
	val ctx = INSTANCE.rcContext_create()
	val mesh = INSTANCE.load_mesh(ctx!!,"/Users/alexsparrow/Downloads/CatalinaWaterTower 100m OBJ/Data/Tile_+007_+006/Tile_+007_+006_L21.obj")

	val cellSize = 0.3
	val cellHeight = 0.2
	val agentHeight = 2.0
	val agentRadius = 0.6
	val agentMaxClimb = 20
	val agentMaxSlope = 45.0
	val regionMinSize = 8
	val regionMergeSize = 20
	val edgeMaxLen = 12.0
	val edgeMaxError = 1.3
	val vertsPerPoly = 6.0
	val detailSampleDist = 6.0
	val detailSampleMaxError = 1.0
	val tileSize = 32

	val walkableRadius = Math.ceil((agentRadius / cellSize)).toInt()
	val borderSize = walkableRadius + 3

	val config = RcConfig.ByReference().apply {
		width = tileSize + 2 * borderSize
		height = tileSize + 2 * borderSize
		this.tileSize = tileSize
		this.borderSize = borderSize
		cs = cellSize.toFloat()
		ch = cellHeight.toFloat()
		bmin = floatArrayOf(0.0f, 0.0f, 0.0f)
		bmax = floatArrayOf(0.0f, 0.0f, 0.0f)
		walkableSlopeAngle = agentMaxSlope.toFloat()
		walkableHeight = Math.ceil(agentHeight / cellHeight).toInt()
		walkableClimb = Math.ceil(agentMaxClimb / cellHeight).toInt()
		this.walkableRadius = walkableRadius
		maxEdgeLen = (edgeMaxLen / cellSize).toInt()
		maxSimplificationError = edgeMaxError.toFloat()
		minRegionArea = regionMinSize * regionMinSize
		mergeRegionArea = regionMergeSize * regionMergeSize
		maxVertsPerPoly = vertsPerPoly.toInt()
		this.detailSampleDist = if (detailSampleDist < 0.9) {
			0.0f
		} else {
			(cellSize * detailSampleDist).toFloat()
		}
		this.detailSampleMaxError = (cellHeight * detailSampleMaxError).toFloat()
	}

	val chf = INSTANCE.compact_heightfield_create(ctx!!, config, mesh!!)!!
	println(config.bmin[0])
	val polymesh = INSTANCE.polymesh_create(ctx!!, config, chf!!)!!
	val polyMeshDetail = INSTANCE.polymesh_detail_create(ctx!!, config, polymesh, chf)!!

	val navMeshDataResult = INSTANCE.navmesh_data_create(ctx!!, config, polyMeshDetail!!, polymesh, mesh, 0, 0, agentHeight.toFloat(), agentRadius.toFloat(), agentMaxClimb.toFloat())
	println("Got size: ${navMeshDataResult!!.size}")

	INSTANCE.rcContext_delete(ctx!!)
}