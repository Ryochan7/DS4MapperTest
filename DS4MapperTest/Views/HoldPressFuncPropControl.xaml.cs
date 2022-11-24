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
using DS4MapperTest.ActionUtil;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.ViewModels;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for HoldPressFuncPropControl.xaml
    /// </summary>
    public partial class HoldPressFuncPropControl : UserControl
    {
        private HoldPressFuncPropViewModel propViewModel;
        public event EventHandler RequestBindingEditor;
        public event EventHandler<int> RequestChangeFuncType;

        public HoldPressFuncPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, ButtonAction action, HoldPressFunc func)
        {
            propViewModel = new HoldPressFuncPropViewModel(mapper, action, func);

            DataContext = propViewModel;

            funcTypeControl.PostInit(func);
            funcTypeControl.FuncTypeSelectVM.SelectedIndexChanged += FuncTypeSelectVM_SelectedIndexChanged;
        }

        private void FuncTypeSelectVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = funcTypeControl.FuncTypeSelectVM.SelectedIndex;
            RequestChangeFuncType?.Invoke(this, selectedIndex);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RequestBindingEditor?.Invoke(this, EventArgs.Empty);
        }
    }
}
