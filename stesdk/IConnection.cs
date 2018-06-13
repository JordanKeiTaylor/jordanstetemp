using System;
using Improbable;
using Improbable.Collections;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace Shared
{
    public interface IConnection : IDisposable
    {
        bool IsConnected { get; }

        void SendComponentUpdate<C>(EntityId entityId, IComponentUpdate<C> componentUpdate, bool legacyCallbackSemantics = false)
            where C : IComponentMetaclass;

        void SendLogMessage(LogLevel info, string loggerName, string message, Option<EntityId> entityId = default(Option<EntityId>));

        string GetWorkerId();

        void SendComponentInterest(EntityId entityId, System.Collections.Generic.Dictionary<uint, InterestOverride> interestOverrides);

        RequestId<ReserveEntityIdRequest> SendReserveEntityIdRequest(Option<uint> timeoutMillis);

        RequestId<CreateEntityRequest> SendCreateEntityRequest(Entity entity, Option<EntityId> entityId, Option<uint> timeoutMillis);

        RequestId<OutgoingCommandRequest<C>> SendCommandRequest<C>(EntityId entityId, ICommandRequest<C> request, Option<uint> timeoutMillis, CommandParameters parameters = null)
            where C : ICommandMetaclass, new();

        Option<string> GetWorkerFlag(string flagName);

        RequestId<DeleteEntityRequest> SendDeleteEntityRequest(EntityId entityId, Option<uint> timeoutMillis);

        OpList GetOpList(uint timeoutMillis);

        void SendCommandResponse<C>(RequestId<IncomingCommandRequest<C>> requestId, ICommandResponse<C> response)
            where C : ICommandMetaclass, new();

        RequestId<EntityQueryRequest> SendEntityQueryRequest(EntityQuery entityQuery, Option<uint> timeoutMillis);

        void SendMetrics(Metrics metrics);
    }
}