using Simulation;
using UnityEngine;
using EntityID = System.UInt64;

namespace SimLogic
{
    [System.Serializable]
    public class SimCamera : SimComponent
    {
        public bool Enabled;
        public Vector3 Position;

        public SimCamera(EntityID entityID, Vector3 position, bool enabled) : base(entityID)
        {
            Position = position;
            Enabled = enabled;
        }

        public override object Clone() => new SimCamera(EntityID, Position, Enabled);
    }
}