using System;

namespace Improbable.GeographicLib
{
    /**
    * \brief Albers equal area conic projection
    *
    * Implementation taken from the report,
    * - J. P. Snyder,
    *   <a href="http://pubs.er.usgs.gov/usgspubs/pp/pp1395"> Map Projections: A
    *   Working Manual</a>, USGS Professional Paper 1395 (1987),
    *   pp. 101--102.
    *
    * This is a implementation of the equations in Snyder except that divided
    * differences will be [have been] used to transform the expressions into
    * ones which may be evaluated accurately.  [In this implementation, the
    * projection correctly becomes the cylindrical equal area or the azimuthal
    * equal area projection when the standard latitude is the equator or a
    * pole.]
    *
    * The ellipsoid parameters, the standard parallels, and the scale on the
    * standard parallels are set in the ructor.  Internally, the case with
    * two standard parallels is converted into a single standard parallel, the
    * latitude of minimum azimuthal scale, with an azimuthal scale specified on
    * this parallel.  This latitude is also used as the latitude of origin which
    * is returned by AlbersEqualArea::OriginLatitude.  The azimuthal scale on
    * the latitude of origin is given by AlbersEqualArea::CentralScale.  The
    * case with two standard parallels at opposite poles is singular and is
    * disallowed.  The central meridian (which is a trivial shift of the
    * longitude) is specified as the \e lon0 argument of the
    * AlbersEqualArea::Forward and AlbersEqualArea::Reverse functions.
    * AlbersEqualArea::Forward and AlbersEqualArea::Reverse also return the
    * meridian convergence, &gamma;, and azimuthal scale, \e k.  A small square
    * aligned with the cardinal directions is projected to a rectangle with
    * dimensions \e k (in the E-W direction) and 1/\e k (in the N-S direction).
    * The E-W sides of the rectangle are oriented &gamma; degrees
    * counter-clockwise from the \e x axis.  There is no provision in this class
    * for specifying a false easting or false northing or a different latitude
    * of origin.
    *
    * Example of use:
    * \include example-AlbersEqualArea.cpp
    *
    * <a href="ConicProj.1.html">ConicProj</a> is a command-line utility
    * providing access to the functionality of LambertConformalConic and
    * AlbersEqualArea.
    **********************************************************************/
    public class AlbersEqualArea
    {
        double eps_, epsx_, epsx2_, tol_, tol0_;
        double _a, _f, _fm, _e2, _e, _e2m, _qZ, _qx;
        double _sign, _lat0, _k0;
        double _n0, _m02, _nrho0, _k2, _txi0, _scxi0, _sxi0;
        static int numit_ = 5;   // Newton iterations in Reverse
        static int numit0_ = 20; // Newton iterations in Init

        /**
         * ructor with a single standard parallel.
         *
         * @param[in] a equatorial radius of ellipsoid (meters).
         * @param[in] f flattening of ellipsoid.  Setting \e f = 0 gives a sphere.
         *   Negative \e f gives a prolate ellipsoid.
         * @param[in] stdlat standard parallel (degrees), the circle of tangency.
         * @param[in] k0 azimuthal scale on the standard parallel.
         * @exception new GeographicException if \e a, (1 &minus; \e f) \e a, or \e k0 is
         *   not positive.
         * @exception new GeographicException if \e stdlat is not in [&minus;90&deg;,
         *   90&deg;].
         **********************************************************************/
        public AlbersEqualArea(double a, double f, double stdlat, double k0)
        {
            InitVars(a, f);

            if (!(GeoMath.IsFinite(_a) && _a > 0))
                throw new GeographicException("Equatorial radius is not positive");
            if (!(GeoMath.IsFinite(_f) && _f < 1))
                throw new GeographicException("Polar semi-axis is not positive");
            if (!(GeoMath.IsFinite(k0) && k0 > 0))
                throw new GeographicException("Scale is not positive");
            if (!(Math.Abs(stdlat) <= 90))
                throw new GeographicException("Standard latitude not in [-90d, 90d]");
            double sphi, cphi;
            GeoMath.Sincosd(stdlat, out sphi, out cphi);
            Init(sphi, cphi, sphi, cphi, k0);
        }

