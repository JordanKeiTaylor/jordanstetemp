using Improbable;
using Improbable.Navigation;
using Improbable.Navigation.Api;
using NUnit.Framework;

namespace Tests.Navigation
{
    [TestFixture]
    public class DefaultMeshNavigatorTest
    {   
        private DefaultMeshNavigator _navigator;
        
        [SetUp]
        public void Setup()
        {
            _navigator = new DefaultMeshNavigator(Resources.L19ObjBin64);
        }

        [Test]
        public void PathFromAToB_Success()
        {   
            var a = new Coordinates(-534.29, 93.625, -744.29);
            var b = new Coordinates(-283.60, 100.49, -174.30);
            var aPath = new PathNode {Coords = a};
            var bPath = new PathNode {Coords = b};

            var task = _navigator.GetMeshPath(aPath, bPath);
            task.Wait();
            var result = task.Result;
            
            Assert.AreEqual(234, result.Path.Count);
            Assert.AreEqual(PathStatus.Success, result.Status);
        }
        
        [Test]
        public void PathFromAToB_AOutsideOfBounds_NotFound()
        {
            var a = new Coordinates(10000, 10000, 10000);
            var b = new Coordinates(-283.60, 100.49, -174.30);
            var aPath = new PathNode {Coords = a};
            var bPath = new PathNode {Coords = b};

            var task = _navigator.GetMeshPath(aPath, bPath);
            task.Wait();
            var result = task.Result;
            
            Assert.AreEqual(0, result.Path.Count);
            Assert.AreEqual(PathStatus.Error, result.Status);
        }
        
        [Test]
        public void PathFromAToB_BOutsideOfBounds_NotFound()
        {
            var a = new Coordinates(-534.29, 93.625, -744.29);
            var b = new Coordinates(10000, 10000, 10000);
            var aPath = new PathNode {Coords = a};
            var bPath = new PathNode {Coords = b};

            var task = _navigator.GetMeshPath(aPath, bPath);
            task.Wait();
            var result = task.Result;
            
            Assert.AreEqual(0, result.Path.Count);
            Assert.AreEqual(PathStatus.Error, result.Status);
        }
        
        [Test]
        public void GetRandomPoints()
        {
            var result = _navigator.GetRandomPoint().Result;
            Assert.NotNull(result);
        }

        [Test]
        public void GetNearestPoly_Success()
        {
            var a = new Coordinates(-534.29, 93.625, -744.29);
            var halfExtents = new Vector3d(10, 10, 10);
            var result = _navigator.GetNearestPoly(a, halfExtents).Result;
            
            Assert.NotNull(result);
            Assert.AreEqual(-534.289978027344, result.Coords.x, 1e-6);
            Assert.AreEqual(93.6251068115234, result.Coords.y, 1e-6);
            Assert.AreEqual(-744.289978027344, result.Coords.z, 1e-6);
            Assert.AreEqual(281476476174342, result.Id);
            Assert.AreEqual(281476476174342, result.Node);
        }

        [Test]
        public void GetNearestPoly_FailureOutOfBounds()
        {
            var a = new Coordinates(1e5, 1e5, 1e5);
            
            var halfExtents = new Vector3d(10, 10, 10);
            var result = _navigator.GetNearestPoly(a, halfExtents).Result;
            
            Assert.IsNull(result);
        }
    }
}