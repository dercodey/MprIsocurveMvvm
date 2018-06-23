using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Prism.Events;

namespace MprIsocurveGeneration.Events
{
    public class ShowDataEventArgs
    {
        public Guid ImageVolumeGuid;
    }

    public class ShowDataEvent
            : PubSubEvent<ShowDataEventArgs> { }
}
