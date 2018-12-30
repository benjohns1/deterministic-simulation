using Simulation.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using EntityID = System.UInt64;
using TickNumber = System.UInt32;

namespace Simulation.State
{
    public class SimState
    {
        public TickNumber Tick { get; private set; } = 0;

        private Snapshot CurrentSnapshot => Snapshots[Tick];

        private EntityID NextEntityID = 0;

        // @TODO: implement as circular dictionary buffer (with max memory size)
        private readonly Dictionary<TickNumber, Snapshot> Snapshots = new Dictionary<TickNumber, Snapshot>();

        private Serializer Serializer = new Serializer();

        public SimState(Snapshot snapshot)
        {
            Snapshots.Add(0, (Snapshot)snapshot.Clone());

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

        public SimState()
        {
            Snapshots.Add(0, new Snapshot());
        }

        public EntityID CreateEntity()
        {
            EntityID EntityID = NextEntityID;
            NextEntityID++;
            return EntityID;
        }

        public void AddComponent<T>(T component) where T : SimComponent
        {
            CurrentSnapshot.AddComponent<T>(component);
        }

        public IEnumerable<T> GetComponents<T>() where T : SimComponent
        {
            return CurrentSnapshot.GetComponents<T>();
        }

        public IEnumerable<SimComponent> GetComponents()
        {
            return CurrentSnapshot.GetComponents();
        }

        public Snapshot GetSnapshot(TickNumber tick)
        {
            return (Snapshot)Snapshots[tick].Clone();
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
            if (!Snapshots.ContainsKey(tick) || !Snapshots.ContainsKey(tick+1))
            {
                UnityEngine.Debug.Log("Tick: " + tick);
            }
            return new FrameSnapshot(tick, Snapshots[tick], Snapshots[tick + 1]);
        }

        internal void Update(IEnumerable<ComponentUpdate> updates)
        {
            CurrentSnapshot.Update(updates);
        }
    }
}
