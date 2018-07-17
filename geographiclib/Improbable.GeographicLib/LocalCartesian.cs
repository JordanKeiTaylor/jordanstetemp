using System;
using System.Collections.Generic;

namespace Improbable.GeographicLib
{
    /**
      * \brief Local cartesian coordinates
      *
      * Convert between geodetic coordinates latitude = \e lat, longitude = \e
      * lon, height = \e h (measured vertically from the surface of the ellipsoid)
      * to local cartesian coordinates (\e x, \e y, \e z).  The origin of local
      * cartesian coordinate system is at \e lat = \e lat0, \e lon = \e lon0, \e h
      * = \e h0. The \e z axis is normal to the ellipsoid; the \e y axis points
      * due north.  The plane \e z = - \e h0 is tangent to the ellipsoid.
      *
      * The conversions all take place via geocentric coordinates using a
      * Geocentric object (by default Geocentric::WGS84()).
      *
      * Example of use:
      * \include example-LocalCartesian.cpp
      *
      * <a href="CartConvert.1.html">CartConvert</a> is a command-line utility
      * providing access to the functionality of Geocentric and LocalCartesian.
      **********************************************************************/
    public class LocalCartesian
    {
        static long dim_ = 3;
        static long dim2_ = dim_ * dim_;
        Geocentric _earth;
        double _lat0, _lon0, _h0;
        double _x0, _y0, _z0;
        double[] _r = new double[dim2_];


        /**
         * Constructor setting the origin.
         *
         * @param[in] lat0 latitude at origin (degrees).
         * @param[in] lon0 longitude at origin (degrees).
         * @param[in] h0 height above ellipsoid at origin (meters); default 0.
         * @param[in] earth Geocentric object for the transformation; default
         *   Geocentric::WGS84().
         *
         * \e lat0 should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public LocalCartesian(double lat0, double lon0, Geocentric earth, double h0 = 0)
        {
            _earth = earth;
            Reset(lat0, lon0, h0);
        }

        public LocalCartesian(double lat0, double lon0, double h0 = 0)
          : this(lat0, lon0, Geocentric.WGS84(), h0)
        { }

        /**
         * Default ructor.
         *
         * @param[in] earth Geocentric object for the transformation; default
         *   Geocentric::WGS84().
         *
         * Sets \e lat0 = 0, \e lon0 = 0, \e h0 = 0.
         **********************************************************************/
        public LocalCartesian(Geocentric earth)
        {
            _earth = earth;
            Reset(0, 0, 0);
        }

        public LocalCartesian()
          : this(Geocentric.WGS84())
        { }

        /**
         * Reset the origin.
         *
         * @param[in] lat0 latitude at origin (degrees).
         * @param[in] lon0 longitude at origin (degrees).
         * @param[in] h0 height above ellipsoid at origin (meters); default 0.
         *
         * \e lat0 should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public void Reset(double lat0, double lon0, double h0 = 0)
        {
            _lat0 = GeoMath.LatFix(lat0);
            _lon0 = GeoMath.AngNormalize(lon0);
            _h0 = h0;
            _earth.Forward(_lat0, _lon0, _h0, out _x0, out _y0, out _z0);
            double sphi, cphi, slam, clam;
            GeoMath.Sincosd(_lat0, out sphi, out cphi);
            GeoMath.Sincosd(_lon0, out slam, out clam);
            Geocentric.Rotation(sphi, cphi, slam, clam, _r);
        }

        /**
         * Convert from geodetic to local cartesian coordinates.
         *
         * @param[in] lat latitude of point (degrees).
         * @param[in] lon longitude of point (degrees).
         * @param[in] h height of point above the ellipsoid (meters).
         * @param[out] x local cartesian coordinate (meters).
         * @param[out] y local cartesian coordinate (meters).
         * @param[out] z local cartesian coordinate (meters).
         *
         * \e lat should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public void Forward(double lat, double lon, double h, out double x, out double y, out double z)
        {
            IntForward(lat, lon, h, out x, out y, out z, null);
        }

        /**
         * Convert from geodetic to local cartesian coordinates and return rotation
         * matrix.
         *
         * @param[in] lat latitude of point (degrees).
         * @param[in] lon longitude of point (degrees).
         * @param[in] h height of point above the ellipsoid (meters).
         * @param[out] x local cartesian coordinate (meters).
         * @param[out] y local cartesian coordinate (meters).
         * @param[out] z local cartesian coordinate (meters).
         * @param[out] M if the length of the vector is 9, fill with the rotation
         *   matrix in row-major order.
         *
         * \e lat should be in the range [&minus;90&deg;, 90&deg;].
         *
         * Let \e v be a unit vector located at (\e lat, \e lon, \e h).  We can
         * express \e v as \e column vectors in one of two ways
         * - in east, north, up coordinates (where the components are relative to a
         *   local coordinate system at (\e lat, \e lon, \e h)); call this
         *   representation \e v1.
         * - in \e x, \e y, \e z coordinates (where the components are relative to
         *   the local coordinate system at (\e lat0, \e lon0, \e h0)); call this
         *   representation \e v0.
         * .
         * Then we have \e v0 = \e M &sdot; \e v1.
         **********************************************************************/
        public void Forward(double lat, double lon, double h, out double x, out double y, out double z,
                     List<double> M)
        {
            if (M.Count == dim2_)
            {
                double[] t = new double[dim2_];
                IntForward(lat, lon, h, out x, out y, out z, t);
                //        std::copy(t, t + dim2_, M.begin());
                M.Clear();
                M.AddRange(t);
            }
            else
                IntForward(lat, lon, h, out x, out y, out z, null);
        }

