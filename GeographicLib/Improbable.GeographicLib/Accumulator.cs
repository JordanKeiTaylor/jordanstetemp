/* 
 * Copyright (c) Charles Karney (2009-2017) <charles@karney.com> and licensed
 * under the MIT/X11 License.For more information, see
 * https://geographiclib.sourceforge.io/
 */

using System;

namespace Improbable.GeographicLib
{
    public class Accumulator //TODO: Define GetHashCode and Equals
    {
        // _s + _t accumulators for the sum.
        double _s, _t;

        public Accumulator(double y)
        {
            _s = y;
            _t = 0;
        }

        public Accumulator(Accumulator a)
        {
            _s = a._s;
            _t = a._t;
        }

        public void Add(double y)
        {
            // Here's Shewchuk's solution...
            double u;                       // hold exact sum as [s, t, u]
                                            // Accumulate starting at least significant end
            y = GeoMath.Sum(y, _t, out u);
            _s = GeoMath.Sum(y, _s, out _t);
            // Start is _s, _t decreasing and non-adjacent.  Sum is now (s + t + u)
            // exactly with s, t, u non-adjacent and in decreasing order (except for
            // possible zeros).  The following code tries to normalize the result.
            // Ideally, we want _s = round(s+t+u) and _u = round(s+t+u - _s).  The
            // following does an approximate job (and maintains the decreasing
            // non-adjacent property).  Here are two "failures" using 3-bit floats:
            //
            // Case 1: _s is not equal to round(s+t+u) -- off by 1 ulp
            // [12, -1] - 8 -> [4, 0, -1] -> [4, -1] = 3 should be [3, 0] = 3
            //
            // Case 2: _s+_t is not as close to s+t+u as it shold be
            // [64, 5] + 4 -> [64, 8, 1] -> [64,  8] = 72 (off by 1)
            //                    should be [80, -7] = 73 (exact)
            //
            // "Fixing" these problems is probably not worth the expense.  The
            // representation inevitably leads to small errors in the accumulated
            // values.  The additional errors illustrated here amount to 1 ulp of the
            // less significant word during each addition to the Accumulator and an
            // additional possible error of 1 ulp in the reported sum.
            //
            // Incidentally, the "ideal" representation described above is not
            // canonical, because _s = round(_s + _t) may not be true.  For example,
            // with 3-bit floats:
            //
            // [128, 16] + 1 -> [160, -16] -- 160 = round(145).
            // But [160, 0] - 16 -> [128, 16] -- 128 = round(144).
            //
            if (_s == 0)              // This implies t == 0,
                _s = u;                 // so result is u
            else
                _t += u;                // otherwise just accumulate u to t.
        }

        /**
         * Set the accumulator to a number.
         *
         * @param[in] y set \e sum = \e y.
         **********************************************************************/
		 public void Set(double y)
		 {
			 _s = y; _t = 0;
		 }
		 
        /**
         * Return the value held in the accumulator.
         *
         * @return \e sum.
         **********************************************************************/
		public double Result()
		{
			return _s;
		}
		
        /**
         * Return the result of adding a number to \e sum (but don't change \e
         * sum).
         *
         * @param[in] y the number to be added to the sum.
         * @return \e sum + \e y.
         **********************************************************************/
        public double Sum(double y)
        {
            var a = new Accumulator(this);
            a.Add(y);
            return a._s;
        }

        #region Operator Overloads

        public static Accumulator operator +(Accumulator accumulator, double y)
        {
            accumulator.Add(y);
            return accumulator;
        }

        public static Accumulator operator -(Accumulator accumulator, double y)
        {
            accumulator.Add(-y);
            return accumulator;
        }

        public static Accumulator operator *(Accumulator accumulator, double y)
        {
            double d = accumulator._s;
            accumulator._s *= y;
            d = GeoMath.Fma(y, d, -accumulator._s);              // error in firt multiplication
            accumulator._t = GeoMath.Fma(y, accumulator._t, d);  // error in second term
            return accumulator;
        }

        public static bool operator ==(Accumulator accumulator, double y)
        {
            return Math.Abs(accumulator._s - y) < GeoMath.Degree;
        }

        public static bool operator !=(Accumulator accumulator, double y)
        {
            return Math.Abs(accumulator._s - y) > GeoMath.Degree;
        }

        public static bool operator <(Accumulator accumulator, double y)
        {
            return accumulator._s < y;
        }

        public static bool operator <=(Accumulator accumulator, double y)
        {
            return accumulator._s <= y;
        }

        public static bool operator >(Accumulator accumulator, double y)
        {
            return accumulator._s > y;
        }

        public static bool operator >=(Accumulator accumulator, double y)
        {
            return accumulator._s >= y;
        }

        #endregion

        // Same as Math::sum, but requires abs(u) >= abs(v).  This isn't currently
        // used.
        // static double fastsum(double u, double v, T& t) {
        //   GEOGRAPHICLIB_VOLATILE double s = u + v;
        //   GEOGRAPHICLIB_VOLATILE double vp = s - u;
        //   t = v - vp;
        //   return s;
        // }
    }
}
