import com.sun.jna.Native
import io.improbable.ste.recast.RcConfig
import io.improbable.ste.recast.RecastLibrary
import org.hamcrest.CoreMatchers.equalTo
import org.hamcrest.MatcherAssert.assertThat
import org.junit.Test
import java.io.File

class RecastShould {
    @Test
    fun create_a_navmesh() {
        val INSTANCE = Native.loadLibrary("recastwrapper", RecastLibrary::class.java) as RecastLibrary

        val url = this.javaClass.getResource("Tile_+007_+006_L21.obj")
        val ctx = INSTANCE.rcContext_create()
        val mesh = INSTANCE.load_mesh(ctx!!, File(url.toURI()).absolutePath)

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
        val polymesh = INSTANCE.polymesh_create(ctx!!, config, chf!!)!!
        val polyMeshDetail = INSTANCE.polymesh_detail_create(ctx!!, config, polymesh, chf)!!

        val navMeshDataResult = INSTANCE.navmesh_data_create(ctx!!, config, polyMeshDetail!!, polymesh, mesh, 0, 0, agentHeight.toFloat(), agentRadius.toFloat(), agentMaxClimb.toFloat())
        assertThat(navMeshDataResult!!.size, equalTo(2248))

        INSTANCE.rcContext_delete(ctx!!)
    }
}