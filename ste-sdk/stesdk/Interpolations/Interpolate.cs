using System;

namespace Improbable.Interpolations
{
    public static class Interpolate
    {
        /// <summary>
        /// Linear interpolation from a start point to end point.
        /// </summary>
        /// <returns>The current point in linear interpolation.</returns>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="step">Step value between 0 and 1.</param>
        public static Vector3d Linear(Vector3d start, Vector3d end, double step)
        {
            var dx = Linear(start.x, end.x, step);
            var dy = Linear(start.y, end.y, step);
            var dz = Linear(start.z, end.z, step);

            return new Vector3d(dx, dy, dz);
        }

        /// <summary>
        /// Bezier interpolation from a start point to end point with a control point.
        /// </summary>
        /// <returns>The current point in bezier interpolation.</returns>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="control">Control point.</param>
        /// <param name="step">Step value between 0 and 1.</param>
        public static Vector3d Bezier(Vector3d start, Vector3d end, Vector3d control, double step)
        {
            var dx = Bezier(start.x, end.x, control.x, step);
            var dy = Bezier(start.y, end.y, control.y, step);
            var dz = Bezier(start.z, end.z, control.z, step);

            return new Vector3d(dx, dy, dz);
        }

        /// <summary>
        /// Bezier interpolation from a start point to end point with
        /// control point 1 and control point 2.
        /// </summary>
        /// <returns>The current point in bezier interpolation.</returns>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="control1">Control point 1.</param>
        /// <param name="control2">Control point 2.</param>
        /// <param name="step">Step value between 0 and 1.</param>
        public static Vector3d Bezier(Vector3d start, Vector3d end, Vector3d control1, Vector3d control2, double step)
        {
            var dx = Bezier(start.x, end.x, control1.x, control2.x, step);
            var dy = Bezier(start.y, end.y, control1.y, control2.y, step);
            var dz = Bezier(start.z, end.z, control1.z, control2.z, step);

            return new Vector3d(dx, dy, dz);
        }

        private static double Linear(double p1, double p2, double t)
        {
            return ((1 - t) * p1) + (t * p2);
        }

        private static double Bezier(double p1, double p2, double c1, double t)
        {
            return (((1 - t) * (1 - t)) * p1) + (2 * t * (1 - t) * c1) + ((t * t) * p2);
        }

        private static double Bezier(double p1, double p2, double c1, double c2, double t)
        {
            return (((-p1 + 3 * (c1 - c2) + p2) * t + (3 * (p1 + c2) - 6 * c1)) * t + 3 * (c1 - p1)) * t + p1;
        }

        private static double Bezier(double[] p, double t)
        {
            var sum = 0.00;
            var n = p.Length;

            for (var i = 0; i < n; i++)
            {
                sum += Math.Pow(1 - t, n - i) * Math.Pow(t, i) * p[i];
            }

            return sum;
        }
    }
}