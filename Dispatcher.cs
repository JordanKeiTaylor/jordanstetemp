using System;
using Improbable.Worker;

namespace Shared
{
    public interface IDispatcher : IDisposable
    {
        void OnAddComponent<C>(Action<AddComponentOp<C>> callback) where C : IComponentMetaclass;
        void OnAddEntity(Action<AddEntityOp> callback);
        void OnAuthorityChange<C>(Action<AuthorityChangeOp> callback) where C : IComponentMetaclass;
        void OnCommandRequest<C>(Action<CommandRequestOp<C>> callback) where C : ICommandMetaclass, new();
        void OnCommandResponse<C>(Action<CommandResponseOp<C>> callback) where C : ICommandMetaclass, new();
        void OnComponentUpdate<C>(Action<ComponentUpdateOp<C>> callback) where C : IComponentMetaclass;
        void OnCreateEntityResponse(Action<CreateEntityResponseOp> callback);
        void OnCriticalSection(Action<CriticalSectionOp> callback);
        void OnDeleteEntityResponse(Action<DeleteEntityResponseOp> callback);
        void OnDisconnect(Action<DisconnectOp> callback);
        void OnEntityQueryResponse(Action<EntityQueryResponseOp> callback);
        void OnFlagUpdate(Action<FlagUpdateOp> callback);
        void OnLogMessage(Action<LogMessageOp> callback);
        void OnMetrics(Action<MetricsOp> callback);
        void OnRemoveComponent<C>(Action<RemoveComponentOp> callback) where C : IComponentMetaclass;
        void OnRemoveEntity(Action<RemoveEntityOp> callback);
        void OnReserveEntityIdResponse(Action<ReserveEntityIdResponseOp> callback);
        void OnReserveEntityIdsResponse(Action<ReserveEntityIdsResponseOp> callback);
    }

    public class DispatcherWrapper : IDispatcher
    {
        private Dispatcher dispatcher;

        public DispatcherWrapper(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public void Dispose()
        {
            dispatcher.Dispose();
        }

        public void OnAddComponent<C>(Action<AddComponentOp<C>> callback) where C : IComponentMetaclass
        {
            dispatcher.OnAddComponent<C>(callback);
        }

        public void OnAddEntity(Action<AddEntityOp> callback)
        {
            dispatcher.OnAddEntity(callback);
        }

        public void OnAuthorityChange<C>(Action<AuthorityChangeOp> callback) where C : IComponentMetaclass
        {
            dispatcher.OnAuthorityChange<C>(callback);
        }

        public void OnCommandRequest<C>(Action<CommandRequestOp<C>> callback) where C : ICommandMetaclass, new()
        {
            dispatcher.OnCommandRequest<C>(callback);
        }

        public void OnCommandResponse<C>(Action<CommandResponseOp<C>> callback) where C : ICommandMetaclass, new()
        {
            dispatcher.OnCommandResponse(callback);
        }

        public void OnComponentUpdate<C>(Action<ComponentUpdateOp<C>> callback) where C : IComponentMetaclass
        {
            dispatcher.OnComponentUpdate<C>(callback);
        }

        public void OnCreateEntityResponse(Action<CreateEntityResponseOp> callback)
        {
            dispatcher.OnCreateEntityResponse(callback);
        }

        public void OnCriticalSection(Action<CriticalSectionOp> callback)
        {
            dispatcher.OnCriticalSection(callback);
        }

        public void OnDeleteEntityResponse(Action<DeleteEntityResponseOp> callback)
        {
            dispatcher.OnDeleteEntityResponse(callback);
        }

        public void OnDisconnect(Action<DisconnectOp> callback)
        {
            dispatcher.OnDisconnect(callback);
        }

        public void OnEntityQueryResponse(Action<EntityQueryResponseOp> callback)
        {
            dispatcher.OnEntityQueryResponse(callback);
        }


        public void OnFlagUpdate(Action<FlagUpdateOp> callback)
        {
            dispatcher.OnFlagUpdate(callback);
        }

        public void OnLogMessage(Action<LogMessageOp> callback)
        {
            dispatcher.OnLogMessage(callback);
        }

        public void OnMetrics(Action<MetricsOp> callback)
        {
            dispatcher.OnMetrics(callback);
        }

        public void OnRemoveComponent<C>(Action<RemoveComponentOp> callback) where C : IComponentMetaclass
        {
            dispatcher.OnRemoveComponent<C>(callback);
        }

        public void OnRemoveEntity(Action<RemoveEntityOp> callback)
        {
            dispatcher.OnRemoveEntity(callback);
        }

        public void OnReserveEntityIdResponse(Action<ReserveEntityIdResponseOp> callback)
        {
            dispatcher.OnReserveEntityIdResponse(callback);
        }

        public void OnReserveEntityIdsResponse(Action<ReserveEntityIdsResponseOp> callback)
        {
            dispatcher.OnReserveEntityIdsResponse(callback);
        }
    }

}
