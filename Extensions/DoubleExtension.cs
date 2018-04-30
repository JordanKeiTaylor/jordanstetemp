using System;

namespace Shared.Extensions
{
    public static class DoubleExtension
    {
        /// <summary>
        /// Determines if this double is equal to the specified value within a tolerance.
        /// </summary>
        /// <returns>True if equal, false otherwise.</returns>
        /// <param name="d">D.</param>
        /// <param name="val">Value to compare.</param>
        /// <param name="tolerance">Tolerance.</param>
        public static bool Equal(this double d, double val, double tolerance)
        {
            return Math.Abs(d - val) < tolerance;
        }

        /// <summary>
        /// Determines if this double is greater than the specified value within a tolerance.
        /// </summary>
        /// <returns>True if greater than, false otherwise.</returns>
        /// <param name="d">D.</param>
        /// <param name="val">Value to compare.</param>
        /// <param name="tolerance">Tolerance.</param>
        public static bool Greater(this double d, double val, double tolerance)
        {
            return d - val > tolerance;
        }

        /// <summary>
        /// Determines if this double is greater than or equal to the specified value within a tolerance.
        /// </summary>
        /// <returns>True if greater than or equal to, false otherwise.</returns>
        /// <param name="d">D.</param>
        /// <param name="val">Value to compare.</param>
        /// <param name="tolerance">Tolerance.</param>
        public static bool GreaterOrEqual(this double d, double val, double tolerance)
        {
            return (Math.Abs(d - val) < tolerance) || d - val > tolerance;
        }

        /// <summary>
        /// Determines if this double is less than the specified value within a tolerance.
        /// </summary>
        /// <returns>True if greater than, false otherwise.</returns>
        /// <param name="d">D.</param>
        /// <param name="val">Value to compare.</param>
        /// <param name="tolerance">Tolerance.</param>
        public static bool Less(this double d, double val, double tolerance)
        {
            return val - d > tolerance;
        }

        /// <summary>
        /// Determines if this double is less than or equal to the specified value within a tolerance.
        /// </summary>
        /// <returns>True if greater than, false otherwise.</returns>
        /// <param name="d">D.</param>
        /// <param name="val">Value to compare.</param>
        /// <param name="tolerance">Tolerance.</param>
        public static bool LessOrEqual(this double d, double val, double tolerance)
        {
            return (Math.Abs(d - val) < tolerance) || val - d > tolerance;
        }

        /// <summary>
        /// Clamps this double between min and max values within a tolerance.
        /// </summary>
        /// <returns>Clamped value.</returns>
        /// <param name="d">D.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        /// <param name="tolerance">Tolerance.</param>
        public static double Clamp(this double d, double min, double max, double tolerance)
        {
            if (d.Less(min, tolerance))
            {
                return min;
            }

            if (d.Greater(max, tolerance))
            {
                return max;
            }

            return d;
        }
    }
}
