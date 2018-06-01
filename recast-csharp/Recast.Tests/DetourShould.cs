using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Recast.Tests
{
    public class DetourShould
    {
        private bool success(uint status)
        {
            return (status & (1u << 30)) != 0;
        }

        [Test]
        public void find_random_point()
        {
            using (var ctx = new RecastContext())
            {
                var navMesh = CreateNavMesh(ctx);
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);
                var result = ctx.FindRandomPoint(navMeshQuery);
                Assert.IsNotNull(result);
                Assert.IsTrue(success(result.status));
                Assert.AreEqual(result.point.Length, 3);
            }
        }
        
        [Test]
        public void find_path()
        {
            using (var ctx = new RecastContext())
            {
                var navMesh = CreateNavMesh(ctx);
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);
                var pointA = ctx.FindRandomPoint(navMeshQuery);
                var pointB = ctx.FindRandomPoint(navMeshQuery);
                
                var result = ctx.FindPath(navMeshQuery, pointA, pointB);
                Assert.IsNotNull(result);
                Assert.IsTrue(success(result.status));
                Assert.AreEqual(Constants.MaxPathLength, result.path.Length);
                Assert.IsTrue(result.pathCount < Constants.MaxPathLength);
            }
        }
        
        [Test]
        public void find_smooth_path()
        {
            using (var ctx = new RecastContext())
            {
                var navMesh = CreateNavMesh(ctx);
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);
                var pointA = ctx.FindRandomPoint(navMeshQuery);
                var pointB = ctx.FindRandomPoint(navMeshQuery);
                
                var result = ctx.FindPath(navMeshQuery, pointA, pointB);
                Assert.IsTrue(success(result.status));
                var smoothResult = ctx.FindSmoothPath(navMeshQuery, navMesh, result, pointA, pointB);
                Assert.IsNotNull(smoothResult);
                Assert.AreEqual(3 * Constants.MaxSmoothPathLength, smoothResult.path.Length);
                Assert.IsTrue(smoothResult.pathCount < Constants.MaxSmoothPathLength * 3);
            }
        }

        [Test]
        public void be_fast()
        {
            using (var ctx = new RecastContext())
            {
                var navMesh = CreateNavMesh(ctx);
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);

                const int N = 10000;
                var NRan = 0;

                var stopwatch = Stopwatch.StartNew();
                for (var i = 0; i < N; i++)
                {
                    stopwatch.Stop();
                    var pointA = ctx.FindRandomPoint(navMeshQuery);
                    var pointB = ctx.FindRandomPoint(navMeshQuery);
                    if (success(pointA.status) && success(pointB.status))
                    {
                        stopwatch.Start();
                        var result = ctx.FindPath(navMeshQuery, pointA, pointB);
                        NRan++;
                        if (success(result.status))
                        {
                            Assert.IsNotNull(result);
                            Assert.GreaterOrEqual(result.pathCount, 1);
                        }
                    }
                }
                
                Console.WriteLine($"\nAverage time (ms) for {NRan} run(s): {(float)stopwatch.ElapsedMilliseconds / NRan}");
            }
        }

        private NavMesh CreateNavMesh(RecastContext ctx)
        {
            var mesh = ctx.LoadInputGeom("./Resources/Tile_+007_+006_L21.obj", true);
            ctx.CalcGridSize(ref _config, mesh);
            var chf = ctx.CreateCompactHeightfield(_config, mesh);
            var polyMesh = ctx.CreatePolyMesh(_config, chf);
            var polyMeshDetail = ctx.CreatePolyMeshDetail(_config, polyMesh, chf);
            var navMeshData = ctx.CreateNavMeshData(_config, polyMeshDetail, polyMesh, mesh, 0, 0,
                BuildSettings.agentHeight, BuildSettings.agentRadius, BuildSettings.agentMaxClimb);
            return ctx.CreateNavMesh(navMeshData);
        }
        
        private RcConfig _config = BuildSettings.createDefault();
    }
}