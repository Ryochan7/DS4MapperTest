using DS4MapperTest.TouchpadActions;
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

namespace DS4MapperTest.Views.TouchpadActionPropControls
{
    /// <summary>
    /// Interaction logic for TouchpadStickActionPropControl.xaml
    /// </summary>
    public partial class TouchpadStickActionPropControl : UserControl
    {
        private TouchpadStickActionPropViewModel touchStickPropVM;
        public TouchpadStickActionPropViewModel TouchStickPropVM => touchStickPropVM;

        public TouchpadStickActionPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TouchpadMapAction action)
        {
            touchStickPropVM = new TouchpadStickActionPropViewModel(mapper, action);

            DataContext = touchStickPropVM;
        }
    }
}
