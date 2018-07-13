using System;

namespace Improbable.GeographicLib
{
    public class Geodesic
    {
        internal static readonly int maxit1_ = 20;
        internal static readonly int maxit2_ = maxit1_ + GeoMath.Digits + 10;
        internal static readonly double tiny_ = Math.Sqrt(GeoMath.Min);
        internal static readonly double tol0_ = GeoMath.Epsilon;
        internal static readonly double tol1_ = 200 * tol0_;
        internal static readonly double tol2_ = Math.Sqrt(tol0_);
        internal static readonly double tolb_ = tol0_ * tol2_;
        internal static readonly double xthresh_ = 1000 * tol2_;

        internal double _a, _f, _f1, _e2, _ep2, _b, _c2, _n, _etol2;
        double[] _A3x, _C3x, _C4x;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NetStandardGeographicLib.Geodesic"/> class.
        /// </summary>
        /// <param name="a">Equatorial radius (meters).</param>
        /// <param name="f">
        /// Flattening of ellipsoid (f = 0 gives a sphere, -f gives a prolate ellipsoid).
        /// </param>
        public Geodesic(double a, double f)
        {
            _a = a;
            _f = f;
            _f1 = 1 - _f;
            _e2 = _f * (2 - _f);
            _ep2 = _e2 / GeoMath.Square(_f1);
            _n = _f / (2 - _f);
            _b = _a * _f1;

            _c2 = (GeoMath.Square(_a) + GeoMath.Square(_b) *
                   (Math.Abs(_e2) < GeoMath.Epsilon ? 1 :
                    (_e2 > 0 ? GeoMath.Atanh(Math.Sqrt(_e2)) :
                     Math.Atan(Math.Sqrt(-_e2))) /
                      Math.Sqrt(Math.Abs(_e2)))) / 2;

            _etol2 = 0.1 * tol2_ /
                      Math.Sqrt(Math.Max(0.001, Math.Abs(_f)) *
                                Math.Min(1.0, 1 - _f / 2) / 2);

            if (!(GeoMath.IsFinite(_a) && _a > 0))
                throw new GeographicException("Equatorial radius is not positive");
            if (!(GeoMath.IsFinite(_b) && _b > 0))
                throw new GeographicException("Polar semi-axis is not positive");

            _A3x = GeodesicCoeff.GenerateA3(_n);
            _C3x = GeodesicCoeff.GenerateC3(_n);
            _C4x = GeodesicCoeff.GenerateC4(_n);
        }

        /** \name Direct geodesic problem specified in terms of distance.
        **********************************************************************/
        ///@{
        /**
         * Solve the direct geodesic problem where the length of the geodesic
         * is specified in terms of distance.
         *
         * @param[in] lat1 latitude of point 1 (degrees).
         * @param[in] lon1 longitude of point 1 (degrees).
         * @param[in] azi1 azimuth at point 1 (degrees).
         * @param[in] s12 distance between point 1 and point 2 (meters); it can be
         *   negative.
         * @param[out] lat2 latitude of point 2 (degrees).
         * @param[out] lon2 longitude of point 2 (degrees).
         * @param[out] azi2 (forward) azimuth at point 2 (degrees).
         * @param[out] m12 reduced length of geodesic (meters).
         * @param[out] M12 geodesic scale of point 2 relative to point 1
         *   (dimensionless).
         * @param[out] M21 geodesic scale of point 1 relative to point 2
         *   (dimensionless).
         * @param[out] S12 area under the geodesic (meters<sup>2</sup>).
         * @return \e a12 arc length of between point 1 and point 2 (degrees).
         *
         * \e lat1 should be in the range [&minus;90&deg;, 90&deg;].  The values of
         * \e lon2 and \e azi2 returned are in the range [&minus;180&deg;,
         * 180&deg;].
         *
         * If either point is at a pole, the azimuth is defined by keeping the
         * longitude fixed, writing \e lat = &plusmn;(90&deg; &minus; &epsilon;),
         * and taking the limit &epsilon; &rarr; 0+.  An arc length greater that
         * 180&deg; signifies a geodesic which is not a shortest path.  (For a
         * prolate ellipsoid, an additional condition is necessary for a shortest
         * path: the longitudinal extent must not exceed of 180&deg;.)
         *
         * The following functions are overloaded versions of Geodesic::Direct
         * which omit some of the output parameters.  Note, however, that the arc
         * length is always computed and returned as the function value.
         **********************************************************************/
        public double Direct(double lat1, double lon1, double azi1, double s12,
                             out double lat2, out double lon2, out double azi2,
                             out double m12, out double M12, out double M21, out double S12)
        {
            double t;

            return GenDirect(lat1, lon1, azi1, false, s12,
                             GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH |
                             GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE | GeodesicMask.AREA,
                             out lat2, out lon2, out azi2, out t, out m12, out M12, out M21, out S12);
        }

        public double Direct(double lat1, double lon1, double azi1, double s12,
                             out double lat2, out double lon2)
        {
            double t;

            return GenDirect(lat1, lon1, azi1, false, s12,
                             GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE,
                             out lat2, out lon2, out t, out t, out t, out t, out t, out t);
        }

        public double Direct(double lat1, double lon1, double azi1, double s12,
                             out double lat2, out double lon2, out double azi2)
        {
            double t;

            return GenDirect(lat1, lon1, azi1, false, s12,
                             GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH,
                             out lat2, out lon2, out azi2, out t, out t, out t, out t, out t);
        }

        public double Direct(double lat1, double lon1, double azi1, double s12,
                             out double lat2, out double lon2, out double azi2, out double m12)
        {
            double t;

            return GenDirect(lat1, lon1, azi1, false, s12,
                             GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE |
                             GeodesicMask.AZIMUTH | GeodesicMask.REDUCEDLENGTH,
                             out lat2, out lon2, out azi2, out t, out m12, out t, out t, out t);
        }

        public double Direct(double lat1, double lon1, double azi1, double s12,
                             out double lat2, out double lon2, out double azi2,
                             out double M12, out double M21)
        {
            double t;

            return GenDirect(lat1, lon1, azi1, false, s12,
                             GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE |
                             GeodesicMask.AZIMUTH | GeodesicMask.GEODESICSCALE,
                             out lat2, out lon2, out azi2, out t, out t, out M12, out M21, out t);
        }

        public double Direct(double lat1, double lon1, double azi1, double s12,
                             out double lat2, out double lon2, out double azi2,
                             out double m12, out double M12, out double M21)
        {
            double t;

            return GenDirect(lat1, lon1, azi1, false, s12,
                             GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH |
                             GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE,
                             out lat2, out lon2, out azi2, out t, out m12, out M12, out M21, out t);
        }

        ///@}

        /** \name Direct geodesic problem specified in terms of arc length.
         **********************************************************************/
        ///@{
        /**
         * Solve the direct geodesic problem where the length of the geodesic
         * is specified in terms of arc length.
         *
         * @param[in] lat1 latitude of point 1 (degrees).
         * @param[in] lon1 longitude of point 1 (degrees).
         * @param[in] azi1 azimuth at point 1 (degrees).
         * @param[in] a12 arc length between point 1 and point 2 (degrees); it can
         *   be negative.
         * @param[out] lat2 latitude of point 2 (degrees).
         * @param[out] lon2 longitude of point 2 (degrees).
         * @param[out] azi2 (forward) azimuth at point 2 (degrees).
         * @param[out] s12 distance between point 1 and point 2 (meters).
         * @param[out] m12 reduced length of geodesic (meters).
         * @param[out] M12 geodesic scale of point 2 relative to point 1
         *   (dimensionless).
         * @param[out] M21 geodesic scale of point 1 relative to point 2
         *   (dimensionless).
         * @param[out] S12 area under the geodesic (meters<sup>2</sup>).
         *
         * \e lat1 should be in the range [&minus;90&deg;, 90&deg;].  The values of
         * \e lon2 and \e azi2 returned are in the range [&minus;180&deg;,
         * 180&deg;].
         *
         * If either point is at a pole, the azimuth is defined by keeping the
         * longitude fixed, writing \e lat = &plusmn;(90&deg; &minus; &epsilon;),
         * and taking the limit &epsilon; &rarr; 0+.  An arc length greater that
         * 180&deg; signifies a geodesic which is not a shortest path.  (For a
         * prolate ellipsoid, an additional condition is necessary for a shortest
         * path: the longitudinal extent must not exceed of 180&deg;.)
         *
         * The following functions are overloaded versions of Geodesic::Direct
         * which omit some of the output parameters.
         **********************************************************************/
        public void ArcDirect(double lat1, double lon1, double azi1, double a12,
                       out double lat2, out double lon2, out double azi2, out double s12,
                       out double m12, out double M12, out double M21, out double S12)
        {
            GenDirect(lat1, lon1, azi1, true, a12,
                      GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH | GeodesicMask.DISTANCE |
                      GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE | GeodesicMask.AREA,
                      out lat2, out lon2, out azi2, out s12, out m12, out M12, out M21, out S12);
        }


