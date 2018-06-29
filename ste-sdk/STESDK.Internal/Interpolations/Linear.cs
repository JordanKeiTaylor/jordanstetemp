using Improbable.Extensions;

namespace Improbable.Interpolations
{
    /// <summary>
    /// Utility for linear interpolation.
    /// </summary>
    public class Linear : IInterpolate
    {
        private readonly Vector3d _p1;
        private readonly Vector3d _p2;
        private readonly double _tolerance;
        
        public double Length { get; }

        public Linear(Vector3d point1, Vector3d point2, double tolerance = 0.0001)
        {
            _p1 = point1;
            _p2 = point2;
            _tolerance = tolerance;
            Length = _p1.DistanceTo(_p2);
        }

        public double LengthAt(double step)
        {
            var safeStep = step.Clamp(0, 1, _tolerance);
            var p = PositionAt(safeStep);
            return _p1.DistanceTo(p);
        }

        public double LengthAt(Vector3d vector)
        {
            return _p1.DistanceTo(vector);
        }

        public Vector3d PositionAt(double step)
        {
            var safeStep = step.Clamp(0, 1, _tolerance);
            return Interpolate.Linear(_p1, _p2, safeStep);
        }

        public double StepAt(double distance)
        {
            if (distance <= 0)
            {
                return 0;
            }

            if (distance > Length)
            {
                return 1;
            }

            return distance / Length;
        }

        public double StepAt(Vector3d vector)
        {
            var distance = LengthAt(vector);
            return distance / Length;
        }
    }
}
