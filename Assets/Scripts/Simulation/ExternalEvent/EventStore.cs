using System;
using System.Collections.Generic;

namespace Simulation.ExternalEvent
{
    internal class EventStore
    {
        private readonly Dictionary<uint, List<IEvent>> events = new Dictionary<uint, List<IEvent>>();

        internal EventStore(Dictionary<uint, List<IEvent>> events = null)
        {
            if (events != null && events.Count > 0)
            {
                AddEvents(0, events);
            }
        }

        /// <summary>
        /// Add dictionary of events for multiple ticks
        /// </summary>
        /// <param name="currentTick"></param>
        /// <param name="newEvents"></param>
        internal void AddEvents(uint currentTick, Dictionary<uint, List<IEvent>> newEvents)
        {
            foreach (KeyValuePair<uint, List<IEvent>> tickEvents in newEvents)
            {
                if (currentTick >= tickEvents.Key)
                {
                    throw new System.Exception(typeof(IEvent) + " tick " + tickEvents.Key + " is before or equal to current tick" + currentTick);
                }

                AddEvents(tickEvents.Key, tickEvents.Value);
            }
        }

        /// <summary>
        /// Add events to a single tick
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="newEvents"></param>
        internal void AddEvents(uint tick, List<IEvent> newEvents)
        {
            if (newEvents == null || newEvents.Count <= 0)
            {
                return;
            }

            if (!events.ContainsKey(tick))
            {
                events.Add(tick, new List<IEvent>(newEvents));
            }
            else
            {
                events[tick].AddRange(newEvents);
            }
        }

        internal IEnumerable<IEvent> GetEvents(uint tick)
        {
            if (events.ContainsKey(tick))
            {
                return events[tick];
            }
            return new List<IEvent>();
        }
    }
}