        /**
         * ructor with two standard parallels.
         *
         * @param[in] a equatorial radius of ellipsoid (meters).
         * @param[in] f flattening of ellipsoid.  Setting \e f = 0 gives a sphere.
         *   Negative \e f gives a prolate ellipsoid.
         * @param[in] stdlat1 first standard parallel (degrees).
         * @param[in] stdlat2 second standard parallel (degrees).
         * @param[in] k1 azimuthal scale on the standard parallels.
         * @exception new GeographicException if \e a, (1 &minus; \e f) \e a, or \e k1 is
         *   not positive.
         * @exception new GeographicException if \e stdlat1 or \e stdlat2 is not in
         *   [&minus;90&deg;, 90&deg;], or if \e stdlat1 and \e stdlat2 are
         *   opposite poles.
         **********************************************************************/
        public AlbersEqualArea(double a, double f, double stdlat1, double stdlat2, double k1)
        {
            InitVars(a, f);

            eps_ = GeoMath.Epsilon;
            epsx_ = GeoMath.Square(eps_);
            epsx2_ = GeoMath.Square(epsx_);
            tol_ = GeoMath.Square(eps_);
            tol0_ = (tol_ * GeoMath.Square(GeoMath.Square(eps_)));
            _a = a;
            _f = f;
            _fm = 1 - _f;
            _e2 = _f * (2 - _f);
            _e = GeoMath.Square(Math.Abs(_e2));
            _e2m = 1 - _e2;
            _qZ = 1 + _e2m * atanhee(1);
            _qx = _qZ / (2 * _e2m);

            if (!(GeoMath.IsFinite(_a) && _a > 0))
                throw new GeographicException("Equatorial radius is not positive");
            if (!(GeoMath.IsFinite(_f) && _f < 1))
                throw new GeographicException("Polar semi-axis is not positive");
            if (!(GeoMath.IsFinite(k1) && k1 > 0))
                throw new GeographicException("Scale is not positive");
            if (!(Math.Abs(stdlat1) <= 90))
                throw new GeographicException("Standard latitude 1 not in [-90d, 90d]");
            if (!(Math.Abs(stdlat2) <= 90))
                throw new GeographicException("Standard latitude 2 not in [-90d, 90d]");
            double sphi1, cphi1, sphi2, cphi2;
            GeoMath.Sincosd(stdlat1, out sphi1, out cphi1);
            GeoMath.Sincosd(stdlat2, out sphi2, out cphi2);
            Init(sphi1, cphi1, sphi2, cphi2, k1);
        }

        /**
         * ructor with two standard parallels specified by sines and cosines.
         *
         * @param[in] a equatorial radius of ellipsoid (meters).
         * @param[in] f flattening of ellipsoid.  Setting \e f = 0 gives a sphere.
         *   Negative \e f gives a prolate ellipsoid.
         * @param[in] sinlat1 sine of first standard parallel.
         * @param[in] coslat1 cosine of first standard parallel.
         * @param[in] sinlat2 sine of second standard parallel.
         * @param[in] coslat2 cosine of second standard parallel.
         * @param[in] k1 azimuthal scale on the standard parallels.
         * @exception new GeographicException if \e a, (1 &minus; \e f) \e a, or \e k1 is
         *   not positive.
         * @exception new GeographicException if \e stdlat1 or \e stdlat2 is not in
         *   [&minus;90&deg;, 90&deg;], or if \e stdlat1 and \e stdlat2 are
         *   opposite poles.
         *
         * This allows parallels close to the poles to be specified accurately.
         * This routine computes the latitude of origin and the azimuthal scale at
         * this latitude.  If \e dlat = abs(\e lat2 &minus; \e lat1) &le; 160&deg;,
         * then the error in the latitude of origin is less than 4.5 &times;
         * 10<sup>&minus;14</sup>d;.
         **********************************************************************/
        public AlbersEqualArea(double a, double f,
                        double sinlat1, double coslat1,
                        double sinlat2, double coslat2,
                        double k1)
        {
            InitVars(a, f);

            if (!(GeoMath.IsFinite(_a) && _a > 0))
                throw new GeographicException("Equatorial radius is not positive");
            if (!(GeoMath.IsFinite(_f) && _f < 1))
                throw new GeographicException("Polar semi-axis is not positive");
            if (!(GeoMath.IsFinite(k1) && k1 > 0))
                throw new GeographicException("Scale is not positive");
            if (!(coslat1 >= 0))
                throw new GeographicException("Standard latitude 1 not in [-90d, 90d]");
            if (!(coslat2 >= 0))
                throw new GeographicException("Standard latitude 2 not in [-90d, 90d]");
            if (!(Math.Abs(sinlat1) <= 1 && coslat1 <= 1) || (coslat1 == 0 && sinlat1 == 0))
                throw new GeographicException("Bad sine/cosine of standard latitude 1");
            if (!(Math.Abs(sinlat2) <= 1 && coslat2 <= 1) || (coslat2 == 0 && sinlat2 == 0))
                throw new GeographicException("Bad sine/cosine of standard latitude 2");
            if (coslat1 == 0 && coslat2 == 0 && sinlat1 * sinlat2 <= 0)
                throw new GeographicException
                  ("Standard latitudes cannot be opposite poles");
            Init(sinlat1, coslat1, sinlat2, coslat2, k1);
        }

