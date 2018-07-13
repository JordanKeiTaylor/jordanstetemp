/* 
 * Copyright (c) Charles Karney (2009-2017) <charles@karney.com> and licensed
 * under the MIT/X11 License.For more information, see
 * https://geographiclib.sourceforge.io/
 */

// used: https://github.com/suryapratap/GeographicLib/blob/master/GeographicLib/GeoMath.cs

using System;

namespace Improbable.GeographicLib
{
    internal class GeoMath
    {
        /// <summary>
        /// The number of binary Digits in the fraction of a double precision
        //// number(equivalent to C++'s {@code numeric_limits<double>::Digits}.
        /// </summary>
        public static readonly int Digits = 53;

        /// <summary>
        /// Equivalent to C++'s {@code numeric_limits<double>::Epsilon()}.
        /// </summary>
        public static readonly double Epsilon = Math.Pow(0.5, Digits - 1);

        /// <summary>
        /// Equivalent to C++'s {@code numeric_limits<double>::Min()}.
        /// </summary>
        public static readonly double Min = Math.Pow(0.5, 1022);

        /// <summary>
        /// The number of radians in a Degree.
        /// </summary>
        public static readonly double Degree = Math.PI / 180;

        /// <summary>
        /// Square a number.
        /// </summary>
        /// <returns>The squared number.</returns>
        /// <param name="x">The number to square.</param>
        public static double Square(double x) { return x * x; }

        /// <summary>
        /// The hypotenuse function (avoides underflow and overflow).
        /// </summary>
        /// <returns>The hypotenuse of x and y.</returns>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        public static double Hypot(double x, double y)
        {
            x = Math.Abs(x); y = Math.Abs(y);
            double a = Math.Max(x, y), b = Math.Min(x, y) / (Math.Abs(a) > Epsilon ? a : 1);
            return a * Math.Sqrt(1 + b * b);
        }

        /// <summary>
        /// Log(1 + x) accuratre near x = 0;
        /// </summary>
        /// <returns>The log1p.</returns>
        /// <param name="x">The x coordinate.</param>
        public static double Log1p(double x)
        {
            double y = 1 + x;
            double z = y - 1;
            // Here's the explanation for this magic: y = 1 + z, exactly, and z
            // approx x, thus log(y)/z (which is nearly constant near z = 0) returns
            // a good approximation to the true log(1 + x)/x.  The multiplication x *
            // (log(y)/z) introduces little additional error.
            return Math.Abs(z) < Epsilon ? x : x * Math.Log(y) / z;
        }

        /// <summary>
        /// The inverse hyperbolic tangent function.  This is defined in terms of
        /// GeoMath.Log1p(x) in order to maintain accuracy near x = 0.
        /// In addition, the odd parity of the function is enforced.
        /// </summary>
        /// <returns>The inverse hyperbolic tangent.</returns>
        /// <param name="x">The x value.</param>
        public static double Atanh(double x)
        {
            double y = Math.Abs(x); // Enforce odd parity
            y = Math.Log(2 * y / (1 - y)) / 2;
            return x < 0 ? -y : y;
        }

        public static double Taupf(double tau, double es)
        {
            double tau1 = Hypot(1, tau),
              sig = Math.Sinh(Eatanhe(tau / tau1, es));
            return Hypot(1, sig) * tau - sig * tau1;
        }

        public static double Eatanhe(double x, double es)
        {
            return es > 0 ? es * Atanh(es * x) : -es * Math.Atan(es * x);
        }

        /**
        * The inverse hyperbolic sine function.
        *
        * @tparam T the type of the argument and the returned value.
        * @param[in] x
        * @return asinh(\e x).
        **********************************************************************/
        public static double Asinh(double x)
        {
            double y = Math.Abs(x); // Enforce odd parity
            y = Log1p(y * (1 + y / (Hypot(1, y) + 1)));
            return x < 0 ? -y : y;
        }

        public static double Tauf(double taup, double es)
        {
            const int numit = 5;
            double tol = Math.Sqrt(Epsilon) / 10;
            double e2m = 1 - Square(es),
              // To lowest order in e^2, taup = (1 - e^2) * tau = _e2m * tau; so use
              // tau = taup/_e2m as a starting guess.  (This starting guess is the
              // geocentric latitude which, to first order in the flattening, is equal
              // to the conformal latitude.)  Only 1 iteration is needed for |lat| <
              // 3.35 deg, otherwise 2 iterations are needed.  If, instead, tau = taup
              // is used the mean number of iterations increases to 1.99 (2 iterations
              // are needed except near tau = 0).
              tau = taup / e2m,
              stol = tol * Math.Max(1, Math.Abs(taup));
            // min iterations = 1, max iterations = 2; mean = 1.94
            for (int i = 0; i < numit; ++i)
            {
                double taupa = Taupf(tau, es),
                  dtau = (taup - taupa) * (1 + e2m * Square(tau)) /
                  (e2m * Hypot(1, tau) * Hypot(1, taupa));
                tau += dtau;
                if (!(Math.Abs(dtau) >= stol))
                    break;
            }
            return tau;
        }

