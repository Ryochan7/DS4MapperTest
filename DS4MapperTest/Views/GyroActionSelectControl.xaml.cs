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
using DS4MapperTest.GyroActions;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for GyroActionSelectControl.xaml
    /// </summary>
    public partial class GyroActionSelectControl : UserControl
    {
        private GyroActionSelectViewModel gyroActSelVM;
        public GyroActionSelectViewModel GyroActSelVM => gyroActSelVM;

        public GyroActionSelectControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, GyroMapAction action)
        {
            gyroActSelVM = new GyroActionSelectViewModel(mapper, action);
            gyroActSelVM.PrepareView();

            DataContext = gyroActSelVM;
        }
    }
}
