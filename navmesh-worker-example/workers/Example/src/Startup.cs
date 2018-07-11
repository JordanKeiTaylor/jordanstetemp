using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Improbable.Worker;
using CommandLine;
using Improbable.Context;
using Improbable.Log;

namespace Example {
    internal class Options {
        [Option("hostname", Required = true, HelpText = "hostname of the receptionist to connect to")]
        public String Hostname { get; set; }

        [Option("port", Required = true, HelpText = "port to use.")]
        public ushort Port { get; set; }

        [Option("worker-id", Required = true, HelpText = "name of the worker assigned by SpatialOS.")]
        public string WorkerId { get; set; }
    }

    internal class Startup {
        private const string WorkerType = "Example";
        private static string WorkerId = "";

        private const string LoggerName = "Startup.cs";

        private const int ErrorExitStatus = 1;

        private static bool isConnected;

        private static bool inCritical;

        private static double tickTimeMillis = 1000;

        private static Stopwatch workTimer = new Stopwatch();
        private static Stopwatch metricsGapTimer = new Stopwatch();

        private static Environment env;
        private static Connection connection = null;
        private static Dispatcher dispatcher = null;
        private static IDispatcher wrappedDispatcher = null;
        private static ConnectionWrapper wrappedConnection = null;
        private static NamedLogger logger = Logger.DefaultWithName(LoggerName);

        private static int Main(string[] args) {
            var parser = new Parser(_ => new ParserSettings { HelpWriter = null });
            var result = parser.ParseArguments<Options>(args);

            logger.Info("Beginning");
            Console.WriteLine("Beginning");
            
            return result.MapResult(opts => Run(opts), errs => {
                Console.WriteLine(errs.ToString());
                return ErrorExitStatus;
            });
        }

        private static int Run(Options options) {
            Console.WriteLine("before gen");
            Assembly.Load("GeneratedCode");
            Console.WriteLine("after gen");

           

            WorkerId = options.WorkerId;

            try {
                connection = CreateConnection(options);
                dispatcher = CreateDispatcher(connection);
                if (connection == null || !connection.IsConnected) {
                    logger.Fatal("Failed to connect to SpatialOS");
                    return ErrorExitStatus;
                }
                wrappedDispatcher = new DispatcherWrapper(dispatcher);
                wrappedConnection = new ConnectionWrapper(connection);

                Logger.DefaultLogger.AttachConnection(wrappedConnection);

                isConnected = true;

                env = new Environment(wrappedConnection, wrappedDispatcher);

                FetchAndProcessOps(env.Connection, dispatcher, tickTimeMillis);
                
                logger.Info("Completed prewarming - entering main loop");
                Console.WriteLine("run loop");
                RunEventLoop();
            } finally {
                if (connection != null) connection.Dispose();
                if (dispatcher != null) dispatcher.Dispose();
            }
            Console.WriteLine("exiting");
            return ErrorExitStatus;
        }

        private static void RunEventLoop() {
            var exampleBehaviour = new ExampleBehaviour(env);

            metricsGapTimer.Start();

            // run loop
            while (isConnected) {
                FetchAndProcessOps(env.Connection, dispatcher, tickTimeMillis);
                try {
                    workTimer.Start();
                    exampleBehaviour.Tick(tickTimeMillis);
                } catch (Exception e) {
                    logger.Error("Caught exception during Tick()", e);
                } finally {
                    workTimer.Stop();
                    Thread.Sleep((int)tickTimeMillis);
                }
            }
        }

        private static void FetchAndProcessOps(IConnection connection, Dispatcher dispatcher, double timeoutMillis) {
            var opList = connection.GetOpList((uint)(timeoutMillis));
            ProcessOps(opList, dispatcher);

            while (inCritical) {
                opList = connection.GetOpList(0);
                ProcessOps(opList, dispatcher);
            }
        }

        private static void ProcessOps(OpList opList, Dispatcher dispatcher) {
            workTimer.Start();
            dispatcher.Process(opList);
            workTimer.Stop();
        }

        private static Dispatcher CreateDispatcher(Connection connection) {
            var dispatcher = new Dispatcher();

            dispatcher.OnDisconnect(op => {
                Console.Error.WriteLine("[disconnect] " + op.Reason);
                isConnected = false;
            });

            dispatcher.OnLogMessage(op => {
                connection.SendLogMessage(op.Level, LoggerName, op.Message);
                if (op.Level == LogLevel.Fatal) {
                    Console.Error.WriteLine("Fatal error: " + op.Message);
                    Environment.Exit(connection, ErrorExitStatus);
                }
            });

            dispatcher.OnCriticalSection(section => {
                inCritical = section.InCriticalSection;
            });

            dispatcher.OnMetrics(metricOp => {
                metricsGapTimer.Stop();
                workTimer.Stop();

                metricOp.Metrics.Load = workTimer.ElapsedMilliseconds / (double)metricsGapTimer.ElapsedMilliseconds;

                metricsGapTimer.Restart();
                workTimer.Restart();

                connection.SendMetrics(metricOp.Metrics);
            });

            return dispatcher;
        }

        private static Connection CreateConnection(Options options) {
            var connectionParameters = new ConnectionParameters {
                WorkerType = WorkerType,
                Network =
                {
                    ConnectionType = NetworkConnectionType.Tcp,
                    Tcp = new TcpNetworkParameters()
                    {
                        MultiplexLevel = 1
                    }
                }
            };

            var connection = ConnectWithReceptionist(
                options.Hostname,
                options.Port,
                options.WorkerId,
                connectionParameters
            );
            return connection;
        }

        private static Connection ConnectWithReceptionist(
            string hostname,
            ushort port,
            string workerId,
            ConnectionParameters connectionParameters
        ) {
            Connection connection;

            // You might want to change this to true or expose it as a command-line option
            // if using `spatial cloud connect external` for debugging
            connectionParameters.Network.UseExternalIp = false;

            using (var future = Connection.ConnectAsync(
                hostname,
                port,
                workerId,
                connectionParameters
            )) {
                connection = future.Get();
            }

            logger.Info("Successfully connected using the Receptionist");

            return connection;
        }
    }
}
