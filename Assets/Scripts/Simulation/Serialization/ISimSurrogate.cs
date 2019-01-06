using System;
using System.Runtime.Serialization;

namespace Simulation.Serialization
{
    public interface ISimSurrogate : ISerializationSurrogate
    {
        Type Type { get; }
    }
}
