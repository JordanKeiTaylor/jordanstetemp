using System;
namespace GeographicLib
{
    /**
   * \brief Azimuthal equidistant projection
   *
   * Azimuthal equidistant projection centered at an arbitrary position on the
   * ellipsoid.  For a point in projected space (\e x, \e y), the geodesic
   * distance from the center position is hypot(\e x, \e y) and the azimuth of
   * the geodesic from the center point is atan2(\e x, \e y).  The Forward and
   * Reverse methods also return the azimuth \e azi of the geodesic at (\e x,
   * \e y) and reciprocal scale \e rk in the azimuthal direction which,
   * together with the basic properties of the projection, serve to specify
   * completely the local affine transformation between geographic and
   * projected coordinates.
   *
   * The conversions all take place using a Geodesic object (by default
   * Geodesic::WGS84()).  For more information on geodesics see \ref geodesic.
   *
   * Example of use:
   * \include example-AzimuthalEquidistant.cpp
   *
   * <a href="GeodesicProj.1.html">GeodesicProj</a> is a command-line utility
   * providing access to the functionality of AzimuthalEquidistant, Gnomonic,
   * and CassiniSoldner.
   **********************************************************************/
    public class AzimuthalEquidistant
    {
        private Geodesic _earth;
        private double eps_ = 0.01 * GeoMath.Min;

        /**
     * Constructor for AzimuthalEquidistant.
     *
     * @param[in] earth the Geodesic object to use for geodesic calculations.
     *   By default this uses the WGS84 ellipsoid.
     **********************************************************************/
        public AzimuthalEquidistant()
        : this(Geodesic.WGS84()) { }

        public AzimuthalEquidistant(Geodesic earth)
        {
            _earth = earth;
        }

        /**
         * Forward projection, from geographic to azimuthal equidistant.
         *
         * @param[in] lat0 latitude of center point of projection (degrees).
         * @param[in] lon0 longitude of center point of projection (degrees).
         * @param[in] lat latitude of point (degrees).
         * @param[in] lon longitude of point (degrees).
         * @param[out] x easting of point (meters).
         * @param[out] y northing of point (meters).
         * @param[out] azi azimuth of geodesic at point (degrees).
         * @param[out] rk reciprocal of azimuthal scale at point.
         *
         * \e lat0 and \e lat should be in the range [&minus;90&deg;, 90&deg;].
         * The scale of the projection is 1 in the "radial" direction, \e azi
         * clockwise from true north, and is 1/\e rk in the direction perpendicular
         * to this.  A call to Forward followed by a call to Reverse will return
         * the original (\e lat, \e lon) (to within roundoff).
         **********************************************************************/
        public void Forward(double lat0, double lon0, double lat, double lon,
                            out double x, out double y, out double azi, out double rk)
        {
            double sig, s, azi0, m;
            sig = _earth.Inverse(lat0, lon0, lat, lon, out s, out azi0, out azi, out m);
            GeoMath.Sincosd(azi0, out x, out y);
            x *= s; y *= s;
            rk = !(sig <= eps_) ? m / s : 1;
        }

        /**
         * Reverse projection, from azimuthal equidistant to geographic.
         *
         * @param[in] lat0 latitude of center point of projection (degrees).
         * @param[in] lon0 longitude of center point of projection (degrees).
         * @param[in] x easting of point (meters).
         * @param[in] y northing of point (meters).
         * @param[out] lat latitude of point (degrees).
         * @param[out] lon longitude of point (degrees).
         * @param[out] azi azimuth of geodesic at point (degrees).
         * @param[out] rk reciprocal of azimuthal scale at point.
         *
         * \e lat0 should be in the range [&minus;90&deg;, 90&deg;].  \e lat will
         * be in the range [&minus;90&deg;, 90&deg;] and \e lon will be in the
         * range [&minus;180&deg;, 180&deg;].  The scale of the projection is 1 in
         * the "radial" direction, \e azi clockwise from true north, and is 1/\e rk
         * in the direction perpendicular to this.  A call to Reverse followed by a
         * call to Forward will return the original (\e x, \e y) (to roundoff) only
         * if the geodesic to (\e x, \e y) is a shortest path.
         **********************************************************************/
        public void Reverse(double lat0, double lon0, double x, double y,
                     out double lat, out double lon, out double azi, out double rk)
        {
            double azi0 = GeoMath.Atan2d(x, y);
            double s = GeoMath.Hypot(x, y);
            double sig, m;
            sig = _earth.Direct(lat0, lon0, azi0, s, out lat, out lon, out azi, out m);
            rk = !(sig <= eps_) ? m / s : 1;
        }

        /**
         * AzimuthalEquidistant::Forward without returning the azimuth and scale.
         **********************************************************************/
        public void Forward(double lat0, double lon0, double lat, double lon,
                     out double x, out double y)
        {
            double azi, rk;
            Forward(lat0, lon0, lat, lon, out x, out y, out azi, out rk);
        }

        /**
         * AzimuthalEquidistant::Reverse without returning the azimuth and scale.
         **********************************************************************/
        public void Reverse(double lat0, double lon0, double x, double y,
                     out double lat, out double lon)
        {
            double azi, rk;
            Reverse(lat0, lon0, x, y, out lat, out lon, out azi, out rk);
        }

        /** \name Inspector functions
         **********************************************************************/
        ///@{
        /**
         * @return \e a the equatorial radius of the ellipsoid (meters).  This is
         *   the value inherited from the Geodesic object used in the constructor.
         **********************************************************************/
        public double MajorRadius()
        {
            return _earth.MajorRadius();
        }

        /**
         * @return \e f the flattening of the ellipsoid.  This is the value
         *   inherited from the Geodesic object used in the constructor.
         **********************************************************************/
        public double Flattening()
        {
            return _earth.Flattening();
        }
        ///@}
    }
}