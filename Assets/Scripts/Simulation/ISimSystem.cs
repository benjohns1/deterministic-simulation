using Simulation.ExternalEvent;
using System.Collections.Generic;

namespace Simulation
{
    public interface ISimSystem
    {
        IEnumerable<State.ComponentUpdate> Tick(State.SimState state, IEnumerable<IEvent> events);

        IEnumerable<Subscription> Subscriptions { get; }
    }
}
