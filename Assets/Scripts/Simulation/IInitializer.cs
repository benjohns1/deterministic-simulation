using Simulation.ExternalEvent;
using System;
using System.Collections.Generic;

namespace Simulation
{
    public interface IInitializer
    {
        SimState InitialSimState { get; }
        Dictionary<uint, List<IEvent>> InitialEvents { get; }
    }
}
