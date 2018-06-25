
using Prism.Events;

namespace Infrastructure.Events
{
    public class SetIsocurveLevelEventArgs
    {
        public int Levels;
    }

    public class SetIsocurveLevelEvent
            : PubSubEvent<SetIsocurveLevelEventArgs> { }
}
