using System;
namespace GeographicLib
{
    internal static class GeodesicCoeff
    {
        /// <summary>
        /// The default precision of floating point numbers is a double. 
        /// This equates to an order of 6 for the coefficients generated 
        /// in this class (include/GeographicLib/Math.hpp).
        /// </summary>
        public static readonly int Order = 6;

        public static readonly int nA1_ = Order;
        public static readonly int nC1_ = Order;
        public static readonly int nC1p_ = Order;
        public static readonly int nA2_ = Order;
        public static readonly int nC2_ = Order;
        public static readonly int nA3_ = Order;
        public static readonly int nA3x_ = nA3_;
        public static readonly int nC3_ = Order;
        public static readonly int nC3x_ = (nC3_ * (nC3_ - 1)) / 2;
        public static readonly int nC4_ = Order;
        public static readonly int nC4x_ = (nC4_ * (nC4_ + 1)) / 2;

        //TODO: Can make this more efficient by cloning the array once it has been created

        public static double[] GenerateA3(double n)
        {
            double[] coeff = {
                -3, 128,         // coeff of eps^5, polynomial in n of order 0
    			-2,  -3, 64,     // coeff of eps^4, polynomial in n of order 1
    			-1,  -3, -1, 16, // coeff of eps^3, polynomial in n of order 2
    			 3,  -1, -2,  8, // coeff of eps^2, polynomial in n of order 2
    			 1,  -1,  2,     // coeff of eps^1, polynomial in n of order 1
    			 1,  1,          // coeff of eps^0, polynomial in n of order 0
			};

            var A3x = new double[nA3x_];

            int o = 0, k = 0;
            for (int j = nA3_ - 1; j >= 0; --j) // coeff of eps^j
            {
                int m = Math.Min(nA3_ - j - 1, j);  // order of polynomial in n
                A3x[k++] = GeoMath.PolyVal(m, coeff, o, n) / coeff[o + m + 1];
                o += m + 2;
            }
            return A3x;
        }

        public static double[] GenerateC3(double n)
        {
            double[] coeff = {
                  3,  128,           // C3[1], coeff of eps^5, polynomial in n of order 0
				  2,    5, 128,      // C3[1], coeff of eps^4, polynomial in n of order 1
				 -1,    3,   3,  64, // C3[1], coeff of eps^3, polynomial in n of order 2
				 -1,    0,   1,   8, // C3[1], coeff of eps^2, polynomial in n of order 2
				 -1,    1,   4,      // C3[1], coeff of eps^1, polynomial in n of order 1
				  5,  256,           // C3[2], coeff of eps^5, polynomial in n of order 0
				  1,    3, 128,      // C3[2], coeff of eps^4, polynomial in n of order 1
				 -3,   -2,   3,  64, // C3[2], coeff of eps^3, polynomial in n of order 2
				  1,   -3,   2,  32, // C3[2], coeff of eps^2, polynomial in n of order 2
				  7,  512,           // C3[3], coeff of eps^5, polynomial in n of order 0
				-10,    9, 384,      // C3[3], coeff of eps^4, polynomial in n of order 1
				  5,   -9,   5, 192, // C3[3], coeff of eps^3, polynomial in n of order 2
				  7,  512,           // C3[4], coeff of eps^5, polynomial in n of order 0
				-14,    7, 512,      // C3[4], coeff of eps^4, polynomial in n of order 1
				 21, 2560            // C3[5], coeff of eps^5, polynomial in n of order 0
            };

            var C3x = new double[nC3x_];

            int o = 0, k = 0;
            for (int l = 1; l < nC3_; ++l) // l is index of C3[l]
            {
                for (int j = nC3_ - 1; j >= l; --j) // coeff of eps^j
                {
                    int m = Math.Min(nC3_ - j - 1, j);  // order of polynomial in n
                    C3x[k++] = GeoMath.PolyVal(m, coeff, o, n) / coeff[o + m + 1];
                    o += m + 2;
                }
            }
            return C3x;
        }

