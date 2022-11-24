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
using DS4MapperTest.ViewModels;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for TriggerActionSelectControl.xaml
    /// </summary>
    public partial class TriggerActionSelectControl : UserControl
    {
        private TriggerActionSelectViewModel trigActionSelVM;
        public TriggerActionSelectViewModel TrigActionSelVM => trigActionSelVM;

        public TriggerActionSelectControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TriggerMapAction action)
        {
            trigActionSelVM = new TriggerActionSelectViewModel(mapper, action);
            trigActionSelVM.PrepareView();

            DataContext = trigActionSelVM;
        }
    }
}
