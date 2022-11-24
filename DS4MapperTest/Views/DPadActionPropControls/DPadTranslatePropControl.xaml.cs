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
using DS4MapperTest.DPadActions;
using DS4MapperTest.ViewModels.DPadActionPropViewModels;
using DS4MapperTest.ViewModels.StickActionPropViewModels;

namespace DS4MapperTest.Views.DPadActionPropControls
{
    /// <summary>
    /// Interaction logic for DPadTranslatePropControl.xaml
    /// </summary>
    public partial class DPadTranslatePropControl : UserControl
    {
        private DPadTranslatePropViewModel dpadTransVM;
        public DPadTranslatePropViewModel DPadTransVM => dpadTransVM;

        public event EventHandler<int> ActionTypeIndexChanged;

        public DPadTranslatePropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, DPadMapAction action)
        {
            dpadTransVM = new DPadTranslatePropViewModel(mapper, action);
            DataContext = dpadTransVM;

            dpadSelectControl.PostInit(mapper, action);
            dpadSelectControl.DPadActSelVM.SelectedIndexChanged += DPadActSelVM_SelectedIndexChanged;
        }

        private void DPadActSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                dpadSelectControl.DPadActSelVM.SelectedIndex);
        }
    }
}
