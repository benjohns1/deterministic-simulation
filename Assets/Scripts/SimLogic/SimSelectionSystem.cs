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

            IEnumerable<SimSelectable> selectables = state.GetComponents<SimSelectable>();
            foreach (SelectionUpdatedEvent @event in events.Where(e => e.GetType() == typeof(SelectionUpdatedEvent)))
            {
                if (@event.Action == SelectAction.Cleared)
                {
                    // Deselect everything
                    foreach (SimSelectable clearSelectable in selectables)
                    {
                        AddUpdate(ref updates, clearSelectable, false);
                    }
                    continue;
                }

                SimSelectable selectable = selectables.First(s => s.EntityID == @event.EntityID);
                if (@event.Action == SelectAction.Selected || @event.Action == SelectAction.Deselected)
                {
                    bool select = (@event.Action == SelectAction.Selected);
                    AddUpdate(ref updates, selectable, select);
                }
            }

            return updates;
        }

        private void AddUpdate(ref List<ComponentUpdate> updates, SimSelectable selectable, bool newSelectVal)
        {
            if (selectable.Selected != newSelectVal)
            {
                SimSelectable newSelectable = selectable.Clone() as SimSelectable;
                newSelectable.Selected = newSelectVal;
                updates.Add(new ComponentUpdate(newSelectable));
            }
        }
    }
}
