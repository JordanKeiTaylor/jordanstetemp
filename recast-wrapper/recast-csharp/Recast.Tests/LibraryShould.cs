using NUnit.Framework;

namespace Recast.Tests
{
    public class LibraryShould
    {
        [Test]
        public void use_64bit_polyref()
        {
            Assert.IsTrue(RecastContext.IsUsing64BitPolyRefs());
        }

        [Test]
        public void match_struct_size()
        {
            Assert.AreEqual(32, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PolyPointResult)));
        }
    }
}