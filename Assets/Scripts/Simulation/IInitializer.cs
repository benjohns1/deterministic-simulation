using Simulation.ExternalEvent;
using System.Collections.Generic;
using TickNumber = System.UInt32;

namespace Simulation
{
    public interface IInitializer
    {
        State.SimState InitialSimState { get; }
        Dictionary<TickNumber, List<IEvent>> InitialEvents { get; }
    }
}
