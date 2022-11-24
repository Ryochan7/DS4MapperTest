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
using DS4MapperTest.StickActions;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for StickActionSelectControl.xaml
    /// </summary>
    public partial class StickActionSelectControl : UserControl
    {
        private StickActionSelectViewModel stickActSelVM;
        public StickActionSelectViewModel StickActSelVM => stickActSelVM;

        public StickActionSelectControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, StickMapAction action)
        {
            stickActSelVM = new StickActionSelectViewModel(mapper, action);
            stickActSelVM.PrepareView();

            DataContext = stickActSelVM;
        }
    }
}
