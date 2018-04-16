using Improbable;
using Improbable.Collections;
using Improbable.Worker;
using Improbable.Worker.Query;
using System;

namespace Shared
{
    public interface IConnection : IDisposable
    {
        bool IsConnected { get; }

        void SendComponentUpdate<C>(EntityId entityId, IComponentUpdate<C> componentUpdate, bool legacyCallbackSemantics = false)
            where C : IComponentMetaclass;

        void SendLogMessage(LogLevel info, String loggerName, String message, Option<EntityId> entityId = default(Option<EntityId>));

        string GetWorkerId();

        void SendComponentInterest(EntityId entityId, System.Collections.Generic.Dictionary<uint, InterestOverride> interestOverrides);

        RequestId<ReserveEntityIdRequest> SendReserveEntityIdRequest(Option<uint> timeoutMillis);

        RequestId<CreateEntityRequest> SendCreateEntityRequest(Entity entity, Option<EntityId> entityId, Option<uint> timeoutMillis);

        RequestId<OutgoingCommandRequest<C>> SendCommandRequest<C>(EntityId entityId, ICommandRequest<C> request, Option<uint> timeoutMillis, CommandParameters parameters = null) where C : ICommandMetaclass, new();

        Option<string> GetWorkerFlag(string flagName);

        RequestId<DeleteEntityRequest> SendDeleteEntityRequest(EntityId entityId, Option<uint> timeoutMillis);

        OpList GetOpList(uint timeoutMillis);

        void SendCommandResponse<C>(RequestId<IncomingCommandRequest<C>> requestId, ICommandResponse<C> response)
            where C : ICommandMetaclass, new();

        RequestId<EntityQueryRequest> SendEntityQueryRequest(EntityQuery entityQuery, Option<uint> timeoutMillis);

        void SendMetrics(Metrics metrics);
    }

    public class ConnectionWrapper : IConnection
    {
        private readonly Connection connection;

        public bool IsConnected => connection.IsConnected;

        public ConnectionWrapper(Connection connection)
        {
            this.connection = connection;
        }

        public void SendComponentUpdate<C>(EntityId entityId, IComponentUpdate<C> componentUpdate, bool legacyCallbackSemantics = false)
            where C : IComponentMetaclass
        {
            connection.SendComponentUpdate<C>(entityId, componentUpdate, legacyCallbackSemantics);
        }

        public string GetWorkerId()
        {
            return connection.GetWorkerId();
        }

        public void SendLogMessage(LogLevel level, string loggerName, string message, Option<EntityId> entityId)
        {
            connection.SendLogMessage(level, loggerName, message);
        }

        public void SendComponentInterest(EntityId entityId, System.Collections.Generic.Dictionary<uint, InterestOverride> interestOverrides)
        {
            connection.SendComponentInterest(entityId, interestOverrides);
        }

        public RequestId<ReserveEntityIdRequest> SendReserveEntityIdRequest(Option<uint> timeoutMillis)
        {
            return connection.SendReserveEntityIdRequest(timeoutMillis);
        }

        public RequestId<CreateEntityRequest> SendCreateEntityRequest(Entity entity, Option<EntityId> entityId, Option<uint> timeoutMillis)
        {
            return connection.SendCreateEntityRequest(entity, entityId, timeoutMillis);
        }

        public void SendCommandResponse<C>(RequestId<IncomingCommandRequest<C>> requestId, ICommandResponse<C> response)
            where C : ICommandMetaclass, new()
        {
            connection.SendCommandResponse(requestId, response);
        }

        public Option<string> GetWorkerFlag(string flagName)
        {
            return connection.GetWorkerFlag(flagName);
        }

        public RequestId<DeleteEntityRequest> SendDeleteEntityRequest(EntityId entityId, Option<uint> timeoutMillis)
        {
            return connection.SendDeleteEntityRequest(entityId, timeoutMillis);
        }

        public OpList GetOpList(uint timeoutMillis)
        {
            return connection.GetOpList(timeoutMillis);
        }

        public RequestId<OutgoingCommandRequest<C>> SendCommandRequest<C>(EntityId entityId, ICommandRequest<C> request, Option<uint> timeoutMillis, CommandParameters parameters = null) where C : ICommandMetaclass, new()
        {
            return connection.SendCommandRequest<C>(entityId, request, timeoutMillis, parameters);
        }

        public RequestId<EntityQueryRequest> SendEntityQueryRequest(EntityQuery entityQuery, Option<uint> timeoutMillis)
        {
            return connection.SendEntityQueryRequest(entityQuery, timeoutMillis);
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public void SendMetrics(Metrics metrics)
        {
            connection.SendMetrics(metrics);
        }
    }
}
