using System;
using System.Collections;
using Improbable.Log;
using Improbable.Worker;

namespace Improbable.Environment
{
    /// <summary>
    /// EnvironmentBase creates and exposes the following:
    /// * Connection Wrapper to SpatialOS
    /// * Dispatcher Wrapper to SpatialOS
    /// * TimeStep Amount
    ///
    /// For worker specific environment management, this class can be extended for addition functionality.
    /// </summary>
    [Obsolete]
    public abstract class Environment
    {
        private const string LoggerName = "Environment.cs";
        private static readonly Logger.NamedLogger Logger = Log.Logger.DefaultWithName(LoggerName);

        private readonly string _workerType;
        private readonly Connection _connection;
        private readonly Dispatcher _dispatcher;
        private readonly IDispatcher _wrappedDispatcher;
        private readonly IConnection _wrappedConnection;
        private readonly TickTimeRollingMetric _tickTimeRollingMetric;
        public bool IsDispatcherConnected { get; set; }
        public bool IsDispatcherInCritical { get; set; }
        public double TimeStepMs { get; set; }
        private const int ErrorExitStatus = 1;

        /// <summary>
        /// Environment constructor establishes a connection to SpatialOS in addition to creating a dispatcher.
        /// </summary>
        /// <param name="workerType">Type of Worker</param>
        /// <param name="hostname">Hostname Worker is located on</param>
        /// <param name="port">Port Worker is exposed on</param>
        /// <param name="workerId">Id of Worker</param>
        protected Environment(string workerType, string hostname, ushort port, string workerId)
        {
            TimeStepMs = 1000.0;
            _workerType = workerType;
            _connection = CreateConnection(hostname, port, workerId);
            _dispatcher = CreateDispatcher(_connection);
            _tickTimeRollingMetric = new TickTimeRollingMetric(30);

            if (_connection == null || !_connection.IsConnected)
            {
                Logger.Fatal("Failed to connect to SpatialOS");
                return;
            }

            _wrappedConnection = new ConnectionWrapper(_connection);
            _wrappedDispatcher = new DispatcherWrapper(_dispatcher);

            Log.Logger.DefaultLogger.AttachConnection(_wrappedConnection);

            IsDispatcherConnected = _connection.IsConnected;
        }

        /// <summary>
        /// This constructor is intended for use during unit testing only.
        /// </summary>
        /// <param name="connection">IConnection, mocked</param>
        /// <param name="dispatcher">IDispatcher, mocked</param>
        protected Environment(IConnection connection, IDispatcher dispatcher)
        {
            _wrappedConnection = connection;
            _wrappedDispatcher = dispatcher;
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

        internal TickTimeRollingMetric GetTickTimeRollingMetric()
        {
            return _tickTimeRollingMetric;
        }

        public void Exit(int status)
        {
            _connection.Dispose();
            _dispatcher.Dispose();
            _wrappedConnection.Dispose();
            _wrappedDispatcher.Dispose();
            Logger.Warn("Exiting: " + status);
        }

        /// <summary>
        /// Creates an Improbable.Worker.Connection. This will be wrapped by EnvironmentBase into an
        /// Improbable.Shared.Environment.IConnection. The options parameters provides the following:
        /// * Hostname
        /// * Port
        /// * WorkerId
        ///
        /// To create a different Connection, override CreateConnection using a sub-class.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="workerId"></param>
        /// <returns>Improbable.Worker.Connection</returns>
        protected Connection CreateConnection(string hostname, ushort port, string workerId)
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

        /// <summary>
        /// Creates an Improbable.Worker.Dispatcher. EnvironmentBase will wrap this in
        /// Improbable.Shared.Environment.IDispatcher.
        ///
        /// To create a different Dispatcher, override CreateDispatcher using a sub-class.
        /// </summary>
        /// <param name="conn">Improbable.Worker.Connection used by the dispatcher to send log messages</param>
        /// <returns>Improbable.Worker.Dispatcher</returns>
        protected Dispatcher CreateDispatcher(Connection conn)
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
                    Exit(ErrorExitStatus);
                }
            });

            dispatch.OnCriticalSection(section =>
            {
                IsDispatcherInCritical = section.InCriticalSection;
            });

            dispatch.OnMetrics(metricOp =>
            {
                var avgSleepTimeMs = TimeStepMs - _tickTimeRollingMetric.GetAvg();
                if (avgSleepTimeMs < 0.0)
                {
                    metricOp.Metrics.Load = 1.0 + ((-avgSleepTimeMs) / TimeStepMs);
                }
                else
                {
                    metricOp.Metrics.Load = (TimeStepMs - avgSleepTimeMs) / TimeStepMs;
                }

                conn.SendMetrics(metricOp.Metrics);
            });

            return dispatch;
        }

        internal class TickTimeRollingMetric {

            private double _total;
            private readonly Queue _values;
            private readonly int _size;

            private readonly object _lock = new object();

            public TickTimeRollingMetric(int size) {
                _size = size;
                _values = new Queue(size + 1);
            }

            public void Record(double value) {
                lock (_lock) {
                    _total += value;
                    _values.Enqueue(value);
                    while (_values.Count > _size) {
                        _total = _total - (double)_values.Dequeue();
                    }
                }
            }

            public double GetAvg() {
                lock (_lock) {
                    if (_values.Count == 0) {
                        return 0.0;
                    }
                    return _total / _values.Count;
                }
            }
        }
    }
}
