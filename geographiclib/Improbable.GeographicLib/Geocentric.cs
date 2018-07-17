using System;
using System.Collections.Generic;

namespace Improbable.GeographicLib
{
    /**
   * \brief %Geocentric coordinates
   *
   * Convert between geodetic coordinates latitude = \e lat, longitude = \e
   * lon, height = \e h (measured vertically from the surface of the ellipsoid)
   * to geocentric coordinates (\e X, \e Y, \e Z).  The origin of geocentric
   * coordinates is at the center of the earth.  The \e Z axis goes thru the
   * north pole, \e lat = 90&deg;.  The \e X axis goes thru \e lat = 0,
   * \e lon = 0.  %Geocentric coordinates are also known as earth centered,
   * earth fixed (ECEF) coordinates.
   *
   * The conversion from geographic to geocentric coordinates is
   * straightforward.  For the reverse transformation we use
   * - H. Vermeille,
   *   <a href="https://doi.org/10.1007/s00190-002-0273-6"> Direct
   *   transformation from geocentric coordinates to geodetic coordinates</a>,
   *   J. Geodesy 76, 451--454 (2002).
   * .
   * Several changes have been made to ensure that the method returns accurate
   * results for all finite inputs (even if \e h is infinite).  The changes are
   * described in Appendix B of
   * - C. F. F. Karney,
   *   <a href="https://arxiv.org/abs/1102.1215v1">Geodesics
   *   on an ellipsoid of revolution</a>,
   *   Feb. 2011;
   *   preprint
   *   <a href="https://arxiv.org/abs/1102.1215v1">arxiv:1102.1215v1</a>.
   * .
   * Vermeille similarly updated his method in
   * - H. Vermeille,
   *   <a href="https://doi.org/10.1007/s00190-010-0419-x">
   *   An analytical method to transform geocentric into
   *   geodetic coordinates</a>, J. Geodesy 85, 105--117 (2011).
   * .
   * See \ref geocentric for more information.
   *
   * The errors in these routines are close to round-off.  Specifically, for
   * points within 5000 km of the surface of the ellipsoid (either inside or
   * outside the ellipsoid), the error is bounded by 7 nm (7 nanometers) for
   * the WGS84 ellipsoid.  See \ref geocentric for further information on the
   * errors.
   *
   * Example of use:
   * \include example-Geocentric.cpp
   *
   * <a href="CartConvert.1.html">CartConvert</a> is a command-line utility
   * providing access to the functionality of Geocentric and LocalCartesian.
   **********************************************************************/
    public class Geocentric
    {
        static long dim_ = 3;
        static long dim2_ = dim_ * dim_;
        double _a, _f, _e2, _e2m, _e2a, _e4a, _maxrad;

        /**
         * Constructor for a ellipsoid with
         *
         * @param[in] a equatorial radius (meters).
         * @param[in] f flattening of ellipsoid.  Setting \e f = 0 gives a sphere.
         *   Negative \e f gives a prolate ellipsoid.
         * @exception GeographicErr if \e a or (1 &minus; \e f) \e a is not
         *   positive.
         **********************************************************************/
        public Geocentric(double a, double f)
        {
            _a = a;
            _f = f;
            _e2 = _f * (2 - _f);
            _e2m = GeoMath.Square(1 - _f);
            _e2a = Math.Abs(_e2);
            _e4a = GeoMath.Square(_e2);
            _maxrad = 2 * _a / GeoMath.Epsilon;

            if (!(GeoMath.IsFinite(_a) && _a > 0))
                throw new GeographicException("Equatorial radius is not positive");
            if (!(GeoMath.IsFinite(_f) && _f < 1))
                throw new GeographicException("Polar semi-axis is not positive");
        }

        /**
         * A default constructor (for use by NormalGravity).
         **********************************************************************/
        public Geocentric()
        {
            _a = -1;
        }

        /**
         * Convert from geodetic to geocentric coordinates.
         *
         * @param[in] lat latitude of point (degrees).
         * @param[in] lon longitude of point (degrees).
         * @param[in] h height of point above the ellipsoid (meters).
         * @param[out] X geocentric coordinate (meters).
         * @param[out] Y geocentric coordinate (meters).
         * @param[out] Z geocentric coordinate (meters).
         *
         * \e lat should be in the range [&minus;90&deg;, 90&deg;].
         **********************************************************************/
        public void Forward(double lat, double lon, double h, out double X, out double Y, out double Z)
        {
            if (Init())
            {
                IntForward(lat, lon, h, out X, out Y, out Z, null);
            }
            else
            {
                X = double.NaN;
                Y = double.NaN;
                Z = double.NaN;
            }
        }

