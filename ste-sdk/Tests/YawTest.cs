using System;
using Improbable.Sandbox;
using NUnit.Framework;

namespace Tests
{
    public class YawTest
    {
        private const double TOLERANCE = 1E-9;

        [Test]
        public void Should_Update_ByCorrectAmount()
        {
            Assert.AreEqual(
                Math.PI / 2,
                Yaw.RotateBy(0, Math.PI / 2, Math.PI / 2, 1.0),
                TOLERANCE
            );

            Assert.AreEqual(
                0.0,
                Yaw.RotateBy(Math.PI / 2, 0, Math.PI / 2, 1.0),
                TOLERANCE
            );
        }

        [Test]
        public void Should_Interpolate()
        {
            Assert.AreEqual(
                0.1,
                Yaw.RotateBy(0, Math.PI / 2.0, 0.1, 1),
                TOLERANCE
            );

            Assert.AreEqual(
                -0.05,
                Yaw.RotateBy(0, -0.1, 0.05, 1),
                TOLERANCE
            );
        }

        [Test]
        public void Should_IgnoreYawRateSign()
        {
            Assert.AreEqual(
                0.1,
                Yaw.RotateBy(0, Math.PI/2, -0.1, 1),
                TOLERANCE
            );
        }

        [Test]
        public void Should_SnapIfLargeTurn()
        {
            Assert.AreEqual(
                Math.PI,
                Yaw.RotateBy(0, Math.PI , 0.1, 1),
                TOLERANCE
            );
        }
    }
}
