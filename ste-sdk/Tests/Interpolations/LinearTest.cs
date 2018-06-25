using Improbable;
using Improbable.Sandbox.Extensions;
using Improbable.Sandbox.Interpolations;
using NUnit.Framework;

namespace Tests.Interpolations
{
    [TestFixture]
    public class LinearTest
    {
        [Test]
        public void Should_CalculatePoint_ForLinearZeroStep()
        {
            var step = 0.0;
            var interpolator = new Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(0.0, 0.0, 0.0), p);
        }

        [Test]
        public void Should_CalculatePoint_ForLinearHalfStep()
        {
            var step = 0.5;
            var interpolator = new Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(0.5, 0.5, 0.5), p);
        }

        [Test]
        public void Should_CalculatePoint_ForLinearFullStep()
        {
            var step = 1.0;
            var interpolator = new Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(1.0, 1.0, 1.0), p);
        }

        [Test]
        public void Should_CalculateLength_ForUnitLength()
        {
            var interpolator = new Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1).Normalize()
            );

            Assert.AreEqual(1.0, interpolator.Length);
        }

        [Test]
        public void Should_CalculateStep_ForHalfLength()
        {
            var interpolator = new Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1)
            );

            var length = interpolator.Length;
            var step = interpolator.StepAt(length / 2);

            Assert.AreEqual(0.5, step);
        }

        [Test]
        public void Should_CalculateStep_ForFullLength()
        {
            var interpolator = new Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1)
            );

            var length = interpolator.Length;
            var step = interpolator.StepAt(length);

            Assert.AreEqual(1.0, step);
        }

        [Test]
        public void Should_CalculateZeroLength_ForZeroStep()
        {
            var interpolator = new Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1)
            );

            var length = interpolator.LengthAt(0);

            Assert.AreEqual(0, length);
        }

        [Test]
        public void Should_CalculateMaxLength_ForFullStep()
        {
            var interpolator = new Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1)
            );

            var length = interpolator.LengthAt(1);

            Assert.AreEqual(interpolator.Length, length);
        }

        [Test]
        public void Should_CalculateZeroLength_ForStartPoint()
        {
            var interpolator = new Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1)
            );

            var length = interpolator.LengthAt(new Vector3d(0, 0, 0));

            Assert.AreEqual(0, length);
        }

        [Test]
        public void Should_CalculateMaxLength_ForFullEndPoint()
        {
            var interpolator = new Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1)
            );

            var length = interpolator.LengthAt(new Vector3d(1, 1, 1));

            Assert.AreEqual(interpolator.Length, length);
        }
    }
}
