using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Prism.Events;

namespace MprIsocurveGeneration.Events
{
    public class VolumeUpdateEventArgs
    {
        public Int64 TimeStamp;
        public Guid ImageVolumeGuid;
    }

    public class VolumeUpdateEvent
            : PubSubEvent<VolumeUpdateEventArgs> { }
}
