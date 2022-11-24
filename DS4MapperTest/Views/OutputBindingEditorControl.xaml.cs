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
using DS4MapperTest.MapperUtil;
using DS4MapperTest.ViewModels;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for OutputBindingEditorControl.xaml
    /// </summary>
    public partial class OutputBindingEditorControl : UserControl
    {
        private ButtonActionEditViewModel buttonActionEditVM;
        public event EventHandler Finished;

        public OutputBindingEditorControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, ButtonAction currentAction, ActionFunc func)
        {
            buttonActionEditVM = new ButtonActionEditViewModel(mapper, currentAction, func);

            DataContext = buttonActionEditVM;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Finished?.Invoke(this, EventArgs.Empty);
        }

        private void AddOutputSlot_Click(object sender, RoutedEventArgs e)
        {
            buttonActionEditVM.AddTempOutputSlot();
        }

        private void RemoveOutputSlot_Click(object sender, RoutedEventArgs e)
        {
            DataContext = null;

            buttonActionEditVM.RemoveOutputSlot(buttonActionEditVM.SelectedSlotItemIndex);

            DataContext = buttonActionEditVM;
        }

        private void UnboundBtn_Click(object sender, RoutedEventArgs e)
        {
            DataContext = null;

            buttonActionEditVM.AssignUnbound();

            DataContext = buttonActionEditVM;
        }
    }
}
