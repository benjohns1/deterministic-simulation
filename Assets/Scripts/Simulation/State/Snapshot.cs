using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public void AssertEquals(Snapshot other)
        {
            if (components.Count != other.components.Count)
            {
                throw new Exception("Component count mismatch");
            }

            foreach (ComponentTypeName name in components.Keys)
            {
                if (!other.components.ContainsKey(name))
                {
                    throw new Exception("Component not found: " + name);
                }

                for (int i = 0; i < components[name].Count; i++)
                {
                    SimComponent thisComponent = components[name][i];
                    SimComponent otherComponent = other.components[name][i];
                    Type componentType = thisComponent.GetType();

                    // Compare SimComponent property values
                    PropertyInfo[] props = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (PropertyInfo prop in props)
                    {
                        object val1 = prop.GetValue(thisComponent);
                        object val2 = prop.GetValue(otherComponent);
                        if (!Equals(val1, val2))
                        {
                            throw new Exception(string.Format("Component {0} property '{1}' value not equal. Expected: {2}, actual: {3}", name, prop.Name, val1, val2));
                        }
                    }

                    // Compare SimComponent field values
                    FieldInfo[] fields = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (FieldInfo field in fields)
                    {
                        object val1 = field.GetValue(thisComponent);
                        object val2 = field.GetValue(otherComponent);
                        if (!Equals(val1, val2))
                        {
                            throw new Exception(string.Format("Component {0} field '{1}' value not equal. Expected: {2}, actual: {3}", name, field.Name, val1, val2));
                        }
                    }
                }
            }
        }
    }
}
