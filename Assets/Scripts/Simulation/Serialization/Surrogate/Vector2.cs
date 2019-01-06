using System;
using System.Runtime.Serialization;
using Vector2 = UnityEngine.Vector2;

namespace Simulation.Serialization.Surrogate
{
    class Vector2Surrogate : ISimSurrogate
    {
        public Type Type => typeof(Vector2);

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Vector2 v2 = (Vector2)obj;
            info.AddValue("x", v2.x);
            info.AddValue("y", v2.y);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Vector2 v2 = (Vector2)obj;
            Type floatType = typeof(float);
            v2.x = (float)info.GetValue("x", floatType);
            v2.y = (float)info.GetValue("y", floatType);
            return v2;
        }
    }
}
