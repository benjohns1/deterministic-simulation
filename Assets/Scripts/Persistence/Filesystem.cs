using Simulation;
using Simulation.ExternalEvent;
using Simulation.Serialization;
using Simulation.State;
using System.Collections.Generic;
using System.IO;
using TickNumber = System.UInt32;
using EntityID = System.UInt64;

namespace Persistence
{
    public class Filesystem : IPersistence
    {
        Serializer Serializer;

        public Filesystem(Serializer serializer)
        {
            Serializer = serializer;
        }

        public void SaveGame(string Filename, TickNumber currentTick, Sim sim, Game.IGameState gameState)
        {
            (new FileInfo(Filename)).Directory.Create();
            using (FileStream fileStream = new FileStream(Filename, FileMode.Create, FileAccess.Write))
            {
                Snapshot initialSnapshot = sim.State.InitialSnapshot;
                EntityID nextEntityID = sim.State.NextEntityID;
                Dictionary<TickNumber, List<SerializableEvent>> events = sim.GetSerializableEvents();
                Dictionary<ulong, string> archetypes = gameState.GetEntityArchetypes();
                Dictionary<TickNumber, Snapshot> snapshotHistory = sim.State.GetAllSnapshots();
                SerializableGame data = new SerializableGame(initialSnapshot, nextEntityID, archetypes, events, snapshotHistory);
                Serializer.Serialize(fileStream, data);
            }
        }

        public SerializableGame LoadGame(string Filename)
        {
            using (FileStream stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                return (SerializableGame)Serializer.Deserialize(stream);
            }
        }
    }
}
