using Improbable;
using Shared.Extensions;

namespace Shared.Interpolations
{
    public class Linear : IInterpolate
    {
        private double tolerance;
        private double length;
        private Vector3d p1;
        private Vector3d p2;

        public Linear(
            Vector3d point1, 
            Vector3d point2, 
            double tolerance = 0.0001
        ) {
            p1 = point1;
            p2 = point2;
            this.tolerance = tolerance;

            length = p1.DistanceTo(p2);
        }

        public double Length => length;

        public double LengthAt(double step)
        {
            var safeStep = step.Clamp(0, 1, tolerance);
            var p = PositionAt(safeStep);
            return p1.DistanceTo(p);
        }

        public double LengthAt(Vector3d vector)
        {
            return p1.DistanceTo(vector);
        }

        public Vector3d PositionAt(double step)
        {
            var safeStep = step.Clamp(0, 1, tolerance);
            return Interpolate.Linear(p1, p2, safeStep);
        }

        public double StepAt(double distance)
        {
            if (distance <= 0) { return 0; }
            if (distance > length) { return 1; }
            return distance / length;
        }

        public double StepAt(Vector3d vector)
        {
            var distance = LengthAt(vector);
            return distance / length;
        }
    }
}
