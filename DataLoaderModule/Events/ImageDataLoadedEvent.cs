using System;

using Prism.Events;

namespace DataLoaderModule.Events
{
    public class ImageDataLoadedEventArgs
    {
        public Guid ImageVolumeGuid;
    }

    public class ImageDataLoadedEvent
            : PubSubEvent<ImageDataLoadedEventArgs> { }
}
