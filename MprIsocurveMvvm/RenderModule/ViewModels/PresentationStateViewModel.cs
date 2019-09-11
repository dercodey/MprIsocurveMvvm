using System;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Threading;
using System.Windows.Threading;

using Prism.Mvvm;
using Prism.Events;

using AutoMapper;

using Infrastructure.Events;

using RenderModule.Interfaces;

namespace RenderModule.ViewModels
{
    /// <summary>
    /// represents the presentation state for a view: view geometry
    /// </summary>
    public class PresentationStateViewModel : BindableBase
    {
        IEventAggregator _eventAggregator;
        IFrameUpdateManager _fum;
        SynchronizationContext _syncContext;

        /// <summary>
        /// construct the presentation state with an event aggregator, to send/receive update events
        /// </summary>
        /// <param name="eventAggregator"></param>
        public PresentationStateViewModel(IEventAggregator eventAggregator, IFrameUpdateManager fum)
        {
            _eventAggregator = eventAggregator;
            _fum = fum;

            // store the UI thread SynchronizationContext
            _syncContext = SynchronizationContext.Current;
            System.Diagnostics.Trace.Assert(_syncContext != null);
        }

        /// <summary>
        /// the NavigationPoint position for the presentation state
        /// </summary>
        public Point3D NavigationPoint
        {
            get { return _navigationPoint; }
            set 
            {
                System.Diagnostics.Trace.Assert(Dispatcher.CurrentDispatcher == Application.Current.Dispatcher);
                _fum.ProcessQueue();

                // TODO: report the time stamp of this event, and also the time stamp of the

                SetNavigationPointNoEvent(value);

                // send the update event
                var updateEvent = _eventAggregator.GetEvent<NavigationPointUpdateEvent>();
                var eventArgs = 
                    new NavigationPointUpdateEventArgs(this, DateTime.Now.Ticks, _navigationPoint);
                updateEvent.Publish(eventArgs);
            }
        }
        private Point3D _navigationPoint = new Point3D(0.0, 0.0, 0.0);

        // helper to set the navigation without sending the update event (just change notification)
        internal void SetNavigationPointNoEvent(Point3D value)
        {
            SetProperty(ref _navigationPoint, value);
            RaisePropertyChanged(nameof(NavigationPointOnPlane));
        }

        /// <summary>
        /// read-only property for the slice position, calculated from orientation and navigation point
        ///    position
        /// </summary>
        public double SlicePosition
        {
            get
            {
                switch (ViewOrientation)
                {
                    case Orientation.Transverse:
                        return NavigationPoint.Z;

                    case Orientation.Sagittal:
                        return NavigationPoint.X;

                    case Orientation.Coronal:
                        return NavigationPoint.Y;
                }
                return NavigationPoint.Z;
            }
        }

        /// <summary>
        /// read/write property for the projection of the navigation point on the plane
        ///     useful for binding to 2D elements
        /// </summary>
        public Point NavigationPointOnPlane
        {
            get
            {
                switch (ViewOrientation)
                {
                    case Orientation.Transverse:
                        return new Point(NavigationPoint.X, NavigationPoint.Y);

                    case Orientation.Sagittal:
                        return new Point(NavigationPoint.Y, NavigationPoint.Z);

                    case Orientation.Coronal:
                        return new Point(NavigationPoint.X, NavigationPoint.Z);
                }
                return new Point(NavigationPoint.X, NavigationPoint.Y);
            }

            set
            {
                switch (ViewOrientation)
                {
                    case Orientation.Transverse:
                        NavigationPoint = new Point3D(value.X, value.Y, NavigationPoint.Z);
                        break;

                    case Orientation.Sagittal:
                        NavigationPoint = new Point3D(NavigationPoint.X, value.X, value.Y);
                        break;

                    case Orientation.Coronal:
                        NavigationPoint = new Point3D(value.X, NavigationPoint.Y, value.Y);
                        break;
                }
            }
        }

        /// <summary>
        /// represents patient orientation
        /// </summary>
        public enum Orientation
        {
            Transverse,
            Sagittal,
            Coronal
        };

        /// <summary>
        /// property for the orientation of the view
        /// </summary>
        public Orientation ViewOrientation
        {
            get { return _orientation; }
            set { SetProperty(ref _orientation, value); }
        }
        private Orientation _orientation;

        /// <summary>
        /// string-based orientation, to help with binding to view-first XAML views
        /// </summary>
        public string ViewOrientationString
        {
            get { return Mapper.Map<string>(_orientation); }
            set 
            {
                ViewOrientation = Mapper.Map<Orientation>(value);            
            }
        }
    }
}
