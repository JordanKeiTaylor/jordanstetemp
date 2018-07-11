using System;
namespace GeographicLib
{
    public class GeodesicLine
    {
        private static readonly int nC1_ = GeodesicCoeff.nC1_;
        private static readonly int nC1p_ = GeodesicCoeff.nC1p_;
        private static readonly int nC2_ = GeodesicCoeff.nC2_;
        private static readonly int nC3_ = GeodesicCoeff.nC3_;
        private static readonly int nC4_ = GeodesicCoeff.nC4_;

        private double _lat1, _lon1, _azi1;

        private double _a, _f, _b, _c2, _f1, _salp0, _calp0, _k2,
          _salp1, _calp1, _ssig1, _csig1, _dn1, _stau1, _ctau1, _somg1, _comg1,
          _A1m1, _A2m1, _A3c, _B11, _B21, _B31, _A4, _B41;

        private double _a13, _s13;

        // index zero elements of _C1a, _C1pa, _C2a, _C3a are unused
        private double[] _C1a, _C1pa, _C2a, _C3a,
          _C4a;    // all the elements of _C4a are used

        private int _caps;

        /** \name Constructors
     **********************************************************************/
        ///@{

        /**
         * Constructor for a geodesic line staring at GeodesicMask.LATITUDE \e lat1, GeodesicMask.LONGITUDE
         * \e lon1, and GeodesicMask.AZIMUTH \e azi1 (all in degrees).
         *
         * @param[in] g A Geodesic object used to compute the necessary information
         *   about the GeodesicLine.
         * @param[in] lat1 GeodesicMask.LATITUDE of point 1 (degrees).
         * @param[in] lon1 GeodesicMask.LONGITUDE of point 1 (degrees).
         * @param[in] azi1 GeodesicMask.AZIMUTH at point 1 (degrees).
         * @param[in] caps bitor'ed combination of GeodesicLine::mask values
         *   specifying the capabilities the GeodesicLine object should possess,
         *   i.e., which quantities can be returned in calls to
         *   GeodesicLine::Position.
         *
         * \e lat1 should be in the range [&minus;90&deg;, 90&deg;].
         *
         * The GeodesicLine::mask values are
         * - \e caps |= GeodesicLine::GeodesicMask.LATITUDE for the GeodesicMask.LATITUDE \e lat2; this is
         *   added automatically;
         * - \e caps |= GeodesicLine::GeodesicMask.LONGITUDE for the GeodesicMask.LATITUDE \e lon2;
         * - \e caps |= GeodesicLine::GeodesicMask.AZIMUTH for the GeodesicMask.LATITUDE \e azi2; this is
         *   added automatically;
         * - \e caps |= GeodesicLine::DISTANCE for the distance \e s12;
         * - \e caps |= GeodesicLine::GeodesicMask.REDUCEDLENGTH for the reduced length \e m12;
         * - \e caps |= GeodesicLine::GeodesicMask.GEODESICSCALE for the geodesic scales \e M12
         *   and \e M21;
         * - \e caps |= GeodesicLine::GeodesicMask.AREA for the GeodesicMask.AREA \e S12;
         * - \e caps |= GeodesicLine::DISTANCE_IN permits the length of the
         *   geodesic to be given in terms of \e s12; without this capability the
         *   length can only be specified in terms of arc length;
         * - \e caps |= GeodesicLine::ALL for all of the above.
         * .
         * The default value of \e caps is GeodesicLine::ALL.
         *
         * If the point is at a pole, the GeodesicMask.AZIMUTH is defined by keeping \e lon1
         * fixed, writing \e lat1 = &plusmn;(90&deg; &minus; &epsilon;), and taking
         * the limit &epsilon; &rarr; 0+.
         **********************************************************************/
        public GeodesicLine(Geodesic g, double lat1, double lon1, double azi1)
            : this(g, lat1, lon1, azi1, GeodesicMask.ALL) { }

        public GeodesicLine(Geodesic g,
                            double lat1, double lon1, double azi1,
                            int caps)
        {
            azi1 = GeoMath.AngNormalize(azi1);
            double salp1, calp1;
            // Guard against underflow in salp0.  Also -0 is converted to +0.
            GeoMath.Sincosd(GeoMath.AngRound(azi1), out salp1, out calp1);
            LineInit(g, lat1, lon1, azi1, salp1, calp1, caps); //TODO: Check this cast
        }

        public GeodesicLine(Geodesic g,
                            double lat1, double lon1,
                            double azi1, double salp1, double calp1,
                            int caps, bool arcmode, double s13_a13)
        {
            LineInit(g, lat1, lon1, azi1, salp1, calp1, caps);
            GenSetDistance(arcmode, s13_a13);
        }

        /**
         * A default constructor.  If GeodesicLine::Position is called on the
         * resulting objecout t, it returns immediately (without doing any
         * calculations).  The object can be set with a call to Geodesic::Line.
         * Use Init() to test whether object is still in this uninitialized state.
         **********************************************************************/
        public GeodesicLine()
        {
            _caps = 0;
        }

