using DS4MapperTest.DPadActions;
using DS4MapperTest.ViewModels;
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

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for DPadActionSelectControl.xaml
    /// </summary>
    public partial class DPadActionSelectControl : UserControl
    {
        private DPadActionSelectViewModel dpadActSelVM;
        public DPadActionSelectViewModel DPadActSelVM => dpadActSelVM;

        public DPadActionSelectControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, DPadMapAction action)
        {
            dpadActSelVM = new DPadActionSelectViewModel(mapper, action);
            dpadActSelVM.PrepareView();

            DataContext = dpadActSelVM;
        }
    }
}
