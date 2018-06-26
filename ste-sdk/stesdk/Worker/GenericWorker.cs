using Improbable.Environment;
using Improbable.Log;

namespace Improbable.Worker
{
    public class GenericWorker
    {
        private const string LoggerName = "GenericWorker.cs";
        private readonly Logger.NamedLogger _logger = Log.Logger.DefaultWithName(LoggerName);
        
        private readonly string _workerId;
        private readonly string _workerType;

        protected GenericWorker(string workerType, string workerId, string hostname, ushort port)
        {
            _workerId = workerId;
            _workerType = workerType;
            
            DeploymentContext.GetInstance().Init(workerType, workerId, hostname, port);
            
            _logger.Info("Initialized Deployment Context");
        }

        /// <summary>
        /// Test Constructor
        /// </summary>
        /// <param name="connection">Mocked IConnection</param>
        /// <param name="dispatcher">Mocked IDispatcher</param>
        protected GenericWorker(IConnection connection, IDispatcher dispatcher)
        {
            DeploymentContext.GetInstance().TestInit(connection, dispatcher);
            
            _logger.Info("Initialized Test Deployment Context");
        }
        
        protected string GetWorkerId()
        {
            return _workerId;
        }
        
        protected string GetWorkerType()
        {
            return _workerType;
        }
    }
}