        /** \name Position in terms of distance
     **********************************************************************/
        ///@{

        /**
         * Compute the position of point 2 which is a distance \e s12 (meters) from
         * point 1.
         *
         * @param[in] s12 distance from point 1 to point 2 (meters); it can be
         *   negative.
         * @param[out] lat2 GeodesicMask.LATITUDE of point 2 (degrees).
         * @param[out] lon2 GeodesicMask.LONGITUDE of point 2 (degrees); requires that the
         *   GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::GeodesicMask.LONGITUDE.
         * @param[out] azi2 (forward) GeodesicMask.AZIMUTH at point 2 (degrees).
         * @param[out] m12 reduced length of geodesic (meters); requires that the
         *   GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::GeodesicMask.REDUCEDLENGTH.
         * @param[out] M12 geodesic scale of point 2 relative to point 1
         *   (dimensionless); requires that the GeodesicLine object was constructed
         *   with \e caps |= GeodesicLine::GeodesicMask.GEODESICSCALE.
         * @param[out] M21 geodesic scale of point 1 relative to point 2
         *   (dimensionless); requires that the GeodesicLine object was constructed
         *   with \e caps |= GeodesicLine::GeodesicMask.GEODESICSCALE.
         * @param[out] S12 GeodesicMask.AREA under the geodesic (meters<sup>2</sup>); requires
         *   that the GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::GeodesicMask.AREA.
         * @return \e a12 arc length from point 1 to point 2 (degrees).
         *
         * The values of \e lon2 and \e azi2 returned are in the range
         * [&minus;180&deg;, 180&deg;].
         *
         * The GeodesicLine object \e must have been constructed with \e caps |=
         * GeodesicLine::DISTANCE_IN; otherwise double.NaN is returned and no
         * parameters are set.  Requesting a value which the GeodesicLine object is
         * not capable of computing is not an error; the corresponding argument
         * will not be altered.
         *
         * The following functions are overloaded versions of
         * GeodesicLine::Position which omit some of the output parameters.  Note,
         * however, that the arc length is always computed and returned as the
         * function value.
         **********************************************************************/
        public double Position(double s12,
                            out double lat2, out double lon2, out double azi2,
                            out double m12, out double M12, out double M21,
                            out double S12)
        {
            double t;
            return GenPosition(false, s12,
                               GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH |
                               GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE | GeodesicMask.AREA,
                               out lat2, out lon2, out azi2, out t, out m12, out M12, out M21, out S12);
        }

        /**
         * See the documentation for GeodesicLine::Position.
         **********************************************************************/
        public double Position(double s12, out double lat2, out double lon2)
        {
            double t;
            return GenPosition(false, s12,
                               GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE,
                               out lat2, out lon2, out t, out t, out t, out t, out t, out t);
        }

        /**
         * See the documentation for GeodesicLine::Position.
         **********************************************************************/
        public double Position(double s12, out double lat2, out double lon2,
                            out double azi2)
        {
            double t;
            return GenPosition(false, s12,
                               GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH,
                               out lat2, out lon2, out azi2, out t, out t, out t, out t, out t);
        }

        /**
         * See the documentation for GeodesicLine::Position.
         **********************************************************************/
        public double Position(double s12, out double lat2, out double lon2,
                            out double azi2, out double m12)
        {
            double t;
            return GenPosition(false, s12,
                               GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE |
                               GeodesicMask.AZIMUTH | GeodesicMask.REDUCEDLENGTH,
                               out lat2, out lon2, out azi2, out t, out m12, out t, out t, out t);
        }

        /**
         * See the documentation for GeodesicLine::Position.
         **********************************************************************/
        public double Position(double s12, out double lat2, out double lon2,
                            out double azi2, out double M12, out double M21)
        {
            double t;
            return GenPosition(false, s12,
                               GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE |
                               GeodesicMask.AZIMUTH | GeodesicMask.GEODESICSCALE,
                               out lat2, out lon2, out azi2, out t, out t, out M12, out M21, out t);
        }

        /**
         * See the documentation for GeodesicLine::Position.
         **********************************************************************/
        public double Position(double s12,
                            out double lat2, out double lon2, out double azi2,
                            out double m12, out double M12, out double M21)
        {
            double t;
            return GenPosition(false, s12,
                               GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH |
                               GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE,
                               out lat2, out lon2, out azi2, out t, out m12, out M12, out M21, out t);
        }
        ///@}

        /** \name Position in terms of arc length
         **********************************************************************/
        ///@{

