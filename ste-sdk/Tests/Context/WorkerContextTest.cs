using Improbable.Context;
using Improbable.Context.Exception;
using Moq;
using NUnit.Framework;

namespace Tests.Context
{
    public class WorkerContextTest
    {
        private readonly WorkerContext _context = WorkerContext.GetInstance();

        [SetUp]
        public void Setup()
        {
            if (_context.GetStatus() != Status.Uninitialized)
            {
                _context.Dispose();
            }
            
            Assert.AreEqual(Status.Uninitialized, _context.GetStatus());
        }

        [Test]
        public void UninitializedConnectionFailureTest()
        {
            Assert.AreEqual(Status.Uninitialized, _context.GetStatus());

            Assert.Throws<ContextUninitializedException>(() => _context.GetConnection());
        }

        [Test]
        public void UninitializedDispatcherFailureTest()
        {
            Assert.AreEqual(Status.Uninitialized, _context.GetStatus());
            
            Assert.Throws<ContextUninitializedException>(() => _context.GetDispatcher());
        }
    }
}