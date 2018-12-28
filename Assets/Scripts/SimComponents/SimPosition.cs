using UnityEngine;

namespace Simulation
{
    public class SimPosition : SimComponent
    {
        public Vector2 Position;

        public SimPosition(ulong entityID, Vector2 position) : base(entityID)
        {
            Position = position;
        }

        public override SimComponent InitPreview => new SimPosition(EntityID, Position);
    }
}