        /**
         * Compute the position of point 2 which is an arc length \e a12 (degrees)
         * from point 1.
         *
         * @param[in] a12 arc length from point 1 to point 2 (degrees); it can
         *   be negative.
         * @param[out] lat2 GeodesicMask.LATITUDE of point 2 (degrees).
         * @param[out] lon2 GeodesicMask.LONGITUDE of point 2 (degrees); requires that the
         *   GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::GeodesicMask.LONGITUDE.
         * @param[out] azi2 (forward) GeodesicMask.AZIMUTH at point 2 (degrees).
         * @param[out] s12 distance from point 1 to point 2 (meters); requires
         *   that the GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::DISTANCE.
         * @param[out] m12 reduced length of geodesic (meters); requires that the
         *   GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::GeodesicMask.REDUCEDLENGTH.
         * @param[out] M12 geodesic scale of point 2 relative to point 1
         *   (dimensionless); requires that the GeodesicLine object was constructed
         *   with \e caps |= GeodesicLine::GeodesicMask.GEODESICSCALE.
         * @param[out] M21 geodesic scale of point 1 relative to point 2
         *   (dimensionless); requires that the GeodesicLine object was constructed
         *   with \e caps |= GeodesicLine::GeodesicMask.GEODESICSCALE.
         * @param[out] S12 GeodesicMask.AREA under the geodesic (meters<sup>2</sup>); requires
         *   that the GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::GeodesicMask.AREA.
         *
         * The values of \e lon2 and \e azi2 returned are in the range
         * [&minus;180&deg;, 180&deg;].
         *
         * Requesting a value which the GeodesicLine object is not capable of
         * computing is not an error; the corresponding argument will not be
         * altered.
         *
         * The following functions are overloaded versions of
         * GeodesicLine::ArcPosition which omit some of the output parameters.
         **********************************************************************/
        void ArcPosition(double a12, out double lat2, out double lon2, out double azi2,
                         out double s12, out double m12, out double M12, out double M21,
                         out double S12)
        {
            GenPosition(true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH | GeodesicMask.DISTANCE |
                        GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE | GeodesicMask.AREA,
                        out lat2, out lon2, out azi2, out s12, out m12, out M12, out M21, out S12);
        }

        /**
         * See the documentation for GeodesicLine::ArcPosition.
         **********************************************************************/
        void ArcPosition(double a12, out double lat2, out double lon2)
        {
            double t;
            GenPosition(true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE,
                        out lat2, out lon2, out t, out t, out t, out t, out t, out t);
        }

        /**
         * See the documentation for GeodesicLine::ArcPosition.
         **********************************************************************/
        void ArcPosition(double a12,
                         out double lat2, out double lon2, out double azi2)
        {
            double t;
            GenPosition(true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH,
                        out lat2, out lon2, out azi2, out t, out t, out t, out t, out t);
        }

        /**
         * See the documentation for GeodesicLine::ArcPosition.
         **********************************************************************/
        void ArcPosition(double a12, out double lat2, out double lon2, out double azi2,
                         out double s12)
        {
            double t;
            GenPosition(true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH | GeodesicMask.DISTANCE,
                        out lat2, out lon2, out azi2, out s12, out t, out t, out t, out t);
        }

        /**
         * See the documentation for GeodesicLine::ArcPosition.
         **********************************************************************/
        void ArcPosition(double a12, out double lat2, out double lon2, out double azi2,
                         out double s12, out double m12)
        {
            double t;
            GenPosition(true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH |
                        GeodesicMask.DISTANCE | GeodesicMask.REDUCEDLENGTH,
                        out lat2, out lon2, out azi2, out s12, out m12, out t, out t, out t);
        }

        /**
         * See the documentation for GeodesicLine::ArcPosition.
         **********************************************************************/
        void ArcPosition(double a12, out double lat2, out double lon2, out double azi2,
                         out double s12, out double M12, out double M21)
        {
            double t;
            GenPosition(true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH |
                        GeodesicMask.DISTANCE | GeodesicMask.GEODESICSCALE,
                        out lat2, out lon2, out azi2, out s12, out t, out M12, out M21, out t);
        }

        /**
         * See the documentation for GeodesicLine::ArcPosition.
         **********************************************************************/
        void ArcPosition(double a12, out double lat2, out double lon2, out double azi2,
                         out double s12, out double m12, out double M12, out double M21)
        {
            double t;
            GenPosition(true, a12,
                        GeodesicMask.LATITUDE | GeodesicMask.LONGITUDE | GeodesicMask.AZIMUTH |
                        GeodesicMask.DISTANCE | GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE,
                        out lat2, out lon2, out azi2, out s12, out m12, out M12, out M21, out t);
        }
        ///@}

        /** \name The general position function.
         **********************************************************************/
        ///@{

