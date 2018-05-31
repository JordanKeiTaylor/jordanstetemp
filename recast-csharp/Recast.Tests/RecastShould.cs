using NUnit.Framework;

namespace Recast.Tests
{
    using Recast;

    class RecastShould
    {
        [Test]
        public void create_a_context()
        {
            using (var ctx = new RecastContext())
            {
                Assert.IsNotNull(ctx);
            }
        }

        [Test]
        public void load_a_mesh()
        {
            using (var ctx = new RecastContext())
            {
                var mesh = ctx.LoadInputGeom("Resources/Tile_+007_+006_L21.obj", true);
                Assert.IsNotNull(mesh);
            }
        }

        [Test]
        public void create_compact_heightfield()
        {
            using (var ctx = new RecastContext())
            {
                var config = Config.createDefaultConfig();
                var mesh = ctx.LoadInputGeom("./Resources/Tile_+007_+006_L21.obj", true);
                ctx.CalcGridSize(ref config, mesh);
                var chf = ctx.CreateCompactHeightfield(config, mesh);
                Assert.IsNotNull(chf);
            }
        }

        [Test]
        public void create_polymesh()
        {
            using (var ctx = new RecastContext())
            {
                var config = Config.createDefaultConfig();
                var mesh = ctx.LoadInputGeom("./Resources/Tile_+007_+006_L21.obj", true);
                ctx.CalcGridSize(ref config, mesh);
                var chf = ctx.CreateCompactHeightfield(config, mesh);
                var polyMesh = ctx.CreatePolyMesh(config, chf);
                Assert.IsNotNull(polyMesh);
            }
        }
        
        [Test]
        public void create_polymesh_detail()
        {
            using (var ctx = new RecastContext())
            {
                var config = Config.createDefaultConfig();
                var mesh = ctx.LoadInputGeom("./Resources/Tile_+007_+006_L21.obj", true);
                ctx.CalcGridSize(ref config, mesh);
                var chf = ctx.CreateCompactHeightfield(config, mesh);
                var polyMesh = ctx.CreatePolyMesh(config, chf);
                var polyMeshDetail = ctx.CreatePolyMeshDetail(config, polyMesh, chf);
                Assert.IsNotNull(polyMeshDetail);
            }
        }
        
        [Test]
        public void create_navmesh_data()
        {
            using (var ctx = new RecastContext())
            {
                var config = Config.createDefaultConfig();
                var mesh = ctx.LoadInputGeom("./Resources/Tile_+007_+006_L21.obj", true);
                ctx.CalcGridSize(ref config, mesh);
                var chf = ctx.CreateCompactHeightfield(config, mesh);
                var polyMesh = ctx.CreatePolyMesh(config, chf);
                var polyMeshDetail = ctx.CreatePolyMeshDetail(config, polyMesh, chf);
                var navMeshData = ctx.CreateNavMeshData(config, polyMeshDetail, polyMesh, mesh, 0, 0,
                    Config.agentHeight, Config.agentRadius, Config.agentMaxClimb);
                // TODO: This is different to Java!!
                Assert.AreEqual(navMeshData.size, 105692);

                var bytes = navMeshData.GetData();
                Assert.AreEqual(bytes.Length, navMeshData.size);
            }
        }
        
        [Test]
        public void create_navmesh()
        {
            using (var ctx = new RecastContext())
            {
                var config = Config.createDefaultConfig();
                var mesh = ctx.LoadInputGeom("./Resources/Tile_+007_+006_L21.obj", true);
                ctx.CalcGridSize(ref config, mesh);
                var chf = ctx.CreateCompactHeightfield(config, mesh);
                var polyMesh = ctx.CreatePolyMesh(config, chf);
                var polyMeshDetail = ctx.CreatePolyMeshDetail(config, polyMesh, chf);
                var navMeshData = ctx.CreateNavMeshData(config, polyMeshDetail, polyMesh, mesh, 0, 0,
                    Config.agentHeight, Config.agentRadius, Config.agentMaxClimb);
                var navMesh = ctx.CreateNavMesh(navMeshData);
                Assert.IsNotNull(navMesh);
            }
        }
        
        [Test]
        public void create_navmesh_query()
        {
            using (var ctx = new RecastContext())
            {
                var config = Config.createDefaultConfig();
                var mesh = ctx.LoadInputGeom("./Resources/Tile_+007_+006_L21.obj", true);
                ctx.CalcGridSize(ref config, mesh);
                var chf = ctx.CreateCompactHeightfield(config, mesh);
                var polyMesh = ctx.CreatePolyMesh(config, chf);
                var polyMeshDetail = ctx.CreatePolyMeshDetail(config, polyMesh, chf);
                var navMeshData = ctx.CreateNavMeshData(config, polyMeshDetail, polyMesh, mesh, 0, 0,
                    Config.agentHeight, Config.agentRadius, Config.agentMaxClimb);
                var navMesh = ctx.CreateNavMesh(navMeshData);
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);
                Assert.IsNotNull(navMeshQuery);
            }
        }
    }
}