        public void ArcDirect(double lat1, double lon1, double azi1, double a12,
                   out double lat2, out double lon2)
        {
            double t;

            GenDirect(lat1, lon1, azi1, true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE,
                      out lat2, out lon2, out t, out t, out t, out t, out t, out t);
        }

        /**
         * See the documentation for Geodesic::ArcDirect.
         **********************************************************************/
        public void ArcDirect(double lat1, double lon1, double azi1, double a12,
                   out double lat2, out double lon2, out double azi2)
        {
            double t;
            GenDirect(lat1, lon1, azi1, true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH,
                      out lat2, out lon2, out azi2, out t, out t, out t, out t, out t);
        }

        /**
         * See the documentation for Geodesic::ArcDirect.
         **********************************************************************/
        public void ArcDirect(double lat1, double lon1, double azi1, double a12,
                   out double lat2, out double lon2, out double azi2, out double s12)
        {
            double t;
            GenDirect(lat1, lon1, azi1, true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH | GeodesicMask.DISTANCE,
                      out lat2, out lon2, out azi2, out s12, out t, out t, out t, out t);
        }

        /**
         * See the documentation for Geodesic::ArcDirect.
         **********************************************************************/
        public void ArcDirect(double lat1, double lon1, double azi1, double a12,
                   out double lat2, out double lon2, out double azi2,
                   out double s12, out double m12)
        {
            double t;
            GenDirect(lat1, lon1, azi1, true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH | GeodesicMask.DISTANCE |

                        GeodesicMask.REDUCEDLENGTH,
                      out lat2, out lon2, out azi2, out s12, out m12, out t, out t, out t);
        }

        /**
         * See the documentation for Geodesic::ArcDirect.
         **********************************************************************/
        public void ArcDirect(double lat1, double lon1, double azi1, double a12,
                   out double lat2, out double lon2, out double azi2, out double s12,
                   out double M12, out double M21)
        {
            double t;
            GenDirect(lat1, lon1, azi1, true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH | GeodesicMask.DISTANCE |

                        GeodesicMask.GEODESICSCALE,
                      out lat2, out lon2, out azi2, out s12, out t, out M12, out M21, out t);
        }

        /**
         * See the documentation for Geodesic::ArcDirect.
         **********************************************************************/
        public void ArcDirect(double lat1, double lon1, double azi1, double a12,
                   out double lat2, out double lon2, out double azi2, out double s12,
                   out double m12, out double M12, out double M21)
        {
            double t;
            GenDirect(lat1, lon1, azi1, true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH | GeodesicMask.DISTANCE |

                        GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE,
                        out lat2, out lon2, out azi2, out s12, out m12, out M12, out M21, out t);
        }
        ///@}

        /** \name General version of the direct geodesic solution.
         **********************************************************************/
        ///@{

        /**
         * The general direct geodesic problem.  Geodesic::Direct and
         * Geodesic::ArcDirect are defined in terms of this function.
         *
         * @param[in] lat1 latitude of point 1 (degrees).
         * @param[in] lon1 longitude of point 1 (degrees).
         * @param[in] azi1 azimuth at point 1 (degrees).
         * @param[in] arcmode boolean flag determining the meaning of the \e
         *   s12_a12.
         * @param[in] s12_a12 if \e arcmode is false, this is the distance between
         *   point 1 and point 2 (meters); otherwise it is the arc length between
         *   point 1 and point 2 (degrees); it can be negative.
         * @param[in] outmask a bitor'ed combination of Geodesic::mask values
         *   specifying which of the following parameters should be set.
         * @param[out] lat2 latitude of point 2 (degrees).
         * @param[out] lon2 longitude of point 2 (degrees).
         * @param[out] azi2 (forward) azimuth at point 2 (degrees).
         * @param[out] s12 distance between point 1 and point 2 (meters).
         * @param[out] m12 reduced length of geodesic (meters).
         * @param[out] M12 geodesic scale of point 2 relative to point 1
         *   (dimensionless).
         * @param[out] M21 geodesic scale of point 1 relative to point 2
         *   (dimensionless).
         * @param[out] S12 area under the geodesic (meters<sup>2</sup>).
         * @return \e a12 arc length of between point 1 and point 2 (degrees).
         *
         * The Geodesic::mask values possible for \e outmask are
         * - \e outmask |= Geodesic::LATITUDE for the latitude \e lat2;
         * - \e outmask |= Geodesic::LONGITUDE for the latitude \e lon2;
         * - \e outmask |= Geodesic::AZIMUTH for the latitude \e azi2;
         * - \e outmask |= Geodesic::DISTANCE for the distance \e s12;
         * - \e outmask |= Geodesic::REDUCEDLENGTH for the reduced length \e
         *   m12;
         * - \e outmask |= Geodesic::GEODESICSCALE for the geodesic scales \e
         *   M12 and \e M21;
         * - \e outmask |= Geodesic::AREA for the area \e S12;
         * - \e outmask |= Geodesic::ALL for all of the above;
         * - \e outmask |= Geodesic::LONG_UNROLL to unroll \e lon2 instead of
         *   wrapping it into the range [&minus;180&deg;, 180&deg;].
         * .
         * The function value \e a12 is always computed and returned and this
         * equals \e s12_a12 is \e arcmode is true.  If \e outmask includes
         * Geodesic::DISTANCE and \e arcmode is false, then \e s12 = \e s12_a12.
         * It is not necessary to include Geodesic::DISTANCE_IN in \e outmask; this
         * is automatically included is \e arcmode is false.
         *
         * With the Geodesic::LONG_UNROLL bit seout t, the quantity \e lon2 &minus; \e
         * lon1 indicates how many times and in what sense the geodesic encircles
         * the ellipsoid.
         **********************************************************************/
        public double GenDirect(
                double lat1, double lon1, double azi1,
                bool arcmode, double s12_a12, int outmask,
                out double lat2, out double lon2, out double azi2,
                out double s12, out double m12, out double M12,
                out double M21, out double S12)
        {
            // Automatically supply DISTANCE_IN if necessary
            if (!arcmode) outmask |= GeodesicMask.DISTANCE_IN;
            return new GeodesicLine(this, lat1, lon1, azi1, outmask)
              .                         // Note the dot!
              GenPosition(arcmode, s12_a12, outmask,
                          out lat2, out lon2, out azi2, out s12, out m12, out M12, out M21, out S12);
        }

        ///@}

        /** \name Inverse geodesic problem.
         **********************************************************************/
        ///@{
        /**
         * Solve the inverse geodesic problem.
         *
         * @param[in] lat1 latitude of point 1 (degrees).
         * @param[in] lon1 longitude of point 1 (degrees).
         * @param[in] lat2 latitude of point 2 (degrees).
         * @param[in] lon2 longitude of point 2 (degrees).
         * @param[out] s12 distance between point 1 and point 2 (meters).
         * @param[out] azi1 azimuth at point 1 (degrees).
         * @param[out] azi2 (forward) azimuth at point 2 (degrees).
         * @param[out] m12 reduced length of geodesic (meters).
         * @param[out] M12 geodesic scale of point 2 relative to point 1
         *   (dimensionless).
         * @param[out] M21 geodesic scale of point 1 relative to point 2
         *   (dimensionless).
         * @param[out] S12 area under the geodesic (meters<sup>2</sup>).
         * @return \e a12 arc length of between point 1 and point 2 (degrees).
         *
         * \e lat1 and \e lat2 should be in the range [&minus;90&deg;, 90&deg;].
         * The values of \e azi1 and \e azi2 returned are in the range
         * [&minus;180&deg;, 180&deg;].
         *
         * If either point is at a pole, the azimuth is defined by keeping the
         * longitude fixed, writing \e lat = &plusmn;(90&deg; &minus; &epsilon;),
         * and taking the limit &epsilon; &rarr; 0+.
         *
         * The solution to the inverse problem is found using Newton's method.  If
         * this fails to converge (this is very unlikely in geodetic applications
         * but does occur for very eccentric ellipsoids), then the bisection method
         * is used to refine the solution.
         *
         * The following functions are overloaded versions of Geodesic::Inverse
         * which omit some of the output parameters.  Note, however, that the arc
         * length is always computed and returned as the function value.
         **********************************************************************/
        public double Inverse(double lat1, double lon1, double lat2, double lon2,
                           out double s12, out double azi1, out double azi2, out double m12,
                           out double M12, out double M21, out double S12)
        {
            return GenInverse(lat1, lon1, lat2, lon2,
                              GeodesicMask.DISTANCE | GeodesicMask.AZIMUTH |
                              GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE | GeodesicMask.AREA,
                              out s12, out azi1, out azi2, out m12, out M12, out M21, out S12);
        }

