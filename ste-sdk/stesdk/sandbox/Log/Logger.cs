using System;
using System.Collections.Generic;
using Improbable.Collections;
using Improbable.Sandbox.Environment;
using Improbable.Worker;

namespace Improbable.Sandbox.Log
{
    public class Logger : IConnectionReceiver
    {
        public static readonly Logger DefaultLogger = new Logger();
        public static LogLevel AlwaysConsoleLogAtLogLevel = LogLevel.Error;
        private readonly ISet<IConnection> _connections = new HashSet<IConnection>();

        public void AttachConnection(IConnection c)
        {
            _connections.Add(c);
        }

        public void DetachConnection(IConnection c)
        {
            _connections.Remove(c);
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
            var consoleLog = alwaysConsole || (level > AlwaysConsoleLogAtLogLevel);
            var logged = false;
            foreach (var c in _connections)
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
            private readonly string _name;
            private readonly Logger _parent;
            private readonly bool _alwaysConsole;

            public NamedLogger(string name, Logger parent, bool alwaysConsole = false)
            {
                _name = name;
                _parent = parent;
                _alwaysConsole = alwaysConsole;
            }

            public void Fatal(string message, Option<EntityId> entityId = default(Option<EntityId>))
            {
                _parent.Log(LogLevel.Fatal, _name, message, entityId, _alwaysConsole);
            }

            public void Fatal(string message, Exception e, Option<EntityId> entityId = default(Option<EntityId>))
            {
                _parent.Log(LogLevel.Fatal, _name, message + "\n" + e, entityId, _alwaysConsole);
            }

            public void Error(string message, Exception e, Option<EntityId> entityId = default(Option<EntityId>))
            {
                _parent.Log(LogLevel.Error, _name, message + "\n" + e, entityId, _alwaysConsole);
            }

            public void Error(string message, Option<EntityId> entityId = default(Option<EntityId>))
            {
                _parent.Log(LogLevel.Error, _name, message, entityId, _alwaysConsole);
            }

            public void Warn(string message, Option<EntityId> entityId = default(Option<EntityId>))
            {
                _parent.Log(LogLevel.Warn, _name, message, entityId, _alwaysConsole);
            }

            public void Info(string message, Option<EntityId> entityId = default(Option<EntityId>))
            {
                _parent.Log(LogLevel.Info, _name, message, entityId, _alwaysConsole);
            }

            public void Debug(string message, Option<EntityId> entityId = default(Option<EntityId>))
            {
                _parent.Log(LogLevel.Debug, _name, message, entityId, _alwaysConsole);
            }
        }

    }
}