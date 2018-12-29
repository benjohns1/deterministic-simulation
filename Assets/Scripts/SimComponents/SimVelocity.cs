using UnityEngine;
using EntityID = System.UInt64;

namespace Simulation
{
    [System.Serializable]
    public class SimVelocity : SimComponent
    {
        public Vector2 Velocity;
        public float MaxAcceleration;

        public SimVelocity(EntityID entityID,  Vector2 velocity, float maxAcceleration) : base(entityID)
        {
            Velocity = velocity;
            MaxAcceleration = maxAcceleration;
        }

        public override object Clone() => new SimVelocity(EntityID, Velocity, MaxAcceleration);
    }
}