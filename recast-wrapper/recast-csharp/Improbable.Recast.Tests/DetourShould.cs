using System;
using System.Diagnostics;
using Improbable.Recast.Types;
using NUnit.Framework;

namespace Improbable.Recast.Tests
{
    public class DetourShould
    {
        [Test]
        public void load_nav_mesh_tiled_bin_file()
        {
            using (var ctx = new RecastContext())
            {
                NavMesh navMesh = LoadNavMeshBinFile(ctx);
                Assert.IsNotNull(navMesh);
            }
        }

        [Test]
        public void find_random_point()
        {
            using (var ctx = new RecastContext())
            {
                var navMesh = CreateNavMesh(ctx);
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);
                var result = ctx.FindRandomPoint(navMeshQuery);
                Assert.IsTrue(Success(result.status));
                Assert.AreEqual(result.point.Length, 3);
            }
        }

        [Test]
        public void find_nearest_poly()
        {
            using (var ctx = new RecastContext()) {
                var navMesh = CreateNavMesh(ctx);
                
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);

                var point = new float[] { -575f, -69.1874f, 54f };
                var halfExtents = new float[] { 10.0f, 10.0f, 10.0f };
                var result = ctx.FindNearestPoly(navMeshQuery, point, halfExtents);
                Assert.AreEqual(result.polyRef, 281474976711211L);
            }
        }

        [Test]
        public void find_nearest_poly_fail()
        {
            using (var ctx = new RecastContext())
            {
                var navMesh = CreateNavMesh(ctx);
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);

                var point = new float[] { -5750.0f, -6900.1874f, 5400.0f };
                var halfExtents = new float[] { 10.0f, 10.0f, 10.0f };
                var result = ctx.FindNearestPoly(navMeshQuery, point, halfExtents);

                Assert.IsFalse(Success(result.status));
                Assert.AreEqual(result.polyRef, 0);
                Assert.AreEqual(result.point[0], 0);
                Assert.AreEqual(result.point[1], 0);
                Assert.AreEqual(result.point[2], 0);
            }
        }
        
        [Test]
        public void find_path()
        {
            using (var ctx = new RecastContext())
            {
                var navMesh = CreateNavMesh(ctx);
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);
                var pointA = FindRandomPointSafer(ctx, navMeshQuery);
                var pointB = FindRandomPointSafer(ctx, navMeshQuery);
                var result = FindPathSafer(pointA, pointB, ctx, navMeshQuery);
                Assert.IsTrue(Success(result.status));
            }
        }
        
        [Test]
        public void find_smooth_path()
        {
            using (var ctx = new RecastContext())
            {
                var navMesh = CreateNavMesh(ctx);
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);
                var pointA = FindRandomPointSafer(ctx, navMeshQuery);
                var pointB = FindRandomPointSafer(ctx, navMeshQuery);
                
                var result = FindPathSafer(pointA, pointB, ctx, navMeshQuery);
                Assert.IsTrue(Success(result.status));

                var smoothResult = ctx.FindSmoothPath(navMeshQuery, navMesh, result, pointA, pointB);
                Assert.IsNotNull(smoothResult);
                Assert.GreaterOrEqual(smoothResult.pathCount, result.pathCount);
                Assert.AreEqual(3 * Constants.MaxSmoothPathLength, smoothResult.path.Length);
                Assert.IsTrue(smoothResult.pathCount < Constants.MaxSmoothPathLength * 3);
                Assert.AreEqual(smoothResult.path[0], pointA.point[0], 0.00001);
                Assert.AreEqual(smoothResult.path[1], pointA.point[1], 0.00001);
                Assert.AreEqual(smoothResult.path[2], pointA.point[2], 0.00001);
            }
        }

        [Test]
        public void be_fast_from_obj()
        {
            using (var ctx = new RecastContext())
            {
                var navMesh = CreateNavMesh(ctx);
                var avg = be_fast_work(ctx, navMesh);
                Assert.LessOrEqual(avg, 0.1);
            }
        }

        [Test]
        public void be_fast_from_bin()
        {
            using (var ctx = new RecastContext())
            {
                var navMesh = LoadNavMeshBinFile(ctx);
                var avg = be_fast_work(ctx, navMesh);
                Assert.LessOrEqual(avg, 0.1);
            }
        }

        private float be_fast_work(RecastContext ctx, NavMesh navMesh)
        {
            var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);

            const int N = 10000;
            var NRan = 0;
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();
            for (var i = 0; i < N; i++)
            {
                var pointA = FindRandomPointSafer(ctx, navMeshQuery);
                var pointB = FindRandomPointSafer(ctx, navMeshQuery);
                var result = FindPathSafer(pointA, pointB, ctx, navMeshQuery, stopwatch);
                if (Success(result.status))
                {
                    var smoothResult = ctx.FindSmoothPath(navMeshQuery, navMesh, result, pointA, pointB);
                    Assert.GreaterOrEqual(smoothResult.pathCount, result.pathCount, $"smoothResult.pathCount [{ smoothResult.pathCount}] < result.pathCount [{result.pathCount}]");
                }
                NRan++;
            }

            float avg = (float)stopwatch.ElapsedMilliseconds / NRan;
            Console.WriteLine($"\nAverage time (ms) for {NRan} run(s): {avg}");
            return avg;
        }
        
        private NavMesh CreateNavMesh(RecastContext ctx)
        {
            var mesh = ctx.LoadInputGeom(TestUtils.ResolveResource("Resources/Tile_+007_+006_L21.obj"), true);
            Assert.IsNotNull(mesh);
            ctx.CalcGridSize(ref _config, mesh);
            var chf = ctx.CreateCompactHeightfield(_config, mesh);
            var polyMesh = ctx.CreatePolyMesh(_config, chf);
            var polyMeshDetail = ctx.CreatePolyMeshDetail(_config, polyMesh, chf);
            var navMeshData = ctx.CreateNavMeshData(_config, polyMeshDetail, polyMesh, mesh, 0, 0,
                                                    BuildSettings.agentHeight, BuildSettings.agentRadius,
                                                    BuildSettings.agentMaxClimb);
            return ctx.CreateNavMesh(navMeshData);
        }
        
        private RcConfig _config = BuildSettings.createDefault();

        private bool Success(uint status)
        {
            return (status & (1u << 30)) != 0;
        }

        private bool PartialResult(uint status)
        {
            return (status & (1u << 6)) != 0;
        }

        private FindPathResult FindPathSafer(PolyPointResult pointA, PolyPointResult pointB,
                                             RecastContext ctx, NavMeshQuery navMeshQuery,
                                             Stopwatch stopwatch = null)
        {
            Assert.IsTrue(Success(pointA.status), "Point A not a success.");
            Assert.IsTrue(Success(pointB.status), "Point A not a success.");

            if (null != stopwatch)
            {
                stopwatch.Start();
            }
            int retries = 10;
            FindPathResult result;
            do
            {
                result = ctx.FindPath(navMeshQuery, pointA, pointB);
                retries--;
            } while ((!Success(result.status)) && (0 < retries));
            if (null != stopwatch)
            {
                stopwatch.Stop();
            }

            if (Success(result.status))
            {
                Assert.IsNotNull(result, "result NULL");
                Assert.AreEqual(Constants.MaxPathLength, result.path.Length, $"Constants.MaxPathLength != result.path.Length [{Constants.MaxPathLength} != {result.path.Length}]");
                Assert.GreaterOrEqual(result.pathCount, 1, $"result.pathCount [{result.pathCount}] < 1");
                Assert.AreEqual(pointA.polyRef, result.path[0], $"point A polyref does not match [{pointA.polyRef} != {result.path[0]}]");
                if (!PartialResult(result.status))
                {
                    Assert.AreEqual(pointB.polyRef, result.path[result.pathCount - 1],
                                    $"point B polyref does not match [{pointA.polyRef} != {result.path[result.pathCount - 1]}] (status={result.status} [{Convert.ToString(result.status, 2)}])");
                }

            }

            return result;
        }

        private PolyPointResult FindRandomPointSafer(RecastContext ctx, NavMeshQuery navMeshQuery)
        {
            int retries = 10;
            PolyPointResult result;
            do
            {
                result = ctx.FindRandomPoint(navMeshQuery);
                retries--;
            } while ((!Success(result.status)) && (0 < retries));
            return result;
        }

        private NavMesh LoadNavMeshBinFile(RecastContext ctx)
        {
            var navMesh =
                ctx.LoadTiledNavMeshBinFile(TestUtils.ResolveResource("./Resources/Tile_+007_+006_L21.obj.tiled.bin64"));
            Assert.IsNotNull(navMesh);
            return navMesh;
        }
    }
}