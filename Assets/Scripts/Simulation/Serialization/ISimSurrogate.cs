using System;
using System.Runtime.Serialization;

namespace Simulation.Serialization
{
    interface ISimSurrogate : ISerializationSurrogate
    {
        Type Type { get; }
    }
}
