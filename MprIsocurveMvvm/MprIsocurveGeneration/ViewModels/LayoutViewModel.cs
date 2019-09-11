using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Practices.Unity;

using Prism.Events;
using Prism.Mvvm;

using MprIsocurveGeneration.Events;
using MprIsocurveGeneration.Models;

namespace MprIsocurveGeneration.ViewModels
{
    using Orientation = PresentationStateViewModel.Orientation;

    public class LayoutViewModel : BindableBase
    {
        IUnityContainer _container;
        IEventAggregator _eventAggregator;

        public LayoutViewModel(IUnityContainer container, IEventAggregator eventAggregator)
        {
            _container = container;
            _eventAggregator = eventAggregator;

            var newLayeredViews = CreateLayeredViewViewModels();
            _layeredViews.AddRange(newLayeredViews);

            DataLoader = _container.Resolve<DataLoaderViewModel>();
        }

        public DataLoaderViewModel DataLoader
        {
            get;
            set;
        }

        IEnumerable<LayeredViewViewModel> CreateLayeredViewViewModels()
        {
            var transverse = _container.Resolve<LayeredViewViewModel>();
            transverse.PresentationState.ViewOrientation = Orientation.Transverse;
            yield return transverse;

            var sagittal = _container.Resolve<LayeredViewViewModel>();
            sagittal.PresentationState.ViewOrientation = Orientation.Sagittal;
            yield return sagittal;

            var coronal = _container.Resolve<LayeredViewViewModel>();
            coronal.PresentationState.ViewOrientation = Orientation.Coronal;
            yield return coronal;
        }

        private int _rows;

        public int Rows
        {
            get { return _rows; }
            set { SetProperty(ref _rows, value); }
        }
        private int _columns;

        public int Columns
        {
            get { return _columns; }
            set { SetProperty(ref _columns, value); }
        }

        private ObservableCollection<LayeredViewViewModel> _layeredViews =
            new ObservableCollection<LayeredViewViewModel>();

        public ObservableCollection<LayeredViewViewModel> LayeredViews
        {
            get { return _layeredViews; }
        }
    }
}
