using System;
using System.Collections;
using System.Collections.Generic;
using Improbable.Context;
using Improbable.Worker;

namespace Improbable.Collections
{
    /// <summary>
    /// Events that a <see cref="ComponentMap{T}"/> can react to.
    /// <list type="bullet">
    /// <item>AddComponent</item>
    /// <item>UpdateComponent</item>
    /// <item>RemoveEntity</item>
    /// <item>AuthorityChange</item>
    /// </list>
    /// </summary>
    [Flags]
    public enum ComponentMapEvent
    {
        AddComponent = 1,
        UpdateComponent = 2,
        RemoveEntity = 4,
        AuthorityChange = 8,
    }

    /// <summary>
    /// ComponentMap uses the active dispatcher to expose components in the SpatialOS world. The map will only receive
    /// updates for components the current worker can see or has authority over.
    /// </summary>
    /// <typeparam name="T">Component type of the map, must extend <see cref="IComponentMetaclass"/></typeparam>
    public class ComponentMap<T> : IEnumerable<KeyValuePair<EntityId, IComponentData<T>>>
        where T : IComponentMetaclass
    {
        private static readonly Random Rand = new Random();

        private readonly HashSet<EntityId> _authority;
        private readonly HashSet<EntityId> _authorityLossImminent;
        private readonly Dictionary<EntityId, IComponentData<T>> _components;
        public Dictionary<EntityId, IComponentData<T>>.KeyCollection Keys => _components.Keys;
        public Dictionary<EntityId, IComponentData<T>>.ValueCollection Values => _components.Values;
        
        private bool _hasUpdated = true;

        /// <summary>
        /// Initializes a new instance of <see cref="ComponentMap{T}"/>.
        /// </summary>
        /// <param name="dispatcher">
        /// Optional. Dispatcher for the ComponentMap to use. If left null, the map will attempt to use the dispatcher
        /// from the current <see cref="WorkerContext"/>. 
        /// </param>
        /// <param name="disableEvents">
        /// Optional. Events to disable. Can pass in multiple flags using bitwise OR. For example,
        /// AddComponent | UpdateComponent.
        /// </param>
        public ComponentMap(IDispatcher dispatcher = null, ComponentMapEvent? disableEvents = null)
        {
            _authority = new HashSet<EntityId>();
            _authorityLossImminent = new HashSet<EntityId>();
            _components = new Dictionary<EntityId, IComponentData<T>>();

            dispatcher = dispatcher ?? WorkerContext.GetInstance().GetDispatcher();
            
            if (!HasFlag(disableEvents, ComponentMapEvent.AddComponent))
            {
                dispatcher.OnAddComponent<T>(AddComponent);
            }

            if (!HasFlag(disableEvents, ComponentMapEvent.RemoveEntity))
            {
                dispatcher.OnRemoveEntity(RemoveEntity);
            }

            if (!HasFlag(disableEvents, ComponentMapEvent.UpdateComponent))
            {
                dispatcher.OnComponentUpdate<T>(UpdateComponent);
            }

            if (!HasFlag(disableEvents, ComponentMapEvent.AuthorityChange))
            {
                dispatcher.OnAuthorityChange<T>(SetAuthority);
            }
        }

        public bool ContainsKey(EntityId id)
        {
            return _components.ContainsKey(id);
        }

        public IComponentData<T> Get(EntityId id)
        {
            return _components[id];
        }

        public bool TryGetValue(EntityId id, out IComponentData<T> component)
        {
            return _components.TryGetValue(id, out component);
        }

        public bool HasAuthority(EntityId id)
        {
            return _authority.Contains(id);
        }

        public bool HasAuthorityLossImminent(EntityId id)
        {
            return _authorityLossImminent.Contains(id);
        }
        
        public IEnumerator<KeyValuePair<EntityId, IComponentData<T>>> GetEnumerator()
        {
            return _components.GetEnumerator();
        }

        /// <summary>
        /// Check if the ComponentMap has been updated since the last ACK of updates. Component Adds, Removes, and
        /// Updates cause this to return true. To acknowledge all updates, see <see cref="ComponentMap{T}.AckUpdated"/> 
        /// </summary>
        /// <returns>Returns true if the map has been updated since the last ACK of updates</returns>
        public bool HasUpdated()
        {
            return _hasUpdated;
        }

        /// <summary>
        /// Acknowledge that the map has been updated.
        /// </summary>
        public void AckUpdated()
        {
            _hasUpdated = false;
        }

        /// <summary>
        /// Randomly select an EntityId that this map is authoratative over. 
        /// </summary>
        /// <returns>EntityId with write authoriy.</returns>
        public EntityId GetRandomAuthorativeId()
        {
            var e = _authority.GetEnumerator();
            var index = Rand.Next(_authority.Count);
            for (var i = 0; i < index; i++)
            {
                e.MoveNext();
            }

            var entityId = e.Current;
            
            e.Dispose();
            
            return entityId;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void SetAuthority(AuthorityChangeOp authorityChange)
        {
            switch (authorityChange.Authority)
            {
                case Authority.Authoritative:
                    _authority.Add(authorityChange.EntityId);
                    _authorityLossImminent.Remove(authorityChange.EntityId);
                    break;
                case Authority.NotAuthoritative:
                    _authority.Remove(authorityChange.EntityId);
                    _authorityLossImminent.Remove(authorityChange.EntityId);
                    break;
                case Authority.AuthorityLossImminent:
                    _authorityLossImminent.Add(authorityChange.EntityId);
                    _authority.Remove(authorityChange.EntityId);
                    break;
            }
        }

        private void UpdateComponent(ComponentUpdateOp<T> update)
        {
            if (!HasAuthority(update.EntityId) && _components.ContainsKey(update.EntityId))
            {
                update.Update.ApplyTo(_components[update.EntityId]);
                _hasUpdated = true;
            }
        }

        private void AddComponent(AddComponentOp<T> add)
        {
            _components[add.EntityId] = add.Data;
            _hasUpdated = true;
        }

        private void RemoveEntity(RemoveEntityOp removeEntityOp)
        {
            if (_components.ContainsKey(removeEntityOp.EntityId))
            {
                _components.Remove(removeEntityOp.EntityId);
                _hasUpdated = true;
            }
        }

        private bool HasFlag(ComponentMapEvent? allFlags, ComponentMapEvent flag)
        {
            return allFlags.HasValue && allFlags.Value.HasFlag(flag);
        }
    }
}
