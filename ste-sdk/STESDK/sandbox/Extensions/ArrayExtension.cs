using System;

namespace stesdk.sandbox.Extensions
{
    public static class ArrayExtension
    {
        /// <summary>
        /// Performs a binary search on specified array which is assumed to be sorted.
        ///
        /// Note: If the specified value is not contained in array set, this search implementation
        /// will find a value in the array closest to it. For example, given array [1.0, 2.0, 3.0]
        /// and search value 1.9, an index value of 1 will be returned because 1.9 is closer to 2.0
        /// than 1.0. For values outside of array set, a -1 is returned.
        /// </summary>
        /// <returns>The index of the matched value in array on success, -1 otherwise.</returns>
        /// <param name="array">Array to search.</param>
        /// <param name="value">Value to find.</param>
        /// <param name="tolerance">Tolerance in double comparisons.</param>
        public static int BinarySearch(
            this double[] array,
            double value,
            double tolerance = 0.0001)
        {
            if (array.Length == 0 ||
                value.Less(array[0], tolerance) ||
                value.Greater(array[array.Length - 1], tolerance))
            {
                return -1;
            }

            var lower = 0;
            var upper = array.Length;

            while (upper >= lower)
            {
                int mid = (upper + lower) / 2;
                var element = array[mid];
                if (value.Equal(element, tolerance))
                {
                    return mid;
                }

                if (value.Greater(element, tolerance))
                {
                    lower = mid + 1;
                }
                else if (value.Less(element, tolerance))
                {
                    upper = mid - 1;
                }
            }

            var lowDiff = Math.Abs(value - array[lower]);
            var higDiff = Math.Abs(value - array[upper]);

            if (higDiff.LessOrEqual(lowDiff, tolerance))
            {
                return upper;
            }

            return lower;
        }
    }
}
