using System;
using Improbable;
using Improbable.Collections;
using Improbable.Worker;

namespace stesdk.sandbox.Log
{
    public class NamedLogger : ILogger
    {
        private readonly string _name;
        private readonly Logger _parent;
        private readonly bool _alwaysConsole;

        public NamedLogger(string name, Logger parent, bool alwaysConsole = false)
        {
            this._name = name;
            this._parent = parent;
            this._alwaysConsole = alwaysConsole;
        }

        public void Log(LogLevel level, string message, Option<EntityId> entityId = default(Option<EntityId>))
        {
            _parent.Log(level, _name, message, entityId, _alwaysConsole);
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