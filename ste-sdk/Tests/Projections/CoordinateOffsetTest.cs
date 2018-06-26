using Improbable;
using Improbable.sandbox.Projections;
using NUnit.Framework;

namespace Tests.Projections
{
    [TestFixture]
    public class CoordinateOffsetTest
    {
        static readonly Vector3d originalV = new Vector3d(0, 0, 0);

        [TestCase(10.0, 15.0, Description = "Should apply positive offsets")]
        [TestCase(-10.0, -15.0, Description = "Should apply negative offsets")]
        [TestCase(-10.0, 15.0, Description = "Should apply negative x offset")]
        [TestCase(10.0, -15.0, Description = "Should apply negative z offset")]
        public void Should_ApplyOffset(double xOffset, double zOffset)
        {
            var v = CoordinateOffset.ApplyOffset(originalV.x, originalV.y, originalV.z,
                xOffset, zOffset);
            Assert.AreEqual(v.x, xOffset);
            Assert.AreEqual(v.z, zOffset);
        }

        [TestCase(10.0, 15.0, Description = "Should backout positive offsets")]
        [TestCase(-10.0, -15.0, Description = "Should backout negative offsets")]
        [TestCase(-10.0, 15.0, Description = "Should backout negative x offset")]
        [TestCase(10.0, -15.0, Description = "Should backout negative z offset")]
        public void Should_BackoutOffset(double xOffset, double zOffset)
        {
            var v = CoordinateOffset.ApplyOffset(originalV.x, originalV.y, originalV.z,
                xOffset, zOffset);
            double outX = 0.0, outZ = 0.0;
            CoordinateOffset.BackoutOffset(out outX, out outZ, v.x, v.z, xOffset, zOffset);
            Assert.AreEqual(outX, originalV.x);
            Assert.AreEqual(outX, originalV.z);
        }
    }
}