        public static double[] GenerateC4(double n)
        {
            double[] coeff = {
                    97,  15015,                                         // C4[0], coeff of eps^5, polynomial in n of order 0
				  1088,    156,  45045,                                 // C4[0], coeff of eps^4, polynomial in n of order 1
				  -224,  -4784,   1573,  45045,                         // C4[0], coeff of eps^3, polynomial in n of order 2
				-10656,  14144,  -4576,   -858,  45045,                 // C4[0], coeff of eps^2, polynomial in n of order 3
				    64,    624,  -4576,   6864,  -3003,  15015,         // C4[0], coeff of eps^1, polynomial in n of order 4
				   100,    208,    572,   3432, -12012,  30030, 45045,  // C4[0], coeff of eps^0, polynomial in n of order 5
				     1,   9009,                                         // C4[1], coeff of eps^5, polynomial in n of order 0
		         -2944,    468, 135135,                                 // C4[1], coeff of eps^4, polynomial in n of order 1
				  5792,   1040,  -1287, 135135,                         // C4[1], coeff of eps^3, polynomial in n of order 2
				  5952, -11648,   9152,  -2574, 135135,                 // C4[1], coeff of eps^2, polynomial in n of order 3
				   -64,   -624,   4576,  -6864,   3003, 135135,         // C4[1], coeff of eps^1, polynomial in n of order 4
				     8,  10725,                                         // C4[2], coeff of eps^5, polynomial in n of order 0
				  1856,   -936, 225225,                                 // C4[2], coeff of eps^4, polynomial in n of order 1
				 -8448,   4992,  -1144, 225225,                         // C4[2], coeff of eps^3, polynomial in n of order 2
				 -1440,   4160,  -4576,   1716, 225225,                 // C4[2], coeff of eps^2, polynomial in n of order 3
				  -136,  63063,                                         // C4[3], coeff of eps^5, polynomial in n of order 0
				  1024,   -208, 105105,                                 // C4[3], coeff of eps^4, polynomial in n of order 1
				  3584,  -3328,   1144, 315315,                         // C4[3], coeff of eps^3, polynomial in n of order 2
				  -128, 135135,                                         // C4[4], coeff of eps^5, polynomial in n of order 0
				 -2560,    832, 405405,                                 // C4[4], coeff of eps^4, polynomial in n of order 1
				   128,  99099                                          // C4[5], coeff of eps^5, polynomial in n of order 0
            };

            var C4x = new double[nC4x_];

            int o = 0, k = 0;
            for (int l = 0; l < nC4_; ++l) // l is index of C4[l]
            {
                for (int j = nC4_ - 1; j >= l; --j) // coeff of eps^j
                {
                    int m = nC4_ - j - 1;               // order of polynomial in n
                    C4x[k++] = GeoMath.PolyVal(m, coeff, o, n) / coeff[o + m + 1];
                    o += m + 2;
                }
            }
            return C4x;
        }

        // The static const coefficient arrays in the following functions are
        // generated by Maxima and give the coefficients of the Taylor expansions for
        // the geodesics.  The convention on the order of these coefficients is as
        // follows:
        //
        //   ascending order in the trigonometric expansion,
        //   then powers of eps in descending order,
        //   finally powers of n in descending order.
        //
        // (For some expansions, only a subset of levels occur.)  For each polynomial
        // of order n at the lowest level, the (n+1) coefficients of the polynomial
        // are followed by a divisor which is applied to the whole polynomial.  In
        // this way, the coefficients are expressible with no round off error.  The
        // sizes of the coefficient arrays are:
        //
        //   A1m1f, A2m1f            = floor(N/2) + 2
        //   C1f, C1pf, C2f, A3coeff = (N^2 + 7*N - 2*floor(N/2)) / 4
        //   C3coeff       = (N - 1) * (N^2 + 7*N - 2*floor(N/2)) / 8
        //   C4coeff       = N * (N + 1) * (N + 5) / 6
        //
        // where N = GEOGRAPHICLIB_GEODESIC_ORDER
        //         = nA1 = nA2 = nC1 = nC1p = nA3 = nC4

        // The scale factor A1-1 = mean value of (d/dsigma)I1 - 1
        public static double A1m1f(double eps)
        {
            double[] coeff = {
              // (1-eps)*A1-1, polynomial in eps2 of order 3
              1, 4, 64, 0, 256,
            };
            int m = nA1_ / 2;
            double t = GeoMath.PolyVal(m, coeff, 0, GeoMath.Square(eps)) / coeff[m + 1];
            return (t + eps) / (1 - eps);
        }

