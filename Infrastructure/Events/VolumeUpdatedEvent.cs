using System;

using Prism.Events;

namespace Infrastructure.Events
{
    public class VolumeUpdatedEventArgs
    {
        public Int64 TimeStamp;
        public Guid ImageVolumeGuid;
    }

    public class VolumeUpdatedEvent
            : PubSubEvent<VolumeUpdatedEventArgs> { }
}
