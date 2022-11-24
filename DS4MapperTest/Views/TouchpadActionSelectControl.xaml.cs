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
using DS4MapperTest.ViewModels;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for TouchpadActionSelectControl.xaml
    /// </summary>
    public partial class TouchpadActionSelectControl : UserControl
    {
        private TouchpadActionSelectViewModel touchOutputSelVM;
        public TouchpadActionSelectViewModel TouchOutputSelVM
        {
            get => touchOutputSelVM;
        }

        public TouchpadActionSelectControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TouchpadMapAction action)
        {
            touchOutputSelVM = new TouchpadActionSelectViewModel(mapper, action);
            touchOutputSelVM.PrepareView();

            DataContext = touchOutputSelVM;
        }
    }
}
