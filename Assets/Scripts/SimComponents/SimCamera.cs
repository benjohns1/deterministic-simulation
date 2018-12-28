using UnityEngine;

namespace Simulation
{
    public class SimCamera : SimComponent
    {
        public float Speed;
        public float FastSpeed;

        public SimCamera(ulong entityID, float speed, float fastSpeed) : base(entityID)
        {
            Speed = speed;
            FastSpeed = fastSpeed;
        }

        public override SimComponent InitPreview => new SimCamera(EntityID, Speed, FastSpeed);
    }
}