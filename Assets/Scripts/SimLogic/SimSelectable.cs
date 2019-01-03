using Simulation;
using EntityID = System.UInt64;

namespace SimLogic
{
    [System.Serializable]
    public class SimSelectable : SimComponent
    {
        public bool Selected;

        public SimSelectable(EntityID entityID, bool selected) : base(entityID)
        {
            Selected = selected;
        }

        public override object Clone() => new SimSelectable(EntityID, Selected);
    }
}