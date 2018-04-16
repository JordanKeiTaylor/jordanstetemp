using Improbable;

namespace Shared.Projections
{
    public static class CoordinateOffset
    {
        public static Vector3d ApplyOffset(double x, double y, double z, double xOffset, double zOffset)
        {
            return new Vector3d(
                x + xOffset,
                y,
                z + zOffset
            ); 
        }

        public static void BackoutOffset(out double adjustedX, out double adjustedZ,
            double x, double z,
            double xOffset, double zOffset)
        {
            adjustedX = x - xOffset;
            adjustedZ = z - zOffset;
        }
    }
}