using Improbable.Collections;
using Improbable.Worker;

namespace Improbable
{
    public class ComponentManager
    {
        private readonly Map<IComponentMetaclass, ComponentMap<IComponentMetaclass>> _componentMaps;
        
        public ComponentManager()
        {
            _componentMaps = new Map<IComponentMetaclass, ComponentMap<IComponentMetaclass>>();
        }
        
        public void Subscribe(IComponentMetaclass component, ComponentMapEvent? mapEvent = null) 
        {
            if (!_componentMaps.ContainsKey(component))
            {
                _componentMaps.Add(component, new ComponentMap<IComponentMetaclass>(
                    DeploymentContext.GetInstance().GetDispatcher(), mapEvent
                ));
            }
        }
    }
}