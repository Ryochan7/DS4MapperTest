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
using DS4MapperTest.TriggerActions;

namespace DS4MapperTest.Views.TriggerActionPropControls
{
    /// <summary>
    /// Interaction logic for TriggerNoActPropControl.xaml
    /// </summary>
    public partial class TriggerNoActPropControl : UserControl
    {
        public event EventHandler<int> ActionTypeIndexChanged;

        public TriggerNoActPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TriggerMapAction action)
        {
            triggerSelectControl.PostInit(mapper, action);
            triggerSelectControl.TrigActionSelVM.SelectedIndexChanged += TrigActionSelVM_SelectedIndexChanged;
        }

        private void TrigActionSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                triggerSelectControl.TrigActionSelVM.SelectedIndex);
        }
    }
}
