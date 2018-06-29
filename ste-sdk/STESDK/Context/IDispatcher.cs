using System;
using Improbable.Worker;

namespace Improbable.Context
{
    /// <summary>
    /// Wrapper implementation for <see cref="Improbable.Worker.Dispatcher"/>
    /// </summary>
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
        
        void Process(OpList opList);
        
        Dispatcher GetBaseDispatcher();
    }
}