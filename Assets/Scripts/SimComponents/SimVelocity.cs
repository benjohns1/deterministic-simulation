using UnityEngine;

namespace Simulation
{
    public class SimVelocity : SimComponent
    {
        public Vector2 Velocity;
        public float MaxAcceleration;

        public SimVelocity(ulong entityID,  Vector2 velocity, float maxAcceleration) : base(entityID)
        {
            Velocity = velocity;
            MaxAcceleration = maxAcceleration;
        }

        public override SimComponent InitPreview => new SimVelocity(EntityID, Velocity, MaxAcceleration);
    }
}