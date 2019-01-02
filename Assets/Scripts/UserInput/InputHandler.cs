using Simulation.ExternalEvent;
using UnityEngine;

namespace UserInput
{
    public struct Key
    {
        public readonly KeyCode Code;
        public readonly InputAction Action;
        public bool State;

        public Key(KeyCode key, InputAction action)
        {
            Code = key;
            Action = action;
            State = false;
        }
    }

    [System.Serializable]
    public struct KeyEvent : IEvent
    {
        public KeyInteraction KeyInteraction { get; }
        public InputAction Action { get; }

        public KeyEvent(KeyInteraction keyInteraction, InputAction action) : this()
        {
            KeyInteraction = keyInteraction;
            Action = action;
        }
    }

    public delegate void KeyEventHandler(KeyEvent keyEvent);

    public class InputHandler
    {
        public event KeyEventHandler OnKeyEvent;

        private IControlScheme Bindings = new DefaultBindings();

        public void Capture()
        {
            for (int i = 0; i < Bindings.Keys.Length; i++)
            {
                Key key = Bindings.Keys[i];
                bool keyPressed = Input.GetKey(key.Code);
                if (keyPressed == key.State)
                {
                    continue;
                }
                Bindings.Keys[i].State = keyPressed;
                OnKeyEvent?.Invoke(new KeyEvent(keyPressed ? KeyInteraction.Pressed : KeyInteraction.Released, key.Action));
            }
        }
    }
}