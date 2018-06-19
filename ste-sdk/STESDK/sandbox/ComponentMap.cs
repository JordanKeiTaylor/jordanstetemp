using System;
using System.Collections;
using System.Collections.Generic;
using Improbable;
using Improbable.Worker;

namespace stesdk.sandbox
{
    [Flags]
    public enum ComponentMapEvent
    {
        AddComponent = 1,
        UpdateComponent = 2,
        RemoveEntity = 4,
        AuthorityChange = 8,
    }

    public class ComponentMap<T> : IEnumerable<KeyValuePair<EntityId, IComponentData<T>>>
        where T : IComponentMetaclass
    {
        private static readonly Random Rand = new Random();

        private readonly HashSet<EntityId> _authority;
        private readonly Dictionary<EntityId, IComponentData<T>> _components;

        private bool _hasUpdated = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:stesdk.sandbox.ComponentMap`1"/> class.
        /// </summary>
        /// <param name="dispatcher">Dispatcher.</param>
        /// <param name="disableEvents">
        /// Events to disable.
        /// Note: Can pass in multiple flags using bitwise OR. For example, AddComponent | UpdateComponent.
        /// </param>
        public ComponentMap(IDispatcher dispatcher, ComponentMapEvent? disableEvents = null)
        {
            _authority = new HashSet<EntityId>();
            _components = new Dictionary<EntityId, IComponentData<T>>();

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

        public Dictionary<EntityId, IComponentData<T>>.KeyCollection Keys => _components.Keys;

        public Dictionary<EntityId, IComponentData<T>>.ValueCollection Values => _components.Values;

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

        public IEnumerator<KeyValuePair<EntityId, IComponentData<T>>> GetEnumerator()
        {
            return _components.GetEnumerator();
        }

        public bool HasUpdated()
        {
            return _hasUpdated;
        }

        public void AckUpdated()
        {
            _hasUpdated = false;
        }

        public EntityId GetRandomAuthorativeId()
        {
            var e = _authority.GetEnumerator();
            var index = Rand.Next(_authority.Count);
            for (int i = 0; i < index; i++)
            {
                e.MoveNext();
            }

            return e.Current;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void SetAuthority(AuthorityChangeOp authorityChange)
        {
            if (authorityChange.Authority == Authority.Authoritative)
            {
                _authority.Add(authorityChange.EntityId);
            }
            else
            {
                _authority.Remove(authorityChange.EntityId);
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
