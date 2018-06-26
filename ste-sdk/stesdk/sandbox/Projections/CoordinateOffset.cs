namespace Improbable.sandbox.Projections
{
    public static class CoordinateOffset
    {
        public static Vector3d ApplyOffset(double x, double y, double z, double offsetX, double offsetZ)
        {
            return new Vector3d(
                x + offsetX,
                y,
                z + offsetZ);
        }

        public static void BackoutOffset(
            out double adjustedX,
            out double adjustedZ,
            double x,
            double z,
            double offsetX,
            double offsetZ)
        {
            adjustedX = x - offsetX;
            adjustedZ = z - offsetZ;
        }
    }
}