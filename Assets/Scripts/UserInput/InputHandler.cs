
using System.Collections.Generic;
using Simulation.ExternalEvent;
using UnityEngine;

namespace UserInput
{
    public enum MovementAction { Up, Down, Left, Right, Fast }
    public enum KeyAction { Pressed, Released }

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

    public class InputHandler : IEmitter
    {
        List<IEvent> IEmitter.Events => Events;

        private List<IEvent> Events = new List<IEvent>();

        private struct Key
        {
            public readonly KeyCode Code;
            public bool State;

            public Key(KeyCode key)
            {
                Code = key;
                State = Input.GetKey(key);
            }
        }

        private struct MovementKey
        {
            public Key Key;
            public readonly MovementAction MovementAction;

            public MovementKey(KeyCode key, MovementAction movementAction)
            {
                Key = new Key(key);
                MovementAction = movementAction;
            }
        }

        private readonly MovementKey[] MovementKeys = new MovementKey[]
        {
            new MovementKey(KeyCode.W, MovementAction.Up),
            new MovementKey(KeyCode.A, MovementAction.Left),
            new MovementKey(KeyCode.S, MovementAction.Down),
            new MovementKey(KeyCode.D, MovementAction.Right),
            new MovementKey(KeyCode.LeftShift, MovementAction.Fast)
        };

        public void Capture()
        {
            for (int i = 0; i < MovementKeys.Length; i++)
            {
                MovementKey key = MovementKeys[i];
                bool keyPressed = Input.GetKey(key.Key.Code);
                if (keyPressed == key.Key.State)
                {
                    continue;
                }
                MovementKeys[i].Key.State = keyPressed;
                Events.Add(new MovementKeyEvent(keyPressed ? KeyAction.Pressed : KeyAction.Released, key.MovementAction));
            }
        }

        public void Retrieved()
        {
            Events.Clear();
        }
    }
}