        /**
        * Evaluate the sine function with the argument in degrees
        *
        * @tparam T the type of the argument and the returned value.
        * @param[in] x in degrees.
        * @return sin(<i>x</i>).
        **********************************************************************/
        public static double Sind(double x)
        {
            double r; int q;
            r = x % 360;
            q = (int)Math.Floor(r / 90 + 0.5);
            r -= 90 * q;
            // now abs(r) <= 45
            r *= Degree;
            int p = q;
            r = (p & 1) != 0 ? Math.Cos(r) : Math.Sin(r);
            if ((p & 2) != 0) r = -r;
            if (x != 0) r += 0;
            return r;
        }

        /// <summary>
        /// Cube root function.
        /// </summary>
        /// <returns>The cube root.</returns>
        /// <param name="x">The x value.</param>
        public static double CubeRoot(double x)
        {
            double y = Math.Pow(Math.Abs(x), 1 / 3.0); // Return the real cube root
            return x < 0 ? -y : y;
        }

        /// <summary>
        /// The error-free sum of two numbers.
        /// </summary>
        /// <returns>Tuple(s, t) with s = round(u + v) and t = u + v - s.</returns>
        /// <param name="u">U value in sum.</param>
        /// <param name="v">V value in sum.</param>
        public static double Sum(double u, double v, out double t)
        {
            double s = u + v;
            double up = s - v;
            double vpp = s - up;
            // See D. E. Knuth, TAOCP, Vol 2, 4.2.2, Theorem B.
            up -= u;
            vpp -= v;
            t = -(up + vpp);
            return s;
        }

        /// <summary>
        /// Normalize an angle (restricted input range).
        /// </summary>
        /// <returns>The normalized angle (-180 to 180).</returns>
        /// <param name="x">
        /// The x angle in degrees.
        /// Note: Must lie in -540 to 540 deg.
        /// </param>
        public static double AngNormalize(double x)
        {
            x = x % 360; return x != -180 ? x : 180;
        }

        /// <summary>
        /// Normalize an arbitrary angle.
        /// </summary>
        /// <returns>The normalized angle (-180 to 180).</returns>
        /// <param name="x">The x angle in degrees (unrestricted).</param>
        public static double AngNormalize2(double x)
        {
            return AngNormalize(x % 360.0);
        }

        /**
        * The exact difference of two angles reduced to
        * (&minus;180&deg;, 180&deg;].
        *
        * @tparam T the type of the arguments and returned value.
        * @param[in] x the first angle in degrees.
        * @param[in] y the second angle in degrees.
        * @param[out] e the error term in degrees.
        * @return \e d, the truncated value of \e y &minus; \e x.
        *
        * This computes \e z = \e y &minus; \e x exactly, reduced to
        * (&minus;180&deg;, 180&deg;]; and then sets \e z = \e d + \e e where \e d
        * is the nearest representable number to \e z and \e e is the truncation
        * error.  If \e d = &minus;180, then \e e &gt; 0; If \e d = 180, then \e e
        * &le; 0.
        **********************************************************************/
        public static double AngDiff(double x, double y, out double e)
        {
            double t, d = AngNormalize(Sum(-(x % 360), y % 360, out t)); //TODO: Check this negative MOD

            // Here y - x = d + t (mod 360), exactly, where d is in (-180,180] and
            // abs(t) <= eps (eps = 2^-45 for doubles).  The only case where the
            // addition of t takes the result outside the range (-180,180] is d = 180
            // and t > 0.  The case, d = -180 + eps, t = -eps, can't happen, since
            // sum would have returned the exact result in such a case (i.e., given t
            // = 0).
            return Sum(d == 180 && t > 0 ? -180 : d, t, out e);
        }

        /// <summary>
        /// Tests if value is finite.
        /// </summary>
        /// <returns>True if value is finite, false if NaN or infinite.</returns>
        /// <param name="x">The x coordinate.</param>
        public static bool IsFinite(double x)
        {
            return Math.Abs(x) <= Double.MaxValue;
        }

        /// <summary>
        /// Computes (x*y) + z.
        /// </summary>
        /// <returns>The result.</returns>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="z">The z value.</param>
        public static double Fma(double x, double y, double z)
        {
            return x * y + z;
        }

        /// <summary>
        /// Evaluate a polynomial (Horner's method)
        /// </summary>
        /// <returns>The value of the polynomial.</returns>
        /// <param name="N">N the order of the polynomial</param>
        /// <param name="p">p the coefficient array (of size N + 1)</param>
        /// <param name="s">s the index</param>
        /// <param name="x">x the variable</param>
        public static double PolyVal(int N, double[] p, int s, double x)
        {
            double y = N < 0 ? 0 : p[s++];
            while (--N >= 0) y = y * x + p[s++];
            return y;
        }

