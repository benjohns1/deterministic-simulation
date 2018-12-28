using Simulation;
using Simulation.ExternalEvent;
using UnityEngine;
using System.Collections.Generic;
using static Simulation.SimState;
using System.Linq;

namespace SimAdapters
{
    class Camera : SimSystem
    {
        private Vector2 up = Vector2.zero;
        private Vector2 down = Vector2.zero;
        private Vector2 left = Vector2.zero;
        private Vector2 right = Vector2.zero;
        private Vector2 direction = Vector2.zero;
        private bool fast = false;

        public override void Tick(uint tick, SimState simState)
        {
            if (direction.sqrMagnitude == 0)
            {
                return;
            }

            // @TODO: get rid of this cast, use generics in GameState and SimComponent definitions
            IEnumerable<SimPosition> simPositions = simState.GetComponents<SimPosition>().Select(s => s.Current as SimPosition);
            foreach (SimCamera simCamera in simState.GetComponents<SimCamera>().Select(s => s.Current as SimCamera))
            {
                SimPosition simPosition = simPositions.First(p => p.EntityID == simCamera.EntityID);
                simPosition.Position = simPosition.Position + ((fast ? simCamera.FastSpeed : simCamera.Speed) * direction);
            }
        }

        public override void Preview(uint tick, SimState simState)
        {
            IEnumerable<ComponentState> simPositionStates = simState.GetComponents<SimPosition>().Where(s => s.Preview != null);
            foreach (SimCamera simCamera in simState.GetComponents<SimCamera>().Select(s => s.Current as SimCamera))
            {
                ComponentState simPosition = simPositionStates.First(p => p.Preview.EntityID == simCamera.EntityID);
                SimPosition current = simPosition.Current as SimPosition;
                SimPosition preview = simPosition.Preview as SimPosition;
                preview.Position = current.Position + ((fast ? simCamera.FastSpeed : simCamera.Speed) * direction);
            }
        }

        public override IEnumerable<Subscription> Subscriptions => new List<Subscription>()
        {
            new Subscription(typeof(UserInput.MovementKeyEvent), HandleMovementKey),
        };

        private void HandleMovementKey(IEvent @event)
        {
            UserInput.MovementKeyEvent movementKey = (UserInput.MovementKeyEvent)@event;
            bool pressed = movementKey.KeyAction == UserInput.KeyAction.Pressed;
            switch (movementKey.MovementAction)
            {
                case UserInput.MovementAction.Up:
                    up = pressed ? Vector2.up : Vector2.zero;
                    break;
                case UserInput.MovementAction.Down:
                    down = pressed ? Vector2.down : Vector2.zero;
                    break;
                case UserInput.MovementAction.Left:
                    left = pressed ? Vector2.left : Vector2.zero;
                    break;
                case UserInput.MovementAction.Right:
                    right = pressed ? Vector2.right : Vector2.zero;
                    break;
                case UserInput.MovementAction.Fast:
                    fast = pressed;
                    break;
            }
            direction = (up + down + left + right).normalized;
        }
    }
}
