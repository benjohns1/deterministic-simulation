using SimLogic;
using Simulation.ExternalEvent;
using Simulation.State;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UserInput;

namespace Game.Camera
{
    [System.Serializable]
    public struct PositionUpdatedEvent : IEvent
    {
        public Vector3 Position { get; }

        public PositionUpdatedEvent(Vector3 position) : this()
        {
            Position = position;
        }
    }

    public class CameraSystem : IGameSystem
    {
        InputHandler InputHandler;
        GameState GameState;

        public delegate void PositionUpdateHandler(PositionUpdatedEvent positionUpdatedEvent);

        public event PositionUpdateHandler OnPositionUpdated;

        private readonly InputAction[] InputActions = new InputAction[]
        {
            InputAction.Up,
            InputAction.Down,
            InputAction.Left,
            InputAction.Right,
            InputAction.Fast
        };

        private List<CameraComponent> Cameras = new List<CameraComponent>();

        public CameraSystem(InputHandler inputHandler)
        {
            InputHandler = inputHandler;
            InputHandler.OnKeyEvent += InputHandler_OnKeyEvent;
        }

        public void SetGameState(GameState gameState)
        {
            GameState = gameState;
        }

        public void Register(CameraComponent camera)
        {
            Cameras.Add(camera);
        }

        public void Unregister(CameraComponent camera)
        {
            Cameras.Remove(camera);
        }

        private void InputHandler_OnKeyEvent(KeyEvent keyEvent)
        {
            if (!InputActions.Contains(keyEvent.Action))
            {
                return;
            }

            foreach (CameraComponent camera in Cameras)
            {
                OnMovementEvent(camera, keyEvent);
            }
        }

        private void OnMovementEvent(CameraComponent camera, KeyEvent keyEvent)
        {
            bool pressed = keyEvent.KeyInteraction == KeyInteraction.Pressed;
            switch (keyEvent.Action)
            {
                case InputAction.Up:
                    camera.Up = pressed ? Vector2.up : Vector2.zero;
                    break;
                case InputAction.Down:
                    camera.Down = pressed ? Vector2.down : Vector2.zero;
                    break;
                case InputAction.Left:
                    camera.Left = pressed ? Vector2.left : Vector2.zero;
                    break;
                case InputAction.Right:
                    camera.Right = pressed ? Vector2.right : Vector2.zero;
                    break;
                case InputAction.Fast:
                    camera.CurrentSpeed = pressed ? camera.FastSpeed : camera.NormalSpeed;
                    break;
                default:
                    throw new System.Exception("Unhandled camera input key event");
            }
            camera.Velocity = camera.CurrentSpeed * (camera.Up + camera.Down + camera.Left + camera.Right).normalized;
        }

        public void Update(bool newTick)
        {
            foreach (CameraComponent camera in Cameras)
            {
                // Control camera position in real game time
                Vector3 position = camera.transform.position;
                if (camera.Velocity.sqrMagnitude != 0)
                {
                    camera.transform.position = position + camera.Velocity * Time.deltaTime;
                }

                // Send updated camera position to simulation every tick
                if (newTick && camera.LastTickPosition != position)
                {
                    camera.LastTickPosition = position;
                    OnPositionUpdated?.Invoke(new PositionUpdatedEvent(position));
                }
            }
        }

        public void OnSimUpdated(FrameSnapshot frame, float interpolation, bool replay)
        {
            if (!replay)
            {
                // Don't update camera location from sim during play, it's controlled via direct user input
                return;
            }

            // Do update camera location during replay

            // @TODO: determine which entities were actually updated last tick and only loop through them (raise event when SimSystem makes update?)
            SimCamera[] simCameras = frame.Snapshot.GetComponents<SimCamera>().ToArray();
            SimCamera[] nextCameras = frame.NextSnapshot.GetComponents<SimCamera>().ToArray();
            for (int i = 0; i < simCameras.Length; i++)
            {
                // @TODO: more efficiently get GameObject, related SimComponents and MonoBehaviours
                GameObject go = GameState.GetGameObject(simCameras[i].EntityID).GameObject;
                Transform transform = go.GetComponent<Transform>();

                Vector2 newPosition = Vector2.Lerp(simCameras[i].Position, nextCameras[i].Position, interpolation);
                transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            }
        }
    }
}
