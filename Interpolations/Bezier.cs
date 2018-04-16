using System;
using Improbable;
using Shared.Extensions;

namespace Shared.Interpolations
{
    public class Bezier : IInterpolate
    {
        private Curve curve;
        private double length;
        private double tolerance;

        private Vector3d p1;
        private Vector3d p2;
        private Vector3d cp1;
        private Vector3d cp2;

        private double[] steps;
        private double[] lengths;
        private Vector3d[] points;

        enum Curve 
        {
            Quadratic,
            Cubic
        }

        /// <summary>
        /// Initializes a new instance of a Quadratic <see cref="T:Shared.Interpolations.Bezier"/> Bezier interpolator.
        /// </summary>
        /// <param name="point1">Start point.</param>
        /// <param name="point2">End point.</param>
        /// <param name="controlPoint">Control point.</param>
        /// <param name="smaples">Number of samples along bezier curve.</param>
        /// <param name="tolerance">Tolerance.</param>
        public Bezier(
            Vector3d point1,
            Vector3d point2,
            Vector3d controlPoint,
            int smaples = 100,
            double tolerance = 0.0001
        ) {
            p1 = point1;
            p2 = point2;
            cp1 = controlPoint;
            curve = Curve.Quadratic;
            this.tolerance = tolerance;
            sample(smaples);
        }

        /// <summary>
        /// Initializes a new instance of a Cubic <see cref="T:Shared.Interpolations.Bezier"/> Bezier interpolator.
        /// </summary>
        /// <param name="point1">Start point.</param>
        /// <param name="point2">End point.</param>
        /// <param name="controlPoint1">Control point1.</param>
        /// <param name="controlPoint2">Control point2.</param>
        /// <param name="smaples">Number of samples along bezier curve.</param>
        /// <param name="tolerance">Tolerance.</param>
        public Bezier(
            Vector3d point1, 
            Vector3d point2, 
            Vector3d controlPoint1, 
            Vector3d controlPoint2,
            int samples = 100,
            double tolerance = 0.0001
        ) {
            p1 = point1;
            p2 = point2;
            cp1 = controlPoint1;
            cp2 = controlPoint2;
            curve = Curve.Cubic;
            this.tolerance = tolerance;
            sample(samples);
        }

        public Vector3d PositionAt(double step) 
        {
            var safeStep = step.Clamp(0, 1, tolerance);
            if (curve == Curve.Quadratic)
            {
                return Interpolate.Bezier(p1, p2, cp1, safeStep);
            }
            return Interpolate.Bezier(p1, p2, cp1, cp2, safeStep);
        }

        public double StepAt(Vector3d vector)
        {
            var index = findIndexAt(vector);
            return steps[index];
        }

        public double StepAt(double distance)
        {
            var safeDistance = distance;
            if (distance < 0) 
            {
                safeDistance = 0;    
            }
            if (distance.Greater(Length, tolerance))
            {
                safeDistance = Length;
            }
            var index = lengths.BinarySearch(safeDistance);
            return steps[index];
        }

        public double Length => length;

        public double LengthAt(double step)
        {
            var safeStep = step.Clamp(0, 1, tolerance);
            var index = steps.BinarySearch(safeStep);
            return lengths[index];
        }

        public double LengthAt(Vector3d vector)
        {
            var index = findIndexAt(vector);
            return lengths[index];
        }

        private void sample(int samples)
        {
            if (samples <= 0)
            {
                throw new ArgumentException("Parameter divisions must be greater than 0.");
            }
            steps = new double[samples + 1];
            lengths = new double[samples + 1];
            points = new Vector3d[samples + 1];

            steps[0] = 0;
            lengths[0] = 0;
            points[0] = p1;

            Vector3d prevP = p1;

            length = 0;
            var stepSize = 1d / samples;
            for (int i = 1; i <= samples; i++)
            {
                var step = stepSize * i;
                var currP = PositionAt(step);
                length += currP.DistanceTo(prevP);
                prevP = currP;

                steps[i] = step;
                lengths[i] = length;
                points[i] = currP;
            }
        }

        private int findIndexAt(Vector3d vector)
        {
            var minIndex = 0;
            var minDistance = p1.DistanceSquaredTo(vector);
            if (minDistance < tolerance) { return minIndex; }
            // have to loop through all points as this is a curve, O(n)
            for (var i = 0; i < steps.Length; i++)
            {
                var distance = points[i].DistanceSquaredTo(vector);
                if (distance.LessOrEqual(minDistance, tolerance))
                {
                    minDistance = distance;
                    minIndex = i;
                }
            }
            return minIndex;
        }
    }
}
