using System;
using Improbable.Context.Exception;
using Improbable.Log;
using Improbable.Worker;

namespace Improbable.Context
{
    public class WorkerContext : IDisposable
    {   
        private const string LoggerName = "DeploymentContext.cs";
        private readonly NamedLogger _logger = Logger.DefaultWithName(LoggerName);

        private static WorkerContext _context;
        private static Status _status;
        
        private string _workerType;
        private IDispatcher _wrappedDispatcher;
        private IConnection _wrappedConnection;
        
        public bool IsDispatcherInCritical { get; private set; }

        private WorkerContext()
        {
            _status = Status.Uninitialized;
        }
        
        /// <summary>
        /// Returns the initialized DeploymentContext.
        /// </summary>
        /// <returns>DeploymentContext</returns>
        public static WorkerContext GetInstance()
        {
            return _context ?? (_context = new WorkerContext());
        }

        public Status GetStatus()
        {
            return _status;
        }

        /// <summary>
        /// Initializes the DeploymentContext to connect with SpatialOS given a hostname and port.
        /// </summary>
        /// <param name="workerType">Type of worker</param>
        /// <param name="workerId">ID of worker</param>
        /// <param name="hostname">Hostname to connect</param>
        /// <param name="port">Port to connect</param>
        public void Init(string workerType, string workerId, string hostname, ushort port)
        {
            if (_status != Status.Uninitialized)
            {
                _logger.Warn("Attempt to reinitialize DeploymentContext has been cancelled.");
                return;
            }
            
            _workerType = workerType;
            var connection = CreateConnection(hostname, port, workerId);
            var dispatcher = CreateDispatcher(connection);

            if (connection == null || !connection.IsConnected)
            {
                throw new ContextInitializationFailedException("Failed to connect to SpatialOS");
            }

            _wrappedConnection = new ConnectionWrapper(connection);
            _wrappedDispatcher = new DispatcherWrapper(dispatcher);

            Logger.DefaultLogger.AttachConnection(_wrappedConnection);

            _status = Status.Initialized;
        }

        /// <summary>
        /// Initializes the DeploymentContext given an IConnection and IDispatcher.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="dispatcher"></param>
        public void Init(IConnection connection, IDispatcher dispatcher)
        {
            _wrappedConnection = connection;
            _wrappedDispatcher = dispatcher;
            
            _status = Status.Initialized;
        }
        
        /// <summary>
        /// Is this environment connected to a SpatialOS instance?
        /// </summary>
        public bool IsConnected => _wrappedConnection.IsConnected;
        
        /// <summary>
        /// Returns an IConnection wrapper of the instantiated Worker SDK Connection.
        /// </summary>
        /// <returns>IConnection</returns>
        public IConnection GetConnection()
        {
            if (_status == Status.Uninitialized)
            {
                throw new ContextUninitializedException("The context has not been initialized.");
            }
            return _wrappedConnection;
        }

        /// <summary>
        /// Returns an IDispatcher wrapper of the instantiated Worker SDK Dispatcher.
        /// </summary>
        /// <returns>IDispatcher</returns>
        public IDispatcher GetDispatcher()
        {
            if (_status == Status.Uninitialized)
            {
                throw new ContextUninitializedException("The context has not been initialized.");
            }
            return _wrappedDispatcher;
        }
        
        /// <summary>
        /// Disposes of connection and dispatcher.
        /// </summary>
        public void Dispose()
        {
            _wrappedDispatcher?.Dispose();
            _wrappedConnection?.Dispose();
            _logger.Warn("Disposing of Connection and Dispatcher");
            _status = Status.Uninitialized;
        }

        /// <summary>
        /// Type of the worker controlling the context to SpatialOS.
        /// </summary>
        /// <returns>Worker Type</returns>
        public string GetWorkerType()
        {
            return _workerType;
        }
        
        private Connection CreateConnection(string hostname, ushort port, string workerId)
        {
            Connection conn;

            var connectionParameters = new ConnectionParameters
            {
                WorkerType = _workerType,
                Network =
                {
                    ConnectionType = NetworkConnectionType.Tcp,
                    Tcp = new TcpNetworkParameters()
                    {
                        MultiplexLevel = 1
                    }
                }
            };

            using (var future = Connection.ConnectAsync(hostname, port, workerId, connectionParameters))
            {
                conn = future.Get();
            }

            _logger.Info("Successfully connected using the Receptionist");

            return conn;
        }

        private Dispatcher CreateDispatcher(Connection conn)
        {
            var dispatch = new Dispatcher();

            dispatch.OnDisconnect(op =>
            {
                Console.Error.WriteLine("[disconnect] " + op.Reason);
            });

            dispatch.OnLogMessage(op =>
            {
                conn.SendLogMessage(op.Level, LoggerName, op.Message);
                if (op.Level == LogLevel.Fatal)
                {
                    Console.Error.WriteLine("Fatal error: " + op.Message);
                    Dispose();
                }
            });

            dispatch.OnCriticalSection(section =>
            {
                IsDispatcherInCritical = section.InCriticalSection;
            });

            return dispatch;
        }
    }
}