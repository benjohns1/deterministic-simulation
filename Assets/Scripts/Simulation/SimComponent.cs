using System;
using EntityID = System.UInt64;

namespace Simulation
{
    [Serializable]
    public abstract class SimComponent : ICloneable
    {
        public readonly EntityID EntityID;

        protected SimComponent(EntityID entityID)
        {
            EntityID = entityID;
        }

        public abstract object Clone();
    }
}