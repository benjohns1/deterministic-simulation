using Simulation;
using TickNumber = System.UInt32;

namespace Persistence
{
    interface IPersistence
    {
        void SaveGame(string Filename, TickNumber currentTick, Sim sim, Game.IGameState gameState);
        SerializableGame LoadGame(string Filename);
    }
}
