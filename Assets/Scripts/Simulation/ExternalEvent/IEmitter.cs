using System.Collections.Generic;

namespace Simulation.ExternalEvent
{
    public interface IEmitter
    {
        List<IEvent> Events { get; }
        void Retrieved();
    }
}