        /**
         * The general position function.  GeodesicLine::Position and
         * GeodesicLine::ArcPosition are defined in terms of this function.
         *
         * @param[in] arcmode boolean flag determining the meaning of the second
         *   parameter; if \e arcmode is false, then the GeodesicLine object must
         *   have been constructed with \e caps |= GeodesicLine::DISTANCE_IN.
         * @param[in] s12_a12 if \e arcmode is false, this is the distance between
         *   point 1 and point 2 (meters); otherwise it is the arc length between
         *   point 1 and point 2 (degrees); it can be negative.
         * @param[in] outmask a bitor'ed combination of GeodesicLine::mask values
         *   specifying which of the following parameters should be set.
         * @param[out] lat2 GeodesicMask.LATITUDE of point 2 (degrees).
         * @param[out] lon2 GeodesicMask.LONGITUDE of point 2 (degrees); requires that the
         *   GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::GeodesicMask.LONGITUDE.
         * @param[out] azi2 (forward) GeodesicMask.AZIMUTH at point 2 (degrees).
         * @param[out] s12 distance from point 1 to point 2 (meters); requires
         *   that the GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::DISTANCE.
         * @param[out] m12 reduced length of geodesic (meters); requires that the
         *   GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::GeodesicMask.REDUCEDLENGTH.
         * @param[out] M12 geodesic scale of point 2 relative to point 1
         *   (dimensionless); requires that the GeodesicLine object was constructed
         *   with \e caps |= GeodesicLine::GeodesicMask.GEODESICSCALE.
         * @param[out] M21 geodesic scale of point 1 relative to point 2
         *   (dimensionless); requires that the GeodesicLine object was constructed
         *   with \e caps |= GeodesicLine::GeodesicMask.GEODESICSCALE.
         * @param[out] S12 GeodesicMask.AREA under the geodesic (meters<sup>2</sup>); requires
         *   that the GeodesicLine object was constructed with \e caps |=
         *   GeodesicLine::GeodesicMask.AREA.
         * @return \e a12 arc length from point 1 to point 2 (degrees).
         *
         * The GeodesicLine::mask values possible for \e outmask are
         * - \e outmask |= GeodesicLine::GeodesicMask.LATITUDE for the GeodesicMask.LATITUDE \e lat2;
         * - \e outmask |= GeodesicLine::GeodesicMask.LONGITUDE for the GeodesicMask.LATITUDE \e lon2;
         * - \e outmask |= GeodesicLine::GeodesicMask.AZIMUTH for the GeodesicMask.LATITUDE \e azi2;
         * - \e outmask |= GeodesicLine::DISTANCE for the distance \e s12;
         * - \e outmask |= GeodesicLine::GeodesicMask.REDUCEDLENGTH for the reduced length \e
         *   m12;
         * - \e outmask |= GeodesicLine::GeodesicMask.GEODESICSCALE for the geodesic scales \e
         *   M12 and \e M21;
         * - \e outmask |= GeodesicLine::GeodesicMask.AREA for the GeodesicMask.AREA \e S12;
         * - \e outmask |= GeodesicLine::ALL for all of the above;
         * - \e outmask |= GeodesicLine::LONG_UNROLL to unroll \e lon2 instead of
         *   reducing it into the range [&minus;180&deg;, 180&deg;].
         * .
         * Requesting a value which the GeodesicLine object is not capable of
         * computing is not an error; the corresponding argument will not be
         * altered.  Note, however, that the arc length is always computed and
         * returned as the function value.
         *
         * With the GeodesicLine::LONG_UNROLL bit seout t, the quantity \e lon2 &minus;
         * \e lon1 indicates how many times and in what sense the geodesic
         * encircles the ellipsoid.
         **********************************************************************/
        public double GenPosition(bool arcmode, double s12_a12, int outmask,
                               out double lat2, out double lon2, out double azi2,
                               out double s12, out double m12, out double M12, out double M21,
                               out double S12)
        {
            outmask &= (_caps & GeodesicMask.OUT_MASK);

            lat2 = double.NaN;
            lon2 = double.NaN;
            azi2 = double.NaN;
            s12 = double.NaN;
            m12 = double.NaN;
            M12 = double.NaN;
            M21 = double.NaN;
            S12 = double.NaN;

            if (!(Init() && (arcmode || (_caps & (GeodesicMask.OUT_MASK & GeodesicMask.DISTANCE_IN)) != 0)))
                // Uninitialized or impossible distance calculation requested
                return double.NaN;

            // Avoid warning about uninitialized B12.
            double sig12, ssig12, csig12, B12 = 0, AB1 = 0;
            if (arcmode)
            {
                // Interpret s12_a12 as spherical arc length
                sig12 = s12_a12 * GeoMath.Degree;
                GeoMath.Sincosd(s12_a12, out ssig12, out csig12);
            }
            else
            {
                // Interpret s12_a12 as distance
                double
                  tau12 = s12_a12 / (_b * (1 + _A1m1)),
                  s = Math.Sin(tau12),
                  c = Math.Cos(tau12);
                // tau2 = tau1 + tau12
                B12 = -Geodesic.SinCosSeries(true,
                                               _stau1 * c + _ctau1 * s,
                                               _ctau1 * c - _stau1 * s,
                                               _C1pa);
                sig12 = tau12 - (B12 - _B11);
                ssig12 = Math.Sin(sig12); csig12 = Math.Cos(sig12);
                if (Math.Abs(_f) > 0.01)
                {
                    // Reverted distance series is inaccurate for |f| > 1/100, so correct
                    // sig12 with 1 Newton iteration.  The following table shows the
                    // approximate maximum error for a = WGS_a() and various f relative to
                    // GeodesicExact.
                    //     erri = the error in the inverse solution (nm)
                    //     errd = the error in the direct solution (series only) (nm)
                    //     errda = the error in the direct solution
                    //             (series + 1 Newton) (nm)
                    //
                    //       f     erri  errd errda
                    //     -1/5    12e6 1.2e9  69e6
                    //     -1/10  123e3  12e6 765e3
                    //     -1/20   1110 108e3  7155
                    //     -1/50  18.63 200.9 27.12
                    //     -1/100 18.63 23.78 23.37
                    //     -1/150 18.63 21.05 20.26
                    //      1/150 22.35 24.73 25.83
                    //      1/100 22.35 25.03 25.31
                    //      1/50  29.80 231.9 30.44
                    //      1/20   5376 146e3  10e3
                    //      1/10  829e3  22e6 1.5e6
                    //      1/5   157e6 3.8e9 280e6
                    double
                      sssig2 = _ssig1 * csig12 + _csig1 * ssig12,
                      scsig2 = _csig1 * csig12 - _ssig1 * ssig12;
                    B12 = Geodesic.SinCosSeries(true, sssig2, scsig2, _C1a);
                    double serr = (1 + _A1m1) * (sig12 + (B12 - _B11)) - s12_a12 / _b;
                    sig12 = sig12 - serr / Math.Sqrt(1 + _k2 * GeoMath.Square(sssig2));
                    ssig12 = Math.Sin(sig12); csig12 = Math.Cos(sig12);
                    // Update B12 below
                }
            }

            double ssig2, csig2, sbet2, cbet2, salp2, calp2;
            // sig2 = sig1 + sig12
            ssig2 = _ssig1 * csig12 + _csig1 * ssig12;
            csig2 = _csig1 * csig12 - _ssig1 * ssig12;
            double dn2 = Math.Sqrt(1 + _k2 * GeoMath.Square(ssig2));
            if ((outmask & (GeodesicMask.DISTANCE | GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE)) != 0)
            {
                if (arcmode || Math.Abs(_f) > 0.01)
                    B12 = Geodesic.SinCosSeries(true, ssig2, csig2, _C1a);
                AB1 = (1 + _A1m1) * (B12 - _B11);
            }
            // sin(bet2) = cos(alp0) * sin(sig2)
            sbet2 = _calp0 * ssig2;
            // Alt: cbet2 = hypot(csig2, salp0 * ssig2);
            cbet2 = GeoMath.Hypot(_salp0, _calp0 * csig2);
            if (cbet2 == 0)
                // I.e., salp0 = 0, csig2 = 0.  Break the degeneracy in this case
                cbet2 = csig2 = Geodesic.tiny_;
            // tan(alp0) = cos(sig2)*tan(alp2)
            salp2 = _salp0; calp2 = _calp0 * csig2; // No need to normalize

            if ((outmask & GeodesicMask.DISTANCE) != 0)
                s12 = arcmode ? _b * ((1 + _A1m1) * sig12 + AB1) : s12_a12;

            if ((outmask & GeodesicMask.LONGITUDE) != 0)
            {
                // tan(omg2) = sin(alp0) * tan(sig2)
                double somg2 = _salp0 * ssig2, comg2 = csig2,  // No need to normalize
                  E = GeoMath.CopySign(1, _salp0);            // east-going?
                                                              // omg12 = omg2 - omg1
                double omg12 = (outmask & GeodesicMask.LONG_UNROLL) != 0
                  ? E * (sig12
                         - (Math.Atan2(ssig2, csig2) - Math.Atan2(_ssig1, _csig1))
                         + (Math.Atan2(E * somg2, comg2) - Math.Atan2(E * _somg1, _comg1)))
                  : Math.Atan2(somg2 * _comg1 - comg2 * _somg1,
                          comg2 * _comg1 + somg2 * _somg1);
                double lam12 = omg12 + _A3c *
                  (sig12 + (Geodesic.SinCosSeries(true, ssig2, csig2, _C3a)
                             - _B31));
                double lon12 = lam12 / GeoMath.Degree;
                lon2 = (outmask & GeodesicMask.LONG_UNROLL) != 0 ? _lon1 + lon12 :
                  GeoMath.AngNormalize(GeoMath.AngNormalize(_lon1) +
                                       GeoMath.AngNormalize(lon12));
            }

            if ((outmask & GeodesicMask.LATITUDE) != 0)
                lat2 = GeoMath.Atan2d(sbet2, _f1 * cbet2);

            if ((outmask & GeodesicMask.AZIMUTH) != 0)
                azi2 = GeoMath.Atan2d(salp2, calp2);

            if ((outmask & (GeodesicMask.REDUCEDLENGTH | GeodesicMask.GEODESICSCALE)) != 0)
            {
                double
                  B22 = Geodesic.SinCosSeries(true, ssig2, csig2, _C2a),
                  AB2 = (1 + _A2m1) * (B22 - _B21),
                  J12 = (_A1m1 - _A2m1) * sig12 + (AB1 - AB2);
                if ((outmask & GeodesicMask.REDUCEDLENGTH) != 0)
                    // Add parens around (_csig1 * ssig2) and (_ssig1 * csig2) to ensure
                    // accurate cancellation in the case of coincident points.
                    m12 = _b * ((dn2 * (_csig1 * ssig2) - _dn1 * (_ssig1 * csig2))
                                - _csig1 * csig2 * J12);
                if ((outmask & GeodesicMask.GEODESICSCALE) != 0)
                {
                    double t = _k2 * (ssig2 - _ssig1) * (ssig2 + _ssig1) / (_dn1 + dn2);
                    M12 = csig12 + (t * ssig2 - csig2 * J12) * _ssig1 / _dn1;
                    M21 = csig12 - (t * _ssig1 - _csig1 * J12) * ssig2 / dn2;
                }
            }

            if ((outmask & GeodesicMask.AREA) != 0)
            {
                double
                  B42 = Geodesic.SinCosSeries(false, ssig2, csig2, _C4a);
                double salp12, calp12;
                if (_calp0 == 0 || _salp0 == 0)
                {
                    // alp12 = alp2 - alp1, used in Math.Atan2 so no need to normalize
                    salp12 = salp2 * _calp1 - calp2 * _salp1;
                    calp12 = calp2 * _calp1 + salp2 * _salp1;
                    // We used to include here some patch up code that purported to deal
                    // with nearly meridional geodesics properly.  However, this turned out
                    // to be wrong once _salp1 = -0 was allowed (via
                    // Geodesic::InverseLine).  In fact, the calculation of {s,c}alp12
                    // was already correct (following the IEEE rules for handling signed
                    // zeros).  So the patch up code was unnecessary (as well as
                    // dangerous).
                }
                else
                {
                    // tan(alp) = tan(alp0) * sec(sig)
                    // tan(alp2-alp1) = (tan(alp2) -tan(alp1)) / (tan(alp2)*tan(alp1)+1)
                    // = calp0 * salp0 * (csig1-csig2) / (salp0^2 + calp0^2 * csig1*csig2)
                    // If csig12 > 0, write
                    //   csig1 - csig2 = ssig12 * (csig1 * ssig12 / (1 + csig12) + ssig1)
                    // else
                    //   csig1 - csig2 = csig1 * (1 - csig12) + ssig12 * ssig1
                    // No need to normalize
                    salp12 = _calp0 * _salp0 *
                      (csig12 <= 0 ? _csig1 * (1 - csig12) + ssig12 * _ssig1 :
                       ssig12 * (_csig1 * ssig12 / (1 + csig12) + _ssig1));
                    calp12 = GeoMath.Square(_salp0) + GeoMath.Square(_calp0) * _csig1 * csig2;
                }
                S12 = _c2 * Math.Atan2(salp12, calp12) + _A4 * (B42 - _B41);
            }

            return arcmode ? s12_a12 : sig12 / GeoMath.Degree;
        }
        ///@}

