import com.sun.jna.Memory
import io.improbable.ste.recast.*
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

        assertThat(mesh, notNullValue())
        recast.rcConfig_calc_grid_size(config, mesh!!)
        val navMeshDataResult = createNavMeshData(ctx, config, mesh)
        assertThat(navMeshDataResult!!.size, equalTo(105712))

        val navmesh = recast.navmesh_create(ctx, navMeshDataResult)
        assertThat(navmesh, notNullValue())

        val navMeshQuery = recast.navmesh_query_create(navmesh)
        assertThat(navMeshQuery, notNullValue())
        recast.rcContext_delete(ctx!!)
    }

    @Test
    fun do_some_navmesh_queries() {
        val ctx = recast.rcContext_create()
        val config = createDefaultConfig()
        val mesh = getMesh(ctx!!)
        recast.rcConfig_calc_grid_size(config, mesh!!)
        val navMeshDataResult = createNavMeshData(ctx, config, mesh)
        val navmesh = recast.navmesh_create(ctx, navMeshDataResult!!)
        val navMeshQuery = recast.navmesh_query_create(navmesh)
        val point = Memory(3 * 4)
        point.setFloat(0, -575f)
        point.setFloat(4, -70f)
        point.setFloat(8, 54f)

        val halfExtents = Memory(3 * 4)
        halfExtents.setFloat(0, 100.0f)
        halfExtents.setFloat(4, 100.0f);
        halfExtents.setFloat(8, 100.0f);

        val result = recast.navmesh_query_find_nearest_poly(navMeshQuery, point, halfExtents)
        assertThat(result.polyRef, equalTo(1579))

        val filter = recast.dtQueryFilter_create()
        val pathResult = recast.navmesh_query_find_path(navMeshQuery, result.polyRef, result.polyRef, point, point, filter);
        recast.dtQueryFilter_delete(filter)
        assertThat(dtFailed(pathResult.status), equalTo(false))
        assertThat(pathResult.pathCount, equalTo(1))
        assertThat(pathResult.path[0], equalTo(result.polyRef))
        recast.rcContext_delete(ctx!!)

        val randomPoint = recast.navmesh_query_find_random_point(navMeshQuery)
        assertThat(dtFailed(randomPoint.status), equalTo(false))
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

    private fun getMesh(ctx: RcContext) = recast.load_mesh(ctx, terrainTilePath(), true)

    private val recast = RecastLibrary.load()

    private fun terrainTilePath() = File(this.javaClass.getResource("Tile_+007_+006_L21.obj").toURI()).absolutePath
}