        /**
         * See the documentation for Geodesic::Inverse.
         **********************************************************************/
        public double Inverse(double lat1, double lon1, double lat2, double lon2,
                           out double s12)
        {
            double t;
            return GenInverse(lat1, lon1, lat2, lon2,
                              GeodesicMask.DISTANCE,
                              out s12, out t, out t, out t, out t, out t, out t);
        }

        /**
         * See the documentation for Geodesic::Inverse.
         **********************************************************************/
        public double Inverse(double lat1, double lon1, double lat2, double lon2,
                           out double azi1, out double azi2)
        {
            double t;
            return GenInverse(lat1, lon1, lat2, lon2,
                              GeodesicMask.AZIMUTH,
                              out t, out azi1, out azi2, out t, out t, out t, out t);
        }

        /**
         * See the documentation for Geodesic::Inverse.
         **********************************************************************/
        public double Inverse(double lat1, double lon1, double lat2, double lon2,
                           out double s12, out double azi1, out double azi2)
        {
            double t;
            return GenInverse(lat1, lon1, lat2, lon2,
                              GeodesicMask.DISTANCE | GeodesicMask.AZIMUTH,
                              out s12, out azi1, out azi2, out t, out t, out t, out t);
        }

        /**
         * See the documentation for Geodesic::Inverse.
         **********************************************************************/
        public double Inverse(double lat1, double lon1, double lat2, double lon2,
                           out double s12, out double azi1, out double azi2, out double m12)
        {
            double t;
            return GenInverse(lat1, lon1, lat2, lon2,
                              GeodesicMask.DISTANCE | GeodesicMask.AZIMUTH | GeodesicMask.REDUCEDLENGTH,
                              out s12, out azi1, out azi2, out m12, out t, out t, out t);
        }

        /**
         * See the documentation for Geodesic::Inverse.
         **********************************************************************/
        public double Inverse(double lat1, double lon1, double lat2, double lon2,
                           out double s12, out double azi1, out double azi2,
                           out double M12, out double M21)
        {
            double t;
            return GenInverse(lat1, lon1, lat2, lon2,
                              GeodesicMask.DISTANCE | GeodesicMask.AZIMUTH | GeodesicMask.GEODESICSCALE,
                              out s12, out azi1, out azi2, out t, out M12, out M21, out t);
        }

        /**
         * See the documentation for Geodesic::Inverse.
         **********************************************************************/
        public double Inverse(double lat1, double lon1, double lat2, double lon2,
                           out double s12, out double azi1, out double azi2, out double m12,
                           out double M12, out double M21)
        {
            double t;
            return GenInverse(lat1, lon1, lat2, lon2,
                              GeodesicMask.DISTANCE | GeodesicMask.AZIMUTH |
                              GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE,
                              out s12, out azi1, out azi2, out m12, out M12, out M21, out t);
        }
        ///@}