        /** \name Setting point 3
         **********************************************************************/
        ///@{

        /**
         * Specify position of point 3 in terms of distance.
         *
         * @param[in] s13 the distance from point 1 to point 3 (meters); it
         *   can be negative.
         *
         * This is only useful if the GeodesicLine object has been constructed
         * with \e caps |= GeodesicLine::DISTANCE_IN.
         **********************************************************************/
        public void SetDistance(double s13)
        {
            _s13 = s13;
            double t;
            // This will set _a13 to NaN if the GeodesicLine doesn't have the
            // DISTANCE_IN capability.
            _a13 = GenPosition(false, _s13, 0, out t, out t, out t, out t, out t, out t, out t, out t);
        }

        /**
         * Specify position of point 3 in terms of arc length.
         *
         * @param[in] a13 the arc length from point 1 to point 3 (degrees); it
         *   can be negative.
         *
         * The distance \e s13 is only set if the GeodesicLine object has been
         * constructed with \e caps |= GeodesicLine::DISTANCE.
         **********************************************************************/
        public void SetArc(double a13)
        {
            _a13 = a13;
            // In case the GeodesicLine doesn't have the DISTANCE capability.
            _s13 = double.NaN;
            double t;
            GenPosition(true, _a13, GeodesicMask.DISTANCE, out t, out t, out t, out _s13, out t, out t, out t, out t);
        }

