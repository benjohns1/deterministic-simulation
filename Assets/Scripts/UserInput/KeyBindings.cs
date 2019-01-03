using UnityEngine;

namespace UserInput
{
    public enum InteractionSubscription { PressAndRelease, Press, Release }

    public enum InputType { Key, Mouse }

    public enum InputAction { PrimaryInteract, SecondaryInteract, Multi, Up, Down, Left, Right, Fast, QuickSave, QuickLoad, Replay, TestQuickSave }

    public class DefaultBindings : IControlScheme
    {
        Binding[] IControlScheme.Keys => keys;
            
        private readonly Binding[] keys = new Binding[]
        {
            // Mouse bindings
            new Binding(KeyCode.Mouse0, InputAction.PrimaryInteract, InputType.Mouse, InteractionSubscription.Press),
            new Binding(KeyCode.Mouse1, InputAction.SecondaryInteract, InputType.Mouse, InteractionSubscription.Press),
            // Keyboard controls
            new Binding(KeyCode.W, InputAction.Up),
            new Binding(KeyCode.A, InputAction.Left),
            new Binding(KeyCode.S, InputAction.Down),
            new Binding(KeyCode.D, InputAction.Right),
            new Binding(KeyCode.LeftShift, InputAction.Fast),
            new Binding(KeyCode.LeftShift, InputAction.Multi),
            // Function keys
            new Binding(KeyCode.F5, InputAction.QuickSave, InteractionSubscription.Press),
            new Binding(KeyCode.F6, InputAction.QuickLoad, InteractionSubscription.Press),
            new Binding(KeyCode.F7, InputAction.Replay, InteractionSubscription.Press),
            new Binding(KeyCode.F9, InputAction.TestQuickSave, InteractionSubscription.Press),
        };
    }
}
