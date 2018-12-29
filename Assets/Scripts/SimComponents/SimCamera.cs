using UnityEngine;
using EntityID = System.UInt64;

namespace Simulation
{
    [System.Serializable]
    public class SimCamera : SimComponent
    {
        public float NormalSpeed;
        public float FastSpeed;

        public Vector2 Up = Vector2.zero;
        public Vector2 Down = Vector2.zero;
        public Vector2 Left = Vector2.zero;
        public Vector2 Right = Vector2.zero;
        public Vector2 Velocity = Vector2.zero;
        public float CurrentSpeed;

        public SimCamera(EntityID entityID, float speed, float fastSpeed) : base(entityID)
        {
            NormalSpeed = speed;
            FastSpeed = fastSpeed;
            CurrentSpeed = NormalSpeed;
        }

        public SimCamera(SimCamera simCamera) : this(simCamera.EntityID, simCamera.NormalSpeed, simCamera.FastSpeed)
        {
            Up = simCamera.Up;
            Down = simCamera.Down;
            Left = simCamera.Left;
            Right = simCamera.Right;
            Velocity = simCamera.Velocity;
            CurrentSpeed = simCamera.CurrentSpeed;
        }

        public override object Clone()
        {
            return new SimCamera(this);
        }
    }
}