        /**
         * Specify position of point 3 in terms of either distance or arc length.
         *
         * @param[in] arcmode boolean flag determining the meaning of the second
         *   parameter; if \e arcmode is false, then the GeodesicLine object must
         *   have been constructed with \e caps |= GeodesicLine::DISTANCE_IN.
         * @param[in] s13_a13 if \e arcmode is false, this is the distance from
         *   point 1 to point 3 (meters); otherwise it is the arc length from
         *   point 1 to point 3 (degrees); it can be negative.
         **********************************************************************/
        public void GenSetDistance(bool arcmode, double s13_a13)
        {
            if (arcmode)
                SetArc(s13_a13);
            else
                SetDistance(s13_a13);
        }
        ///@}

        /** \name Inspector functions
         **********************************************************************/
        ///@{

        /**
         * @return true if the object has been initialized.
         **********************************************************************/
        public bool Init()
        {
            return _caps != 0U;
        }

        /**
         * @return \e lat1 the latitude of point 1 (degrees).
         **********************************************************************/
        public double Latitude()
        {
            return Init() ? _lat1 : double.NaN;
        }

        /**
         * @return \e lon1 the longitude of point 1 (degrees).
         **********************************************************************/
        public double Longitude()
        {
            return Init() ? _lon1 : double.NaN;
        }

