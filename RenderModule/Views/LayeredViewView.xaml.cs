using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RenderModule.Views
{
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
