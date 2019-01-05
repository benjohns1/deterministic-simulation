using SimLogic;
using Simulation.State;
using System.Linq;
using UnityEngine;

namespace Game.Movement
{
    public class MovementSystem : IGameSystem
    {
        private IGameState GameState;

        public void OnSimUpdated(FrameSnapshot frame, float interpolation, bool replay)
        {
            // @TODO: determine which entities were actually updated last tick and only loop through them (raise event when SimSystem makes update?)
            SimPosition[] positions = frame.Snapshot.GetComponents<SimPosition>().ToArray();
            SimPosition[] nextPositions = frame.NextSnapshot.GetComponents<SimPosition>().ToArray();
            for (int i = 0; i < positions.Length; i++)
            {
                // @TODO: more efficiently get GameObject, related SimComponents and MonoBehaviours
                GameObject go = GameState.GetGameObject(positions[i].EntityID).GameObject;
                Transform transform = go.GetComponent<Transform>();

                Vector2 newPosition = Vector2.Lerp(positions[i].Position, nextPositions[i].Position, interpolation);
                transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            }
        }

        public void SetGameState(IGameState gameState)
        {
            GameState = gameState;
        }

        public void Update(bool newTick) { }
    }
}
