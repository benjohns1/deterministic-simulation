using Simulation;
using Simulation.State;
using System.Linq;
using UnityEngine;

class Updater
{
    private readonly GameState GameState;

    public Updater(GameState gameState)
    {
        GameState = gameState;
    }

    public void UpdateGame(FrameSnapshot frame, float interpolate)
    {
        // @TODO: determine which entities were actually updated last tick and only loop through them (raise event when SimSystem makes update?)
        SimPosition[] positions = frame.Snapshot.GetComponents<SimPosition>().ToArray();
        SimPosition[] nextPositions = frame.NextSnapshot.GetComponents<SimPosition>().ToArray();
        for (int i = 0; i < positions.Length; i++)
        {
            // @TODO: more efficiently get GameObject, related SimComponents and MonoBehaviours
            GameObject go = GameState.GetGameObject(positions[i].EntityID);
            Transform transform = go.GetComponent<Transform>();

            Vector2 newPosition = Vector2.zero;
            newPosition = Vector2.Lerp(positions[i].Position, nextPositions[i].Position, interpolate);
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        }
    }
}