namespace Improbable.GeographicLib
{
    /**
    * \brief Properties of an ellipsoid
    *
    * This class returns various properties of the ellipsoid and converts
    * between various types of latitudes.  The latitude conversions are also
    * possible using the various projections supported by %GeographicLib; but
    * Ellipsoid provides more direct access (sometimes using private functions
    * of the projection classes).  Ellipsoid::RectifyingLatitude,
    * Ellipsoid::InverseRectifyingLatitude, and Ellipsoid::MeridianDistance
    * provide functionality which can be provided by the Geodesic class.
    * However Geodesic uses a series approximation (valid for abs \e f < 1/150),
    * whereas Ellipsoid computes these quantities using EllipticFunction which
    * provides accurate results even when \e f is large.  Use of this class
    * should be limited to &minus;3 < \e f < 3/4 (i.e., 1/4 < b/a < 4).
    *
    * Example of use:
    * \include example-Ellipsoid.cpp
    **********************************************************************/
    public class Ellipsoid
    {
//        static int numit_ = 10;
//        double stol_;
//        double _a, _f, _f1, _f12, _e2, _es, _e12, _n, _b;
//        TransverseMercator _tm;
//        EllipticFunction _ell;
//        AlbersEqualArea _au;
//
//        /** \name Constructor
//         **********************************************************************/
//        ///@{
//
//        /**
//         * Constructor for a ellipsoid with
//         *
//         * @param[in] a equatorial radius (meters).
//         * @param[in] f flattening of ellipsoid.  Setting \e f = 0 gives a sphere.
//         *   Negative \e f gives a prolate ellipsoid.
//         * @exception GeographicErr if \e a or (1 &minus; \e f) \e a is not
//         *   positive.
//         **********************************************************************/
//        public Ellipsoid(double a, double f)
//        {
//            stol_ = 0.01 * Math.Sqrt(GeoMath.Epsilon);
//            _a = a;
//            _f = f;
//            _f1 = 1 - _f;
//            _f12 = GeoMath.Square(_f1);
//            _e2 = _f * (2 - _f);
//            _es = (_f < 0 ? -1 : 1) * Math.Sqrt(Math.Abs(_e2));
//            _e12 = _e2 / (1 - _e2);
//            _n = _f / (2 - _f);
//            _b = _a * _f1;
//            _tm = new TransverseMercator(_a, _f, 1);
//            _ell = new EllipticFunction(-_e12);
//            _au = new AlbersEqualArea(_a, _f, 0, 1, 0, 1, 1);
//        }
//        ///@}
//
//        /** \name %Ellipsoid dimensions.
//         **********************************************************************/
//        ///@{
//
//        // These are the alpha and beta coefficients in the Krueger series from
//        // TransverseMercator.  Thy are used by RhumbSolve to compute
//        // (psi2-psi1)/(mu2-mu1).
//        public double ConformalToRectifyingCoeffs()
//        {
//            return _tm._alp;
//        }
//        public double RectifyingToConformalCoeffs()
//        {
//            return _tm._bet;
//        }
//
//        /**
//         * @return \e a the equatorial radius of the ellipsoid (meters).  This is
//         *   the value used in the ructor.
//         **********************************************************************/
//        public double MajorRadius()
//        {
//            return _a;
//        }
//
//        /**
//         * @return \e b the polar semi-axis (meters).
//         **********************************************************************/
//        public double MinorRadius()
//        {
//            return _b;
//        }
//
//        /**
//         * @return \e L the distance between the equator and a pole along a
//         *   meridian (meters).  For a sphere \e L = (&pi;/2) \e a.  The radius
//         *   of a sphere with the same meridian length is \e L / (&pi;/2).
//         **********************************************************************/
//        public double QuarterMeridian()
//        {
//            return _b * _ell.E();
//        }
//
//        /**
//         * @return \e A the total area of the ellipsoid (meters<sup>2</sup>).  For
//         *   a sphere \e A = 4&pi; <i>a</i><sup>2</sup>.  The radius of a sphere
//         *   with the same area is Math.Sqrt(\e A / (4&pi;)).
//         **********************************************************************/
//        public double Area()
//        {
//            return 4 * Math.PI *
//                  ((GeoMath.Square(_a) + GeoMath.Square(_b) *
//                    (_e2 == 0 ? 1 :
//                     (_e2 > 0 ? GeoMath.Atanh(Math.Sqrt(_e2)) : Math.Atan(Math.Sqrt(-_e2))) /
//                     Math.Sqrt(Math.Abs(_e2)))) / 2);
//        }
//
//        /**
//         * @return \e V the total volume of the ellipsoid (meters<sup>3</sup>).
//         *   For a sphere \e V = (4&pi; / 3) <i>a</i><sup>3</sup>.  The radius of
//         *   a sphere with the same volume is cbrt(\e V / (4&pi;/3)).
//         **********************************************************************/
//        public double Volume()
//        {
//            return (4 * Math.PI) * GeoMath.Square(_a) * _b / 3;
//        }
//        ///@}
//
//        /** \name %Ellipsoid shape
//         **********************************************************************/
//        ///@{
//
//        /**
//         * @return \e f = (\e a &minus; \e b) / \e a, the flattening of the
//         *   ellipsoid.  This is the value used in the ructor.  This is zero,
//         *   positive, or negative for a sphere, oblate ellipsoid, or prolate
//         *   ellipsoid.
//         **********************************************************************/
//        public double Flattening()
//        {
//            return _f;
//        }
//
//        /**
//         * @return \e f ' = (\e a &minus; \e b) / \e b, the second flattening of
//         *   the ellipsoid.  This is zero, positive, or negative for a sphere,
//         *   oblate ellipsoid, or prolate ellipsoid.
//         **********************************************************************/
//        public double SecondFlattening()
//        {
//            return _f / (1 - _f);
//        }
//
//        /**
//         * @return \e n = (\e a &minus; \e b) / (\e a + \e b), the third flattening
//         *   of the ellipsoid.  This is zero, positive, or negative for a sphere,
//         *   oblate ellipsoid, or prolate ellipsoid.
//         **********************************************************************/
//        public double ThirdFlattening()
//        {
//            return _n;
//        }
//
//        /**
//         * @return <i>e</i><sup>2</sup> = (<i>a</i><sup>2</sup> &minus;
//         *   <i>b</i><sup>2</sup>) / <i>a</i><sup>2</sup>, the eccentricity squared
//         *   of the ellipsoid.  This is zero, positive, or negative for a sphere,
//         *   oblate ellipsoid, or prolate ellipsoid.
//         **********************************************************************/
//        public double EccentricitySq()
//        {
//            return _e2;
//        }
//
//        /**
//         * @return <i>e'</i> <sup>2</sup> = (<i>a</i><sup>2</sup> &minus;
//         *   <i>b</i><sup>2</sup>) / <i>b</i><sup>2</sup>, the second eccentricity
//         *   squared of the ellipsoid.  This is zero, positive, or negative for a
//         *   sphere, oblate ellipsoid, or prolate ellipsoid.
//         **********************************************************************/
//        public double SecondEccentricitySq()
//        {
//            return _e12;
//        }
//
//        /**
//         * @return <i>e''</i> <sup>2</sup> = (<i>a</i><sup>2</sup> &minus;
//         *   <i>b</i><sup>2</sup>) / (<i>a</i><sup>2</sup> + <i>b</i><sup>2</sup>),
//         *   the third eccentricity squared of the ellipsoid.  This is zero,
//         *   positive, or negative for a sphere, oblate ellipsoid, or prolate
//         *   ellipsoid.
//         **********************************************************************/
//        public double ThirdEccentricitySq()
//        {
//            return _e2 / (2 - _e2);
//        }
//        ///@}
//
//        /** \name Latitude conversion.
//         **********************************************************************/
//        ///@{
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return &beta; the parametric latitude (degrees).
//         *
//         * The geographic latitude, &phi;, is the angle beween the equatorial
//         * plane and a vector normal to the surface of the ellipsoid.
//         *
//         * The parametric latitude (also called the reduced latitude), &beta;,
//         * allows the cartesian coordinated of a meridian to be expressed
//         * conveniently in parametric form as
//         * - \e R = \e a cos &beta;
//         * - \e Z = \e b sin &beta;
//         * .
//         * where \e a and \e b are the equatorial radius and the polar semi-axis.
//         * For a sphere &beta; = &phi;.
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.  The returned value
//         * &beta; lies in [&minus;90&deg;, 90&deg;].
//         **********************************************************************/
//        public double ParametricLatitude(double phi)
//        {
//            return GeoMath.Atand(_f1 * GeoMath.Atand(GeoMath.LatFix(phi)));
//        }
//
//        /**
//         * @param[in] beta the parametric latitude (degrees).
//         * @return &phi; the geographic latitude (degrees).
//         *
//         * &beta; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.  The returned value
//         * &phi; lies in [&minus;90&deg;, 90&deg;].
//         **********************************************************************/
//        public double InverseParametricLatitude(double beta)
//        {
//            return GeoMath.Atand(GeoMath.Atand(GeoMath.LatFix(beta)) / _f1);
//        }
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return &theta; the geocentric latitude (degrees).
//         *
//         * The geocentric latitude, &theta;, is the angle beween the equatorial
//         * plane and a line between the center of the ellipsoid and a point on the
//         * ellipsoid.  For a sphere &theta; = &phi;.
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.  The returned value
//         * &theta; lies in [&minus;90&deg;, 90&deg;].
//         **********************************************************************/
//        public double GeocentricLatitude(double phi)
//        {
//            return GeoMath.Atand(_f12 * GeoMath.Atand(GeoMath.LatFix(phi)));
//        }
//
//        /**
//         * @param[in] theta the geocentric latitude (degrees).
//         * @return &phi; the geographic latitude (degrees).
//         *
//         * &theta; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.  The returned value
//         * &phi; lies in [&minus;90&deg;, 90&deg;].
//         **********************************************************************/
//        public double InverseGeocentricLatitude(double theta)
//        {
//            return GeoMath.Atand(GeoMath.Atand(GeoMath.LatFix(theta)) / _f12);
//        }
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return &mu; the rectifying latitude (degrees).
//         *
//         * The rectifying latitude, &mu;, has the property that the distance along
//         * a meridian of the ellipsoid between two points with rectifying latitudes
//         * &mu;<sub>1</sub> and &mu;<sub>2</sub> is equal to
//         * (&mu;<sub>2</sub> - &mu;<sub>1</sub>) \e L / 90&deg;,
//         * where \e L = QuarterMeridian().  For a sphere &mu; = &phi;.
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.  The returned value
//         * &mu; lies in [&minus;90&deg;, 90&deg;].
//         **********************************************************************/
//        public double RectifyingLatitude(double phi)
//        {
//            return Math.Abs(phi) == 90 ? phi : 90 * MeridianDistance(phi) / QuarterMeridian();
//        }
//
//        /**
//         * @param[in] mu the rectifying latitude (degrees).
//         * @return &phi; the geographic latitude (degrees).
//         *
//         * &mu; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.  The returned value
//         * &phi; lies in [&minus;90&deg;, 90&deg;].
//         **********************************************************************/
//        public double InverseRectifyingLatitude(double mu)
//        {
//            if (Math.Abs(mu) == 90)
//                return mu;
//            return InverseParametricLatitude(_ell.Einv(mu * _ell.E() / 90) /
//                                             GeoMath.Degree);
//        }
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return &xi; the authalic latitude (degrees).
//         *
//         * The authalic latitude, &xi;, has the property that the area of the
//         * ellipsoid between two circles with authalic latitudes
//         * &xi;<sub>1</sub> and &xi;<sub>2</sub> is equal to (sin
//         * &xi;<sub>2</sub> - sin &xi;<sub>1</sub>) \e A / 2, where \e A
//         * = Area().  For a sphere &xi; = &phi;.
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.  The returned value
//         * &xi; lies in [&minus;90&deg;, 90&deg;].
//         **********************************************************************/
//        public double AuthalicLatitude(double phi)
//        {
//            return GeoMath.Atand(_au.txif(GeoMath.Atand(GeoMath.LatFix(phi))));
//        }
//
//        /**
//         * @param[in] xi the authalic latitude (degrees).
//         * @return &phi; the geographic latitude (degrees).
//         *
//         * &xi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.  The returned value
//         * &phi; lies in [&minus;90&deg;, 90&deg;].
//         **********************************************************************/
//        public double InverseAuthalicLatitude(double xi)
//        {
//            return GeoMath.Atand(_au.tphif(GeoMath.Atand(GeoMath.LatFix(xi))));
//        }
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return &chi; the conformal latitude (degrees).
//         *
//         * The conformal latitude, &chi;, gives the mapping of the ellipsoid to a
//         * sphere which which is conformal (angles are preserved) and in which the
//         * equator of the ellipsoid maps to the equator of the sphere.  For a
//         * sphere &chi; = &phi;.
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.  The returned value
//         * &chi; lies in [&minus;90&deg;, 90&deg;].
//         **********************************************************************/
//        public double ConformalLatitude(double phi)
//        {
//            return GeoMath.Atand(GeoMath.Taupf(GeoMath.Atand(GeoMath.LatFix(phi)), _es));
//        }
//
//        /**
//         * @param[in] chi the conformal latitude (degrees).
//         * @return &phi; the geographic latitude (degrees).
//         *
//         * &chi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.  The returned value
//         * &phi; lies in [&minus;90&deg;, 90&deg;].
//         **********************************************************************/
//        public double InverseConformalLatitude(double chi)
//        {
//            return GeoMath.Atand(GeoMath.Tauf(GeoMath.Atand(GeoMath.LatFix(chi)), _es));
//        }
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return &psi; the isometric latitude (degrees).
//         *
//         * The isometric latitude gives the mapping of the ellipsoid to a plane
//         * which which is conformal (angles are preserved) and in which the equator
//         * of the ellipsoid maps to a straight line of ant scale; this mapping
//         * defines the Mercator projection.  For a sphere &psi; =
//         * sinh<sup>&minus;1</sup> tan &phi;.
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the result is
//         * undefined if this condition does not hold.  The value returned for &phi;
//         * = &plusmn;90&deg; is some (positive or negative) large but finite value,
//         * such that InverseIsometricLatitude returns the original value of &phi;.
//         **********************************************************************/
//        public double IsometricLatitude(double phi)
//        {
//            return GeoMath.Asinh(GeoMath.Taupf(GeoMath.Atand(GeoMath.LatFix(phi)), _es)) / GeoMath.Degree;
//        }
//
//        /**
//         * @param[in] psi the isometric latitude (degrees).
//         * @return &phi; the geographic latitude (degrees).
//         *
//         * The returned value &phi; lies in [&minus;90&deg;, 90&deg;].  For a
//         * sphere &phi; = tan<sup>&minus;1</sup> sinh &psi;.
//         **********************************************************************/
//        public double InverseIsometricLatitude(double psi)
//        {
//            return GeoMath.Atand(GeoMath.Tauf(Math.Sinh(psi * GeoMath.Degree), _es));
//        }
//        ///@}
//
//        /** \name Other quantities.
//         **********************************************************************/
//        ///@{
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return \e R = \e a cos &beta; the radius of a circle of latitude
//         *   &phi; (meters).  \e R (&pi;/180&deg;) gives meters per degree
//         *   longitude measured along a circle of latitude.
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.
//         **********************************************************************/
//        public double CircleRadius(double phi)
//        {
//            return Math.Abs(phi) == 90 ? 0 :
//            // a * cos(beta)
//            _a / GeoMath.Hypot(1, _f1 * GeoMath.Atand(GeoMath.LatFix(phi)));
//        }
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return \e Z = \e b sin &beta; the distance of a circle of latitude
//         *   &phi; from the equator measured parallel to the ellipsoid axis
//         *   (meters).
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.
//         **********************************************************************/
//        public double CircleHeight(double phi)
//        {
//            double tbeta = _f1 * GeoMath.Atand(phi);
//            // b * sin(beta)
//            return _b * tbeta / GeoMath.Hypot(1, _f1 * GeoMath.Atand(GeoMath.LatFix(phi)));
//        }
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return \e s the distance along a meridian
//         *   between the equator and a point of latitude &phi; (meters).  \e s is
//         *   given by \e s = &mu; \e L / 90&deg;, where \e L =
//         *   QuarterMeridian()).
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.
//         **********************************************************************/
//        public double MeridianDistance(double phi)
//        {
//            return _b * _ell.Ed(ParametricLatitude(phi));
//        }
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return &rho; the meridional radius of curvature of the ellipsoid at
//         *   latitude &phi; (meters); this is the curvature of the meridian.  \e
//         *   rho is given by &rho; = (180&deg;/&pi;) d\e s / d&phi;,
//         *   where \e s = MeridianDistance(); thus &rho; (&pi;/180&deg;)
//         *   gives meters per degree latitude measured along a meridian.
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.
//         **********************************************************************/
//        public double MeridionalCurvatureRadius(double phi)
//        {
//            double v = 1 - _e2 * GeoMath.Square(GeoMath.Sind(GeoMath.LatFix(phi)));
//            return _a * (1 - _e2) / (v * Math.Sqrt(v));
//        }
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @return &nu; the transverse radius of curvature of the ellipsoid at
//         *   latitude &phi; (meters); this is the curvature of a curve on the
//         *   ellipsoid which also lies in a plane perpendicular to the ellipsoid
//         *   and to the meridian.  &nu; is related to \e R = CircleRadius() by \e
//         *   R = &nu; cos &phi;.
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the
//         * result is undefined if this condition does not hold.
//         **********************************************************************/
//        public double TransverseCurvatureRadius(double phi)
//        {
//            double v = 1 - _e2 * GeoMath.Square(GeoMath.Sind(GeoMath.LatFix(phi)));
//            return _a / Math.Sqrt(v);
//        }
//
//        /**
//         * @param[in] phi the geographic latitude (degrees).
//         * @param[in] azi the angle between the meridian and the normal section
//         *   (degrees).
//         * @return the radius of curvature of the ellipsoid in the normal
//         *   section at latitude &phi; inclined at an angle \e azi to the
//         *   meridian (meters).
//         *
//         * &phi; must lie in the range [&minus;90&deg;, 90&deg;]; the result is
//         * undefined this condition does not hold.
//         **********************************************************************/
//        public double NormalCurvatureRadius(double phi, double azi)
//        {
//            double calp, salp,
//            v = 1 - _e2 * GeoMath.Square(GeoMath.Sind(GeoMath.LatFix(phi)));
//            GeoMath.Sincosd(azi, out salp, out calp);
//            return _a / (Math.Sqrt(v) * (GeoMath.Square(calp) * v / (1 - _e2) + GeoMath.Square(salp)));
//        }
//        ///@}
//
//        /** \name Eccentricity conversions.
//         **********************************************************************/
//        ///@{
//
//        /**
//         * @param[in] fp = \e f ' = (\e a &minus; \e b) / \e b, the second
//         *   flattening.
//         * @return \e f = (\e a &minus; \e b) / \e a, the flattening.
//         *
//         * \e f ' should lie in (&minus;1, &infin;).
//         * The returned value \e f lies in (&minus;&infin;, 1).
//         **********************************************************************/
//        static public double SecondFlatteningToFlattening(double fp)
//        {
//            return fp / (1 + fp);
//        }
//
//        /**
//         * @param[in] f = (\e a &minus; \e b) / \e a, the flattening.
//         * @return \e f ' = (\e a &minus; \e b) / \e b, the second flattening.
//         *
//         * \e f should lie in (&minus;&infin;, 1).
//         * The returned value \e f ' lies in (&minus;1, &infin;).
//         **********************************************************************/
//        static public double FlatteningToSecondFlattening(double f)
//        {
//            return f / (1 - f);
//        }
//
//        /**
//         * @param[in] n = (\e a &minus; \e b) / (\e a + \e b), the third
//         *   flattening.
//         * @return \e f = (\e a &minus; \e b) / \e a, the flattening.
//         *
//         * \e n should lie in (&minus;1, 1).
//         * The returned value \e f lies in (&minus;&infin;, 1).
//         **********************************************************************/
//        static public double ThirdFlatteningToFlattening(double n)
//        {
//            return 2 * n / (1 + n);
//        }
//
//        /**
//         * @param[in] f = (\e a &minus; \e b) / \e a, the flattening.
//         * @return \e n = (\e a &minus; \e b) / (\e a + \e b), the third
//         *   flattening.
//         *
//         * \e f should lie in (&minus;&infin;, 1).
//         * The returned value \e n lies in (&minus;1, 1).
//         **********************************************************************/
//        static public double FlatteningToThirdFlattening(double f)
//        {
//            return f / (2 - f);
//        }
//
//        /**
//         * @param[in] e2 = <i>e</i><sup>2</sup> = (<i>a</i><sup>2</sup> &minus;
//         *   <i>b</i><sup>2</sup>) / <i>a</i><sup>2</sup>, the eccentricity
//         *   squared.
//         * @return \e f = (\e a &minus; \e b) / \e a, the flattening.
//         *
//         * <i>e</i><sup>2</sup> should lie in (&minus;&infin;, 1).
//         * The returned value \e f lies in (&minus;&infin;, 1).
//         **********************************************************************/
//        static public double EccentricitySqToFlattening(double e2)
//        {
//            return e2 / (Math.Sqrt(1 - e2) + 1);
//        }
//
//        /**
//         * @param[in] f = (\e a &minus; \e b) / \e a, the flattening.
//         * @return <i>e</i><sup>2</sup> = (<i>a</i><sup>2</sup> &minus;
//         *   <i>b</i><sup>2</sup>) / <i>a</i><sup>2</sup>, the eccentricity
//         *   squared.
//         *
//         * \e f should lie in (&minus;&infin;, 1).
//         * The returned value <i>e</i><sup>2</sup> lies in (&minus;&infin;, 1).
//         **********************************************************************/
//        static public double FlatteningToEccentricitySq(double f)
//        {
//            return f * (2 - f);
//        }
//
//        /**
//         * @param[in] ep2 = <i>e'</i> <sup>2</sup> = (<i>a</i><sup>2</sup> &minus;
//         *   <i>b</i><sup>2</sup>) / <i>b</i><sup>2</sup>, the second eccentricity
//         *   squared.
//         * @return \e f = (\e a &minus; \e b) / \e a, the flattening.
//         *
//         * <i>e'</i> <sup>2</sup> should lie in (&minus;1, &infin;).
//         * The returned value \e f lies in (&minus;&infin;, 1).
//         **********************************************************************/
//        static public double SecondEccentricitySqToFlattening(double ep2)
//        {
//            return ep2 / (Math.Sqrt(1 + ep2) + 1 + ep2);
//        }
//
//        /**
//         * @param[in] f = (\e a &minus; \e b) / \e a, the flattening.
//         * @return <i>e'</i> <sup>2</sup> = (<i>a</i><sup>2</sup> &minus;
//         *   <i>b</i><sup>2</sup>) / <i>b</i><sup>2</sup>, the second eccentricity
//         *   squared.
//         *
//         * \e f should lie in (&minus;&infin;, 1).
//         * The returned value <i>e'</i> <sup>2</sup> lies in (&minus;1, &infin;).
//         **********************************************************************/
//        static public double FlatteningToSecondEccentricitySq(double f)
//        {
//            return f * (2 - f) / GeoMath.Square(1 - f);
//        }
//
//        /**
//         * @param[in] epp2 = <i>e''</i> <sup>2</sup> = (<i>a</i><sup>2</sup>
//         *   &minus; <i>b</i><sup>2</sup>) / (<i>a</i><sup>2</sup> +
//         *   <i>b</i><sup>2</sup>), the third eccentricity squared.
//         * @return \e f = (\e a &minus; \e b) / \e a, the flattening.
//         *
//         * <i>e''</i> <sup>2</sup> should lie in (&minus;1, 1).
//         * The returned value \e f lies in (&minus;&infin;, 1).
//         **********************************************************************/
//        static public double ThirdEccentricitySqToFlattening(double epp2)
//        {
//            return 2 * epp2 / (Math.Sqrt((1 - epp2) * (1 + epp2)) + 1 + epp2);
//        }
//
//        /**
//         * @param[in] f = (\e a &minus; \e b) / \e a, the flattening.
//         * @return <i>e''</i> <sup>2</sup> = (<i>a</i><sup>2</sup> &minus;
//         *   <i>b</i><sup>2</sup>) / (<i>a</i><sup>2</sup> + <i>b</i><sup>2</sup>),
//         *   the third eccentricity squared.
//         *
//         * \e f should lie in (&minus;&infin;, 1).
//         * The returned value <i>e''</i> <sup>2</sup> lies in (&minus;1, 1).
//         **********************************************************************/
//        static public double FlatteningToThirdEccentricitySq(double f)
//        {
//            return f * (2 - f) / (1 + GeoMath.Square(1 - f));
//        }
//
//        ///@}
//
//        /**
//         * A global instantiation of Ellipsoid with the parameters for the WGS84
//         * ellipsoid.
//         **********************************************************************/
//        public static Ellipsoid WGS84()
//        {
//            return new Ellipsoid(Constants.WGS84_a, Constants.WGS84_f);
//        }
    }
}
