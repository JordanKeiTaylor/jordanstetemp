namespace stesdk.sandbox.Projections
{
    public static class CoordinateOffset
    {
        public static Coordinate ApplyOffset(double x, double y, double z, double offsetX, double offsetZ)
        {
            return new Coordinate(
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