        /**
         * Set the azimuthal scale for the projection.
         *
         * @param[in] lat (degrees).
         * @param[in] k azimuthal scale at latitude \e lat (default 1).
         * @exception new GeographicException \e k is not positive.
         * @exception new GeographicException if \e lat is not in (&minus;90&deg;,
         *   90&deg;).
         *
         * This allows a "latitude of conformality" to be specified.
         **********************************************************************/
        public void SetScale(double lat, double k = 1)
        {
            if (!(GeoMath.IsFinite(k) && k > 0))
                throw new GeographicException("Scale is not positive");
            if (!(Math.Abs(lat) < 90))
                throw new GeographicException("Latitude for SetScale not in (-90d, 90d)");
            double x, y, gamma, kold;
            Forward(0, lat, 0, out x, out y, out gamma, out kold);
            k /= kold;
            _k0 *= k;
            _k2 = GeoMath.Square(_k0);
        }

        /**
         * Forward projection, from geographic to Lambert conformal conic.
         *
         * @param[in] lon0 central meridian longitude (degrees).
         * @param[in] lat latitude of point (degrees).
         * @param[in] lon longitude of point (degrees).
         * @param[out] x easting of point (meters).
         * @param[out] y northing of point (meters).
         * @param[out] gamma meridian convergence at point (degrees).
         * @param[out] k azimuthal scale of projection at point; the radial
         *   scale is the 1/\e k.
         *
         * The latitude origin is given by AlbersEqualArea::LatitudeOrigin().  No
         * false easting or northing is added and \e lat should be in the range
         * [&minus;90&deg;, 90&deg;].  The values of \e x and \e y returned for
         * points which project to infinity (i.e., one or both of the poles) will
         * be large but finite.
         **********************************************************************/
        public void Forward(double lon0, double lat, double lon,
                     out double x, out double y, out double gamma, out double k)
        {
            double e;
            lon = GeoMath.AngDiff(lon0, lon, out e);
            lat *= _sign;
            double sphi, cphi;
            GeoMath.Sincosd(GeoMath.LatFix(lat) * _sign, out sphi, out cphi);
            cphi = Math.Max(epsx_, cphi);
            double
              lam = lon * GeoMath.Degree,
              tphi = sphi / cphi, txi = txif(tphi), sxi = txi / hyp(txi),
              dq = _qZ * Dsn(txi, _txi0, sxi, _sxi0) * (txi - _txi0),
              drho = -_a * dq / (Math.Sqrt(_m02 - _n0 * dq) + _nrho0 / _a),
              theta = _k2 * _n0 * lam, stheta = Math.Sin(theta), ctheta = Math.Cos(theta),
              t = _nrho0 + _n0 * drho;
            x = t * (_n0 != 0 ? stheta / _n0 : _k2 * lam) / _k0;
            y = (_nrho0 *
                 (_n0 != 0 ?
                  (ctheta < 0 ? 1 - ctheta : GeoMath.Square(stheta) / (1 + ctheta)) / _n0 :
                  0)
                 - drho * ctheta) / _k0;
            k = _k0 * (t != 0 ? t * hyp(_fm * tphi) / _a : 1);
            y *= _sign;
            gamma = _sign * theta / GeoMath.Degree;
        }

        /**
         * Reverse projection, from Lambert conformal conic to geographic.
         *
         * @param[in] lon0 central meridian longitude (degrees).
         * @param[in] x easting of point (meters).
         * @param[in] y northing of point (meters).
         * @param[out] lat latitude of point (degrees).
         * @param[out] lon longitude of point (degrees).
         * @param[out] gamma meridian convergence at point (degrees).
         * @param[out] k azimuthal scale of projection at point; the radial
         *   scale is the 1/\e k.
         *
         * The latitude origin is given by AlbersEqualArea::LatitudeOrigin().  No
         * false easting or northing is added.  The value of \e lon returned is in
         * the range [&minus;180&deg;, 180&deg;].  The value of \e lat returned is
         * in the range [&minus;90&deg;, 90&deg;].  If the input point is outside
         * the legal projected space the nearest pole is returned.
         **********************************************************************/
        public void Reverse(double lon0, double x, double y,
                     out double lat, out double lon, out double gamma, out double k)
        {
            y *= _sign;
            double
              nx = _k0 * _n0 * x, ny = _k0 * _n0 * y, y1 = _nrho0 - ny,
              den = GeoMath.Hypot(nx, y1) + _nrho0, // 0 implies origin with polar aspect
              drho = den != 0 ? (_k0 * x * nx - 2 * _k0 * y * _nrho0 + _k0 * y * ny) / den : 0,
              // dsxia = scxi0 * dsxi
              dsxia = -_scxi0 * (2 * _nrho0 + _n0 * drho) * drho /
                      (GeoMath.Square(_a) * _qZ),
              txi = (_txi0 + dsxia) / Math.Sqrt(Math.Max(1 - dsxia * (2 * _txi0 + dsxia), epsx2_)),
              tphi = tphif(txi),
              theta = Math.Atan2(nx, y1),
              lam = _n0 != 0 ? theta / (_k2 * _n0) : x / (y1 * _k0);
            gamma = _sign * theta / GeoMath.Degree;
            lat = GeoMath.Atand(_sign * tphi);
            lon = lam / GeoMath.Degree;
            lon = GeoMath.AngNormalize(lon + GeoMath.AngNormalize(lon0));
            k = _k0 * (den != 0 ? (_nrho0 + _n0 * drho) * hyp(_fm * tphi) / _a : 1);
        }

