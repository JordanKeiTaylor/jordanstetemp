using Improbable;
using Improbable.Sandbox.Extensions;
using NUnit.Framework;

namespace Tests.Extensions
{
    [TestFixture]
    public class Vector3dExtensionTest
    {
        [Test]
        public void Should_CalculateDistanceOf_1()
        {
            var v1 = new Vector3d(0, 0, 0);
            var v2 = new Vector3d(1, 1, 1).Normalize();
            var distance = v1.DistanceTo(v2);

            Assert.AreEqual(1, distance);
        }

        [Test]
        public void Should_Equal_WithLooseTolerance()
        {
            var tolerance = 0.01;
            var v1 = new Vector3d(1.001, 1.001, 1.001);
            var v2 = new Vector3d(1.009, 1.009, 1.009);

            Assert.IsTrue(v1.EqualsTo(v2, tolerance));
        }

        [Test]
        public void Should_NotEqual_WithTightTolerance()
        {
            var tolerance = 0.001;
            var v1 = new Vector3d(1.001, 1.001, 1.001);
            var v2 = new Vector3d(1.009, 1.009, 1.009);

            Assert.IsFalse(v1.EqualsTo(v2, tolerance));
        }

        [Test]
        public void Should_NormalizeVector_ToUnitVector()
        {
            var v1 = new Vector3d(1.0, 2.0, 3.0);
            var v2 = v1.Normalize();

            Assert.AreEqual(1.0, v2.Length());
        }

        [Test]
        public void Should_CalculateLengthOf_3()
        {
            var v1 = new Vector3d(3.0, 0.0, 0.0);
            var v2 = new Vector3d(0.0, 3.0, 0.0);
            var v3 = new Vector3d(0.0, 0.0, 3.0);

            Assert.AreEqual(3.0, v1.Length());
            Assert.AreEqual(3.0, v2.Length());
            Assert.AreEqual(3.0, v3.Length());
        }

        [Test]
        public void Should_CalculateSquareLengthOf_3()
        {
            var v1 = new Vector3d(3.0, 0.0, 0.0);
            var v2 = new Vector3d(0.0, 3.0, 0.0);
            var v3 = new Vector3d(0.0, 0.0, 3.0);

            Assert.AreEqual(9.0, v1.LengthSquared());
            Assert.AreEqual(9.0, v2.LengthSquared());
            Assert.AreEqual(9.0, v3.LengthSquared());
        }

        [Test]
        public void Should_SubtractVectors()
        {
            var v1 = new Vector3d(1.0, 0.0, 0.0);
            var v2 = new Vector3d(0.0, 1.0, 0.0);
            var v3 = v1.Subtract(v2);

            Assert.AreEqual(new Vector3d(1.0, -1.0, 0.0), v3);
        }
    }
}
