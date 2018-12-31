
using GameLogic;
using Simulation.ExternalEvent;
using System.Collections.Generic;
using System.Linq;
using UserInput;
using static GameLogic.CameraSystem;

namespace SimLogic
{
    /// <summary>
    /// Accepts input events from game and forwards to notify and persist in simulation
    /// </summary>
    public class SimEventEmitter : IEmitter
    {
        List<IEvent> IEmitter.Events => SimEvents;

        private InputHandler InputHandler;
        private CameraSystem CameraSystem;

        private List<IEvent> SimEvents = new List<IEvent>();

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
            InputHandler.OnKeyEvent += InputHandler_OnKeyEvent;

            CameraSystem = cameraSystem;
            CameraSystem.OnPositionUpdated += CameraSystem_OnPositionUpdated;
        }

        ~SimEventEmitter()
        {
            InputHandler.OnKeyEvent -= InputHandler_OnKeyEvent;
            CameraSystem.OnPositionUpdated -= CameraSystem_OnPositionUpdated;
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