using SimLogic;
using Simulation.ExternalEvent;
using Simulation.State;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UserInput;
using EntityID = System.UInt64;

namespace Game.UnitSelection
{
    public enum SelectAction { Selected, Deselected, Cleared }

    [System.Serializable]
    public struct SelectionUpdatedEvent : IEvent
    {
        public SelectAction Action { get; }
        public EntityID EntityID { get; }
        public bool Multi { get; }

        public SelectionUpdatedEvent(SelectAction action, EntityID entityID = 0, bool multi = false) : this()
        {
            Action = action;
            EntityID = entityID;
            Multi = multi;
        }
    }

    public delegate void SelectionUpdateHandler(SelectionUpdatedEvent selectionUpdatedEvent);

    public class SelectionSystem : IGameSystem
    {
        public event SelectionUpdateHandler OnSelectionUpdated;

        private readonly List<SelectableComponent> Selected = new List<SelectableComponent>();

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
            return selectable == null ? false : Selected.Contains(selectable);
        }

        protected void SelectActionAtScreenPosition(Vector2 position, bool multi)
        {
            Vector2 worldPosition = UnityEngine.Camera.main.ScreenToWorldPoint(position);
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            SelectableComponent selection = hit.transform?.gameObject.GetComponent<SelectableComponent>();
            UpdateSelections(selection, multi);
        }

        protected void UpdateSelections(SelectableComponent selection, bool multi)
        {
            if (IsSelected(selection) && (multi || Selected.Any()))
            {
                Deselect(selection, multi);
            }
            else
            {
                Select(selection, multi);
            }
        }

        protected void Deselect(SelectableComponent selection, bool multi)
        {
            if (!multi)
            {
                Clear();
            }
            if (!IsSelected(selection))
            {
                return;
            }
            Selected.Remove(selection);
            selection.Deselect();
            InvokeEvent(SelectAction.Deselected, selection, multi);
        }

        public void Select(SelectableComponent selection, bool multi)
        {
            if (!multi)
            {
                Clear(Selected.Count == 1 ? selection : null);
            }
            if (selection == null)
            {
                return;
            }
            if (IsSelected(selection))
            {
                return;
            }
            Selected.Add(selection);
            selection.Select();
            InvokeEvent(SelectAction.Selected, selection, multi);
        }

        public void Clear(SelectableComponent ignore = null)
        {
            if (Selected.Count <= 0)
            {
                return;
            }
            foreach (SelectableComponent selected in Selected)
            {
                if (selected != ignore)
                {
                    selected.Deselect();
                }
            }
            Selected.Clear();
            InvokeEvent(SelectAction.Cleared);
        }

        private void InvokeEvent(SelectAction action, SelectableComponent selection = null, bool multi = false)
        {
            OnSelectionUpdated?.Invoke(new SelectionUpdatedEvent(action, selection?.GetComponent<Simulation.SimEntityComponent>().EntityID ?? 0, multi));
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
                Clear();
                return;
            }
            List<SelectableComponent> remainingSelections = Selected.ToList();
            for (int i = 0; i < simSelections.Length; i++)
            {
                SelectableComponent selection = GameState.GetGameObject(simSelections[i].EntityID).GameObject.GetComponent<SelectableComponent>();
                remainingSelections.Remove(selection);
                Select(selection, true);
            }
            foreach (SelectableComponent deselect in remainingSelections)
            {
                deselect.Deselect();
            }
        }
    }
}
