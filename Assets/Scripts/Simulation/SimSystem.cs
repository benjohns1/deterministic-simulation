using Simulation.ExternalEvent;
using System.Collections.Generic;

namespace Simulation
{
    public abstract class SimSystem : ISimSystem
    {
        public abstract IEnumerable<State.ComponentUpdate> Tick(State.SimState state, IEnumerable<IEvent> events);

        public virtual IEnumerable<Subscription> Subscriptions => new List<Subscription>();
    }
}
