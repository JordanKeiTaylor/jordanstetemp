using System;
using Improbable;

namespace Shared.Extensions
{
    public static class Vector3dExtension
    {
        public static readonly Vector3d Zero = new Vector3d(0, 0, 0);

        /// <summary>
        /// Euclidean distance from this point to specified point.
        /// </summary>
        /// <returns>Distance.</returns>
        /// <param name="p">P.</param>
        /// <param name="point">Point.</param>
        public static double DistanceTo(this Vector3d p, Vector3d point)
        {
            var dx = p.x - point.x;
            var dy = p.y - point.y;
            var dz = p.z - point.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Euclidean distance squared from this point to specified point.
        /// </summary>
        /// <returns>Distance.</returns>
        /// <param name="p">P.</param>
        /// <param name="point">Point.</param>
        public static double DistanceSquaredTo(this Vector3d p, Vector3d point)
        {
            var dx = p.x - point.x;
            var dy = p.y - point.y;
            var dz = p.z - point.z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// Determines if this point is equal to specified point with a tolerance.
        /// </summary>
        /// <returns><c>true</c>, if equal, <c>false</c> otherwise.</returns>
        /// <param name="p">P.</param>
        /// <param name="point">Point.</param>
        /// <param name="tolerance">Tolerance.</param>
        public static bool EqualsTo(this Vector3d p, Vector3d point, double tolerance)
        {
            var dx = Math.Abs(p.x - point.x);
            var dy = Math.Abs(p.y - point.y);
            var dz = Math.Abs(p.z - point.z);
            return dx < tolerance && dy < tolerance && dz < tolerance;
        }

        /// <summary>
        /// Subtract v2 from v1.
        /// </summary>
        /// <returns><c>v1 - v2</c></returns>
        /// <param name="v1">Vector 1.</param>
        /// <param name="v2">Vector 2.</param>
        public static Vector3d Subtract(this Vector3d v1, Vector3d v2)
        {
            return new Vector3d(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        /// <summary>
        /// Get length of vector.
        /// </summary>
        /// <returns><c>|v|</c></returns>
        public static double Length(this Vector3d v)
        {
            return Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        /// <summary>
        /// Get length squared of vector.
        /// </summary>
        /// <returns><c>|v|^2</c></returns>
        public static double LengthSquared(this Vector3d v)
        {
            return v.x * v.x + v.y * v.y + v.z * v.z;
        }

        /// <summary>
        /// Get the normalized vector of this vector.
        /// </summary>
        /// <param name="v">Vector</param>
        /// <returns>Normalized vector.</returns>
        public static Vector3d Normalize(this Vector3d v)
        {
            var length = v.Length();
            var x = v.x / length;
            var y = v.y / length;
            var z = v.z / length;
            return new Vector3d(x, y, z);
        }

        /// <summary>
        /// Converts this vector to a coordinate.
        /// </summary>
        /// <returns>Vector.</returns>
        /// <param name="coord">Coordinate.</param>
        /// Convert to Coordinates
        /// </summary>
        /// <returns><c>A Coordinate object equal to the given Vector3d</c></returns>
        public static Coordinates ToCoordinates(this Vector3d v)
        {
            return new Coordinates(v.x, v.y, v.z);
        }
    }
}
