using Improbable.Context;
using Improbable.Context.Exception;
using Moq;
using NUnit.Framework;

namespace Tests.Context
{
    public class DeploymentContextTest
    {
        private readonly DeploymentContext _context = DeploymentContext.GetInstance();

        [SetUp]
        public void Setup()
        {
            if (_context.GetStatus() != Status.Uninitialized)
            {
                _context.Exit();
            }
            
            Assert.AreEqual(Status.Uninitialized, _context.GetStatus());
        }
        
        [Test]
        public void TestInitTest()
        {
            var mockConnection = new Mock<IConnection>();
            var mockDispatcher = new Mock<IDispatcher>();
            
            _context.TestInit(mockConnection.Object, mockDispatcher.Object);
            
            Assert.AreEqual(Status.TestInitialized, _context.GetStatus());
            Assert.AreEqual(mockConnection.Object, _context.GetConnection());
            Assert.AreEqual(mockDispatcher.Object, _context.GetDispatcher());
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