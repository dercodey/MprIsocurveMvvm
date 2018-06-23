using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MprIsocurveGeneration.Views
{
    using Orientation = MprIsocurveGeneration.ViewModels.PresentationStateViewModel.Orientation;

    /// <summary>
    /// Interaction logic for LayeredViewView.xaml
    /// </summary>
    public partial class LayeredViewView : UserControl
    {
        public LayeredViewView()
        {
            InitializeComponent();

            Binding binding = new Binding("PresentationState.ViewOrientationString")
            {
                Source = DataContext,
                Mode = BindingMode.TwoWay,
            };
            SetBinding(ViewOrientationProperty, binding);
        }

        public static readonly DependencyProperty ViewOrientationProperty =
            DependencyProperty.Register("ViewOrientation", typeof(string),
                typeof(LayeredViewView),
                    new FrameworkPropertyMetadata((string)"Transverse"));

        public string ViewOrientation
        {
            get { return (string)GetValue(ViewOrientationProperty); }
            set { SetValue(ViewOrientationProperty, value); }
        }
    }
}
