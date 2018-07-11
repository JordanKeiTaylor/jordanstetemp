using System;
using System.Numerics;

namespace GeographicLib
{
    /**
    * \brief Transverse Mercator projection
    *
    * This uses Kr&uuml;ger's method which evaluates the projection and its
    * inverse in terms of a series.  See
    *  - L. Kr&uuml;ger,
    *    <a href="https://doi.org/10.2312/GFZ.b103-krueger28"> Konforme
    *    Abbildung des Erdellipsoids in der Ebene</a> (Conformal mapping of the
    *    ellipsoidal earth to the plane), Royal Prussian Geodetic Institute, New
    *    Series 52, 172 pp. (1912).
    *  - C. F. F. Karney,
    *    <a href="https://doi.org/10.1007/s00190-011-0445-3">
    *    Transverse Mercator with an accuracy of a few nanometers,</a>
    *    J. Geodesy 85(8), 475--485 (Aug. 2011);
    *    preprint
    *    <a href="https://arxiv.org/abs/1002.1417">arXiv:1002.1417</a>.
    *
    * Kr&uuml;ger's method has been extended from 4th to 6th order.  The maximum
    * error is 5 nm (5 nanometers), ground distance, for all positions within 35
    * degrees of the central meridian.  The error in the convergence is 2
    * &times; 10<sup>&minus;15</sup>&quot; and the relative error in the scale
    * is 6 &times; 10<sup>&minus;12</sup>%%.  See Sec. 4 of
    * <a href="https://arxiv.org/abs/1002.1417">arXiv:1002.1417</a> for details.
    * The speed penalty in going to 6th order is only about 1%.
    *
    * There's a singularity in the projection at &phi; = 0&deg;, &lambda;
    * &minus; &lambda;<sub>0</sub> = &plusmn;(1 &minus; \e e)90&deg; (&asymp;
    * &plusmn;82.6&deg; for the WGS84 ellipsoid), where \e e is the
    * eccentricity.  Beyond this point, the series ceases to converge and the
    * results from this method will be garbage.  To be on the safe side, don't
    * use this method if the angular distance from the central meridian exceeds
    * (1 &minus; 2e)90&deg; (&asymp; 75&deg; for the WGS84 ellipsoid)
    *
    * TransverseMercatorExact is an alternative implementation of the projection
    * using exact formulas which yield accurate (to 8 nm) results over the
    * entire ellipsoid.
    *
    * The ellipsoid parameters and the central scale are set in the ructor.
    * The central meridian (which is a trivial shift of the longitude) is
    * specified as the \e lon0 argument of the TransverseMercator::Forward and
    * TransverseMercator::Reverse functions.  The latitude of origin is taken to
    * be the equator.  There is no provision in this class for specifying a
    * false easting or false northing or a different latitude of origin.
    * However these are can be simply included by the calling function.  For
    * example, the UTMUPS class applies the false easting and false northing for
    * the UTM projections.  A more complicated example is the British National
    * Grid (<a href="http://www.spatialreference.org/ref/epsg/7405/">
    * EPSG:7405</a>) which requires the use of a latitude of origin.  This is
    * implemented by the GeographicLib::OSGB class.
    *
    * This class also returns the meridian convergence \e gamma and scale \e k.
    * The meridian convergence is the bearing of grid north (the \e y axis)
    * measured clockwise from true north.
    *
    * See TransverseMercator.cpp for more information on the implementation.
    *
    * See \ref transversemercator for a discussion of this projection.
    *
    * Example of use:
    * \include example-TransverseMercator.cpp
    *
    * <a href="TransverseMercatorProj.1.html">TransverseMercatorProj</a> is a
    * command-line utility providing access to the functionality of
    * TransverseMercator and TransverseMercatorExact.
    **********************************************************************/
    public class TransverseMercator
    {
        static int maxpow_ = 6;
        int numit_ = 5;
        double _a, _f, _k0, _e2, _es, _e2m, _c, _n;
        // _alp[0] and _bet[0] unused
        double _a1, _b1;
        double[] _alp = new double[maxpow_ + 1];
        internal double[] _bet = new double[maxpow_ + 1];

