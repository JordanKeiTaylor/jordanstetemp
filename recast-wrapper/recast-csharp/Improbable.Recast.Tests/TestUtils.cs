using System.IO;
using NUnit.Framework;

namespace Improbable.Recast.Tests
{
    public static class TestUtils
    {
        public static string ResolveResource(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, path);
        }
    }
}