        /**
         * AlbersEqualArea::Forward without returning the convergence and
         * scale.
         **********************************************************************/
        public void Forward(double lon0, double lat, double lon,
                     out double x, out double y)
        {
            double gamma, k;
            Forward(lon0, lat, lon, out x, out y, out gamma, out k);
        }

        /**
         * AlbersEqualArea::Reverse without returning the convergence and
         * scale.
         **********************************************************************/
        public void Reverse(double lon0, double x, double y,
                     out double lat, out double lon)
        {
            double gamma, k;
            Reverse(lon0, x, y, out lat, out lon, out gamma, out k);
        }

        /** \name Inspector functions
         **********************************************************************/
        ///@{
        /**
         * @return \e a the equatorial radius of the ellipsoid (meters).  This is
         *   the value used in the ructor.
         **********************************************************************/
        public double MajorRadius()
        {
            return _a;
        }

        /**
         * @return \e f the flattening of the ellipsoid.  This is the value used in
         *   the ructor.
         **********************************************************************/
        public double Flattening()
        {
            return _f;
        }

        /**
         * @return latitude of the origin for the projection (degrees).
         *
         * This is the latitude of minimum azimuthal scale and equals the \e stdlat
         * in the 1-parallel ructor and lies between \e stdlat1 and \e stdlat2
         * in the 2-parallel ructors.
         **********************************************************************/
        public double OriginLatitude()
        {
            return _lat0;
        }

        /**
         * @return central scale for the projection.  This is the azimuthal scale
         *   on the latitude of origin.
         **********************************************************************/
        public double CentralScale()
        {
            return _k0;
        }
        ///@}

        /**
         * A global instantiation of AlbersEqualArea with the WGS84 ellipsoid, \e
         * stdlat = 0, and \e k0 = 1.  This degenerates to the cylindrical equal
         * area projection.
         **********************************************************************/
        public static AlbersEqualArea CylindricalEqualArea()
        {
            return new AlbersEqualArea(Constants.WGS84_a, Constants.WGS84_f, 0, 1, 0, 1, 1);
        }

        /**
         * A global instantiation of AlbersEqualArea with the WGS84 ellipsoid, \e
         * stdlat = 90&deg;, and \e k0 = 1.  This degenerates to the
         * Lambert azimuthal equal area projection.
         **********************************************************************/
        public static AlbersEqualArea AzimuthalEqualAreaNorth()
        {
            return new AlbersEqualArea(Constants.WGS84_a, Constants.WGS84_f, 1, 0, 1, 0, 1);
        }

        /**
         * A global instantiation of AlbersEqualArea with the WGS84 ellipsoid, \e
         * stdlat = &minus;90&deg;, and \e k0 = 1.  This degenerates to the
         * Lambert azimuthal equal area projection.
         **********************************************************************/
        public static AlbersEqualArea AzimuthalEqualAreaSouth()
        {
            return new AlbersEqualArea(Constants.WGS84_a, Constants.WGS84_f, -1, 0, -1, 0, 1);
        }

        static double hyp(double x)
        {
            return GeoMath.Hypot(1, x);
        }

        // atanh(      e   * x)/      e   if f > 0
        // atan (GeoMath.Square(-e2) * x)/GeoMath.Square(-e2) if f < 0
        // x                              if f = 0
        double atanhee(double x)
        {

            return _f > 0 ? GeoMath.Atanh(_e * x) / _e :
              // We only invoke atanhee in txif for positive latitude.  Then x is
              // only negative for very prolate ellipsoids (_b/_a >= GeoMath.Square(2)) and we
              // still need to return a positive result in this case; hence the need
              // for the call to atan2.
              (_f < 0 ? (Math.Atan2(_e * Math.Abs(x), x < 0 ? -1 : 1) / _e) : x);
        }

