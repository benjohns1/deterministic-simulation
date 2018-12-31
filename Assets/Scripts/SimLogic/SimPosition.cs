using Simulation;
using UnityEngine;
using EntityID = System.UInt64;

namespace SimLogic
{
    [System.Serializable]
    public class SimPosition : SimComponent
    {
        public Vector2 Position;

        public SimPosition(EntityID entityID, Vector2 position) : base(entityID)
        {
            Position = position;
        }

        public override object Clone() => new SimPosition(EntityID, Position);
    }
}