using System;

using Prism.Events;

namespace Infrastructure.Events
{
    public class ImageDataLoadedEventArgs
    {
        public Guid ImageVolumeGuid;
    }

    public class ImageDataLoadedEvent
            : PubSubEvent<ImageDataLoadedEventArgs> { }
}
