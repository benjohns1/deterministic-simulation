using System;
using System.Collections.Generic;
using System.Linq;
using ComponentTypeName = System.String;

namespace Simulation.State
{
    [Serializable]
    public class Snapshot : ICloneable
    {
        private readonly Dictionary<ComponentTypeName, List<SimComponent>> components = new Dictionary<ComponentTypeName, List<SimComponent>>();

        public Snapshot() { }

        public Snapshot(Snapshot snapshot)
        {
            foreach (KeyValuePair<ComponentTypeName, List<SimComponent>> pair in snapshot.components)
            {
                List<SimComponent> componentList = new List<SimComponent>(pair.Value.Capacity);
                foreach (SimComponent component in pair.Value)
                {
                    componentList.Add((SimComponent)component.Clone());
                }
                components.Add(pair.Key, componentList);
            }
        }

        public object Clone()
        {
            return new Snapshot(this);
        }

        public void AddComponent<T>(T component) where T : SimComponent
        {
            ComponentTypeName type = typeof(T).Name;
            if (!components.ContainsKey(type))
            {
                components.Add(type, new List<SimComponent>());
            }
            components[type].Add(component);
        }

        public IEnumerable<T> GetComponents<T>() where T : SimComponent
        {
            ComponentTypeName type = typeof(T).Name;
            if (!components.ContainsKey(type))
            {
                return new List<T>();
            }
            return components[type].Select(c => c as T);
        }

        public IEnumerable<SimComponent> GetComponents()
        {
            return components.SelectMany(c => c.Value);
        }

        internal void Update(IEnumerable<ComponentUpdate> updates)
        {
            foreach (ComponentUpdate update in updates)
            {
                SimComponent component = update.component;
                ComponentTypeName type = component.GetType().Name;
                for (int i = 0; i < components[type].Count; i++)
                {
                    if (components[type][i].EntityID != component.EntityID)
                    {
                        continue;
                    }
                    components[type][i] = component;
                }
            }
        }
    }
}
