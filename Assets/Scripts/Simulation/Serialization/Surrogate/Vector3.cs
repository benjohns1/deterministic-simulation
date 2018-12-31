using System;
using System.Runtime.Serialization;
using V3 = UnityEngine.Vector3;

namespace Simulation.Serialization.Surrogate
{
    class Vector3Surrogate : ISimSurrogate
    {
        public Type Type => typeof(V3);

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            V3 v3 = (V3)obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            V3 v3 = (V3)obj;
            Type floatType = typeof(float);
            v3.x = (float)info.GetValue("x", floatType);
            v3.y = (float)info.GetValue("y", floatType);
            v3.z = (float)info.GetValue("z", floatType);
            return v3;
        }
    }
}
