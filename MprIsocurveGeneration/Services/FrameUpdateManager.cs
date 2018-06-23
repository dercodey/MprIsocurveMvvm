using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Prism.Mvvm;

using MprIsocurveGeneration.Utilities;
using System.Windows;

namespace MprIsocurveGeneration.Services
{
    /// <summary>
    /// handles batched frame updates, based on a time stamp for each frame
    /// </summary>
    public class FrameUpdateManager : IFrameUpdateManager
    {
        static ConcurrentStack<FrameUpdateManager> _fums = new ConcurrentStack<FrameUpdateManager>();
        static CancellationToken _cancellationToken = new CancellationToken();
        static Task _updateTask;

        static FrameUpdateManager()
        {
            _updateTask = new Task(() => 
            {
                while (true)
                { 
                    foreach (var fum in _fums)
                    {
                        Application.Current.Dispatcher.Invoke(() => fum.ProcessQueue());
                        Thread.Sleep(10);
                    }
                }
            }, 
            _cancellationToken, 
            TaskCreationOptions.LongRunning);

            _updateTask.Start();
        }

        public FrameUpdateManager()
        {
            _fums.Push(this);
        }

        /// <summary>
        /// queues the given ui update action to be done in order of time stamp
        /// </summary>
        /// <param name="timeStamp">frame time stamp</param>
        /// <param name="context">the synchronization context to perform the UI update action</param>
        /// <param name="uiUpdateAction">the task which, upon completion, will produce the UI update action</param>
        public void QueueAction(UInt64 timeStamp, 
            SynchronizationContext context, Task<Action> uiUpdateAction)
        {
            lock (syncObject)
            { 
                // find or create the update batch to be used
                FrameUpdateBatch batch = 
                    FrameUpdateBatch.GetOrCreateBatch(timeStamp, context, 
                        ref _queuedBatches);

                // and add to the queue
                batch.UiUpdateActions.Enqueue(uiUpdateAction);
            }
        }

        /// <summary>
        /// called to attempt processing of frame batches in the queue
        /// </summary>
        public void ProcessQueue()
        {
            FrameUpdateBatch nextBatch = null;
            lock (syncObject)
            {
                if (_queuedBatches.TryPeek(out nextBatch))
                {
                    var nextBatchList = nextBatch.UiUpdateActions.ToList();
                    if (nextBatchList.TrueForAll(t => t.IsCompleted))
                    {
                        // now perform the UI update response actions
                        UiUpdatePerformance.StartEvent();

                        Task<Action> uiUpdateAction;
                        while (nextBatch.UiUpdateActions.TryDequeue(out uiUpdateAction))
                        {
                            var action = uiUpdateAction.Result;
                            action.Invoke();
                        }
                        UiUpdatePerformance.EndEvent();

                        // now remove from the queue, if possible
                        FrameUpdateBatch dummy;
                        _queuedBatches.TryDequeue(out dummy);
                        System.Diagnostics.Trace.Assert(dummy.UiUpdateActions.IsEmpty);

                        FrameUpdateBatch next;
                        if (_queuedBatches.TryPeek(out next))
                        { 
                            System.Diagnostics.Trace.Assert(next.TimeStamp > dummy.TimeStamp);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// helper class to represent a single frame with its associated update actions
        /// </summary>
        class FrameUpdateBatch
        {
            /// <summary>
            /// either retrieves or creates a FrameUpdateBatch for the given time stamp
            /// </summary>
            /// <param name="timeStamp">the frame time stamp</param>
            /// <param name="context">the synchronization context for the frame</param>
            /// <param name="queue">the queue to add the batch to, or to find the batch on</param>
            /// <returns>the FrameUpdateBatch</returns>
            public static FrameUpdateBatch GetOrCreateBatch(UInt64 timeStamp, 
                SynchronizationContext context, 
                ref ConcurrentQueue<FrameUpdateBatch> queue)
            {
                FrameUpdateBatch batch = 
                    queue.FirstOrDefault(b => 
                        b.TimeStamp == timeStamp 
                        && b.Context == context);
                if (batch == null)
                {
                    batch = new FrameUpdateBatch();
                    batch.TimeStamp = timeStamp;
                    batch.Context = context;
                    batch.UiUpdateActions = new ConcurrentQueue<Task<Action>>();
                    queue.Enqueue(batch);
                }

                return batch;
            }

            // stores the time stamp for the update
            public UInt64 TimeStamp;

            // the synchronization context
            public SynchronizationContext Context;

            // the list of tasks that prouce UiUpdateActions
            public ConcurrentQueue<Task<Action>> UiUpdateActions;
        }

        // helper synchronization object
        Object syncObject = new Object();

        // the queue of frame update batches
        ConcurrentQueue<FrameUpdateBatch> _queuedBatches =
            new ConcurrentQueue<FrameUpdateBatch>();

        /// <summary>
        /// helper performance counter
        /// </summary>
        public PerformanceCounter UiUpdatePerformance
        {
            get { return _uiUpdateMS; }
            set { _uiUpdateMS = value; }
        }
        PerformanceCounter _uiUpdateMS;
    }
}
