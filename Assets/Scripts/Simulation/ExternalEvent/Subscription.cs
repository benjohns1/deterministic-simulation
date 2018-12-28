using System;

namespace Simulation.ExternalEvent
{
    public delegate void Callback(IEvent @event);

    public struct Subscription
    {
        internal readonly Type EventType;
        internal readonly Callback Callback;

        public Subscription(Type eventType, Callback callback)
        {
            EventType = eventType;
            Callback = callback;
        }
    }
}
