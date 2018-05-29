using NUnit.Framework;

namespace Recast.Tests
{
    using Recast;

    class RecastShould
    {
        [Test]
        public void create_a_context()
        {
            var ctx = RecastLibrary.rcContext_create();
            Assert.IsNotNull(ctx);
        }
    }
}