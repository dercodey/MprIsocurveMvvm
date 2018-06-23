using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using Prism.Events;


namespace MprIsocurveGeneration.Events
{
    public class NavigationPointUpdateEventArgs
    {
        public NavigationPointUpdateEventArgs(object sender, Int64 timeStamp, Point3D point)
        {
            Sender = sender;
            TimeStamp = timeStamp;
            NavigationPointPosition = point;
        }

        public object Sender;
        public Int64 TimeStamp;
        public Point3D NavigationPointPosition;
    }

    public class NavigationPointUpdateEvent
            : PubSubEvent<NavigationPointUpdateEventArgs> { }
}