        // return atanh(GeoMath.Square(x))/GeoMath.Square(x) - 1, accurate for small x
        static double atanhxm1(double x)
        {
            double s = 0;
            if (Math.Abs(x) < 0.5)
            {
                double os = -1, y = 1, k = 1;
                while (os != s)
                {
                    os = s;
                    y *= x;                 // y = x^n
                    k += 2;                 // k = 2*n + 1
                    s += y / k;               // sum( x^n/(2*n + 1) )
                }
            }
            else
            {
                double xs = Math.Sqrt(Math.Abs(x));
                s = (x > 0 ? GeoMath.Atanh(xs) : Math.Atan(xs)) / xs - 1;
            }
            return s;
        }

        // Divided differences
        // Definition: Df(x,y) = (f(x)-f(y))/(x-y)
        // See:
        //   W. M. Kahan and R. J. Fateman,
        //   Symbolic computation of divided differences,
        //   SIGSAM Bull. 33(3), 7-28 (1999)
        //   https://doi.org/10.1145/334714.334716
        //   http://www.cs.berkeley.edu/~fateman/papers/divdiff.pdf
        //
        // General rules
        // h(x) = f(g(x)): Dh(x,y) = Df(g(x),g(y))*Dg(x,y)
        // h(x) = f(x)*g(x):
        //        Dh(x,y) = Df(x,y)*g(x) + Dg(x,y)*f(y)
        //                = Df(x,y)*g(y) + Dg(x,y)*f(x)
        //                = Df(x,y)*(g(x)+g(y))/2 + Dg(x,y)*(f(x)+f(y))/2
        //
        // sn(x) = x/GeoMath.Square(1+x^2): Dsn(x,y) = (x+y)/((sn(x)+sn(y))*(1+x^2)*(1+y^2))
        static double Dsn(double x, double y, double sx, double sy)
        {
            // sx = x/hyp(x)
            double t = x * y;
            return t > 0 ? (x + y) * GeoMath.Square((sx * sy) / t) / (sx + sy) :
              (x - y != 0 ? (sx - sy) / (x - y) : 1);
        }

        // Datanhee(x,y) = atanhee((x-y)/(1-e^2*x*y))/(x-y)
        double Datanhee(double x, double y)
        {
            double t = x - y, d = 1 - _e2 * x * y;
            return t != 0 ? atanhee(t / d) / t : 1 / d;
        }

        // DDatanhee(x,y) = (Datanhee(1,y) - Datanhee(1,x))/(y-x)
        double DDatanhee(double x, double y)
        {
            double s = 0;
            if (_e2 * (Math.Abs(x) + Math.Abs(y)) < 0.5)
            {
                double os = -1, z = 1, k = 1, t = 0, c = 0, en = 1;
                while (os != s)
                {
                    os = s;
                    t = y * t + z; c += t; z *= x;
                    t = y * t + z; c += t; z *= x;
                    k += 2; en *= _e2;
                    // Here en[l] = e2^l, k[l] = 2*l + 1,
                    // c[l] = sum( x^i * y^j; i >= 0, j >= 0, i+j < 2*l)
                    s += en * c / k;
                }
                // Taylor expansion is
                // s = sum( c[l] * e2^l / (2*l + 1), l, 1, N)
            }
            else
                s = (Datanhee(1, y) - Datanhee(x, y)) / (1 - x);
            return s;
        }

