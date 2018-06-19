using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Improbable.Shared.Behaviour;
using Improbable.Shared.Environment;
using Improbable.Worker;

namespace Improbable.Shared.Worker
{
    public abstract class GenericTickWorker<E> where E : Environment.Environment
    {
        protected string LoggerName = "GenericTickWorker.cs";
        protected Logger.NamedLogger Logger;
        
        protected const int ErrorExitStatus = 1;
        
        protected readonly string WorkerType;
        protected readonly Stopwatch FetchAndProcessOpsTimer = new Stopwatch();
        protected readonly Stopwatch BehaviourTimer = new Stopwatch();
        
        protected string WorkerId = "";

        protected GenericTickWorker(string workerType)
        {
            Logger = Shared.Logger.DefaultWithName(LoggerName);
            WorkerType = workerType;
        }

        public abstract int Run(string hostname, ushort port, string workerId);

        protected abstract E GetEnvironment();
        
        protected abstract Dictionary<string, ITickBehaviour> GetBehaviours();
        
        protected void RunEventLoop()
        {
            var behaviours = GetBehaviours();

            // run loop
            var frameTimer = new Stopwatch();
            while (GetEnvironment().IsDispatcherConnected)
            {
                frameTimer.Restart();

                // process messages
                FetchAndProcessOps(
                    GetEnvironment().GetConnection(), 
                    GetEnvironment().GetDispatcher().GetBaseDispatcher(), 
                    0);

                // process behaviours
                double offsetMs = frameTimer.ElapsedMilliseconds;
                foreach (var behaviour in behaviours)
                {
                    try
                    {
                        BehaviourTimer.Restart();
                        behaviour.Value.Tick();
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Caught exception during Tick() for behaviour [" + behaviour.Key + "]", e);
                    }
                }

                GetEnvironment().GetTickTimeRollingMetric().Record(frameTimer.ElapsedMilliseconds);

                // wait for the next frame to ensure frame rate isn't too fast
                double waitTimeMs = GetEnvironment().TimeStepMs - frameTimer.ElapsedMilliseconds;
                if (waitTimeMs > 10.0)
                {
                    Thread.Sleep((int)Math.Floor(waitTimeMs));
                }
                else if (waitTimeMs < -10.0)
                {
                    Logger.Warn("Worker fell behind by " + waitTimeMs + "ms.");
                }
            }
        }

        protected void FetchAndProcessOps(IConnection conn, Dispatcher dispatch, double waitTimeMs)
        {
            FetchAndProcessOpsTimer.Restart();

            dispatch.Process(conn.GetOpList((uint)waitTimeMs));
            while (GetEnvironment().IsDispatcherInCritical)
            {
                dispatch.Process(conn.GetOpList(0));
            }
        }
    }
}