        /** \name General version of inverse geodesic solution.
         **********************************************************************/
        ///@{
        /**
         * The general inverse geodesic calculation.  Geodesic::Inverse is defined
         * in terms of this function.
         *
         * @param[in] lat1 latitude of point 1 (degrees).
         * @param[in] lon1 longitude of point 1 (degrees).
         * @param[in] lat2 latitude of point 2 (degrees).
         * @param[in] lon2 longitude of point 2 (degrees).
         * @param[in] outmask a bitor'ed combination of Geodesic::mask values
         *   specifying which of the following parameters should be set.
         * @param[out] s12 distance between point 1 and point 2 (meters).
         * @param[out] azi1 azimuth at point 1 (degrees).
         * @param[out] azi2 (forward) azimuth at point 2 (degrees).
         * @param[out] m12 reduced length of geodesic (meters).
         * @param[out] M12 geodesic scale of point 2 relative to point 1
         *   (dimensionless).
         * @param[out] M21 geodesic scale of point 1 relative to point 2
         *   (dimensionless).
         * @param[out] S12 area under the geodesic (meters<sup>2</sup>).
         * @return \e a12 arc length of between point 1 and point 2 (degrees).
         *
         * The Geodesic::mask values possible for \e outmask are
         * - \e outmask |= Geodesic::DISTANCE for the distance \e s12;
         * - \e outmask |= Geodesic::AZIMUTH for the latitude \e azi2;
         * - \e outmask |= Geodesic::REDUCEDLENGTH for the reduced length \e
         *   m12;
         * - \e outmask |= Geodesic::GEODESICSCALE for the geodesic scales \e
         *   M12 and \e M21;
         * - \e outmask |= Geodesic::AREA for the area \e S12;
         * - \e outmask |= Geodesic::ALL for all of the above.
         * .
         * The arc length is always computed and returned as the function value.
         **********************************************************************/
        public double GenInverse(double lat1, double lon1, double lat2, double lon2,
                                 int outmask, out double s12,
                                 out double salp1, out double calp1,
                                 out double salp2, out double calp2,
                                 out double m12, out double M12, out double M21,
                                 out double S12)
        {
            // Compute longitude difference (AngDiff does this carefully).  Result is
            // in [-180, 180] but -180 is only for west-going geodesics.  180 is for
            // east-going and meridional geodesics.

            s12 = double.NaN;
            m12 = double.NaN;
            M12 = double.NaN;
            M21 = double.NaN;
            S12 = double.NaN;

            double lon12s, lon12 = GeoMath.AngDiff(lon1, lon2, out lon12s);
            // Make longitude difference positive.
            int lonsign = lon12 >= 0 ? 1 : -1;
            // If very close to being on the same half-meridian, then make it so.
            lon12 = lonsign * GeoMath.AngRound(lon12);
            lon12s = GeoMath.AngRound((180 - lon12) - lonsign * lon12s);
            double
              lam12 = lon12 * GeoMath.Degree,
              slam12, clam12;
            if (lon12 > 90)
            {
                GeoMath.Sincosd(lon12s, out slam12, out clam12);
                clam12 = -clam12;
            }
            else
                GeoMath.Sincosd(lon12, out slam12, out clam12);

            // If doublely close to the equator, treat as on equator.
            lat1 = GeoMath.AngRound(GeoMath.LatFix(lat1));
            lat2 = GeoMath.AngRound(GeoMath.LatFix(lat2));
            // Swap points so that point with higher (abs) latitude is point 1
            // If one latitude is a nan, then it becomes lat1.
            int swapp = Math.Abs(lat1) < Math.Abs(lat2) ? -1 : 1;
            if (swapp < 0)
            {
                lonsign *= -1;
                Utility.Swap(ref lat1, ref lat2);
            }
            // Make lat1 <= 0
            int latsign = lat1 < 0 ? 1 : -1;
            lat1 *= latsign;
            lat2 *= latsign;
            // Now we have
            //
            //     0 <= lon12 <= 180
            //     -90 <= lat1 <= 0
            //     lat1 <= lat2 <= -lat1
            //
            // longsign, swapp, latsign register the transformation to bring the
            // coordinates to this canonical form.  In all cases, 1 means no change was
            // made.  We make these transformations so that there are few cases to
            // check, e.g., on verifying quadrants in atan2.  In addition, this
            // enforces some symmetries in the results returned.

            double sbet1, cbet1, sbet2, cbet2, s12x, m12x;
            s12x = m12x = Double.NaN;

            GeoMath.Sincosd(lat1, out sbet1, out cbet1); sbet1 *= _f1;
            // Ensure cbet1 = +epsilon at poles; doing the fix on beta means that sig12
            // will be <= 2*tiny for two points at the same pole.
            GeoMath.Norm(ref sbet1, ref cbet1); cbet1 = Math.Max(tiny_, cbet1);

            GeoMath.Sincosd(lat2, out sbet2, out cbet2); sbet2 *= _f1;
            // Ensure cbet2 = +epsilon at poles
            GeoMath.Norm(ref sbet2, ref cbet2); cbet2 = Math.Max(tiny_, cbet2);

            // If cbet1 < -sbet1, then cbet2 - cbet1 is a sensitive measure of the
            // |bet1| - |bet2|.  Alternatively (cbet1 >= -sbet1), Math.Abs(sbet2) + sbet1 is
            // a better measure.  This logic is used in assigning calp2 in Lambda12.
            // Sometimes these quantities vanish and in that case we force bet2 = +/-
            // bet1 exactly.  An example where is is necessary is the inverse problem
            // 48.522876735459 0 -48.52287673545898293 179.599720456223079643
            // which failed with Visual Studio 10 (Release and Debug)

            if (cbet1 < -sbet1)
            {
                if (cbet2 == cbet1)
                    sbet2 = sbet2 < 0 ? sbet1 : -sbet1;
            }
            else
            {
                if (Math.Abs(sbet2) == -sbet1)
                    cbet2 = cbet1;
            }

            double
              dn1 = Math.Sqrt(1 + _ep2 * GeoMath.Square(sbet1)),
              dn2 = Math.Sqrt(1 + _ep2 * GeoMath.Square(sbet2));

            double a12, sig12;
            a12 = sig12 = calp1 = salp1 = calp2 = salp2 = Double.NaN;
            // index zero elements of these arrays are unused
            double[] C1a = new double[GeodesicCoeff.nC1_ + 1];
            double[] C2a = new double[GeodesicCoeff.nC2_ + 1];
            double[] C3a = new double[GeodesicCoeff.nC3_];

            bool meridian = lat1 == -90 || slam12 == 0;

            if (meridian)
            {

                // Endpoints are on a single full meridian, so the geodesic might lie on
                // a meridian.

                calp1 = clam12; salp1 = slam12; // Head to the target longitude
                calp2 = 1; salp2 = 0;           // At the target we're heading north

                double
                  // tan(bet) = tan(sig) * cos(alp)
                  ssig1 = sbet1, csig1 = calp1 * cbet1,
                  ssig2 = sbet2, csig2 = calp2 * cbet2;

                // sig12 = sig2 - sig1
                sig12 = Math.Atan2(Math.Max(0, csig1 * ssig2 - ssig1 * csig2),
                                           csig1 * csig2 + ssig1 * ssig2);
                {
                    s12x = double.NaN;
                    m12x = double.NaN;
                    M12 = double.NaN;
                    M21 = double.NaN;
                    double dummy = double.NaN;
                    Lengths(_n, sig12, ssig1, csig1, dn1, ssig2, csig2, dn2, cbet1, cbet2,
                            outmask | GeodesicMask.DISTANCE | GeodesicMask.REDUCEDLENGTH,
                            ref s12x, ref m12x, ref dummy, ref M12, ref M21, C1a, C2a);
                }
                // Add the check for sig12 since zero length geodesics might yield m12 <
                // 0.  Test case was
                //
                //    echo 20.001 0 20.001 0 | GeodSolve -i
                //
                // In fact, we will have sig12 > pi/2 for meridional geodesic which is
                // not a shortest path.
                if (sig12 < 1 || m12x >= 0)
                {
                    // Need at least 2, to handle 90 0 90 180
                    if (sig12 < 3 * tiny_)
                        sig12 = m12x = s12x = 0;
                    m12x *= _b;
                    s12x *= _b;
                    a12 = sig12 / GeoMath.Degree;
                }
                else
                    // m12 < 0, i.e., prolate and too close to anti-podal
                    meridian = false;
            }

            // somg12 > 1 marks that it needs to be calculated
            double omg12 = 0, somg12 = 2, comg12 = 0;
            if (!meridian &&
                sbet1 == 0 &&   // and sbet2 == 0
                (_f <= 0 || lon12s >= _f * 180))
            {

                // Geodesic runs along equator
                calp1 = calp2 = 0; salp1 = salp2 = 1;
                s12x = _a * lam12;
                sig12 = omg12 = lam12 / _f1;
                m12x = _b * Math.Sin(sig12);
                if ((outmask & GeodesicMask.GEODESICSCALE) != 0)
                    M12 = M21 = Math.Cos(sig12);
                a12 = lon12 / _f1;

            }
            else if (!meridian)
            {

                // Now point1 and point2 belong within a hemisphere bounded by a
                // meridian and geodesic is neither meridional or equatorial.

                // Figure a starting point for Newton's method
                double dnm = double.NaN;
                sig12 = InverseStart(sbet1, cbet1, dn1, sbet2, cbet2, dn2,
                                     lam12, slam12, clam12,
                                     ref salp1, ref calp1, ref salp2, ref calp2, ref dnm,
                                     C1a, C2a);

                if (sig12 >= 0)
                {
                    // Short lines (InverseStart sets salp2, calp2, dnm)
                    s12x = sig12 * _b * dnm;
                    m12x = GeoMath.Square(dnm) * _b * Math.Sin(sig12 / dnm);
                    if ((outmask & GeodesicMask.GEODESICSCALE) != 0)
                        M12 = M21 = Math.Cos(sig12 / dnm);
                    a12 = sig12 / GeoMath.Degree;
                    omg12 = lam12 / (_f1 * dnm);
                }
                else
                {

                    // Newton's method.  This is a straightforward solution of f(alp1) =
                    // lambda12(alp1) - lam12 = 0 with one wrinkle.  f(alp) has exactly one
                    // root in the interval (0, pi) and its derivative is positive at the
                    // root.  Thus f(alp) is positive for alp > alp1 and negative for alp <
                    // alp1.  During the course of the iteration, a range (alp1a, alp1b) is
                    // maintained which brackets the root and with each evaluation of
                    // f(alp) the range is shrunk, if possible.  Newton's method is
                    // restarted whenever the derivative of f is negative (because the new
                    // value of alp1 is then further from the solution) or if the new
                    // estimate of alp1 lies outside (0,pi); in this case, the new starting
                    // guess is taken to be (alp1a + alp1b) / 2.
                    //
                    // initial values to suppress warnings (if loop is executed 0 times)
                    double ssig1 = 0, csig1 = 0, ssig2 = 0, csig2 = 0, eps = 0, domg12 = 0;
                    int numit = 0;
                    // Bracketing range
                    double salp1a = tiny_, calp1a = 1, salp1b = tiny_, calp1b = -1;
                    for (bool tripn = false, tripb = false; numit < maxit2_; ++numit)
                    {
                        // the WGS84 test set: mean = 1.47, sd = 1.25, max = 16
                        // WGS84 and random input: mean = 2.85, sd = 0.60
                        double dv = double.NaN;
                        double v = Lambda12(sbet1, cbet1, dn1, sbet2, cbet2, dn2, salp1, calp1,
                                          slam12, clam12,
                                          ref salp2, ref calp2, ref sig12, ref ssig1, ref csig1, ref ssig2, ref csig2,
                                          ref eps, ref domg12, numit < maxit1_, ref dv, C1a, C2a, C3a);
                        // Reversed test to allow escape with NaNs
                        if (tripb || !(Math.Abs(v) >= (tripn ? 8 : 1) * tol0_)) break;
                        // Update bracketing values
                        if (v > 0 && (numit > maxit1_ || calp1 / salp1 > calp1b / salp1b))
                        { salp1b = salp1; calp1b = calp1; }
                        else if (v < 0 && (numit > maxit1_ || calp1 / salp1 < calp1a / salp1a))
                        { salp1a = salp1; calp1a = calp1; }
                        if (numit < maxit1_ && dv > 0)
                        {
                            double
                              dalp1 = -v / dv;
                            double
                              sdalp1 = Math.Sin(dalp1), cdalp1 = Math.Cos(dalp1),
                              nsalp1 = salp1 * cdalp1 + calp1 * sdalp1;
                            if (nsalp1 > 0 && Math.Abs(dalp1) < Math.PI)
                            {
                                calp1 = calp1 * cdalp1 - salp1 * sdalp1;
                                salp1 = nsalp1;
                                GeoMath.Norm(ref salp1, ref calp1);
                                // In some regimes we don't get quadratic convergence because
                                // slope -> 0.  So use convergence conditions based on epsilon
                                // instead of Math.Math.Math.Sqrt(epsilon).
                                tripn = Math.Abs(v) <= 16 * tol0_;
                                continue;
                            }
                        }
                        // Either dv was not positive or updated value was outside legal
                        // range.  Use the midpoint of the bracket as the next estimate.
                        // This mechanism is not needed for the WGS84 ellipsoid, but it does
                        // catch problems with more eccentric ellipsoids.  Its efficacy is
                        // such for the WGS84 test set with the starting guess set to alp1 =
                        // 90deg:
                        // the WGS84 test set: mean = 5.21, sd = 3.93, max = 24
                        // WGS84 and random input: mean = 4.74, sd = 0.99
                        salp1 = (salp1a + salp1b) / 2;
                        calp1 = (calp1a + calp1b) / 2;
                        GeoMath.Norm(ref salp1, ref calp1);
                        tripn = false;
                        tripb = (Math.Abs(salp1a - salp1) + (calp1a - calp1) < tolb_ ||
                                 Math.Abs(salp1 - salp1b) + (calp1 - calp1b) < tolb_);
                    }
                    {
                        s12x = double.NaN;
                        m12x = double.NaN;
                        M12 = double.NaN;
                        M21 = double.NaN;
                        double dummy = double.NaN;
                        // Ensure that the reduced length and geodesic scale are computed in
                        // a "canonical" way, with the I2 integral.
                        int lengthmask = outmask |
                          ((outmask & (GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE)) != 0 ? GeodesicMask.DISTANCE : GeodesicMask.NONE);
                        Lengths(eps, sig12, ssig1, csig1, dn1, ssig2, csig2, dn2,
                                cbet1, cbet2, lengthmask, ref s12x, ref m12x, ref dummy, ref M12, ref M21, C1a, C2a);
                    }
                    m12x *= _b;
                    s12x *= _b;
                    a12 = sig12 / GeoMath.Degree;
                    if ((outmask & GeodesicMask.AREA) != 0)
                    {
                        // omg12 = lam12 - domg12
                        double sdomg12 = Math.Sin(domg12), cdomg12 = Math.Cos(domg12);
                        somg12 = slam12 * cdomg12 - clam12 * sdomg12;
                        comg12 = clam12 * cdomg12 + slam12 * sdomg12;
                    }
                }
            }

            if ((outmask & GeodesicMask.DISTANCE) != 0)
                s12 = 0 + s12x;           // Convert -0 to 0

            if ((outmask & GeodesicMask.REDUCEDLENGTH) != 0)
                m12 = 0 + m12x;           // Convert -0 to 0

            if ((outmask & GeodesicMask.AREA) != 0)
            {
                double
                  // From Lambda12: sin(alp1) * cos(bet1) = sin(alp0)
                  salp0 = salp1 * cbet1,
                  calp0 = GeoMath.Hypot(calp1, salp1 * sbet1); // calp0 > 0
                double alp12;
                if (calp0 != 0 && salp0 != 0)
                {
                    double
                      // From Lambda12: tan(bet) = tan(sig) * cos(alp)
                      ssig1 = sbet1, csig1 = calp1 * cbet1,
                      ssig2 = sbet2, csig2 = calp2 * cbet2,
                      k2 = GeoMath.Square(calp0) * _ep2,
                      eps = k2 / (2 * (1 + Math.Sqrt(1 + k2)) + k2),
                      // Multiplier = a^2 * e^2 * cos(alpha0) * sin(alpha0).
                      A4 = GeoMath.Square(_a) * calp0 * salp0 * _e2;
                    GeoMath.Norm(ref ssig1, ref csig1);
                    GeoMath.Norm(ref ssig2, ref csig2);
                    double[] C4a = new double[GeodesicCoeff.nC4_];
                    C4f(eps, C4a);
                    double
                      B41 = SinCosSeries(false, ssig1, csig1, C4a),
                      B42 = SinCosSeries(false, ssig2, csig2, C4a);
                    S12 = A4 * (B42 - B41);
                }
                else
                    // Avoid problems with indeterminate sig1, sig2 on equator
                    S12 = 0;

                if (!meridian && somg12 > 1)
                {
                    somg12 = Math.Sin(omg12); comg12 = Math.Cos(omg12);
                }

                if (!meridian &&
                    // omg12 < 3/4 * pi
                    comg12 > -0.7071 &&     // Long difference not too big
                    sbet2 - sbet1 < 1.75)
                { // Lat difference not too big
                  // Use tan(Gamma/2) = tan(omg12/2)
                  // * (tan(bet1/2)+tan(bet2/2))/(1+tan(bet1/2)*tan(bet2/2))
                  // with tan(x/2) = sin(x)/(1+cos(x))
                    double domg12 = 1 + comg12, dbet1 = 1 + cbet1, dbet2 = 1 + cbet2;
                    alp12 = 2 * Math.Atan2(somg12 * (sbet1 * dbet2 + sbet2 * dbet1),
                                       domg12 * (sbet1 * sbet2 + dbet1 * dbet2));
                }
                else
                {
                    // alp12 = alp2 - alp1, used in atan2 so no need to normalize
                    double
                      salp12 = salp2 * calp1 - calp2 * salp1,
                      calp12 = calp2 * calp1 + salp2 * salp1;
                    // The right thing appears to happen if alp1 = +/-180 and alp2 = 0, viz
                    // salp12 = -0 and alp12 = -180.  However this depends on the sign
                    // being attached to 0 correctly.  The following ensures the correct
                    // behavior.
                    if (salp12 == 0 && calp12 < 0)
                    {
                        salp12 = tiny_ * calp1;
                        calp12 = -1;
                    }
                    alp12 = Math.Atan2(salp12, calp12);
                }
                S12 += _c2 * alp12;
                S12 *= swapp * lonsign * latsign;
                // Convert -0 to 0
                S12 += 0;
            }

            // Convert calp, salp to azimuth accounting for lonsign, swapp, latsign.
            if (swapp < 0)
            {
                Utility.Swap(ref salp1, ref salp2);
                Utility.Swap(ref calp1, ref calp2);
                if ((outmask & GeodesicMask.GEODESICSCALE) != 0)
                    Utility.Swap(ref M12, ref M21);
            }

            salp1 *= swapp * lonsign; calp1 *= swapp * latsign;
            salp2 *= swapp * lonsign; calp2 *= swapp * latsign;

            // Returned value in [0, 180]

            return a12;
        }

