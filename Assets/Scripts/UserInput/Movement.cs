using Simulation.ExternalEvent;
using UnityEngine;

namespace UserInput
{
    public enum MovementAction { Up, Down, Left, Right, Fast }

    internal struct MovementKeyEvent : IEvent
    {
        public KeyAction KeyAction { get; }
        public MovementAction MovementAction { get; }

        public MovementKeyEvent(KeyAction keyAction, MovementAction movementAction) : this()
        {
            KeyAction = keyAction;
            MovementAction = movementAction;
        }
    }

    internal struct MovementKey
    {
        public Key Key;
        public readonly MovementAction MovementAction;

        public MovementKey(KeyCode key, MovementAction movementAction)
        {
            Key = new Key(key);
            MovementAction = movementAction;
        }
    }
}
