using UnityEngine;

namespace UserInput
{
    public enum KeyInteraction { Pressed, Released }

    public enum InputAction { Up, Down, Left, Right, Fast, QuickSave, QuickLoad, Replay, TestQuickSave }

    public class DefaultBindings : IControlScheme
    {
        Key[] IControlScheme.Keys => keys;
            
        private readonly Key[] keys = new Key[]
        {
            new Key(KeyCode.W, InputAction.Up),
            new Key(KeyCode.A, InputAction.Left),
            new Key(KeyCode.S, InputAction.Down),
            new Key(KeyCode.D, InputAction.Right),
            new Key(KeyCode.LeftShift, InputAction.Fast),
            new Key(KeyCode.F5, InputAction.QuickSave),
            new Key(KeyCode.F6, InputAction.QuickLoad),
            new Key(KeyCode.F7, InputAction.Replay),
            new Key(KeyCode.F9, InputAction.TestQuickSave),
        };
    }
}
