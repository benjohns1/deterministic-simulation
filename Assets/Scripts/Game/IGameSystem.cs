using Simulation.State;

namespace Game
{
    interface IGameSystem
    {
        void Update(bool newTick);
        void OnSimUpdated(FrameSnapshot frame, float interpolation, bool replay);
    }
}
