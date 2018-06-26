using Improbable;
using Improbable.Sandbox.Interpolations;
using NUnit.Framework;

namespace Tests.Interpolations
{
    [TestFixture]
    public class InterpolateTest
    {
        [Test]
        public void Should_CalculatePoint_ForLinearZeroStep()
        {
            var step = 0.0;
            var p = Interpolate.Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1),
                step
            );

            Assert.AreEqual(new Vector3d(0.0, 0.0, 0.0), p);
        }

        [Test]
        public void Should_CalculatePoint_ForLinearHalfStep()
        {
            var step = 0.5;
            var p = Interpolate.Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1),
                step
            );

            Assert.AreEqual(new Vector3d(0.5, 0.5, 0.5), p);
        }

        [Test]
        public void Should_CalculatePoint_ForLinearFullStep()
        {
            var step = 1.0;
            var p = Interpolate.Linear(
                new Vector3d(0, 0, 0),
                new Vector3d(1, 1, 1),
                step
            );

            Assert.AreEqual(new Vector3d(1.0, 1.0, 1.0), p);
        }

        [Test]
        public void Should_CalculatePoint_ForQuadBezierZeroStep()
        {
            var step = 0.0;
            var p = Interpolate.Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                step
            );

            Assert.AreEqual(new Vector3d(0.0, 0.0, 0.0), p);
        }

        [Test]
        public void Should_CalculatePoint_ForQuadBezier1stQtrStep()
        {
            var step = 0.25;
            var p = Interpolate.Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                step
            );

            Assert.AreEqual(new Vector3d(0.25, 0.25, 0.25), p);
        }

        [Test]
        public void Should_CalculatePoint_ForQuadBezier2ndQtrStep()
        {
            var step = 0.50;
            var p = Interpolate.Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                step
            );

            Assert.AreEqual(new Vector3d(0.5, 0.5, 0.5), p);
        }

        [Test]
        public void Should_CalculatePoint_ForQuadBezier3rdQtrStep()
        {
            var step = 0.75;
            var p = Interpolate.Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                step
            );

            Assert.AreEqual(new Vector3d(0.75, 0.75, 0.75), p);
        }

        [Test]
        public void Should_CalculatePoint_ForQuadBezierFullStep()
        {
            var step = 1.0;
            var p = Interpolate.Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                step
            );

            Assert.AreEqual(new Vector3d(1.0, 1.0, 1.0), p);
        }

        [Test]
        public void Should_CalculatePoint_ForCubeBezierZeroStep()
        {
            var step = 0.0;
            var p = Interpolate.Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5),
                step
            );

            Assert.AreEqual(new Vector3d(0.0, 0.0, 0.0), p);
        }

        [Test]
        public void Should_CalculatePoint_ForCubeBezier1stQtrStep()
        {
            var step = 0.25;
            var p = Interpolate.Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5),
                step
            );

            Assert.AreEqual(new Vector3d(0.15625, 0.15625, 0.15625), p);
        }

        [Test]
        public void Should_CalculatePoint_ForCubeBezier2ndQtrStep()
        {
            var step = 0.50;
            var p = Interpolate.Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5),
                step
            );

            Assert.AreEqual(new Vector3d(0.125, 0.125, 0.125), p);
        }

        [Test]
        public void Should_CalculatePoint_ForCubeBezier3rdQtrStep()
        {
            var step = 0.75;
            var p = Interpolate.Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5),
                step
            );

            Assert.AreEqual(new Vector3d(0.28125, 0.28125, 0.28125), p);
        }

        [Test]
        public void Should_CalculatePoint_ForCubeBezierFullStep()
        {
            var step = 1.0;
            var p = Interpolate.Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5),
                step
            );

            Assert.AreEqual(new Vector3d(1.0, 1.0, 1.0), p);
        }
    }
}
