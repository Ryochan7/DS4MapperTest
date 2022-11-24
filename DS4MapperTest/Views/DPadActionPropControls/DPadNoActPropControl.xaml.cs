using DS4MapperTest.DPadActions;
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

namespace DS4MapperTest.Views.DPadActionPropControls
{
    /// <summary>
    /// Interaction logic for DPadNoActPropControl.xaml
    /// </summary>
    public partial class DPadNoActPropControl : UserControl
    {
        public event EventHandler<int> ActionTypeIndexChanged;

        public DPadNoActPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, DPadMapAction action)
        {
            dpadSelectControl.PostInit(mapper, action);
            dpadSelectControl.DPadActSelVM.SelectedIndexChanged += StickActSelVM_SelectedIndexChanged; ;
        }

        private void StickActSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                dpadSelectControl.DPadActSelVM.SelectedIndex);
        }
    }
}
