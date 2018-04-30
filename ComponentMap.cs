using System.Collections;
using System.Collections.Generic;
using System;
using Improbable;
using Improbable.Worker;

namespace Shared
{
    public class ComponentMap<T> : IEnumerable<KeyValuePair<EntityId, IComponentData<T>>>
        where T : IComponentMetaclass
    {
        private static readonly Random random = new Random();

        readonly HashSet<EntityId> authority;
        readonly Dictionary<EntityId, IComponentData<T>> components;

        private bool _hasUpdated = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Shared.ComponentMap`1"/> class.
        /// </summary>
        /// <param name="dispatcher">Dispatcher.</param>
        /// <param name="disableEvents">
        /// Events to disable. 
        /// Note: Can pass in multiple flags using bitwise OR. For example, AddComponent | UpdateComponent.
        /// </param>
        public ComponentMap(IDispatcher dispatcher, ComponentMapEvent? disableEvents = null)
        {
            authority = new HashSet<EntityId>();
            components = new Dictionary<EntityId, IComponentData<T>>();

            if (!hasFlag(disableEvents, ComponentMapEvent.AddComponent))
            {
                dispatcher.OnAddComponent<T>(addComponent);
            }

            if (!hasFlag(disableEvents, ComponentMapEvent.RemoveEntity))
            {
                dispatcher.OnRemoveEntity(removeEntity);
            }

            if (!hasFlag(disableEvents, ComponentMapEvent.UpdateComponent))
            {
                dispatcher.OnComponentUpdate<T>(updateComponent);
            }

            if (!hasFlag(disableEvents, ComponentMapEvent.AuthorityChange))
            {
                dispatcher.OnAuthorityChange<T>(setAuthority);
            }
        }

        public Dictionary<EntityId, IComponentData<T>>.KeyCollection Keys
        {
            get { return components.Keys; }
        }

        public Dictionary<EntityId, IComponentData<T>>.ValueCollection Values
        {
            get { return components.Values; }
        }

        public bool ContainsKey(EntityId id)
        {
            return components.ContainsKey(id);
        }

        public IComponentData<T> Get(EntityId id)
        {
            return components[id];
        }

        public bool TryGetValue(EntityId id, out IComponentData<T> component)
        {
            return components.TryGetValue(id, out component);
        }

        public bool HasAuthority(EntityId Id)
        {
            return authority.Contains(Id);
        }

        public IEnumerator<KeyValuePair<EntityId, IComponentData<T>>> GetEnumerator()
        {
            return components.GetEnumerator();
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
            var e = authority.GetEnumerator();
            var index = random.Next(authority.Count);
            for (int i = 0; i < index; i++)
            {
                e.MoveNext();
            }
            return e.Current;
        }

        void setAuthority(AuthorityChangeOp authorityChange)
        {
            if (authorityChange.Authority == Authority.Authoritative)
            {
                authority.Add(authorityChange.EntityId);
            }
            else
            {
                authority.Remove(authorityChange.EntityId);
            }
        }

        void updateComponent(ComponentUpdateOp<T> update)
        {
            if (!HasAuthority(update.EntityId) && components.ContainsKey(update.EntityId))
            {
                update.Update.ApplyTo(components[update.EntityId]);
                _hasUpdated = true;
            }
        }

        void addComponent(AddComponentOp<T> add)
        {
            components[add.EntityId] = add.Data;
            _hasUpdated = true;
        }

        void removeEntity(RemoveEntityOp removeEntityOp)
        {
            if (components.ContainsKey(removeEntityOp.EntityId))
            {
                components.Remove(removeEntityOp.EntityId);
                _hasUpdated = true;
            }
        }

        bool hasFlag(ComponentMapEvent? allFlags, ComponentMapEvent flag)
        {
            return allFlags.HasValue && allFlags.Value.HasFlag(flag);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [Flags]
    public enum ComponentMapEvent
    {
        AddComponent = 1,
        UpdateComponent = 2,
        RemoveEntity = 4,
        AuthorityChange = 8
    }
}
