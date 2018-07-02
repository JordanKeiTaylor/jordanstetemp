using System.Collections.Generic;
using Improbable;
using Improbable.Behaviour;
using Improbable.Context;
using Moq;
using NUnit.Framework;

namespace Tests.Worker
{
    public class GenericTickWorkerTest
    {
        private static int _tickCount;
        
        private class TickBehaviourImpl : ITickBehaviour
        {
            public void Tick()
            {
                _tickCount++;
            }
        }
        private class GenericTickWorkerImpl : GenericTickWorker
        {
            protected override Dictionary<string, ITickBehaviour> GetBehaviours()
            {
                return new Dictionary<string, ITickBehaviour>
                {
                    {"i_tick_behaviour_impl", new TickBehaviourImpl()}
                };
            }
        }
        
        [SetUp]
        public void Setup()
        {
            _tickCount = 0;
            var mockConnection = new Mock<IConnection>();
            mockConnection.
            var mockDispatcher = new Mock<IDispatcher>();
            WorkerContext.GetInstance().Init(mockConnection.Object, mockDispatcher.Object);
        }

        [Test]
        public void TestRun()
        {
            var worker = new GenericTickWorkerImpl();
            
            worker.Run();
        }
    }
}