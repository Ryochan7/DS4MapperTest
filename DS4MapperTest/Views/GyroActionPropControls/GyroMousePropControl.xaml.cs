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
using DS4MapperTest.ViewModels.GyroActionPropViewModels;
using DS4MapperTest.GyroActions;

namespace DS4MapperTest.Views.GyroActionPropControls
{
    /// <summary>
    /// Interaction logic for GyroMousePropControl.xaml
    /// </summary>
    public partial class GyroMousePropControl : UserControl
    {
        private GyroMouseActionPropViewModel gyroMouseActVM;
        public GyroMouseActionPropViewModel GyroMouseActVM => gyroMouseActVM;

        public event EventHandler<int> ActionTypeIndexChanged;

        public GyroMousePropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, GyroMapAction action)
        {
            gyroMouseActVM = new GyroMouseActionPropViewModel(mapper, action);

            DataContext = gyroMouseActVM;

            gyroSelectControl.PostInit(mapper, action);
            gyroSelectControl.GyroActSelVM.SelectedIndexChanged += GyroActSelVM_SelectedIndexChanged;
        }

        private void GyroActSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                gyroSelectControl.GyroActSelVM.SelectedIndex);
        }
    }
}
