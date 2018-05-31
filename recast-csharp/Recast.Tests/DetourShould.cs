using System;
using System.Reflection;
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
                var config = Constants.createDefaultConfig();
                var mesh = ctx.LoadInputGeom("./Resources/Tile_+007_+006_L21.obj", true);
                ctx.CalcGridSize(ref config, mesh);
                var chf = ctx.CreateCompactHeightfield(config, mesh);
                var polyMesh = ctx.CreatePolyMesh(config, chf);
                var polyMeshDetail = ctx.CreatePolyMeshDetail(config, polyMesh, chf);
                var navMeshData = ctx.CreateNavMeshData(config, polyMeshDetail, polyMesh, mesh, 0, 0,
                    Constants.agentHeight, Constants.agentRadius, Constants.agentMaxClimb);
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
                var config = Constants.createDefaultConfig();
                var mesh = ctx.LoadInputGeom("./Resources/Tile_+007_+006_L21.obj", true);
                ctx.CalcGridSize(ref config, mesh);
                var chf = ctx.CreateCompactHeightfield(config, mesh);
                var polyMesh = ctx.CreatePolyMesh(config, chf);
                var polyMeshDetail = ctx.CreatePolyMeshDetail(config, polyMesh, chf);
                var navMeshData = ctx.CreateNavMeshData(config, polyMeshDetail, polyMesh, mesh, 0, 0,
                    Constants.agentHeight, Constants.agentRadius, Constants.agentMaxClimb);
                var navMesh = ctx.CreateNavMesh(navMeshData);
                var navMeshQuery = ctx.CreateNavMeshQuery(navMesh);
                var pointA = ctx.FindRandomPoint(navMeshQuery);
                var pointB = ctx.FindRandomPoint(navMeshQuery);
                
                var result = ctx.FindPath(navMeshQuery, pointA, pointB);
                Assert.IsNotNull(result);
            }
        }
    }
}