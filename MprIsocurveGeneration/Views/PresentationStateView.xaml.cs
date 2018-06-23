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

using MprIsocurveGeneration.ViewModels;

namespace MprIsocurveGeneration.Views
{
    /// <summary>
    /// Interaction logic for PresentationStateView.xaml
    /// </summary>
    public partial class PresentationStateView : UserControl
    {
        public PresentationStateView()
        {
            InitializeComponent();
        }

        bool bDrag = false;
        Point ptStart;
        Point navPtStart;

        // TODO: move these mouse movements to a behavior

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bDrag = true;
            ptStart = e.GetPosition(this);

            var vm = (PresentationStateViewModel)this.DataContext;
            navPtStart = vm.NavigationPointOnPlane;
            this.CaptureMouse();
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (bDrag)
            {
                Point ptCurrent = e.GetPosition(this);
                var offset = ptCurrent - ptStart;

                var vm = (PresentationStateViewModel)this.DataContext;
                vm.NavigationPointOnPlane = navPtStart + offset;
            }
        }

        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            bDrag = false;
            Mouse.Capture(null);
        }
    }
}
