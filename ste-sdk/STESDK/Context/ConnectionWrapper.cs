using Improbable.Collections;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace Improbable.Context
{
    /// <summary>
    /// Default wrapper implementation of <see cref="IConnection"/>.
    /// </summary>
    /// <seealso cref="Improbable.Worker.Connection"/>
    public class ConnectionWrapper : IConnection
    {
        private readonly Connection _connection;

        public bool IsConnected => _connection.IsConnected;

        public ConnectionWrapper(Connection connection)
        {
            _connection = connection;
        }

        public void SendComponentUpdate<C>(EntityId entityId, IComponentUpdate<C> componentUpdate, bool legacyCallbackSemantics = false)
            where C : IComponentMetaclass
        {
            _connection.SendComponentUpdate<C>(entityId, componentUpdate, legacyCallbackSemantics);
        }

        public string GetWorkerId()
        {
            return _connection.GetWorkerId();
        }

        public void SendLogMessage(LogLevel level, string loggerName, string message, Option<EntityId> entityId)
        {
            _connection.SendLogMessage(level, loggerName, message);
        }

        public void SendComponentInterest(EntityId entityId, System.Collections.Generic.Dictionary<uint, InterestOverride> interestOverrides)
        {
            _connection.SendComponentInterest(entityId, interestOverrides);
        }

        public RequestId<ReserveEntityIdsRequest> SendReserveEntityIdsRequest(uint numberOfEntityIds, Option<uint> timeoutMillis)
        {
            return _connection.SendReserveEntityIdsRequest(numberOfEntityIds, timeoutMillis);
        }

        public RequestId<CreateEntityRequest> SendCreateEntityRequest(Entity entity, Option<EntityId> entityId, Option<uint> timeoutMillis)
        {
            return _connection.SendCreateEntityRequest(entity, entityId, timeoutMillis);
        }

        public void SendCommandResponse<C>(RequestId<IncomingCommandRequest<C>> requestId, ICommandResponse<C> response)
            where C : ICommandMetaclass, new()
        {
            _connection.SendCommandResponse(requestId, response);
        }

        public Option<string> GetWorkerFlag(string flagName)
        {
            return _connection.GetWorkerFlag(flagName);
        }

        public RequestId<DeleteEntityRequest> SendDeleteEntityRequest(EntityId entityId, Option<uint> timeoutMillis)
        {
            return _connection.SendDeleteEntityRequest(entityId, timeoutMillis);
        }

        public OpList GetOpList(uint timeoutMillis)
        {
            return _connection.GetOpList(timeoutMillis);
        }

        public RequestId<OutgoingCommandRequest<C>> SendCommandRequest<C>(EntityId entityId, ICommandRequest<C> request, Option<uint> timeoutMillis, CommandParameters parameters = null) where C : ICommandMetaclass, new()
        {
            return _connection.SendCommandRequest(entityId, request, timeoutMillis, parameters);
        }

        public RequestId<EntityQueryRequest> SendEntityQueryRequest(EntityQuery entityQuery, Option<uint> timeoutMillis)
        {
            return _connection.SendEntityQueryRequest(entityQuery, timeoutMillis);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public void SendMetrics(Improbable.Worker.Metrics metrics)
        {
            _connection.SendMetrics(metrics);
        }
    }
}