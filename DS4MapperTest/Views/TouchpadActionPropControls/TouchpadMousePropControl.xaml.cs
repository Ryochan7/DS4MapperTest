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
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.Views.TouchpadActionPropControls
{
    /// <summary>
    /// Interaction logic for TouchpadMousePropControl.xaml
    /// </summary>
    public partial class TouchpadMousePropControl : UserControl
    {
        private TouchpadMousePropViewModel touchMousePropVM;
        public TouchpadMousePropViewModel TouchMousePropVM => touchMousePropVM;

        public TouchpadMousePropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TouchpadMapAction action)
        {
            touchMousePropVM = new TouchpadMousePropViewModel(mapper, action);

            DataContext = touchMousePropVM;
        }
    }
}
