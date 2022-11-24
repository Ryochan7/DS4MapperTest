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
using DS4MapperTest.ViewModels.TriggerActionPropViewModels;
using DS4MapperTest.TriggerActions;

namespace DS4MapperTest.Views.TriggerActionPropControls
{
    /// <summary>
    /// Interaction logic for TriggerTranslatePropControl.xaml
    /// </summary>
    public partial class TriggerTranslatePropControl : UserControl
    {
        private TriggerTranslatePropViewModel trigTransPropVM;
        public TriggerTranslatePropViewModel TrigTransPropVM => trigTransPropVM;

        public event EventHandler<int> ActionTypeIndexChanged;

        public TriggerTranslatePropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TriggerMapAction action)
        {
            trigTransPropVM = new TriggerTranslatePropViewModel(mapper, action);

            DataContext = trigTransPropVM;

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
