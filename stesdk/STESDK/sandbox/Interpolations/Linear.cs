namespace stesdk.sandbox.Interpolations
{
    public class Linear : IInterpolate
    {
        private readonly double _tolerance;
        private readonly double _length;
        private readonly Vector3d _p1;
        private readonly Vector3d _p2;

        public Linear(
            Vector3d point1,
            Vector3d point2,
            double tolerance = 0.0001)
        {
            _p1 = point1;
            _p2 = point2;
            this._tolerance = tolerance;

            _length = _p1.DistanceTo(_p2);
        }

        public double Length => _length;

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

            if (distance > _length)
            {
                return 1;
            }

            return distance / _length;
        }

        public double StepAt(Vector3d vector)
        {
            var distance = LengthAt(vector);
            return distance / _length;
        }
    }
}
