using Improbable.Context;
using Improbable.Log;

namespace Improbable
{
    /// <summary>
    /// Basic Worker that attempts to initialize a connection to SpatialOS.
    /// </summary>
    public class GenericWorker
    {
        private const string LoggerName = "GenericWorker.cs";
        private readonly NamedLogger _logger = Logger.DefaultWithName(LoggerName);
        
        private readonly string _workerId;
        private readonly string _workerType;

        /// <summary>
        /// Constructs a GenericWorker. Initializes a <see cref="WorkerContext"/> given the parameters.
        /// </summary>
        /// <param name="workerType">Type of worker</param>
        /// <param name="workerId">Unique ID of worker</param>
        /// <param name="hostname">SpatialOS deployment hostname</param>
        /// <param name="port">SpatialOS deployment port</param>
        protected GenericWorker(string workerType, string workerId, string hostname, ushort port)
        {
            _workerId = workerId;
            _workerType = workerType;
            
            WorkerContext.GetInstance().Init(workerType, workerId, hostname, port);
            
            _logger.Info("Initialized Deployment Context");
        }

        /// <summary>
        /// Test Constructor
        /// </summary>
        protected GenericWorker(IConnection connection, IDispatcher dispatcher)
        {
            WorkerContext.GetInstance().Init(connection, dispatcher);
        }
        
        protected string GetWorkerId()
        {
            return _workerId;
        }
        
        protected string GetWorkerType()
        {
            return _workerType;
        }
        
        protected WorkerContext GetContext()
        {
            return WorkerContext.GetInstance();
        }
    }
}