namespace Simulation
{
    public abstract class SimComponent
    {
        public readonly ulong EntityID;
        public virtual SimComponent InitPreview => null;

        protected SimComponent(ulong entityID)
        {
            EntityID = entityID;
        }
    }
}