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
using DS4MapperTest.Views;
using DS4MapperTest.StickActions;

namespace DS4MapperTest.Views.StickActionPropControls
{
    /// <summary>
    /// Interaction logic for StickNoActPropControl.xaml
    /// </summary>
    public partial class StickNoActPropControl : UserControl
    {
        public event EventHandler<int> ActionTypeIndexChanged;

        public StickNoActPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, StickMapAction action)
        {
            stickSelectControl.PostInit(mapper, action);
            stickSelectControl.StickActSelVM.SelectedIndexChanged += StickActSelVM_SelectedIndexChanged; ;
        }

        private void StickActSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                stickSelectControl.StickActSelVM.SelectedIndex);
        }
    }
}
