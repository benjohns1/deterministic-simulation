
using System.Collections.Generic;
using Simulation.ExternalEvent;
using UnityEngine;

namespace UserInput
{
    public enum KeyAction { Pressed, Released }

    internal struct Key
    {
        public readonly KeyCode Code;
        public bool State;

        public Key(KeyCode key)
        {
            Code = key;
            State = Input.GetKey(key);
        }
    }

    public delegate void FunctionKeyEventHandler(FunctionKeyEvent @event);

    public class InputHandler : IEmitter
    {
        List<IEvent> IEmitter.Events => SimEvents;

        private List<IEvent> SimEvents = new List<IEvent>();

        private readonly MovementKey[] MovementKeys = new MovementKey[]
        {
            new MovementKey(KeyCode.W, MovementAction.Up),
            new MovementKey(KeyCode.A, MovementAction.Left),
            new MovementKey(KeyCode.S, MovementAction.Down),
            new MovementKey(KeyCode.D, MovementAction.Right),
            new MovementKey(KeyCode.LeftShift, MovementAction.Fast)
        };

        private readonly FunctionKey[] FunctionKeys = new FunctionKey[]
        {
            new FunctionKey(KeyCode.F5, FunctionAction.QuickSave),
            new FunctionKey(KeyCode.F6, FunctionAction.QuickLoad),
        };

        public event FunctionKeyEventHandler OnFunctionKeyEvent;

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
                SimEvents.Add(new MovementKeyEvent(keyPressed ? KeyAction.Pressed : KeyAction.Released, key.MovementAction));
            }

            for (int i = 0; i < FunctionKeys.Length; i++)
            {
                FunctionKey key = FunctionKeys[i];
                bool keyPressed = Input.GetKey(key.Key.Code);
                if (keyPressed == key.Key.State)
                {
                    continue;
                }
                FunctionKeys[i].Key.State = keyPressed;
                OnFunctionKeyEvent?.Invoke(new FunctionKeyEvent(keyPressed ? KeyAction.Pressed : KeyAction.Released, key.FunctionAction));
            }
        }

        public void Retrieved()
        {
            SimEvents.Clear();
        }
    }
}