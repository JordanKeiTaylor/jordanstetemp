using System;
using Improbable.Collections;
using Improbable.Worker;

namespace Improbable.Enterprise.Sandbox.Log
{
    public interface ILogger
    {
        void Log(LogLevel level, string message, Option<EntityId> entityId = default(Option<EntityId>));

        void Fatal(string message, Option<EntityId> entityId = default(Option<EntityId>));

        void Fatal(string message, Exception e, Option<EntityId> entityId = default(Option<EntityId>));

        void Error(string message, Option<EntityId> entityId = default(Option<EntityId>));

        void Error(string message, Exception e, Option<EntityId> entityId = default(Option<EntityId>));

        void Warn(string message, Option<EntityId> entityId = default(Option<EntityId>));

        void Info(string message, Option<EntityId> entityId = default(Option<EntityId>));

        void Debug(string message, Option<EntityId> entityId = default(Option<EntityId>));
    }
}