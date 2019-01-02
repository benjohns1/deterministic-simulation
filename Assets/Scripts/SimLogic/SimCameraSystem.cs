using Game.Camera;
using Simulation;
using Simulation.ExternalEvent;
using Simulation.State;
using System.Collections.Generic;
using System.Linq;

namespace SimLogic
{
    class SimCameraSystem : SimSystem
    {
        public override IEnumerable<Subscription> Subscriptions => new List<Subscription>()
        {
            new Subscription(typeof(PositionUpdatedEvent)),
        };

        public override IEnumerable<ComponentUpdate> Tick(SimState state, IEnumerable<IEvent> events)
        {
            List<ComponentUpdate> updates = new List<ComponentUpdate>();

            if (!events.Any())
            {
                return Enumerable.Empty<ComponentUpdate>();
            }

            PositionUpdatedEvent @event = (PositionUpdatedEvent)events.LastOrDefault(e => e.GetType() == typeof(PositionUpdatedEvent));

            foreach (SimCamera camera in state.GetComponents<SimCamera>())
            {
                SimCamera newCamera = camera.Clone() as SimCamera;
                newCamera.Position = @event.Position;
                updates.Add(new ComponentUpdate(newCamera));
            }

            return updates;
        }
    }
}
