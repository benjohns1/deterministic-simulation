using Simulation.ExternalEvent;
using UnityEngine;

namespace UserInput
{
    public enum FunctionAction { QuickSave, QuickLoad }

    public struct FunctionKeyEvent : IEvent
    {
        public KeyAction KeyAction { get; }
        public FunctionAction FunctionAction { get; }

        public FunctionKeyEvent(KeyAction keyAction, FunctionAction functionAction) : this()
        {
            KeyAction = keyAction;
            FunctionAction = functionAction;
        }
    }

    internal struct FunctionKey
    {
        public Key Key;
        public readonly FunctionAction FunctionAction;

        public FunctionKey(KeyCode key, FunctionAction functionAction)
        {
            Key = new Key(key);
            FunctionAction = functionAction;
        }
    }
}
