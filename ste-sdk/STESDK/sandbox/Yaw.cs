using System;

namespace Improbable.Enterprise.Sandbox
{
    public static class Yaw
    {
        private const double MaxInterpolateRadians = Math.PI / 2.0;

        public static double RotateBy(double currentYawRadians, double targetYawRadians, double yawRateRadiansPerSecond, double deltaTInSeconds)
        {
            var deltaTheta = deltaTInSeconds * Math.Abs(yawRateRadiansPerSecond);
            var signedDeltaYaw = UnsignedMod((targetYawRadians - currentYawRadians) + Math.PI, Math.PI * 2) - Math.PI;

            if (Math.Abs(signedDeltaYaw) <= deltaTheta)
            {
                return targetYawRadians;
            }

            // If the turn is big then just snap straight to
            if (Math.Abs(signedDeltaYaw) > MaxInterpolateRadians)
            {
                return targetYawRadians;
            }

            return ChangeAngle(currentYawRadians, deltaTheta * signedDeltaYaw / Math.Abs(signedDeltaYaw));
        }

        private static double UnsignedMod(double a, double n)
        {
            return (a % n + n) % n;
        }

        /// <summary>
        /// Changes the given angle theta by a signed amount.
        /// </summary>
        /// <returns>New angle in radians.</returns>
        /// <param name="theta">Theta. Angle in radians.</param>
        /// <param name="deltaTheta">Delta theta. Change in radians.</param>
        private static double ChangeAngle(double theta, double deltaTheta)
        {
            // Add delta and map to 0 -> 2pi.
            double newAngle = ((2.0 * Math.PI) + theta + deltaTheta) % (2 * Math.PI);

            // Convert back to -pi to +pi
            if (newAngle > Math.PI)
            {
                return -(Math.PI * 2.0 - newAngle);
            }

            return newAngle;
        }
    }
}