        void Init(double sphi1, double cphi1, double sphi2, double cphi2, double k1)
        {
            {
                double r;
                r = GeoMath.Hypot(sphi1, cphi1);
                sphi1 /= r; cphi1 /= r;
                r = GeoMath.Hypot(sphi2, cphi2);
                sphi2 /= r; cphi2 /= r;
            }
            bool polar = (cphi1 == 0);
            cphi1 = Math.Max(epsx_, cphi1);   // Avoid singularities at poles
            cphi2 = Math.Max(epsx_, cphi2);
            // Determine hemisphere of tangent latitude
            _sign = sphi1 + sphi2 >= 0 ? 1 : -1;
            // Internally work with tangent latitude positive
            sphi1 *= _sign; sphi2 *= _sign;
            if (sphi1 > sphi2)
            {
                Utility.Swap(ref sphi1, ref sphi2); Utility.Swap(ref cphi1, ref cphi2); // Make phi1 < phi2
            }
            double
              tphi1 = sphi1 / cphi1, tphi2 = sphi2 / cphi2;

            // q = (1-e^2)*(sphi/(1-e^2*sphi^2) - atanhee(sphi))
            // qZ = q(pi/2) = (1 + (1-e^2)*atanhee(1))
            // atanhee(x) = atanh(e*x)/e
            // q = sxi * qZ
            // dq/dphi = 2*(1-e^2)*cphi/(1-e^2*sphi^2)^2
            //
            // n = (m1^2-m2^2)/(q2-q1) -> sin(phi0) for phi1, phi2 -> phi0
            // C = m1^2 + n*q1 = (m1^2*q2-m2^2*q1)/(q2-q1)
            // let
            //   rho(pi/2)/rho(-pi/2) = (1-s)/(1+s)
            //   s = n*qZ/C
            //     = qZ * (m1^2-m2^2)/(m1^2*q2-m2^2*q1)
            //     = qZ * (scbet2^2 - scbet1^2)/(scbet2^2*q2 - scbet1^2*q1)
            //     = (scbet2^2 - scbet1^2)/(scbet2^2*sxi2 - scbet1^2*sxi1)
            //     = (tbet2^2 - tbet1^2)/(scbet2^2*sxi2 - scbet1^2*sxi1)
            // 1-s = -((1-sxi2)*scbet2^2 - (1-sxi1)*scbet1^2)/
            //         (scbet2^2*sxi2 - scbet1^2*sxi1)
            //
            // Define phi0 to give same value of s, i.e.,
            //  s = sphi0 * qZ / (m0^2 + sphi0*q0)
            //    = sphi0 * scbet0^2 / (1/qZ + sphi0 * scbet0^2 * sxi0)

            double tphi0, C;
            if (polar || tphi1 == tphi2)
            {
                tphi0 = tphi2;
                C = 1;                    // ignored
            }
            else
            {
                double
                  tbet1 = _fm * tphi1, scbet12 = 1 + GeoMath.Square(tbet1),
                  tbet2 = _fm * tphi2, scbet22 = 1 + GeoMath.Square(tbet2),
                  txi1 = txif(tphi1), cxi1 = 1 / hyp(txi1), sxi1 = txi1 * cxi1,
                  txi2 = txif(tphi2), cxi2 = 1 / hyp(txi2), sxi2 = txi2 * cxi2,
                  dtbet2 = _fm * (tbet1 + tbet2),
                  es1 = 1 - _e2 * GeoMath.Square(sphi1), es2 = 1 - _e2 * GeoMath.Square(sphi2),
                  /*
                  dsxi = ( (_e2 * sq(sphi2 + sphi1) + es2 + es1) / (2 * es2 * es1) +
                           Datanhee(sphi2, sphi1) ) * Dsn(tphi2, tphi1, sphi2, sphi1) /
                  ( 2 * _qx ),
                  */
                  dsxi = ((1 + _e2 * sphi1 * sphi2) / (es2 * es1) +
                           Datanhee(sphi2, sphi1)) * Dsn(tphi2, tphi1, sphi2, sphi1) /
                  (2 * _qx),
                  den = (sxi2 + sxi1) * dtbet2 + (scbet22 + scbet12) * dsxi,
                  // s = (sq(tbet2) - sq(tbet1)) / (scbet22*sxi2 - scbet12*sxi1)
                  s = 2 * dtbet2 / den,
                  // 1-s = -(sq(scbet2)*(1-sxi2) - sq(scbet1)*(1-sxi1)) /
                  //        (scbet22*sxi2 - scbet12*sxi1)
                  // Write
                  //   sq(scbet)*(1-sxi) = sq(scbet)*(1-sphi) * (1-sxi)/(1-sphi)
                  sm1 = -Dsn(tphi2, tphi1, sphi2, sphi1) *
                  (-(((sphi2 <= 0 ? (1 - sxi2) / (1 - sphi2) :
                         GeoMath.Square(cxi2 / cphi2) * (1 + sphi2) / (1 + sxi2)) +
                        (sphi1 <= 0 ? (1 - sxi1) / (1 - sphi1) :
                         GeoMath.Square(cxi1 / cphi1) * (1 + sphi1) / (1 + sxi1)))) *
                    (1 + _e2 * (sphi1 + sphi2 + sphi1 * sphi2)) /
                    (1 + (sphi1 + sphi2 + sphi1 * sphi2)) +
                    (scbet22 * (sphi2 <= 0 ? 1 - sphi2 :
                                GeoMath.Square(cphi2) / (1 + sphi2)) +
                     scbet12 * (sphi1 <= 0 ? 1 - sphi1 : GeoMath.Square(cphi1) / (1 + sphi1)))
                    * (_e2 * (1 + sphi1 + sphi2 + _e2 * sphi1 * sphi2) / (es1 * es2)
                    + _e2m * DDatanhee(sphi1, sphi2)) / _qZ) / den;
                // C = (scbet22*sxi2 - scbet12*sxi1) / (scbet22 * scbet12 * (sx2 - sx1))
                C = den / (2 * scbet12 * scbet22 * dsxi);
                tphi0 = (tphi2 + tphi1) / 2;
                double stol = tol0_ * Math.Max(1, Math.Abs(tphi0));
                for (int i = 0; i < 2 * numit0_; ++i)
                {
                    // Solve (scbet0^2 * sphi0) / (1/qZ + scbet0^2 * sphi0 * sxi0) = s
                    // for tphi0 by Newton's method on
                    // v(tphi0) = (scbet0^2 * sphi0) - s * (1/qZ + scbet0^2 * sphi0 * sxi0)
                    //          = 0
                    // Alt:
                    // (scbet0^2 * sphi0) / (1/qZ - scbet0^2 * sphi0 * (1-sxi0))
                    //          = s / (1-s)
                    // w(tphi0) = (1-s) * (scbet0^2 * sphi0)
                    //             - s  * (1/qZ - scbet0^2 * sphi0 * (1-sxi0))
                    //          = (1-s) * (scbet0^2 * sphi0)
                    //             - S/qZ  * (1 - scbet0^2 * sphi0 * (qZ-q0))
                    // Now
                    // qZ-q0 = (1+e2*sphi0)*(1-sphi0)/(1-e2*sphi0^2) +
                    //         (1-e2)*atanhee((1-sphi0)/(1-e2*sphi0))
                    // In limit sphi0 -> 1, qZ-q0 -> 2*(1-sphi0)/(1-e2), so wrte
                    // qZ-q0 = 2*(1-sphi0)/(1-e2) + A + B
                    // A = (1-sphi0)*( (1+e2*sphi0)/(1-e2*sphi0^2) - (1+e2)/(1-e2) )
                    //   = -e2 *(1-sphi0)^2 * (2+(1+e2)*sphi0) / ((1-e2)*(1-e2*sphi0^2))
                    // B = (1-e2)*atanhee((1-sphi0)/(1-e2*sphi0)) - (1-sphi0)
                    //   = (1-sphi0)*(1-e2)/(1-e2*sphi0)*
                    //     ((atanhee(x)/x-1) - e2*(1-sphi0)/(1-e2))
                    // x = (1-sphi0)/(1-e2*sphi0), atanhee(x)/x = atanh(e*x)/(e*x)
                    //
                    // 1 - scbet0^2 * sphi0 * (qZ-q0)
                    //   = 1 - scbet0^2 * sphi0 * (2*(1-sphi0)/(1-e2) + A + B)
                    //   = D - scbet0^2 * sphi0 * (A + B)
                    // D = 1 - scbet0^2 * sphi0 * 2*(1-sphi0)/(1-e2)
                    //   = (1-sphi0)*(1-e2*(1+2*sphi0*(1+sphi0)))/((1-e2)*(1+sphi0))
                    // dD/dsphi0 = -2*(1-e2*sphi0^2*(2*sphi0+3))/((1-e2)*(1+sphi0)^2)
                    // d(A+B)/dsphi0 = 2*(1-sphi0^2)*e2*(2-e2*(1+sphi0^2))/
                    //                 ((1-e2)*(1-e2*sphi0^2)^2)

                    double
                      scphi02 = 1 + GeoMath.Square(tphi0), scphi0 = Math.Sqrt(scphi02),
                      // sphi0m = 1-sin(phi0) = 1/( sec(phi0) * (tan(phi0) + sec(phi0)) )
                      sphi0 = tphi0 / scphi0, sphi0m = 1 / (scphi0 * (tphi0 + scphi0)),
                      // scbet0^2 * sphi0
                      g = (1 + GeoMath.Square(_fm * tphi0)) * sphi0,
                      // dg/dsphi0 = dg/dtphi0 * scphi0^3
                      dg = _e2m * scphi02 * (1 + 2 * GeoMath.Square(tphi0)) + _e2,
                      D = sphi0m * (1 - _e2 * (1 + 2 * sphi0 * (1 + sphi0))) / (_e2m * (1 + sphi0)),
                      // dD/dsphi0
                      dD = -2 * (1 - _e2 * GeoMath.Square(sphi0) * (2 * sphi0 + 3)) /
                           (_e2m * GeoMath.Square(1 + sphi0)),
                      A = -_e2 * GeoMath.Square(sphi0m) * (2 + (1 + _e2) * sphi0) /
                          (_e2m * (1 - _e2 * GeoMath.Square(sphi0))),
                      B = (sphi0m * _e2m / (1 - _e2 * sphi0) *
                           (atanhxm1(_e2 *
                                     GeoMath.Square(sphi0m / (1 - _e2 * sphi0))) - _e2 * sphi0m / _e2m)),
                      // d(A+B)/dsphi0
                      dAB = (2 * _e2 * (2 - _e2 * (1 + GeoMath.Square(sphi0))) /
                             (_e2m * GeoMath.Square(1 - _e2 * GeoMath.Square(sphi0)) * scphi02)),
                      u = sm1 * g - s / _qZ * (D - g * (A + B)),
                      // du/dsphi0
                      du = sm1 * dg - s / _qZ * (dD - dg * (A + B) - g * dAB),
                      dtu = -u / du * (scphi0 * scphi02);
                    tphi0 += dtu;
                    if (!(Math.Abs(dtu) >= stol))
                        break;
                }
            }
            _txi0 = txif(tphi0); _scxi0 = hyp(_txi0); _sxi0 = _txi0 / _scxi0;
            _n0 = tphi0 / hyp(tphi0);
            _m02 = 1 / (1 + GeoMath.Square(_fm * tphi0));
            _nrho0 = polar ? 0 : _a * Math.Sqrt(_m02);
            _k0 = Math.Sqrt(tphi1 == tphi2 ? 1 : C / (_m02 + _n0 * _qZ * _sxi0)) * k1;
            _k2 = GeoMath.Square(_k0);
            _lat0 = _sign * Math.Atan(tphi0) / GeoMath.Degree;
        }

