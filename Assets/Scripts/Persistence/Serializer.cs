using System.IO;

namespace Persistence
{
    public class Serializer : ISerializer
    {
        private Simulation.Serialization.Serializer SimSerializer = new Simulation.Serialization.Serializer();

        public object Deserialize(Stream stream)
        {
            return SimSerializer.Deserialize(stream);
        }

        public void Serialize(Stream stream, object graph)
        {
            SimSerializer.Serialize(stream, graph);
        }
    }
}
