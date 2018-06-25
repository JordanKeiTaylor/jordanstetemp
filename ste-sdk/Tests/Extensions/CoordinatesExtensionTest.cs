using Improbable;
using Improbable.Sandbox.Extensions;
using NUnit.Framework;

namespace Tests.Extensions
{
    [TestFixture]
    public class CoordinatesExtensionTest
    {
        [Test]
        public void Should_ConvertTo_Vector3dType()
        {
            var coord = new Coordinates(1, 2, 3);
            var vector = coord.ToVector3d();

            Assert.AreEqual(coord.x, vector.x);
            Assert.AreEqual(coord.y, vector.y);
            Assert.AreEqual(coord.z, vector.z);
        }
    }
}