        /**
         * @return \e azi1 the azimuth (degrees) of the geodesic line at point 1.
         **********************************************************************/
        public double Azimuth()
        {
            return Init() ? _azi1 : double.NaN;
        }

        /**
         * The sine and cosine of \e azi1.
         *
         * @param[out] sazi1 the sine of \e azi1.
         * @param[out] cazi1 the cosine of \e azi1.
         **********************************************************************/
        public void Azimuth(double sazi1, double cazi1)
        {
            if (Init()) { sazi1 = _salp1; cazi1 = _calp1; }
        }

        /**
         * @return \e azi0 the azimuth (degrees) of the geodesic line as it crosses
         *   the equator in a northward direction.
         *
         * The result lies in [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public double EquatorialAzimuth()
        {
            return Init() ? GeoMath.Atan2d(_salp0, _calp0) : double.NaN;
        }

        /**
         * The sine and cosine of \e azi0.
         *
         * @param[out] sazi0 the sine of \e azi0.
         * @param[out] cazi0 the cosine of \e azi0.
         **********************************************************************/
        public void EquatorialAzimuth(double sazi0, double cazi0)
        {
            if (Init()) { sazi0 = _salp0; cazi0 = _calp0; }
        }

        /**
         * @return \e a1 the arc length (degrees) between the northward equatorial
         *   crossing and point 1.
         *
         * The result lies in (&minus;180&deg;, 180&deg;].
         **********************************************************************/
        public double EquatorialArc()
        {
            return Init() ? GeoMath.Atan2d(_ssig1, _csig1) : double.NaN;
        }

        /**
         * @return \e a the equatorial radius of the ellipsoid (meters).  This is
         *   the value inherited from the Geodesic object used in the constructor.
         **********************************************************************/
        public double MajorRadius()
        {
            return Init() ? _a : double.NaN;
        }

        /**
         * @return \e f the flattening of the ellipsoid.  This is the value
         *   inherited from the Geodesic object used in the constructor.
         **********************************************************************/
        public double Flattening()
        {
            return Init() ? _f : double.NaN;
        }

        /**
         * @return \e caps the computational capabilities that this object was
         *   constructed with.  LATITUDE and AZIMUTH are always included.
         **********************************************************************/
        public int Capabilities()
        {
            return _caps;
        }

        /**
         * Test what capabilities are available.
         *
         * @param[in] testcaps a set of bitor'ed GeodesicLine::mask values.
         * @return true if the GeodesicLine object has all these capabilities.
         **********************************************************************/
        public bool Capabilities(int testcaps)
        {
            testcaps &= GeodesicMask.OUT_ALL;
            return (_caps & testcaps) == testcaps;
        }

        /**
         * The distance or arc length to point 3.
         *
         * @param[in] arcmode boolean flag determining the meaning of returned
         *   value.
         * @return \e s13 if \e arcmode is false; \e a13 if \e arcmode is true.
         **********************************************************************/
        public double GenDistance(bool arcmode)
        {
            return Init() ? (arcmode ? _a13 : _s13) : double.NaN;
        }

        /**
         * @return \e s13, the distance to point 3 (meters).
         **********************************************************************/
        public double Distance()
        {
            return GenDistance(false);
        }

        /**
         * @return \e a13, the arc length to point 3 (degrees).
         **********************************************************************/
        public double Arc()
        {
            return GenDistance(true);
        }

