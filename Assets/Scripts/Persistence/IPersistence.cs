using Simulation;
using TickNumber = System.UInt32;

namespace Persistence
{
    interface IPersistence
    {
        void SaveGame(string Filename, TickNumber currentTick, Sim sim, GameState gameState);
        SerializableGame LoadGame(string Filename);
    }
}