        double GenInverse(double lat1, double lon1, double lat2, double lon2,
                               int outmask,
                               out double s12, out double azi1, out double azi2,
                               out double m12, out double M12, out double M21,
                               out double S12)
        {
            azi1 = double.NaN;
            azi2 = double.NaN;
            outmask &= GeodesicMask.OUT_MASK;
            double salp1, calp1, salp2, calp2,
              a12 = GenInverse(lat1, lon1, lat2, lon2,
                                outmask, out s12, out salp1, out calp1, out salp2, out calp2,
                                out m12, out M12, out M21, out S12);
            if ((outmask & GeodesicMask.AZIMUTH) != 0)
            {
                azi1 = GeoMath.Atan2d(salp1, calp1);
                azi2 = GeoMath.Atan2d(salp2, calp2);
            }
            return a12;
        }


        ///@}

        /** \name Interface to GeodesicLine.
         **********************************************************************/
        ///@{

        /**
         * Set up to compute several points on a single geodesic.
         *
         * @param[in] lat1 latitude of point 1 (degrees).
         * @param[in] lon1 longitude of point 1 (degrees).
         * @param[in] azi1 azimuth at point 1 (degrees).
         * @param[in] caps bitor'ed combination of Geodesic::mask values
         *   specifying the capabilities the GeodesicLine object should possess,
         *   i.e., which quantities can be returned in calls to
         *   GeodesicLine::Position.
         * @return a GeodesicLine object.
         *
         * \e lat1 should be in the range [&minus;90&deg;, 90&deg;].
         *
         * The Geodesic::mask values are
         * - \e caps |= Geodesic::LATITUDE for the latitude \e lat2; this is
         *   added automatically;
         * - \e caps |= Geodesic::LONGITUDE for the latitude \e lon2;
         * - \e caps |= Geodesic::AZIMUTH for the azimuth \e azi2; this is
         *   added automatically;
         * - \e caps |= Geodesic::DISTANCE for the distance \e s12;
         * - \e caps |= Geodesic::REDUCEDLENGTH for the reduced length \e m12;
         * - \e caps |= Geodesic::GEODESICSCALE for the geodesic scales \e M12
         *   and \e M21;
         * - \e caps |= Geodesic::AREA for the area \e S12;
         * - \e caps |= Geodesic::DISTANCE_IN permits the length of the
         *   geodesic to be given in terms of \e s12; without this capability the
         *   length can only be specified in terms of arc length;
         * - \e caps |= Geodesic::ALL for all of the above.
         * .
         * The default value of \e caps is Geodesic::ALL.
         *
         * If the point is at a pole, the azimuth is defined by keeping \e lon1
         * fixed, writing \e lat1 = &plusmn;(90 &minus; &epsilon;), and taking the
         * limit &epsilon; &rarr; 0+.
         **********************************************************************/
        public GeodesicLine Line(double lat1, double lon1, double azi1, int caps = GeodesicMask.ALL)
        {
            return new GeodesicLine(this, lat1, lon1, azi1, caps);
        }

