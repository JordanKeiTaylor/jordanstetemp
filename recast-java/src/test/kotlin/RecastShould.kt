import com.sun.jna.Memory
import java.awt.image.BufferedImage
import java.io.File
import javax.imageio.ImageIO
import com.natpryce.hamkrest.equalTo
import com.natpryce.hamkrest.greaterThanOrEqualTo
import com.natpryce.hamkrest.lessThanOrEqualTo
import com.natpryce.hamkrest.assertion.assertThat
import com.natpryce.hamkrest.present
import org.junit.Test

import io.improbable.ste.recast.*

class RecastShould {
    @Test
    fun create_a_context() {
        val ctx = recast.rcContext_create()
        assertThat(ctx, present())
        recast.rcContext_delete(ctx!!)
    }

    @Test
    fun create_polymesh() {
        val ctx = recast.rcContext_create()
        val config = createDefaultConfig()

        val mesh = getMesh(ctx!!)
        recast.rcConfig_calc_grid_size(config, mesh!!)
        val chf = recast.compact_heightfield_create(ctx!!, config, mesh!!)!!
        val polymesh = recast.polymesh_create(ctx!!, config, chf!!)!!
        assertThat(polymesh.ch, equalTo(Constants.cellHeight.toFloat()));
    }

    @Test
    fun create_a_navmesh() {
        val ctx = recast.rcContext_create()
        val config = createDefaultConfig()
        val mesh = getMesh(ctx!!)

        assertThat(mesh, present())
        recast.rcConfig_calc_grid_size(config, mesh!!)
        val navMeshDataResult = createNavMeshData(ctx, config, mesh)
        assertThat(navMeshDataResult!!.size, equalTo(105712))

        val navMesh = recast.navmesh_create(ctx, navMeshDataResult)
        assertThat(navMesh, present())

        val navMeshQuery = recast.navmesh_query_create(navMesh)
        assertThat(navMeshQuery, present())

        recast.navmesh_delete(navMesh)
        recast.rcContext_delete(ctx!!)
    }

    @Test
    fun do_some_navmesh_queries() {
        val ctx = recast.rcContext_create()
        val config = createDefaultConfig()
        val mesh = getMesh(ctx!!)
        recast.rcConfig_calc_grid_size(config, mesh!!)
        val navMeshDataResult = createNavMeshData(ctx, config, mesh)
        val navMesh = recast.navmesh_create(ctx, navMeshDataResult!!)
        val navMeshQuery = recast.navmesh_query_create(navMesh)
        val point = Memory(3 * 4)
        point.setFloat(0, -575f)
        point.setFloat(4, -69.1874f)
        point.setFloat(8, 54f)

        val halfExtents = Memory(3 * 4)
        halfExtents.setFloat(0, 100.0f)
        halfExtents.setFloat(4, 100.0f);
        halfExtents.setFloat(8, 100.0f);

        val result = recast.navmesh_query_find_nearest_poly(navMeshQuery, point, halfExtents)
        assertThat(result.polyRef, equalTo(1579))

        val filter = recast.dtQueryFilter_create()

        val pathResult = recast.navmesh_query_find_path(navMeshQuery, result.polyRef, result.polyRef, point, point, filter);
        assertThat(dtFailed(pathResult.status), equalTo(false))
        assertThat(pathResult.pathCount, equalTo(1))
        assertThat(pathResult.path[0], equalTo(result.polyRef))

        val smoothPathResult = recast.navmesh_query_get_smooth_path(point, result.polyRef, point, pathResult, filter, navMesh, navMeshQuery)
        assertThat(smoothPathResult, present())
        assertThat(smoothPathResult.path[0], equalTo(point.getFloat(0)))
        assertThat(smoothPathResult.path[1], equalTo(point.getFloat(4)))
        assertThat(smoothPathResult.path[2], equalTo(point.getFloat(8)))
        assertThat(smoothPathResult.pathCount, equalTo(1))

        val randomPoint = recast.navmesh_query_find_random_point(navMeshQuery)
        assertThat(dtFailed(randomPoint.status), equalTo(false))

        recast.dtQueryFilter_delete(filter)
        recast.navmesh_delete(navMesh)
        recast.rcContext_delete(ctx!!)
    }

    @Test
    fun load_tiled_mesh() {
        val navMesh = recast.navmesh_load_tiled_bin(navMeshTiledBinPath())
        assertThat(navMesh, present())

        val navMeshQuery = recast.navmesh_query_create(navMesh)
        assertThat(navMeshQuery, present())
        for (i in 1..1000) {
            val randomPoint = recast.navmesh_query_find_random_point(navMeshQuery)
            if (dtSuccess(randomPoint.status)) {
                assertThat(randomPoint, present())
                println(StringBuilder()
                    .append(i.toString())
                    .append(" {")
                    .append(dtSuccess(randomPoint.status))
                    .append("} [")
                    .append(randomPoint.point[0])
                    .append(", ")
                    .append(randomPoint.point[1])
                    .append(", ")
                    .append(randomPoint.point[2])
                    .append("] (")
                    .append(randomPoint.polyRef)
                    .append(")")
                    .toString())
                assertWithinLimits(randomPoint)
            } else {
                println(i.toString() + "Failed status found: " + dtStatusDetailString(randomPoint.status))
            }
        }

        recast.navmesh_delete(navMesh)
    }

    @Test
    fun draw_a_polymesh() {
        val ctx = recast.rcContext_create()
        val config = createDefaultConfig()
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

    private fun createNavMeshData(ctx: RcContext, config: RcConfig.ByReference, mesh: InputGeom): NavMeshDataResult.ByReference? {
        val chf = recast.compact_heightfield_create(ctx, config, mesh)!!
        val polymesh = recast.polymesh_create(ctx, config, chf)!!
        val polyMeshDetail = recast.polymesh_detail_create(ctx, config, polymesh, chf)!!
        return recast.navmesh_data_create(ctx, config, polyMeshDetail, polymesh, mesh, 0, 0, Constants.agentHeight.toFloat(), Constants.agentRadius.toFloat(), Constants.agentMaxClimb.toFloat())
    }

    private fun getMesh(ctx: RcContext) = recast.InputGeom_load(ctx, terrainTilePath(), true)

    private val recast = RecastLibrary.load()

    private fun terrainTilePath() = File(this.javaClass.getResource("Tile_+007_+006_L21.obj").toURI()).absolutePath

    private fun navMeshTiledBinPath() = File(this.javaClass.getResource("Tile_+007_+006_L21.obj.tiled.bin").toURI()).absolutePath

    private fun assertWithinLimits(point: PolyPointResult) {
        assertThat(point.point[0].toDouble(), greaterThanOrEqualTo(-431.48))
        assertThat(point.point[0].toDouble(), lessThanOrEqualTo(-330.52))
        assertThat(point.point[1].toDouble(), greaterThanOrEqualTo(98.50))
        assertThat(point.point[1].toDouble(), lessThanOrEqualTo(119.83))
        assertThat(point.point[2].toDouble(), greaterThanOrEqualTo(-289.48))
        assertThat(point.point[2].toDouble(), lessThanOrEqualTo(-188.52))
    }
}