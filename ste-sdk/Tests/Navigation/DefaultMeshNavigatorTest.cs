using System;
using System.IO;
using Improbable.sandbox.Navigation;
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
        public void Test()
        {
            _navigator.GetHashCode();
        }
    }
}