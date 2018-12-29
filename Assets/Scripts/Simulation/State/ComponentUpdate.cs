namespace Simulation.State
{
    public struct ComponentUpdate
    {
        public readonly SimComponent component;

        public ComponentUpdate(SimComponent component)
        {
            this.component = component;
        }
    }
}
