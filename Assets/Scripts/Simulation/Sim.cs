using Simulation.ExternalEvent;
using System.Collections.Generic;

namespace Simulation
{
    public delegate void UpdateCallback(uint tick, bool stateUpdated, SimState State);

    public class Sim
    {
        public const byte MaxTicksPerUpdate = 8;
        private uint Tick = 0;
        private SimState State;
        private EventStore EventStore;
        private PubSub PubSub;
        private IEmitter EventEmitter;
        private readonly IEnumerable<SimSystem> Systems;
        private readonly UpdateCallback UpdateCallback;

        public Sim(IInitializer initializer, IEmitter eventEmitter, IEnumerable<SimSystem> systems, UpdateCallback callback)
        {
            State = initializer.InitialSimState;
            Dictionary<uint, List<IEvent>> events = initializer.InitialEvents;
            EventStore = new EventStore(events);
            PubSub = new PubSub();
            EventEmitter = eventEmitter;
            Systems = systems;
            UpdateCallback = callback;

            foreach (SimSystem system in Systems)
            {
                if (system.Subscriptions != null)
                {
                    PubSub.Subscribe(system.Subscriptions);
                }
            }
        }

        public void Update(uint tick)
        {
            // Retrieve external events
            List<IEvent> events = EventEmitter.Events;
            EventStore.AddEvents(tick, events);
            EventEmitter.Retrieved();

            // Update all systems until current tick is reached
            bool update = Tick != tick;
            byte tickCount;
            for (tickCount = 0; Tick < tick && tickCount < MaxTicksPerUpdate; tickCount++, Tick++)
            {
                // Apply events
                foreach (IEvent @event in EventStore.GetEvents(Tick))
                {
                    PubSub.Publish(@event);
                }

                // Run system logic
                foreach (SimSystem system in Systems)
                {
                    system.Tick(Tick, State);
                }
            }

            if (tickCount == MaxTicksPerUpdate)
            {
                // @TODO: callback for gamemanager to adjust sim speed & notify player
                UnityEngine.Debug.Log("max ticks per update");
            }

            // Apply preview of next tick for smooth value interpolation
            foreach (SimSystem system in Systems)
            {
                // @TODO: current preview doesn't apply events, which causes jumping, need to apply events to preview to avoid this
                system.Preview(Tick + 1, State);
            }

            UpdateCallback(tick, update, State);
        }
    }
}