        /**
         * Constructor for a ellipsoid with
         *
         * @param[in] a equatorial radius (meters).
         * @param[in] f flattening of ellipsoid.  Setting \e f = 0 gives a sphere.
         *   Negative \e f gives a prolate ellipsoid.
         * @param[in] k0 central scale factor.
         * @exception GeographicErr if \e a, (1 &minus; \e f) \e a, or \e k0 is
         *   not positive.
         **********************************************************************/
        public TransverseMercator(double a, double f, double k0)
        {
            _a = a;
            _f = f;
            _k0 = k0;
            _e2 = _f * (2 - _f);
            _es = (f < 0 ? -1 : 1) * Math.Sqrt(Math.Abs(_e2));
            _e2m = 1 - _e2;
            // _c = Math.Sqrt( pow(1 + _e, 1 + _e) * pow(1 - _e, 1 - _e) ) )
            // See, for example, Lee (1976), p 100.
            _c = Math.Sqrt(_e2m) * Math.Exp(GeoMath.Eatanhe(1, _es));
            _n = _f / (2 - _f);


            if (!(GeoMath.IsFinite(_a) && _a > 0))
                throw new GeographicException("Equatorial radius is not positive");
            if (!(GeoMath.IsFinite(_f) && _f < 1))
                throw new GeographicException("Polar semi-axis is not positive");
            if (!(GeoMath.IsFinite(_k0) && _k0 > 0))
                throw new GeographicException("Scale is not positive");

            double[] b1coeff = {
                // b1*(n+1), polynomial in n2 of order 3
                1, 4, 64, 256, 256,
            };

            double[] alpcoeff = {
                // alp[1]/n^1, polynomial in n of order 5
                31564, -66675, 34440, 47250, -100800, 75600, 151200,
                // alp[2]/n^2, polynomial in n of order 4
                -1983433, 863232, 748608, -1161216, 524160, 1935360,
                // alp[3]/n^3, polynomial in n of order 3
                670412, 406647, -533952, 184464, 725760,
                // alp[4]/n^4, polynomial in n of order 2
                6601661, -7732800, 2230245, 7257600,
                // alp[5]/n^5, polynomial in n of order 1
                -13675556, 3438171, 7983360,
                // alp[6]/n^6, polynomial in n of order 0
                212378941, 319334400,
            };  // count = 27

            double[] betcoeff = {
                // bet[1]/n^1, polynomial in n of order 5
                384796, -382725, -6720, 932400, -1612800, 1209600, 2419200,
                // bet[2]/n^2, polynomial in n of order 4
                -1118711, 1695744, -1174656, 258048, 80640, 3870720,
                // bet[3]/n^3, polynomial in n of order 3
                22276, -16929, -15984, 12852, 362880,
                // bet[4]/n^4, polynomial in n of order 2
                -830251, -158400, 197865, 7257600,
                // bet[5]/n^5, polynomial in n of order 1
                -435388, 453717, 15966720,
                // bet[6]/n^6, polynomial in n of order 0
                20648693, 638668800,
            };  // count = 27

            int m = maxpow_ / 2;
            _b1 = GeoMath.PolyVal(m, b1coeff, 0, GeoMath.Square(_n)) / (b1coeff[m + 1] * (1 + _n));
            // _a1 is the equivalent radius for computing the circumference of
            // ellipse.
            _a1 = _b1 * _a;
            int o = 0;
            double d = _n;
            for (int l = 1; l <= maxpow_; ++l)
            {
                m = maxpow_ - l;
                _alp[l] = d * GeoMath.PolyVal(m, alpcoeff, o, _n) / alpcoeff[o + m + 1];
                _bet[l] = d * GeoMath.PolyVal(m, betcoeff, o, _n) / betcoeff[o + m + 1];
                o += m + 2;
                d *= _n;
            }
            // Post condition: o == sizeof(alpcoeff) / sizeof(double) &&
            // o == sizeof(betcoeff) / sizeof(double)
        }

