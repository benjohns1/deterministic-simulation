using System.Collections.Generic;
using System.Linq;
using SimLogic;
using Simulation;
using Simulation.ExternalEvent;
using Simulation.State;
using UnityEngine;
using UserInput;
using EntityID = System.UInt64;

namespace Game.UnitSelection
{
    [System.Serializable]
    public struct SelectionUpdatedEvent : IEvent
    {
        public enum SelectAction { Selected, Deselected, Cleared }

        public SelectAction Action { get;  }
        public EntityID EntityID { get; }
        public bool Multi { get; }

        public SelectionUpdatedEvent(SelectAction action, EntityID entityID = 0, bool multi = false) : this()
        {
            Action = action;
            EntityID = entityID;
            Multi = multi;
        }
    }

    public class SelectionSystem : IGameSystem
    {
        public delegate void SelectionUpdateHandler(SelectionUpdatedEvent selectionUpdatedEvent);

        public event SelectionUpdateHandler OnSelectionUpdated;

        private readonly Selections Selections = new Selections();
        
        private IInputHandler InputHandler;
        private IGameState GameState;
        private bool Multi = false;

        public SelectionSystem(IInputHandler inputHandler)
        {
            InputHandler = inputHandler;
            InputHandler.OnPointerEvent += InputHandler_OnPointerEvent;
            InputHandler.OnKeyEvent += InputHandler_OnKeyEvent;
        }

        public void SetGameState(IGameState gameState)
        {
            GameState = gameState;
        }

        private void InputHandler_OnKeyEvent(KeyEvent keyEvent)
        {
            if (keyEvent.Action == InputAction.Multi)
            {
                Multi = keyEvent.KeyInteraction == Interaction.Pressed;
            }
        }

        private void InputHandler_OnPointerEvent(PointerEvent pointerEvent)
        {
            if (pointerEvent.Action == InputAction.PrimaryInteract)
            {
                SelectActionAtScreenPosition(pointerEvent.Position, Multi);
            }
        }

        public bool IsSelected(SelectableComponent selectable)
        {
            return Selections.IsSelected(selectable);
        }

        public IEnumerable<SelectableComponent> GetSelections()
        {
            return Selections.GetSelections();
        }

        protected void SelectActionAtScreenPosition(Vector2 position, bool multi)
        {
            Vector2 worldPosition = UnityEngine.Camera.main.ScreenToWorldPoint(position);
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            SelectableComponent selection = hit.transform?.gameObject.GetComponent<SelectableComponent>();
            SelectionUpdatedEvent.SelectAction? action = Selections.UpdateSelections(selection, multi);
            if (action != null)
            {
                OnSelectionUpdated?.Invoke(new SelectionUpdatedEvent((SelectionUpdatedEvent.SelectAction)action, selection.GetComponent<SimEntityComponent>().EntityID, multi));
            }
        }

        public void Update(bool newTick) { }

        public void OnSimUpdated(FrameSnapshot frame, float interpolation, bool replay)
        {
            if (!replay)
            {
                // Don't update selections from sim during play, it's controlled via direct user input
                return;
            }

            // Do update selections during replay

            // @TODO: determine which entities were actually updated last tick and only loop through them (raise event when SimSystem makes update?)
            SimSelectable[] simSelections = frame.Snapshot.GetComponents<SimSelectable>().Where(s => s.Selected).ToArray();
            if (simSelections.Length <= 0)
            {
                Selections.Clear();
                return;
            }
            List<SelectableComponent> remainingSelections = Selections.GetSelections().ToList();
            for (int i = 0; i < simSelections.Length; i++)
            {
                SelectableComponent selection = GameState.GetGameObject(simSelections[i].EntityID).GameObject.GetComponent<SelectableComponent>();
                remainingSelections.Remove(selection);
                Selections.Select(selection, true);
            }
            foreach (SelectableComponent deselect in remainingSelections)
            {
                deselect.Deselect();
            }
        }
    }
}
