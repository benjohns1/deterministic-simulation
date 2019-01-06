using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Simulation.Serialization
{
    public class Serializer
    {
        protected IFormatter Formatter;

        public Serializer(IEnumerable<ISimSurrogate> additionalSurrogates = null)
        {
            Formatter = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            StreamingContext streamingContext = new StreamingContext(StreamingContextStates.All);

            Type surrogateInterfaceType = typeof(ISimSurrogate);
            foreach (Type surrogateType in Assembly.GetAssembly(GetType()).GetTypes().Where(t => surrogateInterfaceType.IsAssignableFrom(t) && t.IsClass))
            {
                ISimSurrogate surrogate = (ISimSurrogate)Activator.CreateInstance(surrogateType);
                surrogateSelector.AddSurrogate(surrogate.Type, streamingContext, surrogate);
            }
            if (additionalSurrogates != null)
            {
                foreach (ISimSurrogate surrogate in additionalSurrogates)
                {
                    surrogateSelector.AddSurrogate(surrogate.Type, streamingContext, surrogate);
                }
            }
            Formatter.SurrogateSelector = surrogateSelector;
        }

        public void Serialize(Stream stream, object graph)
        {
            Formatter.Serialize(stream, graph);
        }

        public object Deserialize(Stream stream)
        {
            return Formatter.Deserialize(stream);
        }
    }
}
