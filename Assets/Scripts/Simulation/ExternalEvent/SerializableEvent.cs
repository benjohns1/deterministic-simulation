using System.Runtime.Serialization;

namespace Simulation.ExternalEvent
{
    [System.Serializable]
    public class SerializableEvent : ISerializable
    {
        public IEvent Event;

        public SerializableEvent(IEvent @event)
        {
            Event = @event;
        }

        public SerializableEvent(SerializationInfo info, StreamingContext context)
        {
            System.Type type = info.GetValue("type", typeof(System.Type)) as System.Type;
            Event = info.GetValue("event", type) as IEvent;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("type", Event.GetType());
            info.AddValue("event", Event);
        }
    }
}
