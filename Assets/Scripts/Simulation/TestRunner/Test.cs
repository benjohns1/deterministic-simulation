using Simulation.ExternalEvent;
using Simulation.State;
using System.Collections.Generic;
using TickNumber = System.UInt32;

namespace Simulation.TestRunner
{
    public class Test
    {
        uint AssertEvery;
        TickNumber MaxTick;
        SimState ExpectedState;
        Sim Sim;
        ILogger Logger;

        public Test(ILogger logger, SimState simState, IEnumerable<ISimSystem> systems, Dictionary<TickNumber, List<IEvent>> events, uint assertEvery = 1)
        {
            MaxTick = simState.GetMaxTick();
            AssertEvery = assertEvery;
            ExpectedState = simState;
            SimState initialState = new SimState(simState.InitialSnapshot);
            Logger = logger;
            Sim = new Sim(logger, initialState, null, systems, null, events);
        }

        public void Run()
        {
            for (TickNumber tick = 0; tick < MaxTick; tick++)
            {
                if (tick % AssertEvery == 0)
                {
                    Snapshot expected = (Snapshot)ExpectedState.GetSnapshot(tick).Clone();
                    Sim.RunTick(tick);
                    Snapshot actual = (Snapshot)Sim.State.GetSnapshot(tick).Clone();
                    expected.AssertEquals(actual);
                }
                else
                {
                    Sim.RunTick(tick);
                }
            }
        }
    }
}
