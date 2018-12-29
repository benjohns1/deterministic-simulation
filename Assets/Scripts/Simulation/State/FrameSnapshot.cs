using TickNumber = System.UInt32;

namespace Simulation.State
{
    public struct FrameSnapshot
    {
        public TickNumber Tick { get; }
        public TickNumber NextTick { get; }

        public Snapshot Snapshot { get; }
        public Snapshot NextSnapshot { get; }

        public FrameSnapshot(TickNumber tick, Snapshot snapshot, Snapshot nextSnapshot)
        {
            Tick = tick;
            NextTick = tick + 1;
            Snapshot = snapshot;
            NextSnapshot = nextSnapshot;
        }
    }
}
