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
using DS4MapperTest.GyroActions;

namespace DS4MapperTest.Views.GyroActionPropControls
{
    /// <summary>
    /// Interaction logic for GyroNoActionControl.xaml
    /// </summary>
    public partial class GyroNoActionControl : UserControl
    {
        public event EventHandler<int> ActionTypeIndexChanged;

        public GyroNoActionControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, GyroMapAction action)
        {
            gyroSelectControl.PostInit(mapper, action);
            gyroSelectControl.GyroActSelVM.SelectedIndexChanged += GyroActSelVM_SelectedIndexChanged; ;
        }

        private void GyroActSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                gyroSelectControl.GyroActSelVM.SelectedIndex);
        }
    }
}
