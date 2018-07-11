using System;
namespace GeographicLib
{
    /**
   * \brief Cassini-Soldner projection
   *
   * Cassini-Soldner projection centered at an arbitrary position, \e lat0, \e
   * lon0, on the ellipsoid.  This projection is a transverse cylindrical
   * equidistant projection.  The projection from (\e lat, \e lon) to easting
   * and northing (\e x, \e y) is defined by geodesics as follows.  Go north
   * along a geodesic a distance \e y from the central point; then turn
   * clockwise 90&deg; and go a distance \e x along a geodesic.
   * (Although the initial heading is north, this changes to south if the pole
   * is crossed.)  This procedure uniquely defines the reverse projection.  The
   * forward projection is ructed as follows.  Find the point (\e lat1, \e
   * lon1) on the meridian closest to (\e lat, \e lon).  Here we consider the
   * full meridian so that \e lon1 may be either \e lon0 or \e lon0 +
   * 180&deg;.  \e x is the geodesic distance from (\e lat1, \e lon1) to
   * (\e lat, \e lon), appropriately signed according to which side of the
   * central meridian (\e lat, \e lon) lies.  \e y is the shortest distance
   * along the meridian from (\e lat0, \e lon0) to (\e lat1, \e lon1), again,
   * appropriately signed according to the initial heading.  [Note that, in the
   * case of prolate ellipsoids, the shortest meridional path from (\e lat0, \e
   * lon0) to (\e lat1, \e lon1) may not be the shortest path.]  This procedure
   * uniquely defines the forward projection except for a small class of points
   * for which there may be two equally short routes for either leg of the
   * path.
   *
   * Because of the properties of geodesics, the (\e x, \e y) grid is
   * orthogonal.  The scale in the easting direction is unity.  The scale, \e
   * k, in the northing direction is unity on the central meridian and
   * increases away from the central meridian.  The projection routines return
   * \e azi, the true bearing of the easting direction, and \e rk = 1/\e k, the
   * reciprocal of the scale in the northing direction.
   *
   * The conversions all take place using a Geodesic object (by default
   * GeodesicMask.WGS84()).  For more information on geodesics see \ref geodesic.
   * The determination of (\e lat1, \e lon1) in the forward projection is by
   * solving the inverse geodesic problem for (\e lat, \e lon) and its twin
   * obtained by reflection in the meridional plane.  The scale is found by
   * determining where two neighboring geodesics intersecting the central
   * meridian at \e lat1 and \e lat1 + \e dlat1 intersect and taking the ratio
   * of the reduced lengths for the two geodesics between that point and,
   * respectively, (\e lat1, \e lon1) and (\e lat, \e lon).
   *
   * Example of use:
   * \include example-CassiniSoldner.cpp
   *
   * <a href="GeodesicProj.1.html">GeodesicProj</a> is a command-line utility
   * providing access to the functionality of AzimuthalEquidistant, Gnomonic,
   * and CassiniSoldner.
   **********************************************************************/
    public class CassiniSoldner
    {
        Geodesic _earth;
        GeodesicLine _meridian;
        double _sbet0, _cbet0;
        static int maxit_ = 10;

        /**
         * Constructor for CassiniSoldner.
         *
         * @param[in] earth the Geodesic object to use for geodesic calculations.
         *   By default this uses the WGS84 ellipsoid.
         *
         * This ructor makes an "uninitialized" object.  Call Reset to set the
         * central latitude and longitude, prior to calling Forward and Reverse.
         **********************************************************************/
        public CassiniSoldner()
            : this(Geodesic.WGS84()) { }
        public CassiniSoldner(Geodesic earth)
        {
            _earth = earth;
        }