        /// <summary>
        /// Normalize a latitude.
        /// </summary>
        /// <returns>x if it is in the range -90 to 90, NaN otherwise.</returns>
        /// <param name="x">the angle in degrees</param>
        public static double LatFix(double x)
        {
            return Math.Abs(x) > 90 ? Double.NaN : x;
        }

        /**
     * Coarsen a value close to zero.
     *
     * @tparam T the type of the argument and returned value.
     * @param[in] x
     * @return the coarsened value.
     *
     * The makes the smallest gap in \e x = 1/16 - nextafter(1/16, 0) =
     * 1/2<sup>57</sup> for reals = 0.7 pm on the earth if \e x is an angle in
     * degrees.  (This is about 1000 times more resolution than we get with
     * angles around 90&deg;.)  We use this to avoid having to deal with near
     * singular cases when \e x is non-zero but tiny (e.g.,
     * 10<sup>&minus;200</sup>).  This converts -0 to +0; however tiny negative
     * numbers get converted to -0.
     **********************************************************************/
        public static double AngRound(double x)
        {
            double z = 1 / 16d;
            if (x == 0) return 0;
            double y = Math.Abs(x);
            // The compiler mustn't "simplify" z - (z - y) to y
            y = y < z ? z - (z - y) : y;
            return x < 0 ? -y : y;
        }

        /**
    * Evaluate the sine and cosine function with the argument in degrees
    *
    * @tparam T the type of the arguments.
    * @param[in] x in degrees.
    * @param[out] sinx sin(<i>x</i>).
    * @param[out] cosx cos(<i>x</i>).
    *
    * The results obey exactly the elementary properties of the trigonometric
    * functions, e.g., sin 9&deg; = cos 81&deg; = &minus; sin 123456789&deg;.
    * If x = &minus;0, then \e sinx = &minus;0; this is the only case where
    * &minus;0 is returned.
    **********************************************************************/
        public static void Sincosd(double x, out double sinx, out double cosx)
        {
            // In order to minimize round-off errors, this function exactly reduces
            // the argument to the range [-45, 45] before converting it to radians.
            double r; int q;
            r = x % 360.0;
            q = (int)Math.Floor(r / 90 + 0.5);
            r -= 90 * q;
            // now abs(r) <= 45
            r = ToRadians(r);
            // Possibly could call the gnu extension sincos
            double s = Math.Sin(r), c = Math.Cos(r);
            switch (q & 3)
            {
                case 0: sinx = s; cosx = c; break;
                case 1: sinx = c; cosx = -s; break;
                case 2: sinx = -s; cosx = -c; break;
                default: sinx = -c; cosx = s; break; // case 3
            }
            if (x != 0)
            {
                sinx += 0.0; cosx += 0.0;
            }
        }

        /**
     * Normalize a two-vector.
     *
     * @tparam T the type of the argument and the returned value.
     * @param[in,out] x on output set to <i>x</i>/hypot(<i>x</i>, <i>y</i>).
     * @param[in,out] y on output set to <i>y</i>/hypot(<i>x</i>, <i>y</i>).
     **********************************************************************/
        public static void Norm(ref double x, ref double y)
        {
            double h = Hypot(x, y);
            x /= h; y /= h;
        }

        public static double ToRadians(double x)
        {
            return Math.PI * x / 180.0;
        }

        public static double ToDegrees(double x)
        {
            return x * (180.0 / Math.PI);
        }

        public static double CopySign(double x, double y)
        {
            return Math.Abs(x) * (y < 0 || (y == 0 && 1 / y < 0) ? -1 : 1);
        }

        public static double Atan2d(double y, double x)
        {
            // In order to minimize round-off errors, this function rearranges the
            // arguments so that result of atan2 is in the range [-pi/4, pi/4] before
            // converting it to degrees and mapping the result to the correct
            // quadrant.
            int q = 0;
            if (Math.Abs(y) > Math.Abs(x)) { double t; t = x; x = y; y = t; q = 2; }
            if (x < 0) { x = -x; ++q; }
            // here x >= 0 and x >= abs(y), so angle is in [-pi/4, pi/4]
            double ang = GeoMath.ToDegrees(Math.Atan2(y, x));
            switch (q)
            {
                // Note that atan2d(-0.0, 1.0) will return -0.  However, we expect that
                // atan2d will not be called with y = -0.  If need be, include
                //
                //   case 0: ang = 0 + ang; break;
                //
                // and handle mpfr as in AngRound.
                case 1: ang = (y >= 0 ? 180 : -180) - ang; break;
                case 2: ang = 90 - ang; break;
                case 3: ang = -90 + ang; break;
            }
            return ang;
        }

        public static double Atand(double x)
        {
            return Atan2d(x, 1);
        }
    }
}
