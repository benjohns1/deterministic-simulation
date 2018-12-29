using System;

namespace Simulation.ExternalEvent
{
    public struct Subscription
    {
        internal readonly Type EventType;

        public Subscription(Type eventType)
        {
            EventType = eventType;
        }
    }
}
