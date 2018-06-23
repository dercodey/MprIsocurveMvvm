using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Prism.Events;

namespace MprIsocurveGeneration.Events
{
    public class SetIsocurveLevelEventArgs
    {
        public int Levels;
    }

    public class SetIsocurveLevelEvent
            : PubSubEvent<SetIsocurveLevelEventArgs> { }
}
