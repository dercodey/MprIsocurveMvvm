using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Mvvm;
using Prism.Events;

using MprIsocurveGeneration.Events;
using MprIsocurveGeneration.Services;


namespace MprIsocurveGeneration.ViewModels
{
    /// <summary>
    /// DataLoader is responsible for "loading" test data to display, which is usually just generated
    /// </summary>
    public class IsocurveControlViewModel : BindableBase
    {
        IEventAggregator _eventAggregator;

        /// <summary>
        /// constructor via container will pass the event aggregator
        /// </summary>
        /// <param name="eventAggregator"></param>
        public IsocurveControlViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// determines the number of isocurves to be displayed
        /// </summary>
        public int IsocurveLevels
        {
            get { return _isocurveLevels; }
            set 
            {
                SetProperty(ref _isocurveLevels, value);

                // trigger the isocurve event to update
                var setIsocurveEvent = _eventAggregator.GetEvent<SetIsocurveLevelEvent>();
                setIsocurveEvent.Publish(
                    new SetIsocurveLevelEventArgs() 
                    { 
                        Levels = _isocurveLevels, 
                    });
            }
        }
        private int _isocurveLevels = 10;
    }
}