        /**
         * Define a GeodesicLine in terms of the inverse geodesic problem.
         *
         * @param[in] lat1 latitude of point 1 (degrees).
         * @param[in] lon1 longitude of point 1 (degrees).
         * @param[in] lat2 latitude of point 2 (degrees).
         * @param[in] lon2 longitude of point 2 (degrees).
         * @param[in] caps bitor'ed combination of Geodesic::mask values
         *   specifying the capabilities the GeodesicLine object should possess,
         *   i.e., which quantities can be returned in calls to
         *   GeodesicLine::Position.
         * @return a GeodesicLine object.
         *
         * This function sets point 3 of the GeodesicLine to correspond to point 2
         * of the inverse geodesic problem.
         *
         * \e lat1 and \e lat2 should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public GeodesicLine InverseLine(double lat1, double lon1, double lat2, double lon2,
                                        int caps = GeodesicMask.ALL)
        {
            double t, salp1, calp1, salp2, calp2,
            a12 = GenInverse(lat1, lon1, lat2, lon2,
                       // No need to specify AZIMUTH here
                       0, out t, out salp1, out calp1, out salp2, out calp2,
                       out t, out t, out t, out t),
            azi1 = GeoMath.Atan2d(salp1, calp1);
            // Ensure that a12 can be converted to a distance
            if ((caps & (GeodesicMask.OUT_MASK & GeodesicMask.DISTANCE_IN)) != 0) caps |= GeodesicMask.DISTANCE;
            return
                new GeodesicLine(this, lat1, lon1, azi1, salp1, calp1, caps, true, a12);
        }

        /**
         * Define a GeodesicLine in terms of the direct geodesic problem specified
         * in terms of distance.
         *
         * @param[in] lat1 latitude of point 1 (degrees).
         * @param[in] lon1 longitude of point 1 (degrees).
         * @param[in] azi1 azimuth at point 1 (degrees).
         * @param[in] s12 distance between point 1 and point 2 (meters); it can be
         *   negative.
         * @param[in] caps bitor'ed combination of Geodesic::mask values
         *   specifying the capabilities the GeodesicLine object should possess,
         *   i.e., which quantities can be returned in calls to
         *   GeodesicLine::Position.
         * @return a GeodesicLine object.
         *
         * This function sets point 3 of the GeodesicLine to correspond to point 2
         * of the direct geodesic problem.
         *
         * \e lat1 should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public GeodesicLine DirectLine(double lat1, double lon1, double azi1, double s12,
                                int caps = GeodesicMask.ALL)
        {
            return GenDirectLine(lat1, lon1, azi1, false, s12, caps);
        }

        /**
         * Define a GeodesicLine in terms of the direct geodesic problem specified
         * in terms of arc length.
         *
         * @param[in] lat1 latitude of point 1 (degrees).
         * @param[in] lon1 longitude of point 1 (degrees).
         * @param[in] azi1 azimuth at point 1 (degrees).
         * @param[in] a12 arc length between point 1 and point 2 (degrees); it can
         *   be negative.
         * @param[in] caps bitor'ed combination of Geodesic::mask values
         *   specifying the capabilities the GeodesicLine object should possess,
         *   i.e., which quantities can be returned in calls to
         *   GeodesicLine::Position.
         * @return a GeodesicLine object.
         *
         * This function sets point 3 of the GeodesicLine to correspond to point 2
         * of the direct geodesic problem.
         *
         * \e lat1 should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public GeodesicLine ArcDirectLine(double lat1, double lon1, double azi1, double a12,
                                   int caps = GeodesicMask.ALL)
        {
            return null;
        }

        /**
         * Define a GeodesicLine in terms of the direct geodesic problem specified
         * in terms of either distance or arc length.
         *
         * @param[in] lat1 latitude of point 1 (degrees).
         * @param[in] lon1 longitude of point 1 (degrees).
         * @param[in] azi1 azimuth at point 1 (degrees).
         * @param[in] arcmode boolean flag determining the meaning of the \e
         *   s12_a12.
         * @param[in] s12_a12 if \e arcmode is false, this is the distance between
         *   point 1 and point 2 (meters); otherwise it is the arc length between
         *   point 1 and point 2 (degrees); it can be negative.
         * @param[in] caps bitor'ed combination of Geodesic::mask values
         *   specifying the capabilities the GeodesicLine object should possess,
         *   i.e., which quantities can be returned in calls to
         *   GeodesicLine::Position.
         * @return a GeodesicLine object.
         *
         * This function sets point 3 of the GeodesicLine to correspond to point 2
         * of the direct geodesic problem.
         *
         * \e lat1 should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public GeodesicLine GenDirectLine(double lat1, double lon1, double azi1,
                                   bool arcmode, double s12_a12,
                                   int caps = GeodesicMask.ALL)
        {
            azi1 = GeoMath.AngNormalize(azi1);
            double salp1, calp1;
            // Guard against underflow in salp0.  Also -0 is converted to +0.
            GeoMath.Sincosd(GeoMath.AngRound(azi1), out salp1, out calp1);
            // Automatically supply DISTANCE_IN if necessary
            if (!arcmode) caps |= GeodesicMask.DISTANCE_IN;
            return new GeodesicLine(this, lat1, lon1, azi1, salp1, calp1,
                                    caps, arcmode, s12_a12);
        }
        ///@}

        /** \name Inspector functions.
         **********************************************************************/
        ///@{

        /**
         * @return \e a the equatorial radius of the ellipsoid (meters).  This is
         *   the value used in the constructor.
         **********************************************************************/
        public double MajorRadius()
        {
            return _a;
        }

        /**
         * @return \e f the  flattening of the ellipsoid.  This is the
         *   value used in the constructor.
         **********************************************************************/
        public double Flattening()
        {
            return _f;
        }

        /**
         * @return total area of ellipsoid in meters<sup>2</sup>.  The area of a
         *   polygon encircling a pole can be found by adding
         *   Geodesic::EllipsoidArea()/2 to the sum of \e S12 for each side of the
         *   polygon.
         **********************************************************************/
        public double EllipsoidArea()
        {
            return 4 * Math.PI * _c2;
        }
        ///@}

        /**
         * A global instantiation of Geodesic with the parameters for the WGS84
         * ellipsoid.
         **********************************************************************/

        private static Geodesic _wgs84;
        
        public static Geodesic WGS84()
        {
            if (_wgs84 == null)
            {
                _wgs84 = new Geodesic(Constants.WGS84_a, Constants.WGS84_f);
            }
            return _wgs84;
        }

        internal double A3f(double eps)
        {
            // Evaluate A3
            return GeoMath.PolyVal(GeodesicCoeff.nA3_ - 1, _A3x, 0, eps);
        }

        internal void C3f(double eps, double[] c)
        {
            // Evaluate C3 coeffs
            // Elements c[1] thru c[nC3_ - 1] are set
            double mult = 1;
            int o = 0;
            for (int l = 1; l < GeodesicCoeff.nC3_; ++l)
            { // l is index of C3[l]
                int m = GeodesicCoeff.nC3_ - l - 1;          // order of polynomial in eps
                mult *= eps;
                c[l] = mult * GeoMath.PolyVal(m, _C3x, o, eps);
                o += m + 1;
            }
            // Post condition: o == nC3x_
        }

        internal void C4f(double eps, double[] c)
        {
            // Evaluate C4 coeffs
            // Elements c[0] thru c[nC4_ - 1] are set
            double mult = 1;
            int o = 0;
            for (int l = 0; l < GeodesicCoeff.nC4_; ++l)
            { // l is index of C4[l]
                int m = GeodesicCoeff.nC4_ - l - 1;          // order of polynomial in eps
                c[l] = mult * GeoMath.PolyVal(m, _C4x, o, eps);
                o += m + 1;
                mult *= eps;
            }
        }


        //         Math::double Geodesic::SinCosSeries(bool sinp,
        //                                     double sinx, double cosx,
        //                                     const double c[], int n) {
        //     // Evaluate
        //     // y = sinp ? sum(c[i] * sin( 2*i    * x), i, 1, n) :
        //     //            sum(c[i] * cos((2*i+1) * x), i, 0, n-1)
        //     // using Clenshaw summation.  N.B. c[0] is unused for sin series
        //     // Approx operation count = (n + 5) mult and (2 * n + 2) add
        //     c += (n + sinp);            // Point to one beyond last element
        //     double
        //       ar = 2 * (cosx - sinx) * (cosx + sinx), // 2 * cos(2 * x)
        //       y0 = n & 1 ? *--c : 0, y1 = 0;          // accumulators for sum
        //     // Now n is even
        //     n /= 2;
        //     while (n--) {
        //       // Unroll loop x 2, so accumulators return to their original role
        //       y1 = ar * y0 - y1 + *--c;
        //       y0 = ar * y1 - y0 + *--c;
        //     }
        //     return sinp
        //       ? 2 * sinx * cosx * y0    // sin(2 * x) * y0
        //       : cosx * (y0 - y1);       // cos(x) * (y0 - y1)
        //   }

        internal static double SinCosSeries(bool sinp, double sinx, double cosx, double[] c)
        {
            // Evaluate
            // y = sinp ? sum(c[i] * sin( 2*i    * x), i, 1, n) :
            //            sum(c[i] * cos((2*i+1) * x), i, 0, n-1)
            // using Clenshaw summation.  N.B. c[0] is unused for sin series
            // Approx operation count = (n + 5) mult and (2 * n + 2) add
            int
              k = c.Length,             // Point to one beyond last element
              n = k - (sinp ? 1 : 0);
            double
              ar = 2 * (cosx - sinx) * (cosx + sinx), // 2 * cos(2 * x)
              y0 = (n & 1) != 0 ? c[--k] : 0, y1 = 0;        // accumulators for sum
                                                             // Now n is even
            n /= 2;
            while (n-- != 0)
            {
                // Unroll loop x 2, so accumulators return to their original role
                y1 = ar * y0 - y1 + c[--k];
                y0 = ar * y1 - y0 + c[--k];
            }
            return sinp
              ? 2 * sinx * cosx * y0    // sin(2 * x) * y0
              : cosx * (y0 - y1);       // cos(x) * (y0 - y1)
        }

