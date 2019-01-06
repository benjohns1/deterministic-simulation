using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TickNumber = System.UInt32;

namespace Simulation.ExternalEvent
{
    internal class EventStore
    {
        private readonly Dictionary<TickNumber, List<IEvent>> events = new Dictionary<TickNumber, List<IEvent>>();

        internal EventStore(Dictionary<TickNumber, List<IEvent>> events = null)
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
        internal void AddEvents(TickNumber currentTick, Dictionary<TickNumber, List<IEvent>> newEvents)
        {
            foreach (KeyValuePair<TickNumber, List<IEvent>> tickEvents in newEvents)
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
        internal void AddEvents(TickNumber tick, List<IEvent> newEvents)
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

        internal Dictionary<TickNumber, List<SerializableEvent>> GetSerializableEvents()
        {
            return events.ToDictionary(o => o.Key, o => o.Value.Select(e => new SerializableEvent(e)).ToList());
        }

        internal IEnumerable<IEvent> GetEvents(TickNumber tick)
        {
            if (events.ContainsKey(tick))
            {
                return events[tick];
            }
            return new List<IEvent>();
        }
    }
}
