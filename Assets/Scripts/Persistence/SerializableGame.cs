using Simulation.ExternalEvent;
using Simulation.State;
using System;
using System.Collections.Generic;
using System.Linq;
using ArchetypeName = System.String;
using EntityID = System.UInt64;
using TickNumber = System.UInt32;

namespace Persistence
{
    [Serializable]
    public class SerializableGame
    {
        public Snapshot InitialSnapshot { get; }
        public EntityID NextEntityID { get; }
        public Dictionary<EntityID, ArchetypeName> Archetypes { get; }
        public Dictionary<TickNumber, List<SerializableEvent>> Events { get; }
        public Dictionary<TickNumber, Snapshot> SnapshotHistory { get; }
        public Dictionary<TickNumber, List<IEvent>> DeserializedEvents => Events.ToDictionary(e => e.Key, e => e.Value.Select(v => v.Event).ToList());

        public SerializableGame(Snapshot initialSnapshot, EntityID nextEntityID, Dictionary<EntityID, ArchetypeName> archetypes, Dictionary<TickNumber, List<SerializableEvent>> events, Dictionary<TickNumber, Snapshot> snapshotHistory)
        {
            InitialSnapshot = initialSnapshot;
            NextEntityID = nextEntityID;
            Archetypes = archetypes;
            Events = events;
            SnapshotHistory = snapshotHistory;
        }
    }
}