        /**
         * Forward projection, from geographic to transverse Mercator.
         *
         * @param[in] lon0 central meridian of the projection (degrees).
         * @param[in] lat latitude of point (degrees).
         * @param[in] lon longitude of point (degrees).
         * @param[out] x easting of point (meters).
         * @param[out] y northing of point (meters).
         * @param[out] gamma meridian convergence at point (degrees).
         * @param[out] k scale of projection at point.
         *
         * No false easting or northing is added. \e lat should be in the range
         * [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public void Forward(double lon0, double lat, double lon,
                     out double x, out double y, out double gamma, out double k)
        {
            double e;
            lat = GeoMath.LatFix(lat);
            lon = GeoMath.AngDiff(lon0, lon, out e);
            // Explicitly enforce the parity
            int
              latsign = (lat < 0) ? -1 : 1,
              lonsign = (lon < 0) ? -1 : 1;
            lon *= lonsign;
            lat *= latsign;
            bool backside = lon > 90;
            if (backside)
            {
                if (lat == 0)
                    latsign = -1;
                lon = 180 - lon;
            }
            double sphi, cphi, slam, clam;
            GeoMath.Sincosd(lat, out sphi, out cphi);
            GeoMath.Sincosd(lon, out slam, out clam);
            // phi = latitude
            // phi' = conformal latitude
            // psi = isometric latitude
            // tau = tan(phi)
            // tau' = tan(phi')
            // [xi', eta'] = Gauss-Schreiber TM coordinates
            // [xi, eta] = Gauss-Krueger TM coordinates
            //
            // We use
            //   tan(phi') = Math.Sinh(psi)
            //   Math.Sin(phi') = tanh(psi)
            //   Math.Cos(phi') = sech(psi)
            //   denom^2    = 1-Math.Cos(phi')^2*Math.Sin(lam)^2 = 1-sech(psi)^2*Math.Sin(lam)^2
            //   Math.Sin(xip)   = Math.Sin(phi')/denom          = tanh(psi)/denom
            //   Math.Cos(xip)   = Math.Cos(phi')*Math.Cos(lam)/denom = sech(psi)*Math.Cos(lam)/denom
            //   Math.Cosh(etap) = 1/denom                  = 1/denom
            //   Math.Sinh(etap) = Math.Cos(phi')*Math.Sin(lam)/denom = sech(psi)*Math.Sin(lam)/denom
            double etap, xip;
            if (lat != 90)
            {
                double
                  tau = sphi / cphi,
                  taup = GeoMath.Taupf(tau, _es);
                xip = Math.Atan2(taup, clam);
                // Used to be
                //   etap = Math::atanh(Math.Sin(lam) / Math.Cosh(psi));
                etap = GeoMath.Asinh(slam / GeoMath.Hypot(taup, clam));
                // convergence and scale for Gauss-Schreiber TM (xip, etap) -- gamma0 =
                // atan(tan(xip) * tanh(etap)) = atan(tan(lam) * Math.Sin(phi'));
                // Math.Sin(phi') = tau'/Math.Sqrt(1 + tau'^2)
                // Krueger p 22 (44)
                gamma = GeoMath.Atan2d(slam * taup, clam * GeoMath.Hypot(1, taup));
                // k0 = Math.Sqrt(1 - _e2 * Math.Sin(phi)^2) * (Math.Cos(phi') / Math.Cos(phi)) * Math.Cosh(etap)
                // Note 1/Math.Cos(phi) = Math.Cosh(psip);
                // and Math.Cos(phi') * Math.Cosh(etap) = 1/hypot(Math.Sinh(psi), Math.Cos(lam))
                //
                // This form has cancelling errors.  This property is lost if Math.Cosh(psip)
                // is replaced by 1/Math.Cos(phi), even though it's using "primary" data (phi
                // instead of psip).
                k = Math.Sqrt(_e2m + _e2 * GeoMath.Square(cphi)) * GeoMath.Hypot(1, tau)
                  / GeoMath.Hypot(taup, clam);
            }
            else
            {
                xip = Math.PI / 2;
                etap = 0;
                gamma = lon;
                k = _c;
            }
            // {xi',eta'} is {northing,easting} for Gauss-Schreiber transverse Mercator
            // (for eta' = 0, xi' = bet). {xi,eta} is {northing,easting} for transverse
            // Mercator with ant scale on the central meridian (for eta = 0, xip =
            // rectifying latitude).  Define
            //
            //   zeta = xi + i*eta
            //   zeta' = xi' + i*eta'
            //
            // The conversion from conformal to rectifying latitude can be expressed as
            // a series in _n:
            //
            //   zeta = zeta' + sum(h[j-1]' * Math.Sin(2 * j * zeta'), j = 1..maxpow_)
            //
            // where h[j]' = O(_n^j).  The reversion of this series gives
            //
            //   zeta' = zeta - sum(h[j-1] * Math.Sin(2 * j * zeta), j = 1..maxpow_)
            //
            // which is used in Reverse.
            //
            // Evaluate sums via Clenshaw method.  See
            //    https://en.wikipedia.org/wiki/Clenshaw_algorithm
            //
            // Let
            //
            //    S = sum(a[k] * phi[k](x), k = 0..n)
            //    phi[k+1](x) = alpha[k](x) * phi[k](x) + beta[k](x) * phi[k-1](x)
            //
            // Evaluate S with
            //
            //    b[n+2] = b[n+1] = 0
            //    b[k] = alpha[k](x) * b[k+1] + beta[k+1](x) * b[k+2] + a[k]
            //    S = (a[0] + beta[1](x) * b[2]) * phi[0](x) + b[1] * phi[1](x)
            //
            // Here we have
            //
            //    x = 2 * zeta'
            //    phi[k](x) = Math.Sin(k * x)
            //    alpha[k](x) = 2 * Math.Cos(x)
            //    beta[k](x) = -1
            //    [ Math.Sin(A+B) - 2*Math.Cos(B)*Math.Sin(A) + Math.Sin(A-B) = 0, A = k*x, B = x ]
            //    n = maxpow_
            //    a[k] = _alp[k]
            //    S = b[1] * Math.Sin(x)
            //
            // For the derivative we have
            //
            //    x = 2 * zeta'
            //    phi[k](x) = Math.Cos(k * x)
            //    alpha[k](x) = 2 * Math.Cos(x)
            //    beta[k](x) = -1
            //    [ Math.Cos(A+B) - 2*Math.Cos(B)*Math.Cos(A) + Math.Cos(A-B) = 0, A = k*x, B = x ]
            //    a[0] = 1; a[k] = 2*k*_alp[k]
            //    S = (a[0] - b[2]) + b[1] * Math.Cos(x)
            //
            // Matrix formulation (not used here):
            //    phi[k](x) = [Math.Sin(k * x); k * Math.Cos(k * x)]
            //    alpha[k](x) = 2 * [Math.Cos(x), 0; -Math.Sin(x), Math.Cos(x)]
            //    beta[k](x) = -1 * [1, 0; 0, 1]
            //    a[k] = _alp[k] * [1, 0; 0, 1]
            //    b[n+2] = b[n+1] = [0, 0; 0, 0]
            //    b[k] = alpha[k](x) * b[k+1] + beta[k+1](x) * b[k+2] + a[k]
            //    N.B., for all k: b[k](1,2) = 0; b[k](1,1) = b[k](2,2)
            //    S = (a[0] + beta[1](x) * b[2]) * phi[0](x) + b[1] * phi[1](x)
            //    phi[0](x) = [0; 0]
            //    phi[1](x) = [Math.Sin(x); Math.Cos(x)]
            double
              c0 = Math.Cos(2 * xip), ch0 = Math.Cosh(2 * etap),
              s0 = Math.Sin(2 * xip), sh0 = Math.Sinh(2 * etap);

            int n = maxpow_;
            Complex a = new Complex(2 * c0 * ch0, -2 * s0 * sh0); // 2 * Math.Cos(2*zeta')
            Complex y0 = new Complex((n & 1) != 0 ? _alp[n] : 0, 0);
            Complex y1; // default initializer is 0+i0
            Complex z0 = new Complex((n & 1) != 0 ? 2 * n * _alp[n] : 0, 0);
            Complex z1;

            if ((n & 1) != 0) --n;
            while (n > 0)
            {
                y1 = a * y0 - y1 + _alp[n];
                z1 = a * z0 - z1 + 2 * n * _alp[n];
                --n;
                y0 = a * y1 - y0 + _alp[n];
                z0 = a * z1 - z0 + 2 * n * _alp[n];
                --n;
            }
            a /= 2;               // Math.Cos(2*zeta')
            z1 = 1 - z1 + a * z0;
            a = new Complex(s0 * ch0, c0 * sh0); // Math.Sin(2*zeta')
            y1 = new Complex(xip, etap) + a * y0;
            // Fold in change in convergence and scale for Gauss-Schreiber TM to
            // Gauss-Krueger TM.
            gamma -= GeoMath.Atan2d(z1.Imaginary, z1.Real);
            k *= _b1 * Complex.Abs(z1);
            double xi = y1.Real, eta = y1.Imaginary;
            y = _a1 * _k0 * (backside ? Math.PI - xi : xi) * latsign;
            x = _a1 * _k0 * eta * lonsign;
            if (backside)
                gamma = 180 - gamma;
            gamma *= latsign * lonsign;
            gamma = GeoMath.AngNormalize(gamma);
            k *= _k0;
        }

        /**
         * Reverse projection, from transverse Mercator to geographic.
         *
         * @param[in] lon0 central meridian of the projection (degrees).
         * @param[in] x easting of point (meters).
         * @param[in] y northing of point (meters).
         * @param[out] lat latitude of point (degrees).
         * @param[out] lon longitude of point (degrees).
         * @param[out] gamma meridian convergence at point (degrees).
         * @param[out] k scale of projection at point.
         *
         * No false easting or northing is added.  The value of \e lon returned is
         * in the range [&minus;180&deg;, 180&deg;].
         **********************************************************************/
        public void Reverse(double lon0, double x, double y,
                     out double lat, out double lon, out double gamma, out double k)
        {
            // This undoes the steps in Forward.  The wrinkles are: (1) Use of the
            // reverted series to express zeta' in terms of zeta. (2) Newton's method
            // to solve for phi in terms of tan(phi).
            double
              xi = y / (_a1 * _k0),
              eta = x / (_a1 * _k0);
            // Explicitly enforce the parity
            int
              xisign = (xi < 0) ? -1 : 1,
              etasign = (eta < 0) ? -1 : 1;
            xi *= xisign;
            eta *= etasign;
            bool backside = xi > Math.PI / 2;
            if (backside)
                xi = Math.PI - xi;
            double
              c0 = Math.Cos(2 * xi), ch0 = Math.Cosh(2 * eta),
              s0 = Math.Sin(2 * xi), sh0 = Math.Sinh(2 * eta);


            int n = maxpow_;
            Complex a = new Complex(2 * c0 * ch0, -2 * s0 * sh0); // 2 * Math.Cos(2*zeta')
            Complex y0 = new Complex((n & 1) != 0 ? -_bet[n] : 0, 0);
            Complex y1; // default initializer is 0+i0
            Complex z0 = new Complex((n & 1) != 0 ? -2 * n * _bet[n] : 0, 0);
            Complex z1;
            if ((n & 1) != 0) --n;
            while (n > 0)
            {
                y1 = a * y0 - y1 - _bet[n];
                z1 = a * z0 - z1 - 2 * n * _bet[n];
                --n;
                y0 = a * y1 - y0 - _bet[n];
                z0 = a * z1 - z0 - 2 * n * _bet[n];
                --n;
            }
            a /= 2;               // Math.Cos(2*zeta)
            z1 = 1 - z1 + a * z0;
            a = new Complex(s0 * ch0, c0 * sh0); // Math.Sin(2*zeta)
            y1 = new Complex(xi, eta) + a * y0;
            // Convergence and scale for Gauss-Schreiber TM to Gauss-Krueger TM.
            gamma = GeoMath.Atan2d(z1.Imaginary, z1.Real);
            k = _b1 / Complex.Abs(z1);
            // JHS 154 has
            //
            //   phi' = asin(Math.Sin(xi') / Math.Cosh(eta')) (Krueger p 17 (25))
            //   lam = asin(tanh(eta') / Math.Cos(phi')
            //   psi = asinh(tan(phi'))
            double
              xip = y1.Real, etap = y1.Imaginary,
              s = Math.Sinh(etap),
              c = Math.Max(0, Math.Cos(xip)), // Math.Cos(pi/2) might be negative
              r = GeoMath.Hypot(s, c);
            if (r != 0)
            {
                lon = GeoMath.Atan2d(s, c); // Krueger p 17 (25)
                                            // Use Newton's method to solve for tau
                double
                  sxip = Math.Sin(xip),
                  tau = GeoMath.Tauf(sxip / r, _es);
                gamma += GeoMath.Atan2d(sxip * Math.Tanh(etap), c); // Krueger p 19 (31)
                lat = GeoMath.Atand(tau);
                // Note Math.Cos(phi') * Math.Cosh(eta') = r
                k *= Math.Sqrt(_e2m + _e2 / (1 + GeoMath.Square(tau))) *
                  GeoMath.Hypot(1, tau) * r;
            }
            else
            {
                lat = 90;
                lon = 0;
                k *= _c;
            }
            lat *= xisign;
            if (backside)
                lon = 180 - lon;
            lon *= etasign;
            lon = GeoMath.AngNormalize(lon + lon0);
            if (backside)
                gamma = 180 - gamma;
            gamma *= xisign * etasign;
            gamma = GeoMath.AngNormalize(gamma);
            k *= _k0;
        }

        /**
         * TransverseMercator::Forward without returning the convergence and scale.
         **********************************************************************/
        void Forward(double lon0, double lat, double lon,
                     out double x, out double y)
        {
            double gamma, k;
            Forward(lon0, lat, lon, out x, out y, out gamma, out k);
        }

        /**
         * TransverseMercator::Reverse without returning the convergence and scale.
         **********************************************************************/
        void Reverse(double lon0, double x, double y,
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
         * @return \e k0 central scale for the projection.  This is the value of \e
         *   k0 used in the ructor and is the scale on the central meridian.
         **********************************************************************/
        public double CentralScale()
        {
            return _k0;
        }
        ///@}

        /**
         * A global instantiation of TransverseMercator with the WGS84 ellipsoid
         * and the UTM scale factor.  However, unlike UTM, no false easting or
         * northing is added.
         **********************************************************************/
        public static TransverseMercator UTM()
        {
            return new TransverseMercator(Constants.WGS84_a, Constants.WGS84_f, Constants.UTM_k0);
        }
    }
}
