namespace Improbable.GeographicLib
{
	internal static class Utility
    {
		public static void Swap(ref double x, ref double y)
        {
            double t = x;
            x = y;
            y = t;
        }
    }
}
