using System;
using System.Collections.Generic;
using Improbable.Collections;
using Improbable.Context;
using Improbable.Sandbox;
using Improbable.Worker;

namespace Improbable.Log
{
    public class Logger : IConnectionReceiver
    {
        public static readonly Logger DefaultLogger = new Logger();
        public const LogLevel AlwaysConsoleLogAtLogLevel = LogLevel.Error;
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
    }
}