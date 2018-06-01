import java.io.File
import kotlin.system.measureTimeMillis
import org.hamcrest.CoreMatchers.notNullValue
import org.hamcrest.MatcherAssert.assertThat
import org.junit.Test

import io.improbable.ste.recast.*
import org.junit.Ignore

class Benchmark {
    @Test
    fun ddos_mesh() {
        val ctx = recast.rcContext_create()
        val config = createDefaultConfig()
        val mesh = getMesh(ctx!!)
        recast.rcConfig_calc_grid_size(config, mesh!!)

        val navMeshDataResult = createNavMeshData(ctx!!, config, mesh!!)
        val navMesh = recast.navmesh_create(ctx!!, navMeshDataResult!!)
        val navMeshQuery = recast.navmesh_query_create(navMesh)
        assertThat(navMeshQuery, notNullValue())

        val filter = recast.dtQueryFilter_create()
        
        val times = (0..10000).map {
            measureTimeMillis {
                val randomPointA = recast.navmesh_query_find_random_point(navMeshQuery)
                val randomPointB = recast.navmesh_query_find_random_point(navMeshQuery)
                val pointA = Common.toFloat3(randomPointA)
                val pointB = Common.toFloat3(randomPointB)

                val pathResult = recast.navmesh_query_find_path(navMeshQuery, randomPointA.polyRef, randomPointB.polyRef, pointA, pointB, filter)
                assertThat(pathResult, notNullValue())

                val smoothPathResult = recast.navmesh_query_get_smooth_path(pointA, randomPointA.polyRef, pointB, pathResult, filter, navMesh, navMeshQuery)
                assertThat(smoothPathResult, notNullValue())
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

    private fun getMesh(ctx: RcContext) = recast.InputGeom_load(ctx, terrainTilePath(), true)

    private val recast = RecastLibrary.load()

    private fun terrainTilePath() = File(this.javaClass.getResource("Tile_+007_+006_L21.obj").toURI()).absolutePath
}
