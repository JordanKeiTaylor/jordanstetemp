using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Recast.Tests
{
    public class DetourShould
    {
        [Test]
        public void find_random_point()
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
                var result = ctx.FindRandomPoint(navMeshQuery);
                Assert.IsNotNull(result);
                Assert.IsTrue((result.status & (1u << 30)) != 0);
                Assert.AreEqual(result.point.Length, 3);
            }
        }
        
        [Test]
        public void find_path()
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
                var pointA = ctx.FindRandomPoint(navMeshQuery);
                var pointB = ctx.FindRandomPoint(navMeshQuery);
                
                var result = ctx.FindPath(navMeshQuery, pointA, pointB);
                Assert.IsNotNull(result);
                Assert.IsTrue((result.status & (1u << 30)) != 0);
                Assert.AreEqual(Constants.MaxPathLength, result.path.Length);
                Assert.IsTrue(result.pathCount < Constants.MaxPathLength);
            }
        }
        
        [Test]
        public void be_fast()
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

                var N = 10000;

                var stopwatch = Stopwatch.StartNew();
                for (var i = 0; i < N; i++)
                {
                    stopwatch.Stop();
                    var pointA = ctx.FindRandomPoint(navMeshQuery);
                    var pointB = ctx.FindRandomPoint(navMeshQuery);

                    stopwatch.Start();
                    var result = ctx.FindPath(navMeshQuery, pointA, pointB);
                    Assert.IsNotNull(result);
                }
                
                Console.WriteLine($"Average time (ms): {(float)stopwatch.ElapsedMilliseconds / N}");
            }
        }
    }
}