        /**
         * Convert from geodetic to geocentric coordinates and return rotation
         * matrix.
         *
         * @param[in] lat latitude of point (degrees).
         * @param[in] lon longitude of point (degrees).
         * @param[in] h height of point above the ellipsoid (meters).
         * @param[out] X geocentric coordinate (meters).
         * @param[out] Y geocentric coordinate (meters).
         * @param[out] Z geocentric coordinate (meters).
         * @param[out] M if the length of the vector is 9, fill with the rotation
         *   matrix in row-major order.
         *
         * Let \e v be a unit vector located at (\e lat, \e lon, \e h).  We can
         * express \e v as \e column vectors in one of two ways
         * - in east, north, up coordinates (where the components are relative to a
         *   local coordinate system at (\e lat, \e lon, \e h)); call this
         *   representation \e v1.
         * - in geocentric \e X, \e Y, \e Z coordinates; call this representation
         *   \e v0.
         * .
         * Then we have \e v0 = \e M &sdot; \e v1.
         **********************************************************************/
        public void Forward(double lat, double lon, double h, out double X, out double Y, out double Z,
                     ref List<double> M)
        {
            if (!Init())
            {
                X = double.NaN;
                Y = double.NaN;
                Z = double.NaN;
                return;
            }

            if (M != null && M.Count == dim2_)
            {
                var t = new double[dim2_];
                IntForward(lat, lon, h, out X, out Y, out Z, t);
                M.Clear();
                M.AddRange(t);
            }
            else
            {
                IntForward(lat, lon, h, out X, out Y, out Z, null);
            }
        }

        /**
         * Convert from geocentric to geodetic to coordinates.
         *
         * @param[in] X geocentric coordinate (meters).
         * @param[in] Y geocentric coordinate (meters).
         * @param[in] Z geocentric coordinate (meters).
         * @param[out] lat latitude of point (degrees).
         * @param[out] lon longitude of point (degrees).
         * @param[out] h height of point above the ellipsoid (meters).
         *
         * In general there are multiple solutions and the result which maximizes
         * \e h is returned.  If there are still multiple solutions with different
         * latitudes (applies only if \e Z = 0), then the solution with \e lat > 0
         * is returned.  If there are still multiple solutions with different
         * longitudes (applies only if \e X = \e Y = 0) then \e lon = 0 is
         * returned.  The value of \e h returned satisfies \e h &ge; &minus; \e a
         * (1 &minus; <i>e</i><sup>2</sup>) / Math.Sqrt(1 &minus; <i>e</i><sup>2</sup>
         * sin<sup>2</sup>\e lat).  The value of \e lon returned is in the range
         * [&minus;180&deg;, 180&deg;].
         **********************************************************************/
        public void Reverse(double X, double Y, double Z, out double lat, out double lon, out double h)
        {
            if (Init())
            {
                IntReverse(X, Y, Z, out lat, out lon, out h, null);
            }
            else
            {
                lat = double.NaN;
                lon = double.NaN;
                h = double.NaN;
            }
        }

        /**
         * Convert from geocentric to geodetic to coordinates.
         *
         * @param[in] X geocentric coordinate (meters).
         * @param[in] Y geocentric coordinate (meters).
         * @param[in] Z geocentric coordinate (meters).
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
         * - in geocentric \e X, \e Y, \e Z coordinates; call this representation
         *   \e v0.
         * .
         * Then we have \e v1 = <i>M</i><sup>T</sup> &sdot; \e v0, where
         * <i>M</i><sup>T</sup> is the transpose of \e M.
         **********************************************************************/
        public void Reverse(double X, double Y, double Z, out double lat, out double lon, out double h,
                     ref List<double> M)
        {
            if (!Init())
            {
                lat = double.NaN;
                lon = double.NaN;
                h = double.NaN;
                return;
            }

            if (M != null && M.Count == dim2_)
            {
                var t = new double[dim2_];
                IntReverse(X, Y, Z, out lat, out lon, out h, t);
                M.Clear();
                M.AddRange(t);
            }
            else
            {
                IntReverse(X, Y, Z, out lat, out lon, out h, null);
            }
        }

