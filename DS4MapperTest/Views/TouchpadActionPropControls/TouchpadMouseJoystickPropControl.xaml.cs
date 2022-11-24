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
using DS4MapperTest.ViewModels.TouchpadActionPropViewModels;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.Views.TouchpadActionPropControls
{
    /// <summary>
    /// Interaction logic for TouchpadMouseJoystickPropControl.xaml
    /// </summary>
    public partial class TouchpadMouseJoystickPropControl : UserControl
    {
        private TouchpadMouseJoystickPropViewModel touchMouseJoyPropVM;
        public TouchpadMouseJoystickPropViewModel TouchMouseJoyPropVM => touchMouseJoyPropVM;
        private Mapper mapper;
        private TouchpadMapAction action;

        public TouchpadMouseJoystickPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TouchpadMapAction action)
        {
            touchMouseJoyPropVM = new TouchpadMouseJoystickPropViewModel(mapper, action);

            DataContext = touchMouseJoyPropVM;
        }
    }
}