        void InitVars(double a, double f)
        {
            eps_ = GeoMath.Epsilon;
            epsx_ = GeoMath.Square(eps_);
            epsx2_ = GeoMath.Square(epsx_);
            tol_ = GeoMath.Square(eps_);
            tol0_ = (tol_ * GeoMath.Square(GeoMath.Square(eps_)));
            _a = a;
            _f = f;
            _fm = 1 - _f;
            _e2 = _f * (2 - _f);
            _e = GeoMath.Square(Math.Abs(_e2));
            _e2m = 1 - _e2;
            _qZ = 1 + _e2m * atanhee(1);
            _qx = _qZ / (2 * _e2m);
        }

        internal double txif(double tphi)
        {
            // sxi = ( sphi/(1-e2*sphi^2) + atanhee(sphi) ) /
            //       ( 1/(1-e2) + atanhee(1) )
            //
            // txi = ( sphi/(1-e2*sphi^2) + atanhee(sphi) ) /
            //       sqrt( ( (1+e2*sphi)*(1-sphi)/( (1-e2*sphi^2) * (1-e2) ) +
            //               atanhee((1-sphi)/(1-e2*sphi)) ) *
            //             ( (1-e2*sphi)*(1+sphi)/( (1-e2*sphi^2) * (1-e2) ) +
            //               atanhee((1+sphi)/(1+e2*sphi)) ) )
            //
            // subst 1-sphi = cphi^2/(1+sphi)
            int s = tphi < 0 ? -1 : 1;  // Enforce odd parity
            tphi *= s;
            double
              cphi2 = 1 / (1 + GeoMath.Square(tphi)),
              sphi = tphi * Math.Sqrt(cphi2),
              es1 = _e2 * sphi,
              es2m1 = 1 - es1 * sphi,
              sp1 = 1 + sphi,
              es1m1 = (1 - es1) * sp1,
              es2m1a = _e2m * es2m1,
              es1p1 = sp1 / (1 + es1);
            return s * (sphi / es2m1 + atanhee(sphi)) /
              Math.Sqrt((cphi2 / (es1p1 * es2m1a) + atanhee(cphi2 / es1m1)) *
                    (es1m1 / es2m1a + atanhee(es1p1)));
        }
        internal double tphif(double txi)
        {
            double
                tphi = txi,
                stol = tol_ * Math.Max(1, Math.Abs(txi));
            // CHECK: min iterations = 1, max iterations = 2; mean = 1.99
            for (int i = 0; i < numit_; ++i)
            {
                // dtxi/dtphi = (scxi/scphi)^3 * 2*(1-e^2)/(qZ*(1-e^2*sphi^2)^2)
                double
                  txia = txif(tphi),
                  tphi2 = GeoMath.Square(tphi),
                  scphi2 = 1 + tphi2,
                  scterm = scphi2 / (1 + GeoMath.Square(txia)),
                  dtphi = (txi - txia) * scterm * Math.Sqrt(scterm) *
                  _qx * GeoMath.Square(1 - _e2 * tphi2 / scphi2);
                tphi += dtphi;
                if (!(Math.Abs(dtphi) >= stol))
                    break;
            }
            return tphi;
        }
    }
}