        private void LineInit(Geodesic g,
                              double lat1, double lon1,
                              double azi1, double salp1, double calp1,
                              int caps)
        {
            _lat1 = GeoMath.LatFix(lat1);
            _lon1 = lon1;
            _azi1 = azi1;
            _salp1 = salp1;
            _calp1 = calp1;
            _a = g._a;
            _f = g._f;
            _b = g._b;
            _c2 = g._c2;
            _f1 = g._f1;
            // Always allow latitude and azimuth and unrolling of longitude
            _caps = ((int)caps | (int)GeodesicMask.LATITUDE | (int)GeodesicMask.AZIMUTH | (int)GeodesicMask.LONG_UNROLL); //TODO: check this

            double cbet1, sbet1;
            GeoMath.Sincosd(GeoMath.AngRound(_lat1), out sbet1, out cbet1); sbet1 *= _f1;
            // Ensure cbet1 = +epsilon at poles
            GeoMath.Norm(ref sbet1, ref cbet1);
            cbet1 = Math.Max(Geodesic.tiny_, cbet1);
            _dn1 = Math.Sqrt(1 + g._ep2 * GeoMath.Square(sbet1));

            // Evaluate alp0 from sin(alp1) * cos(bet1) = sin(alp0),
            _salp0 = _salp1 * cbet1; // alp0 in [0, pi/2 - |bet1|]
                                     // Alt: calp0 = hypot(sbet1, calp1 * cbet1).  The following
                                     // is slightly better (consider the case salp1 = 0).
            _calp0 = GeoMath.Hypot(_calp1, _salp1 * sbet1);
            // Evaluate sig with tan(bet1) = tan(sig1) * cos(alp1).
            // sig = 0 is nearest northward crossing of equator.
            // With bet1 = 0, alp1 = pi/2, we have sig1 = 0 (equatorial line).
            // With bet1 =  pi/2, alp1 = -pi, sig1 =  pi/2
            // With bet1 = -pi/2, alp1 =  0 , sig1 = -pi/2
            // Evaluate omg1 with tan(omg1) = sin(alp0) * tan(sig1).
            // With alp0 in (0, pi/2], quadrants for sig and omg coincide.
            // No Math.Atan2(0,0) ambiguity at poles since cbet1 = +epsilon.
            // With alp0 = 0, omg1 = 0 for alp1 = 0, omg1 = pi for alp1 = pi.
            _ssig1 = sbet1; _somg1 = _salp0 * sbet1;
            _csig1 = _comg1 = sbet1 != 0 || _calp1 != 0 ? cbet1 * _calp1 : 1;
            GeoMath.Norm(ref _ssig1, ref _csig1); // sig1 in (-pi, pi]
                                                  // Math::norm(_somg1, _comg1); -- don't need to normalize!

            _k2 = GeoMath.Square(_calp0) * g._ep2;
            double eps = _k2 / (2 * (1 + Math.Sqrt(1 + _k2)) + _k2);

            if ((_caps & GeodesicMask.CAP_C1) != 0)
            {
                _C1a = new double[nC1_ + 1];
                _A1m1 = GeodesicCoeff.A1m1f(eps);
                GeodesicCoeff.C1f(eps, _C1a);
                _B11 = Geodesic.SinCosSeries(true, _ssig1, _csig1, _C1a);
                double s = Math.Sin(_B11), c = Math.Cos(_B11);
                // tau1 = sig1 + B11
                _stau1 = _ssig1 * c + _csig1 * s;
                _ctau1 = _csig1 * c - _ssig1 * s;
                // Not necessary because C1pa reverts C1a
                //    _B11 = -SinCosSeries(true, _stau1, _ctau1, _C1pa, nC1p_);
            }

            if ((_caps & GeodesicMask.CAP_C1p) != 0)
            {
                _C1pa = new double[nC1p_ + 1];
                GeodesicCoeff.C1pf(eps, _C1pa);
            }

            if ((_caps & GeodesicMask.CAP_C2) != 0)
            {
                _C2a = new double[nC2_ + 1];
                _A2m1 = GeodesicCoeff.A2m1f(eps);
                GeodesicCoeff.C2f(eps, _C2a);
                _B21 = Geodesic.SinCosSeries(true, _ssig1, _csig1, _C2a);
            }

            if ((_caps & GeodesicMask.CAP_C3) != 0)
            {
                _C3a = new double[nC3_];
                g.C3f(eps, _C3a);
                _A3c = -_f * _salp0 * g.A3f(eps);
                _B31 = Geodesic.SinCosSeries(true, _ssig1, _csig1, _C3a);
            }

            if ((_caps & GeodesicMask.CAP_C4) != 0)
            {
                _C4a = new double[nC4_];
                g.C4f(eps, _C4a);
                // Multiplier = a^2 * e^2 * cos(alpha0) * sin(alpha0)
                _A4 = GeoMath.Square(_a) * _calp0 * _salp0 * g._e2;
                _B41 = Geodesic.SinCosSeries(false, _ssig1, _csig1, _C4a);
            }

            _a13 = _s13 = double.NaN;
        }
    }
}
