using System;

namespace Improbable.Projections
{
    public class Globe
    {
        public const double MilesPerHourToMetersPerSecondConversionFactor = 0.44704;

        public static double DegreesToRadians(double n)
        {
            return n / 360.0f * 2 * Math.PI;
        }

        public static double RadiansToDegrees(double n)
        {
            return n / (2 * Math.PI) * 360.0f;
        }
    }
}