using DS4MapperTest.TouchpadActions;
using DS4MapperTest.ViewModels.TouchpadActionPropViewModels;
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

namespace DS4MapperTest.Views.TouchpadActionPropControls
{
    /// <summary>
    /// Interaction logic for TouchpadFlickStickPropControl.xaml
    /// </summary>
    public partial class TouchpadFlickStickPropControl : UserControl
    {
        private TouchpadFlickStickPropViewModel touchFlickPropVM;
        public TouchpadFlickStickPropViewModel TouchFlickPropVM => touchFlickPropVM;

        public TouchpadFlickStickPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TouchpadMapAction action)
        {
            touchFlickPropVM = new TouchpadFlickStickPropViewModel(mapper, action);

            DataContext = touchFlickPropVM;
        }
    }
}
