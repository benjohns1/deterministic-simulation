using Simulation;
using UnityEngine;
using static Simulation.SimState;

class Updater
{
    private readonly GameState GameState;

    public Updater(GameState gameState)
    {
        GameState = gameState;
    }

    public void UpdateGame(SimState simState, float interpolate)
    {
        // @TODO: determine which entities were actually updated last tick and only loop through them (raise event when SimSystem makes update?)

        foreach (ComponentState state in simState.GetComponents<SimPosition>())
        {
            // @TODO: remove these casts for efficiency
            SimPosition simPosition = state.Current as SimPosition;

            // @TODO: more efficiently get GameObject, related SimComponents and MonoBehaviours
            GameObject go = GameState.GetGameObject(simPosition.EntityID);
            Transform transform = go.GetComponent<Transform>();

            Vector2 newPosition = Vector2.zero;
            if (state.Preview != null)
            {
                SimPosition preview = state.Preview as SimPosition;
                newPosition = Vector2.Lerp(simPosition.Position, preview.Position, interpolate);
            }
            else
            {
                newPosition = simPosition.Position;
            }
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        }
    }
}