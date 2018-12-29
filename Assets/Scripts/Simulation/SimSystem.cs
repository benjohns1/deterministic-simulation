using Simulation.ExternalEvent;
using System.Collections.Generic;
using System.Linq;

namespace Simulation
{
    public abstract class SimSystem
    {
        public abstract IEnumerable<State.ComponentUpdate> Tick(State.SimState state, IEnumerable<IEvent> events);

        public virtual IEnumerable<Subscription> Subscriptions => new List<Subscription>();
    }
}
