using Improbable;
using Improbable.Context;
using Improbable.Worker;

namespace Tests
{
    public class ComponentMapTestWrapper<T> : ComponentMap<T> where T : IComponentMetaclass
    {
        public ComponentMapTestWrapper(IDispatcher dispatcher)
            : base(
                dispatcher, 
                ComponentMapEvent.AddComponent | 
                ComponentMapEvent.AuthorityChange | 
                ComponentMapEvent.RemoveEntity | 
                ComponentMapEvent.UpdateComponent) { }
        
        public new void SetAuthority(AuthorityChangeOp authorityChange)
        {
            base.SetAuthority(authorityChange);
        }

        public new void UpdateComponent(ComponentUpdateOp<T> update)
        {
            base.UpdateComponent(update);
        }

        public new void AddComponent(AddComponentOp<T> add)
        {
            base.AddComponent(add);
        }

        public new void RemoveEntity(RemoveEntityOp removeEntityOp)
        {
            base.RemoveEntity(removeEntityOp);
        }
    }
}