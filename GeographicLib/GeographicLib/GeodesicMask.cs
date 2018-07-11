using System;

namespace GeographicLib
{
    public static class GeodesicMask
    {
        internal const int CAP_NONE = 0;
		internal const int CAP_C1 = 1 << 0;
		internal const int CAP_C1p = 1 << 1;
		internal const int CAP_C2 = 1 << 2;
		internal const int CAP_C3 = 1 << 3;
		internal const int CAP_C4 = 1 << 4;
		internal const int CAP_ALL = 0x1F;
		internal const int CAP_MASK = CAP_ALL;
        internal const int OUT_ALL = 0x7F80;
        internal const int OUT_MASK = 0xFF80; // Includes LONG_UNROLL

        /// <summary>
		/// No capabilities, no output.
        /// </summary>
        public const int NONE = 0;
        
        /// <summary>
		/// Calculate latitude lat2.  (It's not necessary to include this as a
        /// capability to GeodesicLine because this is included by default.)
        /// </summary>
        public const int LATITUDE = 1 << 7 | CAP_NONE;
        
        /// <summary>
		/// Calculate longitude lon2.
        /// </summary>
		public const int LONGITUDE = 1 << 8 | CAP_C3;
      
		/// <summary>
		/// Calculate azimuths azi1 and azi2.  (It's not necessary to include this as a
        /// capability to GeodesicLine because this is included by default.)
        /// </summary>
		public const int AZIMUTH = 1 << 9 | CAP_NONE;

        /// <summary>
		/// Calculate distance s12.
        /// </summary>
		public const int DISTANCE = 1 << 10 | CAP_C1;
       
        /// <summary>
		/// Allow distance s12 to be used as input in the direct geodesic
        /// problem.
        /// </summary>
		public const int DISTANCE_IN = 1 << 11 | CAP_C1 | CAP_C1p;
        
        /// <summary>
		/// Calculate reduced length m12.
        /// </summary>
		public const int REDUCEDLENGTH = 1 << 12 | CAP_C1 | CAP_C2;
        
        /// <summary>
		/// Calculate geodesic scales M12 and M21.
        /// </summary>
		public const int GEODESICSCALE = 1 << 13 | CAP_C1 | CAP_C2;
        
        /// <summary>
		/// Calculate area S12.
        /// </summary>
		public const int AREA = 1 << 14 | CAP_C4;
        
        /// <summary>
		/// Unroll lon2 in the direct calculation.
        /// </summary>
        public const int LONG_UNROLL = 1 << 15;

        /// <summary>
		/// All capabilities, calculate everything.  (LONG_UNROLL is not
        /// included in this mask.)
        /// </summary>
		public const int ALL = OUT_ALL | CAP_ALL;
    }
}
