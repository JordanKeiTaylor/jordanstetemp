using System;
using System.Collections.Generic;
using Improbable;
using Improbable.Collections;
using Improbable.Worker;

namespace Shared
{
    public class Logger : IConnectionReceiver
    {
        public static Logger DefaultLogger = new Logger();
        public static LogLevel AlwaysConsoleLogAtLogLevel = LogLevel.Error;
        private ISet<IConnection> connections = new HashSet<IConnection>();

        public Logger()
        {
        }

        public void AttachConnection(IConnection c)
        {
            this.connections.Add(c);
        }

        public void DetachConnection(IConnection c)
        {
            this.connections.Remove(c);
        }

        public NamedLogger CreateWithName(string name)
        {
            return new NamedLogger(name, this);
        }


        public NamedLogger CreateWithNameAndConsole(string name)
        {
            return new NamedLogger(name, this, true);
        }

        public static NamedLogger DefaultWithName(string name)
        {
            return DefaultLogger.CreateWithName(name);
        }

        public static NamedLogger DefaultWithNameAndConsole(string name)
        {
            return DefaultLogger.CreateWithNameAndConsole(name);
        }

        public void Log(LogLevel level, string name, string message, Option<EntityId> entityId = default(Option<EntityId>), bool alwaysConsole = false)
        {
            var consoleLog = alwaysConsole || (level > Logger.AlwaysConsoleLogAtLogLevel);
            var logged = false;
            foreach (var c in connections)
            {
                if (c.IsConnected)
                {
                    c.SendLogMessage(level, name, message, entityId);
                    logged = true;
                }
            }
            if (consoleLog || !logged)
            {
                if (level >= LogLevel.Error)
                {
                    Console.Error.WriteLine(name + "  : " + message);
                }
                else
                {
                    Console.WriteLine(name + "  : " + message);
                }
            }
        }

        public interface ILogger
        {
            void Fatal(string message, Option<EntityId> entityId = default(Option<EntityId>));
            void Fatal(string message, Exception e, Option<EntityId> entityId = default(Option<EntityId>));

            void Error(string message, Option<EntityId> entityId = default(Option<EntityId>));
            void Error(string message, Exception e, Option<EntityId> entityId = default(Option<EntityId>));

            void Warn(string message, Option<EntityId> entityId = default(Option<EntityId>));

            void Info(string message, Option<EntityId> entityId = default(Option<EntityId>));

            void Debug(string message, Option<EntityId> entityId = default(Option<EntityId>));
        }

        public class NamedLogger : ILogger
        {
            private readonly string name;
            private readonly Logger parent;
            private readonly bool alwaysConsole;

            public NamedLogger(String name, Logger parent, bool alwaysConsole = false)
            {
                this.name = name;
                this.parent = parent;
                this.alwaysConsole = alwaysConsole;
            }

            public void Fatal(string message, Option<EntityId> entityId = default(Option<EntityId>))
            {
                parent.Log(LogLevel.Fatal, name, message, entityId, alwaysConsole);
            }

            public void Fatal(string message, Exception e, Option<EntityId> entityId = default(Option<EntityId>))
            {
                parent.Log(LogLevel.Fatal, name, message + "\n" + e.ToString(), entityId, alwaysConsole);
            }

            public void Error(string message, Exception e, Option<EntityId> entityId = default(Option<EntityId>))
            {
                parent.Log(LogLevel.Error, name, message + "\n" + e.ToString(), entityId, alwaysConsole);
            }

            public void Error(string message, Option<EntityId> entityId = default(Option<EntityId>))
            {
                parent.Log(LogLevel.Error, name, message, entityId, alwaysConsole);
            }

            public void Warn(string message, Option<EntityId> entityId = default(Option<EntityId>))
            {
                parent.Log(LogLevel.Warn, name, message, entityId, alwaysConsole);
            }

            public void Info(string message, Option<EntityId> entityId = default(Option<EntityId>))
            {
                parent.Log(LogLevel.Info, name, message, entityId, alwaysConsole);
            }

            public void Debug(string message, Option<EntityId> entityId = default(Option<EntityId>))
            {
                parent.Log(LogLevel.Debug, name, message, entityId, alwaysConsole);
            }
        }

    }
}