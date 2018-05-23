import io.improbable.ste.recast.RcConfig
import io.improbable.ste.recast.RcContext
import io.improbable.ste.recast.RecastLibrary
import io.improbable.ste.recast.drawPolyMesh
import org.hamcrest.CoreMatchers.equalTo
import org.hamcrest.CoreMatchers.notNullValue
import org.hamcrest.MatcherAssert.assertThat
import org.junit.Test
import java.awt.image.BufferedImage
import java.io.File
import javax.imageio.ImageIO

class RecastShould {
    @Test
    fun create_a_context() {
        val ctx = recast.rcContext_create()
        assertThat(ctx, notNullValue())
        recast.rcContext_delete(ctx!!)
    }

    @Test
    fun create_polymesh() {
        val ctx = recast.rcContext_create()
        val config = createConfig()

        val mesh = getMesh(ctx!!)
        recast.rcConfig_calc_grid_size(config, mesh!!)
        val chf = recast.compact_heightfield_create(ctx!!, config, mesh!!)!!
        val polymesh = recast.polymesh_create(ctx!!, config, chf!!)!!
        assertThat(polymesh.ch, equalTo(Constants.cellHeight.toFloat()));
    }

    @Test
    fun create_a_navmesh() {
        val ctx = recast.rcContext_create()
        val config = createConfig()
        val mesh = getMesh(ctx!!)

        assertThat(mesh, notNullValue())
        recast.rcConfig_calc_grid_size(config, mesh!!)

        val chf = recast.compact_heightfield_create(ctx!!, config, mesh!!)!!
        val polymesh = recast.polymesh_create(ctx!!, config, chf!!)!!
        val polyMeshDetail = recast.polymesh_detail_create(ctx!!, config, polymesh, chf)!!
        val navMeshDataResult = recast.navmesh_data_create(ctx!!, config, polyMeshDetail!!, polymesh, mesh, 0, 0, Constants.agentHeight.toFloat(), Constants.agentRadius.toFloat(), Constants.agentMaxClimb.toFloat())
        assertThat(navMeshDataResult!!.size, equalTo(105712))

        val navmesh = recast.navmesh_create(ctx!!, navMeshDataResult)
        assertThat(navmesh, notNullValue())

        val navMeshQuery = recast.navmesh_query_create(navmesh)
        assertThat(navMeshQuery, notNullValue())

        recast.rcContext_delete(ctx!!)
    }

    @Test
    fun draw_a_polymesh() {
        val ctx = recast.rcContext_create()
        val config = createConfig()
        val mesh = getMesh(ctx!!)
        recast.rcConfig_calc_grid_size(config, mesh!!)

        val chf = recast.compact_heightfield_create(ctx!!, config, mesh!!)!!
        val polymesh = recast.polymesh_create(ctx!!, config, chf!!)!!

        val width = 1280
        val height = 960
        val bImg = BufferedImage(width, height, BufferedImage.TYPE_INT_RGB)
        val cg = bImg.createGraphics()

        drawPolyMesh(polymesh, cg, width, height)
        val outputfile = File("image.png")
        ImageIO.write(bImg, "png", outputfile)
    }

    private fun getMesh(ctx: RcContext) = recast.load_mesh(ctx, terrainTilePath(), true)

    private val recast = RecastLibrary.load()

    private fun terrainTilePath() = File(this.javaClass.getResource("Tile_+007_+006_L21.obj").toURI()).absolutePath

    private fun createConfig() = RcConfig.ByReference().apply {
//        width = Constants.tileSize + 2 * Constants.borderSize
//        height = Constants.tileSize + 2 * Constants.borderSize
//        tileSize = Constants.tileSize
//        borderSize = Constants.borderSize
        cs = Constants.cellSize.toFloat()
        ch = Constants.cellHeight.toFloat()
        walkableSlopeAngle = Constants.agentMaxSlope.toFloat()
        walkableHeight = Math.ceil(Constants.agentHeight / Constants.cellHeight).toInt()
        walkableClimb = Math.ceil(Constants.agentMaxClimb / Constants.cellHeight).toInt()
        walkableRadius = Constants.walkableRadius
        maxEdgeLen = (Constants.edgeMaxLen / Constants.cellSize).toInt()
        maxSimplificationError = Constants.edgeMaxError.toFloat()
        minRegionArea = Constants.regionMinSize * Constants.regionMinSize
        mergeRegionArea = Constants.regionMergeSize * Constants.regionMergeSize
        maxVertsPerPoly = Constants.vertsPerPoly.toInt()
        detailSampleDist = if (Constants.detailSampleDist < 0.9) {
            0.0f
        } else {
            (Constants.cellSize * Constants.detailSampleDist).toFloat()
        }
        detailSampleMaxError = (Constants.cellHeight * Constants.detailSampleMaxError).toFloat()
    }

    object Constants {
        const val cellSize = 0.3
        const val cellHeight = 0.2
        const val agentHeight = 2.0
        const val agentRadius = 0.6
        const val agentMaxClimb = 0.9
        const val agentMaxSlope = 45.0
        const val regionMinSize = 8
        const val regionMergeSize = 20
        const val edgeMaxLen = 12.0
        const val edgeMaxError = 1.3
        const val vertsPerPoly = 6.0
        const val detailSampleDist = 6.0
        const val detailSampleMaxError = 1.0
        const val tileSize = 32

        val walkableRadius = Math.ceil((agentRadius / cellSize)).toInt()
        val borderSize = walkableRadius + 3
    }
}