using System;

namespace stesdk.sandbox.Extensions
{
    public static class CoordinatesExtension
    {

        /// <summary>
        /// Euclidean distance squared from this point to specified point.
        /// </summary>
        /// <returns>Distance squared.</returns>
        /// <param name="p">P.</param>
        /// <param name="point">Point.</param>
        public static double DistanceSquaredTo(this Coordinate p, Coordinate point)
        {
            var dx = p.x - point.x;
            var dy = p.y - point.y;
            var dz = p.z - point.z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// Euclidean distance from this point to specified point.
        /// </summary>
        /// <returns>Distance.</returns>
        /// <param name="p">P.</param>
        /// <param name="point">Point.</param>
        public static double DistanceTo(this Coordinate p, Coordinate point)
        {
            return Math.Sqrt(p.DistanceSquaredTo(point));
        }

        /// <summary>
        /// Subtract c2 from c1.
        /// </summary>
        /// <returns>(c2 -c1).</returns>
        /// <param name="c1">c1.</param>
        /// <param name="c2">c2.</param>
        public static Coordinate Subtract(this Coordinate c1, Coordinate c2)
        {
            return new Coordinate(c1.x - c2.x, c1.y - c2.y, c1.z - c2.z);
        }
    }
}
