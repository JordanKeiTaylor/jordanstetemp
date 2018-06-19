using System;
using Improbable.Worker;

namespace Improbable.Shared.Environment
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
        void OnReserveEntityIdsResponse(Action<ReserveEntityIdsResponseOp> callback);
        Dispatcher GetBaseDispatcher();
    }

    public class DispatcherWrapper : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherWrapper(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Dispose()
        {
            _dispatcher.Dispose();
        }

        public void OnAddComponent<C>(Action<AddComponentOp<C>> callback) where C : IComponentMetaclass
        {
            _dispatcher.OnAddComponent<C>(callback);
        }

        public void OnAddEntity(Action<AddEntityOp> callback)
        {
            _dispatcher.OnAddEntity(callback);
        }

        public void OnAuthorityChange<C>(Action<AuthorityChangeOp> callback) where C : IComponentMetaclass
        {
            _dispatcher.OnAuthorityChange<C>(callback);
        }

        public void OnCommandRequest<C>(Action<CommandRequestOp<C>> callback) where C : ICommandMetaclass, new()
        {
            _dispatcher.OnCommandRequest<C>(callback);
        }

        public void OnCommandResponse<C>(Action<CommandResponseOp<C>> callback) where C : ICommandMetaclass, new()
        {
            _dispatcher.OnCommandResponse(callback);
        }

        public void OnComponentUpdate<C>(Action<ComponentUpdateOp<C>> callback) where C : IComponentMetaclass
        {
            _dispatcher.OnComponentUpdate<C>(callback);
        }

        public void OnCreateEntityResponse(Action<CreateEntityResponseOp> callback)
        {
            _dispatcher.OnCreateEntityResponse(callback);
        }

        public void OnCriticalSection(Action<CriticalSectionOp> callback)
        {
            _dispatcher.OnCriticalSection(callback);
        }

        public void OnDeleteEntityResponse(Action<DeleteEntityResponseOp> callback)
        {
            _dispatcher.OnDeleteEntityResponse(callback);
        }

        public void OnDisconnect(Action<DisconnectOp> callback)
        {
            _dispatcher.OnDisconnect(callback);
        }

        public void OnEntityQueryResponse(Action<EntityQueryResponseOp> callback)
        {
            _dispatcher.OnEntityQueryResponse(callback);
        }


        public void OnFlagUpdate(Action<FlagUpdateOp> callback)
        {
            _dispatcher.OnFlagUpdate(callback);
        }

        public void OnLogMessage(Action<LogMessageOp> callback)
        {
            _dispatcher.OnLogMessage(callback);
        }

        public void OnMetrics(Action<MetricsOp> callback)
        {
            _dispatcher.OnMetrics(callback);
        }

        public void OnRemoveComponent<C>(Action<RemoveComponentOp> callback) where C : IComponentMetaclass
        {
            _dispatcher.OnRemoveComponent<C>(callback);
        }

        public void OnRemoveEntity(Action<RemoveEntityOp> callback)
        {
            _dispatcher.OnRemoveEntity(callback);
        }

        public void OnReserveEntityIdsResponse(Action<ReserveEntityIdsResponseOp> callback)
        {
            _dispatcher.OnReserveEntityIdsResponse(callback);
        }

        public Dispatcher GetBaseDispatcher()
        {
            return _dispatcher;
        }
    }
}
