using UnityEngine;

namespace Simulation
{
    public interface IComponentInitializer
    {
        void InitSimComponent(MonoBehaviour monoBehaviour, SimComponent simComponent);
        void UpdateMonoBehaviour(MonoBehaviour monoBehaviour, SimComponent simComponent);
    }
}
