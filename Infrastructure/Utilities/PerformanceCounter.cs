using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Threading;

using Prism.Mvvm;

namespace Infrastructure.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class PerformanceCounter : BindableBase
    {
        public PerformanceCounter(string label, SynchronizationContext context)
        {
            _label = label;
            QueryPerformanceFrequency(out _frequency);
            _uiUpdateContext = context;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick +=
                (sender, ignore) => _uiUpdateContext.Post(ignore2 => UpdateBindings(), null);
            timer.Start();
        }

        string _label;

        static double ElapsedMS(Int64 start, Int64 end)
        {
            var elapsed = 1000.0 * (double)(end - start) / (double)_frequency;
            return elapsed;
        }

        public static bool Enabled = false;

        Int64 _currentStart;

        public void StartEvent()
        {
            QueryPerformanceCounter(out _currentStart);
        }

        public void EndEvent()
        {
            Int64 currentEnd;
            QueryPerformanceCounter(out currentEnd);

            var elapsedMS = ElapsedMS(_currentStart, currentEnd);
            _queueDurationsMS.Enqueue(elapsedMS);

            while (_queueDurationsMS.Count > 10)
            {
                double discard;
                _queueDurationsMS.TryDequeue(out discard);
            }

            // remove entries that are > 5 sec
            double next;
            while (_queueDurationsMS.TryPeek(out next)
                && next > 5000.0)
            {
                _queueDurationsMS.TryDequeue(out next);
            }

            // _uiUpdateContext.Post(ignore => UpdateBindings(), null);
        }

        SynchronizationContext _uiUpdateContext;

        public void UpdateBindings()
        {
            if (_queueDurationsMS.Count > 0)
            {
                AverageDurationMS = _queueDurationsMS.Average();
                AverageDurationString =
                    String.Format("{0}: {1:F2} msec", _label, AverageDurationMS);
            }
            else
            {
                AverageDurationMS = 0;
                AverageDurationString = String.Format("{0}: -- msec", _label);
            }
        }

        ConcurrentQueue<double> _queueDurationsMS = new ConcurrentQueue<double>();

        public double AverageDurationMS
        {
            get { return _averageDurationMS; }
            set { SetProperty(ref _averageDurationMS, value); }
        }
        private double _averageDurationMS;

        public string AverageDurationString
        {
            get { return _averageDurationString; }
            set { SetProperty(ref _averageDurationString, value); }
        }
        private string _averageDurationString;

        [DllImport("Kernel32.dll")]
        private static extern int QueryPerformanceCounter(out Int64 count);

        [DllImport("Kernel32.dll")]
        private static extern int QueryPerformanceFrequency(out Int64 frequency);

        static Int64 _frequency;

    }
}
