using Improbable.Log;

namespace Improbable.Worker
{
    public class GenericWorker
    {
        private const string LoggerName = "GenericWorker.cs";
        private readonly Logger.NamedLogger _logger = Log.Logger.DefaultWithName(LoggerName);
        
        private readonly string _workerId;
        private readonly string _workerType;
        
        public GenericWorker(string workerType, string workerId, string hostname, ushort port)
        {
            _workerId = workerId;
            _workerType = workerType;
            
            DeploymentContext.GetInstance().Init(workerType, workerId, hostname, port);
            
            _logger.Info("Initialized Deployment Context");
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