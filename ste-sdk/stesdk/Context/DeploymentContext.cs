using System;
using Improbable.Context.Exception;
using Improbable.Log;
using Improbable.Worker;

namespace Improbable.Context
{
    public class DeploymentContext
    {   
        private const string LoggerName = "DeploymentContext.cs";
        private readonly NamedLogger _logger = Logger.DefaultWithName(LoggerName);

        private static DeploymentContext _context;
        private static Status _status;
        
        private string _workerType;
        private Connection _connection;
        private Dispatcher _dispatcher;
        private IDispatcher _wrappedDispatcher;
        private IConnection _wrappedConnection;
        
        public bool IsDispatcherConnected { get; set; }
        public bool IsDispatcherInCritical { get; set; }

        private DeploymentContext()
        {
            _status = Status.Uninitialized;
        }
        
        /// <summary>
        /// Returns the initialized DeploymentContext.
        /// </summary>
        /// <returns>DeploymentContext</returns>
        public static DeploymentContext GetInstance()
        {
            return _context ?? (_context = new DeploymentContext());
        }

        /// <summary>
        /// Initializes the DeploymentContext to connect with SpatialOS.
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
            _connection = CreateConnection(hostname, port, workerId);
            _dispatcher = CreateDispatcher(_connection);

            if (_connection == null || !_connection.IsConnected)
            {
                throw new ContextInitializationFailedException("Failed to connect to SpatialOS");
            }

            _wrappedConnection = new ConnectionWrapper(_connection);
            _wrappedDispatcher = new DispatcherWrapper(_dispatcher);

            Logger.DefaultLogger.AttachConnection(_wrappedConnection);

            IsDispatcherConnected = _connection.IsConnected;

            _status = Status.Initialized;
        }

        /// <summary>
        /// Initializes the DeploymentContext in an unchecked state.
        /// This is only intended to be used for testing.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="dispatcher"></param>
        public void TestInit(IConnection connection, IDispatcher dispatcher)
        {
            if (_status != Status.Uninitialized)
            {
                _logger.Warn("Attempt to reinitialize DeploymentContext has been cancelled.");
                return;
            }

            _wrappedConnection = connection;
            _wrappedDispatcher = dispatcher;
        }
        
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
        /// Exit execution with a specified status. Disposes of connection and dispatcher.
        /// </summary>
        public void Exit()
        {
            _connection.Dispose();
            _dispatcher.Dispose();
            _wrappedConnection.Dispose();
            _wrappedDispatcher.Dispose();
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
                IsDispatcherConnected = false;
            });

            dispatch.OnLogMessage(op =>
            {
                conn.SendLogMessage(op.Level, LoggerName, op.Message);
                if (op.Level == LogLevel.Fatal)
                {
                    Console.Error.WriteLine("Fatal error: " + op.Message);
                    Exit();
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