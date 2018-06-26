using Improbable.sandbox.Extensions;
using NUnit.Framework;

namespace Tests.Extensions
{
    [TestFixture]
    public class ArrayExtensionTest
    {
        [Test]
        public void Should_BinarySearch_SpecifiedValue()
        {
            var array = new [] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0 };
            var index = array.BinarySearch(2.0);

            Assert.AreEqual(2, index);
        }

        [Test]
        public void Should_BinarySearch_LowerBound()
        {
            var array = new [] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0 };
            var index = array.BinarySearch(0.0);

            Assert.AreEqual(0, index);
        }

        [Test]
        public void Should_BinarySearch_UpperBound()
        {
            var array = new [] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0 };
            var index = array.BinarySearch(5.0);

            Assert.AreEqual(5.0, index);
        }

        [Test]
        public void Should_BinarySearch_NearValueLowEnd()
        {
            var offset = 0.1;
            var array = new[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0 };

            for (int i = 0; i < array.Length - 1; i++)
            {
                var v = array[i] + offset;
                var index = array.BinarySearch(v);
                Assert.AreEqual(i, index);
            }
        }

        [Test]
        public void Should_BinarySearch_NearValueHighEnd()
        {
            var offset = 0.9;
            var array = new[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0 };

            for (int i = 0; i < array.Length - 1; i++)
            {
                var v = array[i] + offset;
                var index = array.BinarySearch(v);
                Assert.AreEqual(i + 1, index);
            }
        }

        [Test]
        public void Should_NotBinarySearch_ValuesOutsideArray()
        {
            var array = new [] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0 };
            var lower = array.BinarySearch(-1.1);
            var upper = array.BinarySearch(5.1);

            Assert.AreEqual(-1, lower);
            Assert.AreEqual(-1, upper);
        }
    }
}