        /**
         * Constructor for CassiniSoldner specifying a center point.
         *
         * @param[in] lat0 latitude of center point of projection (degrees).
         * @param[in] lon0 longitude of center point of projection (degrees).
         * @param[in] earth the Geodesic object to use for geodesic calculations.
         *   By default this uses the WGS84 ellipsoid.
         *
         * \e lat0 should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public CassiniSoldner(double lat0, double lon0)
            : this(lat0, lon0, Geodesic.WGS84()) { }
        public CassiniSoldner(double lat0, double lon0, Geodesic earth)
        {
            _earth = earth;
            Reset(lat0, lon0);
        }

        /**
         * Set the central point of the projection
         *
         * @param[in] lat0 latitude of center point of projection (degrees).
         * @param[in] lon0 longitude of center point of projection (degrees).
         *
         * \e lat0 should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public void Reset(double lat0, double lon0)
        {
            _meridian = _earth.Line(lat0, lon0, 0,
                                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE |
                                        GeodesicMask.DISTANCE | GeodesicMask.DISTANCE_IN |
                                        GeodesicMask.AZIMUTH);
            double f = _earth.Flattening();
            GeoMath.Sincosd(LatitudeOrigin(), out _sbet0, out _cbet0);
            _sbet0 *= (1 - f);
            GeoMath.Norm(ref _sbet0, ref _cbet0);
        }

        /**
         * Forward projection, from geographic to Cassini-Soldner.
         *
         * @param[in] lat latitude of point (degrees).
         * @param[in] lon longitude of point (degrees).
         * @param[out] x easting of point (meters).
         * @param[out] y northing of point (meters).
         * @param[out] azi azimuth of easting direction at point (degrees).
         * @param[out] rk reciprocal of azimuthal northing scale at point.
         *
         * \e lat should be in the range [&minus;90&deg;, 90&deg;].  A call to
         * Forward followed by a call to Reverse will return the original (\e lat,
         * \e lon) (to within roundoff).  The routine does nothing if the origin
         * has not been set.
         **********************************************************************/
        public void Forward(double lat, double lon,
                     out double x, out double y, out double azi, out double rk)
        {
            if (!Init())
            {
                x = double.NaN;
                y = double.NaN;
                azi = double.NaN;
                rk = double.NaN;
            }

            double e;
            double dlon = GeoMath.AngDiff(LongitudeOrigin(), lon, out e);
            double sig12, s12, azi1, azi2;
            sig12 = _earth.Inverse(lat, -Math.Abs(dlon), lat, Math.Abs(dlon), out s12, out azi1, out azi2);
            sig12 *= 0.5;
            s12 *= 0.5;
            if (s12 == 0)
            {
                double da = GeoMath.AngDiff(azi1, azi2, out e) / 2;
                if (Math.Abs(dlon) <= 90)
                {
                    azi1 = 90 - da;
                    azi2 = 90 + da;
                }
                else
                {
                    azi1 = -90 - da;
                    azi2 = -90 + da;
                }
            }
            if (dlon < 0)
            {
                azi2 = azi1;
                s12 = -s12;
                sig12 = -sig12;
            }
            x = s12;
            azi = GeoMath.AngNormalize(azi2);
            GeodesicLine perp = _earth.Line(lat, dlon, azi, GeodesicMask.GEODESICSCALE);
            double t;
            perp.GenPosition(true, -sig12,
                                 GeodesicMask.GEODESICSCALE,
                                 out t, out t, out t, out t, out t, out t, out rk, out t);

            double salp0, calp0;
            GeoMath.Sincosd(perp.EquatorialAzimuth(), out salp0, out calp0);
            double
              sbet1 = lat >= 0 ? calp0 : -calp0,
              cbet1 = Math.Abs(dlon) <= 90 ? Math.Abs(salp0) : -Math.Abs(salp0),
              sbet01 = sbet1 * _cbet0 - cbet1 * _sbet0,
              cbet01 = cbet1 * _cbet0 + sbet1 * _sbet0,
              sig01 = Math.Atan2(sbet01, cbet01) / GeoMath.Degree;
            _meridian.GenPosition(true, sig01,
                                      GeodesicMask.DISTANCE,
                                      out t, out t, out t, out y, out t, out t, out t, out t);
        }

        /**
         * Reverse projection, from Cassini-Soldner to geographic.
         *
         * @param[in] x easting of point (meters).
         * @param[in] y northing of point (meters).
         * @param[out] lat latitude of point (degrees).
         * @param[out] lon longitude of point (degrees).
         * @param[out] azi azimuth of easting direction at point (degrees).
         * @param[out] rk reciprocal of azimuthal northing scale at point.
         *
         * A call to Reverse followed by a call to Forward will return the original
         * (\e x, \e y) (to within roundoff), provided that \e x and \e y are
         * sufficiently small not to "wrap around" the earth.  The routine does
         * nothing if the origin has not been set.
         **********************************************************************/
        public void Reverse(double x, double y,
                     out double lat, out double lon, out double azi, out double rk)
        {
            if (!Init())
            {
                lat = double.NaN;
                lon = double.NaN;
                azi = double.NaN;
                rk = double.NaN;
                return;
            }
            double lat1, lon1;
            double azi0, t;
            _meridian.Position(y, out lat1, out lon1, out azi0);
            _earth.Direct(lat1, lon1, azi0 + 90, x, out lat, out lon, out azi, out rk, out t);
        }

        /**
         * CassiniSoldner::Forward without returning the azimuth and scale.
         **********************************************************************/
        public void Forward(double lat, double lon,
                     out double x, out double y)
        {
            double azi, rk;
            Forward(lat, lon, out x, out y, out azi, out rk);
        }

        /**
         * CassiniSoldner::Reverse without returning the azimuth and scale.
         **********************************************************************/
        public void Reverse(double x, double y,
                     out double lat, out double lon)
        {
            double azi, rk;
            Reverse(x, y, out lat, out lon, out azi, out rk);
        }

        /** \name Inspector functions
         **********************************************************************/
        ///@{
        /**
         * @return true if the object has been initialized.
         **********************************************************************/
        public bool Init()
        {
            return _meridian.Init();
        }

        /**
         * @return \e lat0 the latitude of origin (degrees).
         **********************************************************************/
        public double LatitudeOrigin()
        {
            return _meridian.Latitude();
        }

        /**
         * @return \e lon0 the longitude of origin (degrees).
         **********************************************************************/
        public double LongitudeOrigin()
        {
            return _meridian.Longitude();
        }

        /**
         * @return \e a the equatorial radius of the ellipsoid (meters).  This is
         *   the value inherited from the Geodesic object used in the ructor.
         **********************************************************************/
        public double MajorRadius()
        {
            return _earth.MajorRadius();
        }

        /**
         * @return \e f the flattening of the ellipsoid.  This is the value
         *   inherited from the Geodesic object used in the ructor.
         **********************************************************************/
        public double Flattening()
        {
            return _earth.Flattening();
        }
        ///@}

    }
}
