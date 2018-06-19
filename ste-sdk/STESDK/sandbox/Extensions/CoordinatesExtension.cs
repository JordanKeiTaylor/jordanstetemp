using System;

namespace Improbable.Enterprise.Sandbox.Extensions
{
    public static class CoordinatesExtension
    {
        /// <summary>
        /// Converts this coordinate to a vector3d.
        /// </summary>
        /// <returns>Vector.</returns>
        /// <param name="coord">Coordinate.</param>
        public static Vector3d ToVector3d(this Coordinates coord)
        {
            return new Vector3d(coord.x, coord.y, coord.z);
        }

        /// <summary>
        /// Euclidean distance squared from this point to specified point.
        /// </summary>
        /// <returns>Distance squared.</returns>
        /// <param name="p">P.</param>
        /// <param name="point">Point.</param>
        public static double DistanceSquaredTo(this Coordinates p, Coordinates point)
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
        public static double DistanceTo(this Coordinates p, Coordinates point)
        {
            return Math.Sqrt(p.DistanceSquaredTo(point));
        }

        /// <summary>
        /// Subtract c2 from c1.
        /// </summary>
        /// <returns>(c2 -c1).</returns>
        /// <param name="c1">c1.</param>
        /// <param name="c2">c2.</param>
        public static Coordinates Subtract(this Coordinates c1, Coordinates c2)
        {
            return new Coordinates(c1.x - c2.x, c1.y - c2.y, c1.z - c2.z);
        }
    }
}