        void Lengths(double eps, double sig12,
                    double ssig1, double csig1, double dn1,
                    double ssig2, double csig2, double dn2,
                    double cbet1, double cbet2, int outmask,
                    ref double s12b, ref double m12b, ref double m0,
                    ref double M12, ref double M21,
                    // Scratch area of the right size
                    double[] C1a, double[] C2a)
        {
            // Return m12b = (reduced length)/_b; also calculate s12b = distance/_b,
            // and m0 = coefficient of secular term in expression for reduced length.

            outmask &= GeodesicMask.OUT_MASK;
            // outmask & DISTANCE: set s12b
            // outmask & REDUCEDLENGTH: set m12b & m0
            // outmask & GEODESICSCALE: set M12 & M21

            double m0x = 0, J12 = 0, A1 = 0, A2 = 0;
            if ((outmask & (GeodesicMask.DISTANCE | GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE)) != 0)
            {
                A1 = GeodesicCoeff.A1m1f(eps);
                GeodesicCoeff.C1f(eps, C1a);
                if ((outmask & (GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE)) != 0)
                {
                    A2 = GeodesicCoeff.A2m1f(eps);
                    GeodesicCoeff.C2f(eps, C2a);
                    m0x = A1 - A2;
                    A2 = 1 + A2;
                }
                A1 = 1 + A1;
            }
            if ((outmask & GeodesicMask.DISTANCE) != 0)
            {
                double B1 = SinCosSeries(true, ssig2, csig2, C1a) -
                  SinCosSeries(true, ssig1, csig1, C1a);
                // Missing a factor of _b
                s12b = A1 * (sig12 + B1);
                if ((outmask & (GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE)) != 0)
                {
                    double B2 = SinCosSeries(true, ssig2, csig2, C2a) -
                      SinCosSeries(true, ssig1, csig1, C2a);
                    J12 = m0x * sig12 + (A1 * B1 - A2 * B2);
                }
            }
            else if ((outmask & (GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE)) != 0)
            {
                // Assume here that nC1_ >= nC2_
                for (int l = 1; l <= GeodesicCoeff.nC2_; ++l)
                    C2a[l] = A1 * C1a[l] - A2 * C2a[l];
                J12 = m0x * sig12 + (SinCosSeries(true, ssig2, csig2, C2a) -
                                     SinCosSeries(true, ssig1, csig1, C2a));
            }
            if ((outmask & GeodesicMask.REDUCEDLENGTH) != 0)
            {
                m0 = m0x;
                // Missing a factor of _b.
                // Add parens around (csig1 * ssig2) and (ssig1 * csig2) to ensure
                // accurate cancellation in the case of coincident points.
                m12b = dn2 * (csig1 * ssig2) - dn1 * (ssig1 * csig2) -
                  csig1 * csig2 * J12;
            }
            if ((outmask & GeodesicMask.GEODESICSCALE) != 0)
            {
                double csig12 = csig1 * csig2 + ssig1 * ssig2;
                double t = _ep2 * (cbet1 - cbet2) * (cbet1 + cbet2) / (dn1 + dn2);
                M12 = csig12 + (t * ssig2 - csig2 * J12) * ssig1 / dn1;
                M21 = csig12 - (t * ssig1 - csig1 * J12) * ssig2 / dn2;
            }
        }

        double Astroid(double x, double y)
        {
            // Solve k^4+2*k^3-(x^2+y^2-1)*k^2-2*y^2*k-y^2 = 0 for positive root k.
            // This solution is adapted from Geocentric::Reverse.
            double k;
            double
              p = GeoMath.Square(x),
              q = GeoMath.Square(y),
              r = (p + q - 1) / 6;
            if (!(q == 0 && r <= 0))
            {
                double
                  // Avoid possible division by zero when r = 0 by multiplying equations
                  // for s and t by r^3 and r, resp.
                  S = p * q / 4,            // S = r^3 * s
                  r2 = GeoMath.Square(r),
                  r3 = r * r2,
                  // The discriminant of the quadratic equation for T3.  This is zero on
                  // the evolute curve p^(1/3)+q^(1/3) = 1
                  disc = S * (S + 2 * r3);
                double u = r;
                if (disc >= 0)
                {
                    double T3 = S + r3;
                    // Pick the sign on the sqrt to maximize abs(T3).  This minimizes loss
                    // of precision due to cancellation.  The result is unchanged because
                    // of the way the T is used in definition of u.
                    T3 += T3 < 0 ? -Math.Sqrt(disc) : Math.Sqrt(disc); // T3 = (r * t)^3
                                                                       // N.B. cbrt always returns the double root.  cbrt(-8) = -2.
                    double T = GeoMath.CubeRoot(T3); // T = r * t
                                                     // T can be zero; but then r2 / T -> 0.
                    u += T + (T != 0 ? r2 / T : 0);
                }
                else
                {
                    // T is complex, but the way u is defined the result is double.
                    double ang = Math.Atan2(Math.Sqrt(-disc), -(S + r3));
                    // There are three possible cube roots.  We choose the root which
                    // avoids cancellation.  Note that disc < 0 implies that r < 0.
                    u += 2 * r * Math.Cos(ang / 3);
                }
                double
                  v = Math.Sqrt(GeoMath.Square(u) + q),    // guaranteed positive
                                                           // Avoid loss of accuracy when u < 0.
                  uv = u < 0 ? q / (v - u) : u + v, // u+v, guaranteed positive
                  w = (uv - q) / (2 * v);           // positive?
                                                    // Rearrange expression for k to avoid loss of accuracy due to
                                                    // subtraction.  Division by 0 not possible because uv > 0, w >= 0.
                k = uv / (Math.Sqrt(uv + GeoMath.Square(w)) + w);   // guaranteed positive
            }
            else
            {               // q == 0 && r <= 0
                            // y = 0 with |x| <= 1.  Handle this case directly.
                            // for y small, positive root is k = abs(y)/Math.Math.Sqrt(1-x^2)
                k = 0;
            }
            return k;
        }

