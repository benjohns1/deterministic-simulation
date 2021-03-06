using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.InternalEvent
{
    internal class PubSub
    {
        protected List<IEvent> events = new List<IEvent>();
        private List<Subscription> subscriptions = new List<Subscription>();

        internal void Publish(IEvent @event)
        {
            events.Add(@event);
            List<Callback> subCallbacks = subscriptions.Where(sub => sub.EventType.IsAssignableFrom(@event.GetType())).Select(sub => sub.Callback).ToList();
            foreach (Callback callback in subCallbacks)
            {
                callback.Invoke(@event);
            }
        }

        internal void Subscribe(IEnumerable<Subscription> subs)
        {
            subscriptions.AddRange(subs);
        }

        internal void Unsubscribe(Type type, Callback callback)
        {
            Subscription subscription = new Subscription(type, callback);
            if (!subscriptions.Contains(subscription))
            {
                return;
            }
            subscriptions.Remove(subscription);
        }
    }
}
