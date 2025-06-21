using DS4MapperTest.StickActions;
using DS4MapperTest.ViewModels;
using DS4MapperTest.ViewModels.StickActionPropViewModels;
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

namespace DS4MapperTest.Views.StickActionPropControls
{
    /// <summary>
    /// Interaction logic for StickFlickStickPropControl.xaml
    /// </summary>
    public partial class StickFlickStickPropControl : UserControl
    {
        private StickFlickStickPropViewModel flickStickVM;
        public StickFlickStickPropViewModel FlickStickVM => flickStickVM;

        public event EventHandler<int> ActionTypeIndexChanged;

        public StickFlickStickPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, StickMapAction action)
        {
            flickStickVM = new StickFlickStickPropViewModel(mapper, action);

            DataContext = flickStickVM;

            stickSelectControl.PostInit(mapper, action);
            stickSelectControl.StickActSelVM.SelectedIndexChanged += StickActSelVM_SelectedIndexChanged;
        }

        private void StickActSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                stickSelectControl.StickActSelVM.SelectedIndex);
        }
    }
}
