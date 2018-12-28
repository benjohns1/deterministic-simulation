using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation
{
    public class SimState
    {
        public struct ComponentState
        {
            public SimComponent Current { get; }
            public SimComponent Preview { get; }

            public ComponentState(SimComponent current, SimComponent preview) : this()
            {
                Current = current;
                Preview = preview;
            }
        }

        private ulong NextEntityID = 0;
        private readonly Dictionary<Type, List<ComponentState>> components = new Dictionary<Type, List<ComponentState>>();

        public ulong CreateNewEntity()
        {
            ulong EntityID = NextEntityID;
            NextEntityID++;
            return EntityID;
        }

        public void AddComponent(SimComponent component)
        {
            Type componentType = component.GetType();
            if (!components.ContainsKey(componentType))
            {
                components.Add(componentType, new List<ComponentState>());
            }
            components[componentType].Add(new ComponentState(component, component.InitPreview));
        }

        public IEnumerable<ComponentState> GetComponents<T>() where T : SimComponent
        {
            Type type = typeof(T);
            if (!components.ContainsKey(type))
            {
                return new List<ComponentState>();
            }
            return components[typeof(T)];
        }
    }
}
