import com.sun.jna.Memory
import io.improbable.ste.recast.*
import org.hamcrest.CoreMatchers.notNullValue
import org.hamcrest.MatcherAssert.assertThat
import org.junit.Ignore
import org.junit.Test
import java.io.File
import kotlin.system.measureTimeMillis

class Benchmark {
    @Test
    fun ddos_mesh() {
        val recast = io.improbable.ste.recast.RecastLibrary.load()
        val ctx = recast.rcContext_create()
        val config = createDefaultConfig()
        val mesh = getMesh(ctx!!)
        recast.rcConfig_calc_grid_size(config, mesh!!)

        val navMeshDataResult = createNavMeshData(ctx!!, config, mesh!!)
        val navmesh = recast.navmesh_create(ctx!!, navMeshDataResult!!)
        val navMeshQuery = recast.navmesh_query_create(navmesh)
        val filter = recast.dtQueryFilter_create()
        
        val times = (0..10000).map {
            measureTimeMillis {
                val randomPointA = recast.navmesh_query_find_random_point(navMeshQuery)
                val randomPointB = recast.navmesh_query_find_random_point(navMeshQuery)

                val pointA = Memory((3 * 4).toLong())
                pointA.setFloat(0, randomPointA.point[0])
                pointA.setFloat(4, randomPointA.point[1])
                pointA.setFloat(8, randomPointA.point[2])

                val pointB = Memory((3 * 4).toLong())
                pointB.setFloat(0, randomPointB.point[0])
                pointB.setFloat(4, randomPointB.point[1])
                pointB.setFloat(8, randomPointB.point[2])

                val pathResult = recast.navmesh_query_find_path(navMeshQuery, randomPointA.polyRef, randomPointA.polyRef, pointA, pointB, filter)
                assertThat(pathResult, notNullValue())
            }
        }
        println("Average time: ${times.average()}")

        recast.dtQueryFilter_delete(filter)
        recast.rcContext_delete(ctx)
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