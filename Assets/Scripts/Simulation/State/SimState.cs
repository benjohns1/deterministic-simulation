using System;
using System.Collections.Generic;
using System.Linq;
using EntityID = System.UInt64;
using TickNumber = System.UInt32;

namespace Simulation.State
{
    public class SimState
    {
        public TickNumber Tick { get; private set; } = 0;

        public Snapshot InitialSnapshot { get; private set; }

        private Snapshot CurrentSnapshot => Snapshots[Tick];

        public EntityID NextEntityID { get; private set; } = 0;

        // @TODO: implement as circular dictionary buffer (with max memory size, persistently store older snapshots in temp if too large to store in memory)
        private readonly Dictionary<TickNumber, Snapshot> Snapshots = new Dictionary<TickNumber, Snapshot>();

        public SimState()
        {
            InitialSnapshot = new Snapshot();
            Snapshots.Add(0, (Snapshot)InitialSnapshot.Clone());
        }

        public SimState(Snapshot initialSnapshot)
        {
            InitialSnapshot = (Snapshot)initialSnapshot.Clone();
            Snapshots.Add(0, (Snapshot)initialSnapshot.Clone());

            EntityID largestEntityID = 0;
            foreach (SimComponent component in CurrentSnapshot.GetComponents())
            {
                if (component.EntityID > largestEntityID)
                {
                    largestEntityID = component.EntityID;
                }
            }
            NextEntityID = largestEntityID + 1;
        }

        public SimState(Snapshot initialSnapshot, Dictionary<TickNumber, Snapshot> snapshots, EntityID nextEntityID)
        {
            InitialSnapshot = (Snapshot)initialSnapshot.Clone();
            TickNumber lastTick = 0;
            Snapshots = snapshots.ToDictionary(s =>
            {
                if (s.Key > lastTick)
                {
                    lastTick = s.Key;
                }
                return s.Key;
            }, s => (Snapshot)s.Value.Clone());
            Tick = lastTick;
            NextEntityID = nextEntityID;
        }

        internal Dictionary<TickNumber, Snapshot> GetAllSnapshots()
        {
            return Snapshots.ToDictionary(s => s.Key, s => s.Value);
        }

        public TickNumber GetMaxTick()
        {
            return Snapshots.Aggregate<KeyValuePair<TickNumber, Snapshot>, TickNumber>(0, (max, next) => next.Key > max ? next.Key : max);
        }

        public EntityID CreateEntity()
        {
            EntityID EntityID = NextEntityID;
            NextEntityID++;
            return EntityID;
        }

        public void AddInitialComponent<T>(T component) where T : SimComponent
        {
            InitialSnapshot.AddComponent<T>(component);
            if (Snapshots.ContainsKey(0))
            {
                Snapshots[0].AddComponent<T>(component);
            }
        }

        public IEnumerable<T> GetComponents<T>() where T : SimComponent
        {
            return CurrentSnapshot.GetComponents<T>();
        }

        public IEnumerable<SimComponent> GetComponents()
        {
            return CurrentSnapshot.GetComponents();
        }

        public Snapshot GetSnapshot(TickNumber? tick = null)
        {
            TickNumber tickNumber = tick ?? (uint)(Snapshots.Count - 1);
            return (Snapshot)Snapshots[tickNumber].Clone();
        }

        internal void NewSnapshot(TickNumber tick)
        {
            if (tick <= Tick)
            {
                throw new ArgumentOutOfRangeException("New tick number must be greater than current tick:" + Tick);
            }
            Snapshot snapshot = (Snapshot)CurrentSnapshot.Clone();
            Snapshots.Add(tick, snapshot);
            Tick = tick;
        }

        internal FrameSnapshot GetFrameSnapshot(TickNumber tick)
        {
            return new FrameSnapshot(tick, Snapshots[tick], Snapshots[tick + 1]);
        }

        internal void Update(IEnumerable<ComponentUpdate> updates)
        {
            CurrentSnapshot.Update(updates);
        }
    }
}
