using Simulation.ExternalEvent;
using Simulation.State;
using System;
using System.Collections.Generic;
using System.Linq;
using TickNumber = System.UInt32;

namespace Simulation
{
    public delegate void UpdateCallback(FrameSnapshot frame);

    public class Sim
    {
        public const byte MaxTicksPerUpdate = 8;
        public SimState State { get; private set; }
        private EventStore EventStore;
        private IEmitter EventEmitter;
        private readonly IEnumerable<SimSystem> Systems;
        private readonly IEnumerable<SimSystem> UntickableSystems;
        private readonly UpdateCallback UpdateCallback;
        private readonly ILogger Logger;

        public Sim(ILogger logger, SimState initialState, IEmitter eventEmitter, IEnumerable<SimSystem> systems, UpdateCallback callback, Dictionary<TickNumber, List<IEvent>> events = null)
        {
            State = initialState;
            EventStore = new EventStore(events);
            EventEmitter = eventEmitter;
            Systems = systems;
            UpdateCallback = callback;
            Logger = logger;
        }

        public Dictionary<TickNumber, List<SerializableEvent>> GetSerializableEvents()
        {
            return EventStore.GetSerializableEvents();
        }

        public void PlayTick(TickNumber tick)
        {
            // Retrieve external events
            List<IEvent> events = EventEmitter.Events;
            EventStore.AddEvents(tick + 2, events);
            EventEmitter.Retrieved();

            RunTick(tick);
        }

        public void RunTick(TickNumber tick)
        {
            // Run all systems until this tick and next tick have been updated
            byte tickCount;
            TickNumber updateTick = State.Tick;
            for (tickCount = 0; updateTick <= tick && tickCount < MaxTicksPerUpdate; tickCount++)
            {
                updateTick++;
                State.NewSnapshot(updateTick);

                // Get events that occurred this tick
                IEnumerable<IEvent> tickEvents = EventStore.GetEvents(updateTick);

                // Run system logic
                foreach (SimSystem system in Systems)
                {
                    IEnumerable<IEvent> systemEvents = tickEvents.Where(e => system.Subscriptions.Any(s => s.EventType == e.GetType()));
                    State.Update(system.Tick(State, systemEvents));
                }

                EventStore.SetApplied(updateTick);
            }

            bool update = tickCount > 0;

            if (tickCount == MaxTicksPerUpdate)
            {
                // @TODO: callback for gamemanager to adjust sim speed & notify player
                Logger.Debug("Max ticks per update");
            }

            UpdateCallback?.Invoke(State.GetFrameSnapshot(tick));
        }
    }
}
