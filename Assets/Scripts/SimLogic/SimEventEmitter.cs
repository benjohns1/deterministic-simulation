
using Game.Camera;
using Simulation.ExternalEvent;
using System.Collections.Generic;
using System.Linq;
using UserInput;

namespace SimLogic
{
    /// <summary>
    /// Accepts input events from game and forwards to notify and persist in simulation
    /// </summary>
    public class SimEventEmitter : IEmitter
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

        private InputHandler InputHandler;
        private CameraSystem CameraSystem;

        private List<IEvent> SimEvents = new List<IEvent>();

        private bool enable;

        private InputAction[] PassthroughKeyActions = new InputAction[]
        {
            InputAction.Up,
            InputAction.Down,
            InputAction.Left,
            InputAction.Right
        };

        public SimEventEmitter(InputHandler inputHandler, CameraSystem cameraSystem)
        {
            InputHandler = inputHandler;
            CameraSystem = cameraSystem;
            SetupEvents();
        }

        private void SetupEvents(bool setup = true)
        {
            if (setup)
            {
                InputHandler.OnKeyEvent += InputHandler_OnKeyEvent;
                CameraSystem.OnPositionUpdated += CameraSystem_OnPositionUpdated;
            }
            else
            {
                InputHandler.OnKeyEvent -= InputHandler_OnKeyEvent;
                CameraSystem.OnPositionUpdated -= CameraSystem_OnPositionUpdated;
            }
        }

        ~SimEventEmitter()
        {
            SetupEvents(false);
        }

        private void CameraSystem_OnPositionUpdated(PositionUpdatedEvent positionUpdatedEvent)
        {
            SimEvents.Add(positionUpdatedEvent);
        }

        private void InputHandler_OnKeyEvent(KeyEvent keyEvent)
        {
            if (PassthroughKeyActions.Contains(keyEvent.Action))
            {
                SimEvents.Add(keyEvent);
            }
        }

        public void Retrieved()
        {
            SimEvents.Clear();
        }
    }
}