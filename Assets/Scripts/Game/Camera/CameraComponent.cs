using UnityEngine;

namespace Game.Camera
{
    public class CameraComponent : MonoBehaviour
    {
        public float NormalSpeed = 5f;
        public float FastSpeed = 15f;

        public Vector2 Up = Vector2.zero;
        public Vector2 Down = Vector2.zero;
        public Vector2 Left = Vector2.zero;
        public Vector2 Right = Vector2.zero;
        public Vector3 Velocity = Vector2.zero;
        public float CurrentSpeed;

        public Vector3 LastTickPosition = Vector3.zero;

        private void Awake()
        {
            CurrentSpeed = NormalSpeed;
        }

        private void OnEnable()
        {
            GameManager.CameraSystem.Register(this);
        }

        private void OnDisable()
        {
            GameManager.CameraSystem.Unregister(this);
        }
    }
}
