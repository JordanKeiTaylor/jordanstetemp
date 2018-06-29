using System;
using System.IO;
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
            Assert.AreEqual(PathStatus.NotFound, result.Status);
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
            Assert.AreEqual(PathStatus.NotFound, result.Status);
        }
        
    }
}