using System;
using System.Threading;
using System.Threading.Tasks;

using Infrastructure.Utilities;

namespace RenderModule.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFrameUpdateManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="context"></param>
        /// <param name="uiUpdateAction"></param>
        void QueueAction(UInt64 timeStamp, SynchronizationContext context, 
            Task<Action> action);

        /// <summary>
        /// 
        /// </summary>
        void ProcessQueue();

        /// <summary>
        /// 
        /// </summary>
        PerformanceCounter UiUpdatePerformance { get; set; }
    }
}
