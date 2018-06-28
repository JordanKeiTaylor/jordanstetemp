using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Improbable;
using Improbable.Recast;
using Improbable.Recast.Types;
using Improbable.Sandbox.Navigation;
using Improbable.Sandbox.Navigation.Api;
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
            var relPath = "./../../resources/L19.obj.bin64";
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relPath);
            _navigator = new DefaultMeshNavigator(fullPath);
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