        // The coefficients C1[l] in the Fourier expansion of B1
        public static void C1f(double eps, double[] c)
        {

            double[] coeff = {
				// C1[1]/eps^1, polynomial in eps2 of order 2
				-1, 6, -16, 32,
				// C1[2]/eps^2, polynomial in eps2 of order 2
				-9, 64, -128, 2048,
				// C1[3]/eps^3, polynomial in eps2 of order 1
				9, -16, 768,
				// C1[4]/eps^4, polynomial in eps2 of order 1
				3, -5, 512,
				// C1[5]/eps^5, polynomial in eps2 of order 0
				-7, 1280,
				// C1[6]/eps^6, polynomial in eps2 of order 0
				-7, 2048,
            };

            double eps2 = GeoMath.Square(eps);
            double d = eps;
            int o = 0;
            for (int l = 1; l <= nC1_; ++l)
            { // l is index of C1p[l]
                int m = (nC1_ - l) / 2;         // order of polynomial in eps^2
                c[l] = d * GeoMath.PolyVal(m, coeff, o, eps2) / coeff[o + m + 1];
                o += m + 2;
                d *= eps;
            }
        }

        // The coefficients C1p[l] in the Fourier expansion of B1p
        public static void C1pf(double eps, double[] c)
        {
            double[] coeff = {
				// C1p[1]/eps^1, polynomial in eps2 of order 2
				205, -432, 768, 1536,
				// C1p[2]/eps^2, polynomial in eps2 of order 2
				4005, -4736, 3840, 12288,
				// C1p[3]/eps^3, polynomial in eps2 of order 1
				-225, 116, 384,
				// C1p[4]/eps^4, polynomial in eps2 of order 1
				-7173, 2695, 7680,
				// C1p[5]/eps^5, polynomial in eps2 of order 0
				3467, 7680,
				// C1p[6]/eps^6, polynomial in eps2 of order 0
				38081, 61440,
            };

            double eps2 = GeoMath.Square(eps);
            double d = eps; ;
            int o = 0;
            for (int l = 1; l <= nC1p_; ++l)
            { // l is index of C1p[l]
                int m = (nC1p_ - l) / 2;         // order of polynomial in eps^2
                c[l] = d * GeoMath.PolyVal(m, coeff, o, eps2) / coeff[o + m + 1];
                o += m + 2;
                d *= eps;
            }
        }

        // The scale factor A2-1 = mean value of (d/dsigma)I2 - 1
        public static double A2m1f(double eps)
        {
            double[] coeff = {
				// (eps+1)*A2-1, polynomial in eps2 of order 3
				-11, -28, -192, 0, 256,
			};  // count = 5

            int m = nA2_ / 2;
            double t = GeoMath.PolyVal(m, coeff, 0, GeoMath.Square(eps)) / coeff[m + 1];
            return (t - eps) / (1 + eps);
        }

        // The coefficients C2[l] in the Fourier expansion of B2
        public static void C2f(double eps, double[] c)
        {
            double[] coeff = {
				// C2[1]/eps^1, polynomial in eps2 of order 2
				1, 2, 16, 32,
				// C2[2]/eps^2, polynomial in eps2 of order 2
				35, 64, 384, 2048,
				// C2[3]/eps^3, polynomial in eps2 of order 1
				15, 80, 768,
				// C2[4]/eps^4, polynomial in eps2 of order 1
				7, 35, 512,
				// C2[5]/eps^5, polynomial in eps2 of order 0
				63, 1280,
				// C2[6]/eps^6, polynomial in eps2 of order 0
				77, 2048,
            };
            double eps2 = GeoMath.Square(eps),
              d = eps;
            int o = 0;
            for (int l = 1; l <= nC2_; ++l)
            { // l is index of C2[l]
                int m = (nC2_ - l) / 2;         // order of polynomial in eps^2
                c[l] = d * GeoMath.PolyVal(m, coeff, o, eps2) / coeff[o + m + 1];
                o += m + 2;
                d *= eps;
            }
        }
    }
}
