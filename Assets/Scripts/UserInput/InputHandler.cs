using Simulation.ExternalEvent;
using System.Linq;
using UnityEngine;

namespace UserInput
{
    public enum Interaction { Pressed, Released }

    public struct Binding
    {
        public readonly KeyCode Code;
        public readonly InputAction Action;
        public readonly InputType Type;
        public readonly InteractionSubscription InteractionSubscription;
        public bool State;

        public Binding(KeyCode key, InputAction action, InteractionSubscription subscription = InteractionSubscription.PressAndRelease)
        {
            Code = key;
            Action = action;
            Type = InputType.Key;
            InteractionSubscription = subscription;
            State = false;
        }

        public Binding(KeyCode key, InputAction action, InputType type, InteractionSubscription subscription = InteractionSubscription.PressAndRelease)
        {
            Code = key;
            Action = action;
            Type = type;
            InteractionSubscription = subscription;
            State = false;
        }
    }

    [System.Serializable]
    public struct KeyEvent : IEvent
    {
        public Interaction KeyInteraction { get; }
        public InputAction Action { get; }

        public KeyEvent(Interaction keyInteraction, InputAction action) : this()
        {
            KeyInteraction = keyInteraction;
            Action = action;
        }
    }

    public struct PointerEvent : IEvent
    {
        public Interaction KeyInteraction { get; }
        public InputAction Action { get; }
        public Vector3 Position { get; }

        public PointerEvent(Interaction keyInteraction, InputAction action, Vector3 position) : this()
        {
            KeyInteraction = keyInteraction;
            Action = action;
            Position = position;
        }
    }

    public delegate void KeyEventHandler(KeyEvent keyEvent);
    public delegate void PointerEventHandler(PointerEvent pointerEvent);

    public class InputHandler
    {
        public event KeyEventHandler OnKeyEvent;
        public event PointerEventHandler OnPointerEvent;

        private IControlScheme Bindings = new DefaultBindings();

        private static readonly InteractionSubscription[] Release = { InteractionSubscription.Release, InteractionSubscription.PressAndRelease };
        private static readonly InteractionSubscription[] Press = { InteractionSubscription.Press, InteractionSubscription.PressAndRelease };

        public void Capture()
        {
            for (int i = 0; i < Bindings.Keys.Length; i++)
            {
                Binding key = Bindings.Keys[i];
                bool keyPressed = Input.GetKey(key.Code);
                if (keyPressed == key.State)
                {
                    continue;
                }
                Bindings.Keys[i].State = keyPressed;
                if (!keyPressed && !Release.Contains(key.InteractionSubscription))
                {
                    continue;
                }
                if (keyPressed && !Press.Contains(key.InteractionSubscription))
                {
                    continue;
                }
                Interaction interaction = keyPressed ? Interaction.Pressed : Interaction.Released;
                switch (key.Type)
                {
                    case InputType.Key:
                        OnKeyEvent?.Invoke(new KeyEvent(interaction, key.Action));
                        break;
                    case InputType.Mouse:
                        OnPointerEvent?.Invoke(new PointerEvent(interaction, key.Action, Input.mousePosition));
                        break;
                    default:
                        throw new System.Exception("Unhandled input binding type: " + key.Type);
                }
            }
        }
    }
}