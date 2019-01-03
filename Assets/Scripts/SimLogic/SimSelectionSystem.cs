using Game.UnitSelection;
using Simulation;
using Simulation.ExternalEvent;
using Simulation.State;
using System.Collections.Generic;
using System.Linq;

namespace SimLogic
{
    class SimSelectionSystem : SimSystem
    {
        public override IEnumerable<Subscription> Subscriptions => new List<Subscription>()
        {
            new Subscription(typeof(SelectionUpdatedEvent)),
        };

        public override IEnumerable<ComponentUpdate> Tick(SimState state, IEnumerable<IEvent> events)
        {
            List<ComponentUpdate> updates = new List<ComponentUpdate>();

            if (!events.Any())
            {
                return Enumerable.Empty<ComponentUpdate>();
            }

            SelectionUpdatedEvent @event = (SelectionUpdatedEvent)events.LastOrDefault(e => e.GetType() == typeof(SelectionUpdatedEvent));

            foreach (SimSelectable selection in state.GetComponents<SimSelectable>())
            {
                SimSelectable newSelection = selection.Clone() as SimSelectable;
                switch (@event.Action)
                {
                    case SelectionUpdatedEvent.SelectAction.Cleared:
                        newSelection.Selected = false;
                        break;
                    case SelectionUpdatedEvent.SelectAction.Deselected:
                        if (newSelection.EntityID == @event.EntityID)
                        {
                            newSelection.Selected = false;
                        }
                        break;
                    case SelectionUpdatedEvent.SelectAction.Selected:
                        if (newSelection.EntityID == @event.EntityID)
                        {
                            newSelection.Selected = true;
                        }
                        break;
                    default:
                        throw new System.Exception("Unhandled selection action");
                }
                updates.Add(new ComponentUpdate(newSelection));
            }

            return updates;
        }
    }
}
