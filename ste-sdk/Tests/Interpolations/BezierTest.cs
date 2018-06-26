using Improbable;
using Improbable.Sandbox.Extensions;
using Improbable.Sandbox.Interpolations;
using NUnit.Framework;

namespace Tests.Interpolations
{
    [TestFixture]
    public class BezierTest
    {
        [Test]
        public void Should_CalculatePoint_ForQuadZeroStep()
        {
            var step = 0.0;
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(0.0, 0.0, 0.0), p);
        }

        [Test]
        public void Should_CalculatePoint_ForQuad1stQtrStep()
        {
            var step = 0.25;
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(0.25, 0.25, 0.25), p);
        }

        [Test]
        public void Should_CalculatePoint_ForQuad2ndQtrStep()
        {
            var step = 0.50;
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(0.5, 0.5, 0.5), p);
        }

        [Test]
        public void Should_CalculatePoint_ForQuad3rdQtrStep()
        {
            var step = 0.75;
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(0.75, 0.75, 0.75), p);
        }

        [Test]
        public void Should_CalculatePoint_ForQuadFullStep()
        {
            var step = 1.0;
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(1.0, 1.0, 1.0), p);
        }

        [Test]
        public void Should_CalculateStep_ForQuadHalfLength()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var step = interpolator.StepAt(interpolator.Length / 2);

            Assert.AreEqual(0.5, step);
        }

        [Test]
        public void Should_CalculateStep_ForQuadFullLength()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var step = interpolator.StepAt(interpolator.Length);

            Assert.AreEqual(1.0, step);
        }

        [Test]
        public void Should_CalculateLength_ForQuadUnitLength()
        {
            var unit = new Vector3d(1.0, 1.0, 1.0).Normalize();
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                unit,
                unit
            );

            Assert.AreEqual(1.0, interpolator.Length, 0.0001);
        }

        [Test]
        public void Should_CalculateZeroLength_ForQuadStartPoint()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var length = interpolator.LengthAt(new Vector3d(0.0, 0.0, 0.0));

            Assert.AreEqual(0, length, 0.0001);
        }

        [Test]
        public void Should_CalculateMaxLength_ForQuadEndPoint()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var length = interpolator.LengthAt(new Vector3d(1.0, 1.0, 1.0));

            Assert.AreEqual(interpolator.Length, length, 0.0001);
        }

        [Test]
        public void Should_CalculateZeroLength_ForQuadZeroStep()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var length = interpolator.LengthAt(0);

            Assert.AreEqual(0, length, 0.0001);
        }

        [Test]
        public void Should_CalculateMaxLength_ForQuadFullStep()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var length = interpolator.LengthAt(1);

            Assert.AreEqual(interpolator.Length, length, 0.0001);
        }

        [Test]
        public void Should_CalculatePoint_ForCubicZeroStep()
        {
            var step = 0.0;
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(0.0, 0.0, 0.0), p);
        }

        [Test]
        public void Should_CalculatePoint_ForCubic1stQtrStep()
        {
            var step = 0.25;
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(0.15625, 0.15625, 0.15625), p);
        }

        [Test]
        public void Should_CalculatePoint_ForCubic2ndQtrStep()
        {
            var step = 0.50;
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(0.125, 0.125, 0.125), p);
        }

        [Test]
        public void Should_CalculatePoint_ForCubic3rdQtrStep()
        {
            var step = 0.75;
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(0.28125, 0.28125, 0.28125), p);
        }

        [Test]
        public void Should_CalculatePoint_ForCubicFullStep()
        {
            var step = 1.0;
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5)
            );

            var p = interpolator.PositionAt(step);

            Assert.AreEqual(new Vector3d(1.0, 1.0, 1.0), p);
        }

        [Test]
        public void Should_CalculateStep_ForCubicHalfLength()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(0.5, 0.5, 0.5)
            );

            var step = interpolator.StepAt(interpolator.Length/2);

            Assert.AreEqual(0.5, step);
        }

        [Test]
        public void Should_CalculateStep_ForCubicFullLength()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5)
            );

            var step = interpolator.StepAt(interpolator.Length);

            Assert.AreEqual(1.0, step);
        }

        [Test]
        public void Should_CalculateLength_ForCubicUnitLength()
        {
            var unit = new Vector3d(1.0, 1.0, 1.0).Normalize();
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0), 
                unit,
                new Vector3d(0.0, 0.0, 0.0),
                unit
            );

            Assert.AreEqual(1.0, interpolator.Length, 0.0001);
        }

        [Test]
        public void Should_CalculateZeroLength_ForCubicStartPoint()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5)
            );

            var length = interpolator.LengthAt(new Vector3d(0.0, 0.0, 0.0));

            Assert.AreEqual(0, length, 0.0001);
        }

        [Test]
        public void Should_CalculateMaxLength_ForCubicEndPoint()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5)
            );

            var length = interpolator.LengthAt(new Vector3d(1.0, 1.0, 1.0));

            Assert.AreEqual(interpolator.Length, length, 0.0001);
        }

        [Test]
        public void Should_CalculateZeroLength_ForCubicZeroStep()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5)
            );

            var length = interpolator.LengthAt(0);

            Assert.AreEqual(0, length, 0.0001);
        }

        [Test]
        public void Should_CalculateMaxLength_ForCubicFullStep()
        {
            var interpolator = new Bezier(
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(1.0, 1.0, 1.0),
                new Vector3d(0.5, 0.5, 0.5),
                new Vector3d(-0.5, -0.5, -0.5)
            );

            var length = interpolator.LengthAt(1);

            Assert.AreEqual(interpolator.Length, length, 0.0001);
        }
    }
}
