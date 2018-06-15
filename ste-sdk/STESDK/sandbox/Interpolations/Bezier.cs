using System;

namespace stesdk.sandbox.Interpolations
{
    internal enum Curve
    {
        Quadratic,
        Cubic,
    }

    public class Bezier : IInterpolate
    {
        private readonly Curve _curve;
        private readonly double _tolerance;

        private readonly Vector3d _p1;
        private readonly Vector3d _p2;
        private readonly Vector3d _cp1;
        private readonly Vector3d _cp2;

        private double _length;
        private double[] _steps;
        private double[] _lengths;
        private Vector3d[] _points;

        /// <summary>
        /// Initializes a new instance of a Quadratic <see cref="T:stesdk.sandbox.Interpolations.Bezier"/> Bezier interpolator.
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
            double tolerance = 0.0001)
        {
            _p1 = point1;
            _p2 = point2;
            _cp1 = controlPoint;
            _curve = Curve.Quadratic;
            this._tolerance = tolerance;
            Sample(smaples);
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
            double tolerance = 0.0001)
        {
            _p1 = point1;
            _p2 = point2;
            _cp1 = controlPoint1;
            _cp2 = controlPoint2;
            _curve = Curve.Cubic;
            this._tolerance = tolerance;
            Sample(samples);
        }

        public double Length => _length;

        public Vector3d PositionAt(double step)
        {
            var safeStep = step.Clamp(0, 1, _tolerance);
            if (_curve == Curve.Quadratic)
            {
                return Interpolate.Bezier(_p1, _p2, _cp1, safeStep);
            }

            return Interpolate.Bezier(_p1, _p2, _cp1, _cp2, safeStep);
        }

        public double StepAt(Vector3d vector)
        {
            var index = FindIndexAt(vector);
            return _steps[index];
        }

        public double StepAt(double distance)
        {
            var safeDistance = distance;
            if (distance < 0)
            {
                safeDistance = 0;
            }

            if (distance.Greater(Length, _tolerance))
            {
                safeDistance = Length;
            }

            var index = _lengths.BinarySearch(safeDistance);
            return _steps[index];
        }

        public double LengthAt(double step)
        {
            var safeStep = step.Clamp(0, 1, _tolerance);
            var index = _steps.BinarySearch(safeStep);
            return _lengths[index];
        }

        public double LengthAt(Vector3d vector)
        {
            var index = FindIndexAt(vector);
            return _lengths[index];
        }

        private void Sample(int samples)
        {
            if (samples <= 0)
            {
                throw new ArgumentException("Parameter divisions must be greater than 0.");
            }

            _steps = new double[samples + 1];
            _lengths = new double[samples + 1];
            _points = new Vector3d[samples + 1];

            _steps[0] = 0;
            _lengths[0] = 0;
            _points[0] = _p1;

            Vector3d prevP = _p1;

            _length = 0;
            var stepSize = 1d / samples;
            for (int i = 1; i <= samples; i++)
            {
                var step = stepSize * i;
                var currP = PositionAt(step);
                _length += currP.DistanceTo(prevP);
                prevP = currP;

                _steps[i] = step;
                _lengths[i] = _length;
                _points[i] = currP;
            }
        }

        private int FindIndexAt(Vector3d vector)
        {
            var minIndex = 0;
            var minDistance = _p1.DistanceSquaredTo(vector);
            if (minDistance < _tolerance)
            {
                return minIndex;
            }

            // have to loop through all points as this is a curve, O(n)
            for (var i = 0; i < _steps.Length; i++)
            {
                var distance = _points[i].DistanceSquaredTo(vector);
                if (distance.LessOrEqual(minDistance, _tolerance))
                {
                    minDistance = distance;
                    minIndex = i;
                }
            }

            return minIndex;
        }
    }
}
