using Simulation;
using Simulation.ExternalEvent;
using UnityEngine;
using System.Collections.Generic;
using Simulation.State;
using System.Linq;

namespace SimSystems
{
    class CameraSystem : SimSystem
    {
        public override IEnumerable<Subscription> Subscriptions => new List<Subscription>()
        {
            new Subscription(typeof(UserInput.MovementKeyEvent)),
        };

        public override IEnumerable<ComponentUpdate> Tick(SimState state, IEnumerable<IEvent> events)
        {
            List<ComponentUpdate> updates = new List<ComponentUpdate>();

            IEnumerable<SimPosition> positions = state.GetComponents<SimPosition>();
            foreach (SimCamera camera in state.GetComponents<SimCamera>())
            {
                SimCamera newCamera = ApplyExternalEvents(camera, events, out bool updated);
                updates.Add(new ComponentUpdate(newCamera));

                SimPosition position = positions.First(p => p.EntityID == newCamera.EntityID);
                SimPosition newPosition = ApplyVelocity(position, newCamera.Velocity);
                updates.Add(new ComponentUpdate(newPosition));
            }

            return updates;
        }

        private static SimPosition ApplyVelocity(SimPosition simPosition, Vector2 velocity)
        {
            SimPosition newPosition = simPosition.Clone() as SimPosition;
            newPosition.Position = simPosition.Position + velocity;
            return newPosition;
        }

        private static SimCamera ApplyExternalEvents(SimCamera simCamera, IEnumerable<IEvent> events, out bool updated)
        {
            if (!events.Any())
            {
                updated = false;
                return simCamera;
            }

            SimCamera newCamera = simCamera.Clone() as SimCamera;
            foreach (IEvent @event in events)
            {
                if (@event.GetType() == typeof(UserInput.MovementKeyEvent))
                {
                    HandleMovementKey(newCamera, (UserInput.MovementKeyEvent)@event);
                }
            }
            updated = true;
            return newCamera;
        }

        private static void HandleMovementKey(SimCamera simCamera, UserInput.MovementKeyEvent movementKey)
        {
            bool pressed = movementKey.KeyAction == UserInput.KeyAction.Pressed;
            switch (movementKey.MovementAction)
            {
                case UserInput.MovementAction.Up:
                    simCamera.Up = pressed ? Vector2.up : Vector2.zero;
                    break;
                case UserInput.MovementAction.Down:
                    simCamera.Down = pressed ? Vector2.down : Vector2.zero;
                    break;
                case UserInput.MovementAction.Left:
                    simCamera.Left = pressed ? Vector2.left : Vector2.zero;
                    break;
                case UserInput.MovementAction.Right:
                    simCamera.Right = pressed ? Vector2.right : Vector2.zero;
                    break;
                case UserInput.MovementAction.Fast:
                    simCamera.CurrentSpeed = pressed ? simCamera.FastSpeed : simCamera.NormalSpeed;
                    break;
            }
            simCamera.Velocity = simCamera.CurrentSpeed * (simCamera.Up + simCamera.Down + simCamera.Left + simCamera.Right).normalized;
        }
    }
}
