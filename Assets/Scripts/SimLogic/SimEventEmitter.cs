
using Game.Camera;
using Game.UnitSelection;
using Simulation;
using Simulation.ExternalEvent;
using System.Collections.Generic;
using System.Linq;
using UserInput;

namespace SimLogic
{
    public interface ISimEventEmitter : IEmitter
    {
        bool Enable { get; set; }
    }

    /// <summary>
    /// Accepts input events from game and forwards to notify and persist in simulation
    /// </summary>
    public class SimEventEmitter : ISimEventEmitter, System.IDisposable
    {
        List<IEvent> IEmitter.Events => SimEvents;

        public bool Enable {
            get => enable;
            set
            {
                SetupEvents(value);
                enable = value;
            }
        }

        private IInputHandler InputHandler;
        private CameraSystem CameraSystem;
        private SelectionSystem SelectionSystem;
        private readonly ILogger Logger;

        private List<IEvent> SimEvents = new List<IEvent>();

        private bool enable;

        private InputAction[] PassthroughKeyActions = new InputAction[]
        {
            InputAction.Up,
            InputAction.Down,
            InputAction.Left,
            InputAction.Right
        };

        public SimEventEmitter(ILogger logger, IInputHandler inputHandler, CameraSystem cameraSystem, SelectionSystem selectionSystem)
        {
            Logger = logger;
            InputHandler = inputHandler;
            CameraSystem = cameraSystem;
            SelectionSystem = selectionSystem;
            SetupEvents();
        }

        private void SetupEvents(bool setup = true)
        {
            if (setup)
            {
                InputHandler.OnKeyEvent += InputHandler_OnKeyEvent;
                CameraSystem.OnPositionUpdated += CameraSystem_OnPositionUpdated;
                SelectionSystem.OnSelectionUpdated += SelectionSystem_OnSelectionUpdated;
            }
            else
            {
                InputHandler.OnKeyEvent -= InputHandler_OnKeyEvent;
                CameraSystem.OnPositionUpdated -= CameraSystem_OnPositionUpdated;
                SelectionSystem.OnSelectionUpdated -= SelectionSystem_OnSelectionUpdated;
            }
        }

        private void SelectionSystem_OnSelectionUpdated(SelectionUpdatedEvent selectionUpdatedEvent)
        {
            AddEvent(selectionUpdatedEvent);
        }

        private void CameraSystem_OnPositionUpdated(PositionUpdatedEvent positionUpdatedEvent)
        {
            AddEvent(positionUpdatedEvent);
        }

        private void InputHandler_OnKeyEvent(KeyEvent keyEvent)
        {
            if (PassthroughKeyActions.Contains(keyEvent.Action))
            {
                AddEvent(keyEvent);
            }
        }

        private void AddEvent(IEvent @event)
        {
            Logger.Debug("Sim event added: " + @event);
            SimEvents.Add(@event);
        }

        public void Retrieved()
        {
            SimEvents.Clear();
        }

        public void Dispose()
        {
            SetupEvents(false);
        }
    }
}