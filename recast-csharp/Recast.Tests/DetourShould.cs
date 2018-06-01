﻿using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Recast.Tests
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
                    Assert.GreaterOrEqual(smoothResult.pathCount, result.pathCount);
                }
                NRan++;
            }

            float avg = (float)stopwatch.ElapsedMilliseconds / NRan;
            Console.WriteLine($"\nAverage time (ms) for {NRan} run(s): {avg}");
            return avg;
        }

        private NavMesh CreateNavMesh(RecastContext ctx)
        {
            var mesh = ctx.LoadInputGeom("./Resources/Tile_+007_+006_L21.obj", true);
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

        private FindPathResult FindPathSafer(PolyPointResult pointA, PolyPointResult pointB,
                                             RecastContext ctx, NavMeshQuery navMeshQuery,
                                             Stopwatch stopwatch = null)
        {
            Assert.IsTrue(Success(pointA.status));
            Assert.IsTrue(Success(pointB.status));

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
                Assert.IsNotNull(result);
                Assert.AreEqual(Constants.MaxPathLength, result.path.Length);
                Assert.GreaterOrEqual(result.pathCount, 1);
                Assert.AreEqual(pointA.polyRef, result.path[0]);
                Assert.AreEqual(pointB.polyRef, result.path[result.pathCount - 1]);
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
            var navMesh = ctx.LoadTiledNavMeshBinFile("./Resources/Tile_+007_+006_L21.obj.tiled.bin");
            Assert.IsNotNull(navMesh);
            return navMesh;
        }
    }
}