        /** \name Inspector functions
         **********************************************************************/
        ///@{
        /**
         * @return true if the object has been initialized.
         **********************************************************************/
        public bool Init()
        {
            return _a > 0;
        }
        /**
         * @return \e a the equatorial radius of the ellipsoid (meters).  This is
         *   the value used in the constructor.
         **********************************************************************/
        public double MajorRadius()
        {
            return Init() ? _a : double.NaN;
        }

        /**
         * @return \e f the  flattening of the ellipsoid.  This is the
         *   value used in the constructor.
         **********************************************************************/
        public double Flattening()
        {
            return Init() ? _f : double.NaN;
        }
        ///@}

        /**
         * A global instantiation of Geocentric with the parameters for the WGS84
         * ellipsoid.
         **********************************************************************/
        public static Geocentric WGS84()
        {
            return new Geocentric(Constants.WGS84_a, Constants.WGS84_f);
        }

        internal static void Rotation(double sphi, double cphi, double slam, double clam,
         double[] M)
        {
            // This rotation matrix is given by the following quaternion operations
            // qrot(lam, [0,0,1]) * qrot(phi, [0,-1,0]) * [1,1,1,1]/2
            // or
            // qrot(pi/2 + lam, [0,0,1]) * qrot(-pi/2 + phi , [-1,0,0])
            // where
            // qrot(t,v) = [cos(t/2), sin(t/2)*v[1], sin(t/2)*v[2], sin(t/2)*v[3]]

            // Local X axis (east) in geocentric coords
            M[0] = -slam; M[3] = clam; M[6] = 0;
            // Local Y axis (north) in geocentric coords
            M[1] = -clam * sphi; M[4] = -slam * sphi; M[7] = cphi;
            // Local Z axis (up) in geocentric coords
            M[2] = clam * cphi; M[5] = slam * cphi; M[8] = sphi;
        }

        internal static void Rotate(double[] M, double x, double y, double z,
         out double X, out double Y, out double Z)
        {
            // Perform [X,Y,Z]^t = M.[x,y,z]^t
            // (typically local cartesian to geocentric)
            X = M[0] * x + M[1] * y + M[2] * z;
            Y = M[3] * x + M[4] * y + M[5] * z;
            Z = M[6] * x + M[7] * y + M[8] * z;
        }

        internal static void Unrotate(double[] M, double X, double Y, double Z,
         out double x, out double y, out double z)
        {
            // Perform [x,y,z]^t = M^t.[X,Y,Z]^t
            // (typically geocentric to local cartesian)
            x = M[0] * X + M[3] * Y + M[6] * Z;
            y = M[1] * X + M[4] * Y + M[7] * Z;
            z = M[2] * X + M[5] * Y + M[8] * Z;
        }

        internal void IntForward(double lat, double lon, double h, out double X, out double Y, out double Z,
         double[] M)
        {
            double sphi, cphi, slam, clam;
            GeoMath.Sincosd(GeoMath.LatFix(lat), out sphi, out cphi);
            GeoMath.Sincosd(lon, out slam, out clam);
            double n = _a / Math.Sqrt(1 - _e2 * GeoMath.Square(sphi));
            Z = (_e2m * n + h) * sphi;
            X = (n + h) * cphi;
            Y = X * slam;
            X *= clam;
            if (M != null)
            {
                Rotation(sphi, cphi, slam, clam, M);
            }
        }

