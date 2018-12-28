using Simulation.ExternalEvent;
using System.Collections.Generic;

namespace Simulation
{
    public abstract class SimSystem
    {
        public abstract void Tick(uint tick, SimState simState);

        public virtual void Preview(uint tick, SimState simState) { }

        public virtual IEnumerable<Subscription> Subscriptions => new List<Subscription>();
    }
}