        double InverseStart(double sbet1, double cbet1, double dn1,
                                    double sbet2, double cbet2, double dn2,
                                    double lam12, double slam12, double clam12,
                                    ref double salp1, ref double calp1,
                                    // Only updated if return val >= 0
                                    ref double salp2, ref double calp2,
                                    // Only updated for short lines
                                    ref double dnm,
                                    // Scratch area of the right size
                                    double[] C1a, double[] C2a)
        {
            // Return a starting point for Newton's method in salp1 and calp1 (function
            // value is -1).  If Newton's method doesn't need to be used, return also
            // salp2 and calp2 and function value is sig12.
            double
            sig12 = -1,               // Return value
                                      // bet12 = bet2 - bet1 in [0, pi); bet12a = bet2 + bet1 in (-pi, 0]
            sbet12 = sbet2 * cbet1 - cbet2 * sbet1,
            cbet12 = cbet2 * cbet1 + sbet2 * sbet1;
            double sbet12a = sbet2 * cbet1 + cbet2 * sbet1;

            bool shortline = cbet12 >= 0 && sbet12 < 0.5 &&
              cbet2 * lam12 < 0.5;
            double somg12, comg12;
            if (shortline)
            {
                double sbetm2 = GeoMath.Square(sbet1 + sbet2);
                // sin((bet1+bet2)/2)^2
                // =  (sbet1 + sbet2)^2 / ((sbet1 + sbet2)^2 + (cbet1 + cbet2)^2)
                sbetm2 /= sbetm2 + GeoMath.Square(cbet1 + cbet2);
                dnm = Math.Sqrt(1 + _ep2 * sbetm2);
                double omg12 = lam12 / (_f1 * dnm);
                somg12 = Math.Sin(omg12); comg12 = Math.Cos(omg12);
            }
            else
            {
                somg12 = slam12; comg12 = clam12;
            }

            salp1 = cbet2 * somg12;
            calp1 = comg12 >= 0 ?
                  sbet12 + cbet2 * sbet1 * GeoMath.Square(somg12) / (1 + comg12) :
                  sbet12a - cbet2 * sbet1 * GeoMath.Square(somg12) / (1 - comg12);

            double
              ssig12 = GeoMath.Hypot(salp1, calp1),
              csig12 = sbet1 * sbet2 + cbet1 * cbet2 * comg12;

            if (shortline && ssig12 < _etol2)
            {
                // doublely short lines
                salp2 = cbet1 * somg12;
                calp2 = sbet12 - cbet1 * sbet2 *
                        (comg12 >= 0 ? GeoMath.Square(somg12) / (1 + comg12) : 1 - comg12);
                GeoMath.Norm(ref salp2, ref calp2);
                // Set return value
                sig12 = Math.Atan2(ssig12, csig12);
            }
            else if (Math.Abs(_n) > 0.1 || // Skip astroid calc if too eccentric
                     csig12 >= 0 ||
                     ssig12 >= 6 * Math.Abs(_n) * Math.PI * GeoMath.Square(cbet1))
            {
                // Nothing to do, zeroth order spherical approximation is OK
            }
            else
            {
                // Scale lam12 and bet2 to x, y coordinate system where antipodal point
                // is at origin and singular point is at y = 0, x = -1.
                double y, lamscale, betscale;
                // Volatile declaration needed to fix inverse case
                // 56.320923501171 0 -56.320923501171 179.664747671772880215
                // which otherwise fails with g++ 4.4.4 x86 -O3
                double x;
                double lam12x = Math.Atan2(-slam12, -clam12); // lam12 - pi
                if (_f >= 0)
                {            // In fact f == 0 does not get here
                             // x = dlong, y = dlat
                    {
                        double
                          k2 = GeoMath.Square(sbet1) * _ep2,
                          eps = k2 / (2 * (1 + Math.Sqrt(1 + k2)) + k2);
                        lamscale = _f * cbet1 * A3f(eps) * Math.PI;
                    }
                    betscale = lamscale * cbet1;

                    x = lam12x / lamscale;
                    y = sbet12a / betscale;
                }
                else
                {                  // _f < 0
                                   // x = dlat, y = dlong
                    double
                      cbet12a = cbet2 * cbet1 - sbet2 * sbet1,
                      bet12a = Math.Atan2(sbet12a, cbet12a);
                    double m12b = double.NaN, m0 = double.NaN, dummy = double.NaN;
                    // In the case of lon12 = 180, this repeats a calculation made in
                    // Inverse.
                    Lengths(_n, Math.PI + bet12a,
                            sbet1, -cbet1, dn1, sbet2, cbet2, dn2,
                            cbet1, cbet2,
                            GeodesicMask.REDUCEDLENGTH, ref dummy, ref m12b, ref m0, ref dummy, ref dummy, C1a, C2a);
                    x = -1 + m12b / (cbet1 * cbet2 * m0 * Math.PI);
                    betscale = x < -0.01 ? sbet12a / x :
                      -_f * GeoMath.Square(cbet1) * Math.PI;
                    lamscale = betscale / cbet1;
                    y = lam12x / lamscale;
                }

                if (y > -tol1_ && x > -1 - xthresh_)
                {
                    // strip near cut
                    // Need double(x) here to cast away the volatility of x for min/max
                    if (_f >= 0)
                    {
                        salp1 = Math.Min(1, -x); calp1 = -Math.Sqrt(1 - GeoMath.Square(salp1));
                    }
                    else
                    {
                        calp1 = Math.Max(x > -tol1_ ? 0 : -1, x);
                        salp1 = Math.Sqrt(1 - GeoMath.Square(calp1));
                    }
                }
                else
                {
                    // Estimate alp1, by solving the astroid problem.
                    //
                    // Could estimate alpha1 = theta + pi/2, directly, i.e.,
                    //   calp1 = y/k; salp1 = -x/(1+k);  for _f >= 0
                    //   calp1 = x/(1+k); salp1 = -y/k;  for _f < 0 (need to check)
                    //
                    // However, it's better to estimate omg12 from astroid and use
                    // spherical formula to compute alp1.  This reduces the mean number of
                    // Newton iterations for astroid cases from 2.24 (min 0, max 6) to 2.12
                    // (min 0 max 5).  The changes in the number of iterations are as
                    // follows:
                    //
                    // change percent
                    //    1       5
                    //    0      78
                    //   -1      16
                    //   -2       0.6
                    //   -3       0.04
                    //   -4       0.002
                    //
                    // The histogram of iterations is (m = number of iterations estimating
                    // alp1 directly, n = number of iterations estimating via omg12, total
                    // number of trials = 148605):
                    //
                    //  iter    m      n
                    //    0   148    186
                    //    1 13046  13845
                    //    2 93315 102225
                    //    3 36189  32341
                    //    4  5396      7
                    //    5   455      1
                    //    6    56      0
                    //
                    // Because omg12 is near pi, estimate work with omg12a = pi - omg12
                    double k = Astroid(x, y);
                    double
                      omg12a = lamscale * (_f >= 0 ? -x * k / (1 + k) : -y * (1 + k) / k);
                    somg12 = Math.Sin(omg12a); comg12 = -Math.Cos(omg12a);
                    // Update spherical estimate of alp1 using omg12 instead of lam12
                    salp1 = cbet2 * somg12;
                    calp1 = sbet12a - cbet2 * sbet1 * GeoMath.Square(somg12) / (1 - comg12);
                }
            }
            // Sanity check on starting guess.  Backwards check allows NaN through.
            if (!(salp1 <= 0))
                GeoMath.Norm(ref salp1, ref calp1);
            else
            {
                salp1 = 1; calp1 = 0;
            }
            return sig12;
        }

        double Lambda12(double sbet1, double cbet1, double dn1,
                        double sbet2, double cbet2, double dn2,
                        double salp1, double calp1,
                        double slam120, double clam120,
                        ref double salp2, ref double calp2,
                        ref double sig12,
                        ref double ssig1, ref double csig1,
                        ref double ssig2, ref double csig2,
                        ref double eps, ref double domg12,
                        bool diffp, ref double dlam12,
                        // Scratch area of the right size
                        double[] C1a, double[] C2a, double[] C3a)
        {

            if (sbet1 == 0 && calp1 == 0)
                // Break degeneracy of equatorial line.  This case has already been
                // handled.
                calp1 = -tiny_;

            double
              // sin(alp1) * cos(bet1) = sin(alp0)
              salp0 = salp1 * cbet1,
              calp0 = GeoMath.Hypot(calp1, salp1 * sbet1); // calp0 > 0

            double somg1, comg1, somg2, comg2, somg12, comg12, lam12;
            // tan(bet1) = tan(sig1) * cos(alp1)
            // tan(omg1) = sin(alp0) * tan(sig1) = tan(omg1)=tan(alp1)*sin(bet1)
            ssig1 = sbet1; somg1 = salp0 * sbet1;
            csig1 = comg1 = calp1 * cbet1;
            GeoMath.Norm(ref ssig1, ref csig1);
            // GeoMath.Norm(somg1, comg1); -- don't need to normalize!

            // Enforce symmetries in the case abs(bet2) = -bet1.  Need to be careful
            // about this case, since this can yield singularities in the Newton
            // iteration.
            // sin(alp2) * cos(bet2) = sin(alp0)
            salp2 = cbet2 != cbet1 ? salp0 / cbet2 : salp1;
            // calp2 = sqrt(1 - sq(salp2))
            //       = sqrt(sq(calp0) - sq(sbet2)) / cbet2
            // and subst for calp0 and rearrange to give (choose positive sqrt
            // to give alp2 in [0, pi/2]).
            calp2 = cbet2 != cbet1 || Math.Abs(sbet2) != -sbet1 ?
              Math.Sqrt(GeoMath.Square(calp1 * cbet1) +
                   (cbet1 < -sbet1 ?
                    (cbet2 - cbet1) * (cbet1 + cbet2) :
                    (sbet1 - sbet2) * (sbet1 + sbet2))) / cbet2 :
              Math.Abs(calp1);
            // tan(bet2) = tan(sig2) * cos(alp2)
            // tan(omg2) = sin(alp0) * tan(sig2).
            ssig2 = sbet2; somg2 = salp0 * sbet2;
            csig2 = comg2 = calp2 * cbet2;
            GeoMath.Norm(ref ssig2, ref csig2);
            // GeoMath.Norm(somg2, comg2); -- don't need to normalize!

            // sig12 = sig2 - sig1, limit to [0, pi]
            sig12 = Math.Atan2(Math.Max(0, csig1 * ssig2 - ssig1 * csig2),
                                      csig1 * csig2 + ssig1 * ssig2);

            // omg12 = omg2 - omg1, limit to [0, pi]
            somg12 = Math.Max(0, comg1 * somg2 - somg1 * comg2);
            comg12 = comg1 * comg2 + somg1 * somg2;
            // eta = omg12 - lam120
            double eta = Math.Atan2(somg12 * clam120 - comg12 * slam120,
                             comg12 * clam120 + somg12 * slam120);
            double B312;
            double k2 = GeoMath.Square(calp0) * _ep2;
            eps = k2 / (2 * (1 + Math.Sqrt(1 + k2)) + k2);
            C3f(eps, C3a);
            B312 = (SinCosSeries(true, ssig2, csig2, C3a) -
                SinCosSeries(true, ssig1, csig1, C3a));
            domg12 = -_f * A3f(eps) * salp0 * (sig12 + B312);
            lam12 = eta + domg12;

            if (diffp)
            {
                if (calp2 == 0)
                    dlam12 = -2 * _f1 * dn1 / sbet1;
                else
                {
                    double dummy = double.NaN;
                    Lengths(eps, sig12, ssig1, csig1, dn1, ssig2, csig2, dn2,
                            cbet1, cbet2, GeodesicMask.REDUCEDLENGTH,
                            ref dummy, ref dlam12, ref dummy, ref dummy, ref dummy, C1a, C2a);
                    dlam12 *= _f1 / (calp2 * cbet2);
                }
            }

            return lam12;
        }
    }
}
