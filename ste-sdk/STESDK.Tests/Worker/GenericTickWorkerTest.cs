using System;
using System.Collections.Generic;
using System.Threading;
using Improbable;
using Improbable.Behaviour;
using Improbable.Context;
using Improbable.Worker;
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
            public GenericTickWorkerImpl(int tickTimeMs)
                : base(tickTimeMs) { }
            
            public new void Run()
            {
                base.Run();
            }

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
            mockConnection.Setup(_ => _.IsConnected).Returns(true);
            mockConnection.Setup(_ => _.GetOpList(It.IsAny<uint>()));
            
            var mockDispatcher = new Mock<IDispatcher>();
            mockDispatcher.Setup(_ => _.Process(It.IsAny<OpList>()));
            
            WorkerContext.GetInstance().Init(mockConnection.Object, mockDispatcher.Object);
        }

        [Test]
        public void TestRun()
        {
            var workerThread = new Thread(new GenericTickWorkerImpl(1000).Run);
            workerThread.Start();

            if (!workerThread.Join(TimeSpan.FromSeconds(3)))
            {
                workerThread.Abort();
            }
            
            Assert.Greater(_tickCount, 0);
        }
    }
}