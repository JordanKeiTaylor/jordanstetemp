using System;
namespace GeographicLib
{
    public static class Constants
    {
        /// <summary>
        /// The equatorial radius of the WGS84 ellipsoid in meters
        /// </summary>
        public static readonly double WGS84_a = 6378137;

        /// <summary>
		/// The flattening at the poles of the WGS84 ellipsoid
        /// </summary>
        public static readonly double WGS84_f = 1 / 298.257223563;

        /// <summary>
        /// The central scale factor for UTM (0.9996).
        /// </summary>
        public static readonly double UTM_k0 = 9996.0 / 10000.0;
    }
}