        /**
         * Convert from local cartesian to geodetic coordinates.
         *
         * @param[in] x local cartesian coordinate (meters).
         * @param[in] y local cartesian coordinate (meters).
         * @param[in] z local cartesian coordinate (meters).
         * @param[out] lat latitude of point (degrees).
         * @param[out] lon longitude of point (degrees).
         * @param[out] h height of point above the ellipsoid (meters).
         *
         * The value of \e lon returned is in the range [&minus;180&deg;,
         * 180&deg;].
         **********************************************************************/
        public void Reverse(double x, double y, double z, out double lat, out double lon, out double h)
        {
            IntReverse(x, y, z, out lat, out lon, out h, null);
        }

        /**
         * Convert from local cartesian to geodetic coordinates and return rotation
         * matrix.
         *
         * @param[in] x local cartesian coordinate (meters).
         * @param[in] y local cartesian coordinate (meters).
         * @param[in] z local cartesian coordinate (meters).
         * @param[out] lat latitude of point (degrees).
         * @param[out] lon longitude of point (degrees).
         * @param[out] h height of point above the ellipsoid (meters).
         * @param[out] M if the length of the vector is 9, fill with the rotation
         *   matrix in row-major order.
         *
         * Let \e v be a unit vector located at (\e lat, \e lon, \e h).  We can
         * express \e v as \e column vectors in one of two ways
         * - in east, north, up coordinates (where the components are relative to a
         *   local coordinate system at (\e lat, \e lon, \e h)); call this
         *   representation \e v1.
         * - in \e x, \e y, \e z coordinates (where the components are relative to
         *   the local coordinate system at (\e lat0, \e lon0, \e h0)); call this
         *   representation \e v0.
         * .
         * Then we have \e v1 = <i>M</i><sup>T</sup> &sdot; \e v0, where
         * <i>M</i><sup>T</sup> is the transpose of \e M.
         **********************************************************************/
        public void Reverse(double x, double y, double z, out double lat, out double lon, out double h,
                     List<double> M)
        {
            if (M.Count == dim2_)
            {
                double[] t = new double[dim2_];
                IntReverse(x, y, z, out lat, out lon, out h, t);
                //        std::copy(t, t + dim2_, M.begin());
                M.Clear();
                M.AddRange(t);
            }
            else
                IntReverse(x, y, z, out lat, out lon, out h, null);
        }

        /** \name Inspector functions
         **********************************************************************/
        ///@{
        /**
         * @return latitude of the origin (degrees).
         **********************************************************************/
        public double LatitudeOrigin() { return _lat0; }

        /**
         * @return longitude of the origin (degrees).
         **********************************************************************/
        public double LongitudeOrigin() { return _lon0; }

        /**
         * @return height of the origin (meters).
         **********************************************************************/
        public double HeightOrigin() { return _h0; }

        /**
         * @return \e a the equatorial radius of the ellipsoid (meters).  This is
         *   the value of \e a inherited from the Geocentric object used in the
         *   ructor.
         **********************************************************************/
        public double MajorRadius() { return _earth.MajorRadius(); }

        /**
         * @return \e f the flattening of the ellipsoid.  This is the value
         *   inherited from the Geocentric object used in the ructor.
         **********************************************************************/
        public double Flattening() { return _earth.Flattening(); }


        void IntForward(double lat, double lon, double h, out double x, out double y, out double z,
         double[] M)
        {
            double xc, yc, zc;
            _earth.IntForward(lat, lon, h, out xc, out yc, out zc, M);
            xc -= _x0; yc -= _y0; zc -= _z0;
            x = _r[0] * xc + _r[3] * yc + _r[6] * zc;
            y = _r[1] * xc + _r[4] * yc + _r[7] * zc;
            z = _r[2] * xc + _r[5] * yc + _r[8] * zc;
            if (M != null)
                MatrixMultiply(M);
        }

        void IntReverse(double x, double y, double z, out double lat, out double lon, out double h,
         double[] M)
        {
            double
                xc = _x0 + _r[0] * x + _r[1] * y + _r[2] * z,
                yc = _y0 + _r[3] * x + _r[4] * y + _r[5] * z,
                zc = _z0 + _r[6] * x + _r[7] * y + _r[8] * z;
            _earth.IntReverse(xc, yc, zc, out lat, out lon, out h, M);
            if (M != null)
                MatrixMultiply(M);
        }

        void MatrixMultiply(double[] M)
        {
            double[] t = new double[dim2_];
            Array.Copy(M, t, dim2_);
            for (long i = 0; i < dim2_; ++i)
            {
                long row = i / dim_, col = i % dim_;
                M[i] = _r[row] * t[col] + _r[row + 3] * t[col + 3] + _r[row + 6] * t[col + 6];
            }
        }
    }
}