        internal void IntReverse(double X, double Y, double Z, out double lat, out double lon, out double h,
         double[] M)
        {
            double
            R = GeoMath.Hypot(X, Y),
            slam = R != 0 ? Y / R : 0,
            clam = R != 0 ? X / R : 1;
            h = GeoMath.Hypot(R, Z);      // Distance to center of earth
            double sphi, cphi;
            if (h > _maxrad)
            {
                // We doublely far away (> 12 million light years); treat the earth as a
                // point and h, above, is an acceptable approximation to the height.
                // This avoids overflow, e.g., in the computation of disc below.  It's
                // possible that h has overflowed to inf; but that's OK.
                //
                // Treat the case X, Y finite, but R overflows to +inf by scaling by 2.
                R = GeoMath.Hypot(X / 2, Y / 2);
                slam = R != 0 ? (Y / 2) / R : 0;
                clam = R != 0 ? (X / 2) / R : 1;
                double H = GeoMath.Hypot(Z / 2, R);
                sphi = (Z / 2) / H;
                cphi = R / H;
            }
            else if (_e4a == 0)
            {
                // Treat the spherical case.  Dealing with underflow in the general case
                // with _e2 = 0 is difficult.  Origin maps to N pole same as with
                // ellipsoid.
                double H = GeoMath.Hypot(h == 0 ? 1 : Z, R);
                sphi = (h == 0 ? 1 : Z) / H;
                cphi = R / H;
                h -= _a;
            }
            else
            {
                // Treat prolate spheroids by swapping R and Z here and by switching
                // the arguments to phi = atan2(...) at the end.
                double
                  p = GeoMath.Square(R / _a),
                  q = _e2m * GeoMath.Square(Z / _a),
                  r = (p + q - _e4a) / 6;
                if (_f < 0) Utility.Swap(ref p, ref q);
                if (!(_e4a * q == 0 && r <= 0))
                {
                    double
                      // Avoid possible division by zero when r = 0 by multiplying
                      // equations for s and t by r^3 and r, resp.
                      S = _e4a * p * q / 4, // S = r^3 * s
                      r2 = GeoMath.Square(r),
                      r3 = r * r2,
                      disc = S * (2 * r3 + S);
                    double u = r;
                    if (disc >= 0)
                    {
                        double T3 = S + r3;
                        // Pick the sign on the sqrt to maximize abs(T3).  This minimizes
                        // loss of precision due to cancellation.  The result is unchanged
                        // because of the way the T is used in definition of u.
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
                      v = Math.Sqrt(GeoMath.Square(u) + _e4a * q), // guaranteed positive
                                                                   // Avoid loss of accuracy when u < 0.  Underflow doesn't occur in
                                                                   // e4 * q / (v - u) because u ~ e^4 when q is small and u < 0.
                      uv = u < 0 ? _e4a * q / (v - u) : u + v, // u+v, guaranteed positive
                                                               // Need to guard against w going negative due to roundoff in uv - q.
                      w = Math.Max(0, _e2a * (uv - q) / (2 * v)),
                      // Rearrange expression for k to avoid loss of accuracy due to
                      // subtraction.  Division by 0 not possible because uv > 0, w >= 0.
                      k = uv / (Math.Sqrt(uv + GeoMath.Square(w)) + w),
                      k1 = _f >= 0 ? k : k - _e2,
                      k2 = _f >= 0 ? k + _e2 : k,
                      d = k1 * R / k2,
                      H = GeoMath.Hypot(Z / k1, R / k2);
                    sphi = (Z / k1) / H;
                    cphi = (R / k2) / H;
                    h = (1 - _e2m / k1) * GeoMath.Hypot(d, Z);
                }
                else
                {                  // e4 * q == 0 && r <= 0
                                   // This leads to k = 0 (oblate, equatorial plane) and k + e^2 = 0
                                   // (prolate, rotation axis) and the generation of 0/0 in the general
                                   // formulas for phi and h.  using the general formula and division by 0
                                   // in formula for h.  So handle this case by taking the limits:
                                   // f > 0: z -> 0, k      ->   e2 * Math.Sqrt(q)/Math.Sqrt(e4 - p)
                                   // f < 0: R -> 0, k + e2 -> - e2 * Math.Sqrt(q)/Math.Sqrt(e4 - p)
                    double
                      zz = Math.Sqrt((_f >= 0 ? _e4a - p : p) / _e2m),
                      xx = Math.Sqrt(_f < 0 ? _e4a - p : p),
                      H = GeoMath.Hypot(zz, xx);
                    sphi = zz / H;
                    cphi = xx / H;
                    if (Z < 0) sphi = -sphi; // for tiny negative Z (not for prolate)
                    h = -_a * (_f >= 0 ? _e2m : 1) * H / _e2a;
                }
            }
            lat = GeoMath.Atan2d(sphi, cphi);
            lon = GeoMath.Atan2d(slam, clam);
            if (M != null)
            {
                Rotation(sphi, cphi, slam, clam, M);
            }
        }
    }
}
