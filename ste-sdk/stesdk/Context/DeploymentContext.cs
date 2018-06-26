using System;
using Improbable.Environment;
using Improbable.Log;
using Improbable.Worker;

namespace Improbable
{
    public class DeploymentContext
    {   
        private const string LoggerName = "DeploymentContext.cs";
        private static readonly Logger.NamedLogger Logger = Log.Logger.DefaultWithName(LoggerName);

        private static DeploymentContext _context;
        
        private string _workerType;
        private Connection _connection;
        private Dispatcher _dispatcher;
        private IDispatcher _wrappedDispatcher;
        private IConnection _wrappedConnection;
        
        public bool IsDispatcherConnected { get; set; }
        public bool IsDispatcherInCritical { get; set; }

        private bool _initialized;

        public static DeploymentContext GetInstance()
        {
            return _context ?? (_context = new DeploymentContext());
        }
        
        private DeploymentContext()
        {
            _initialized = false;
        }

        public void Init(string workerType, string workerId, string hostname, ushort port)
        {
            if (_initialized)
            {
                Logger.Warn("Attempt to reinitialize DeploymentContext has been cancelled.");
                return;
            }
            
            _workerType = workerType;
            _connection = CreateConnection(hostname, port, workerId);
            _dispatcher = CreateDispatcher(_connection);

            if (_connection == null || !_connection.IsConnected)
            {
                Logger.Fatal("Failed to connect to SpatialOS");
                return;
            }

            _wrappedConnection = new ConnectionWrapper(_connection);
            _wrappedDispatcher = new DispatcherWrapper(_dispatcher);

            Log.Logger.DefaultLogger.AttachConnection(_wrappedConnection);

            IsDispatcherConnected = _connection.IsConnected;

            _initialized = true;
        }

        public void TestInit(IConnection connection, IDispatcher dispatcher)
        {
            if (_initialized)
            {
                Logger.Warn("Attempt to reinitialize DeploymentContext has been cancelled.");
                return;
            }

            _wrappedConnection = connection;
            _wrappedDispatcher = dispatcher;
            
            _initialized = true;
        }
        
        /// <summary>
        /// Returns an IConnection wrapper of the instantiated Worker SDK Connection
        /// </summary>
        /// <returns>IConnection</returns>
        public IConnection GetConnection()
        {
            return _wrappedConnection;
        }

        /// <summary>
        /// Returns an IDispatcher wrapper of the instantiated Worker SDK Dispatcher
        /// </summary>
        /// <returns>IDispatcher</returns>
        public IDispatcher GetDispatcher()
        {
            return _wrappedDispatcher;
        }
        
        /// <summary>
        /// Exit execution with a specified status. Disposes of connection and dispatcher.
        /// </summary>
        /// <param name="status"></param>
        public void Exit(ContextStatus status)
        {
            _connection.Dispose();
            _dispatcher.Dispose();
            _wrappedConnection.Dispose();
            _wrappedDispatcher.Dispose();
            Logger.Warn("Exiting: " + status);
        }

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

            Logger.Info("Successfully connected using the Receptionist");

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
                    Exit(ContextStatus.ErrorExit);
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