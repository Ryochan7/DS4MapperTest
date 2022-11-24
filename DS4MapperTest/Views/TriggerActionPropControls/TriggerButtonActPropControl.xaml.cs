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
using DS4MapperTest.ButtonActions;

namespace DS4MapperTest.Views.TriggerActionPropControls
{
    /// <summary>
    /// Interaction logic for TriggerButtonActPropControl.xaml
    /// </summary>
    public partial class TriggerButtonActPropControl : UserControl
    {
        public class TriggerButtonBindingArgs : EventArgs
        {
            private AxisDirButton actionBtn;
            public AxisDirButton ActionBtn => actionBtn;

            private bool realAction;
            public bool RealAction => realAction;

            public delegate void UpdateActionHandler(ButtonAction oldAction, ButtonAction newAction);
            private UpdateActionHandler updateActHandler;
            public UpdateActionHandler UpdateActHandler => updateActHandler;

            public TriggerButtonBindingArgs(AxisDirButton actionBtn, bool realAction, UpdateActionHandler updateActDel)
            {
                this.actionBtn = actionBtn;
                this.realAction = realAction;
                this.updateActHandler = updateActDel;
            }
        }

        private TriggerButtonActPropViewModel trigBtnActVM;
        public TriggerButtonActPropViewModel TrigBtnActVM => trigBtnActVM;

        public event EventHandler<int> ActionTypeIndexChanged;
        public event EventHandler<TriggerButtonBindingArgs> RequestFuncEditor;

        public TriggerButtonActPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TriggerMapAction action)
        {
            trigBtnActVM = new TriggerButtonActPropViewModel(mapper, action);
            DataContext = trigBtnActVM;

            triggerSelectControl.PostInit(mapper, action);
            triggerSelectControl.TrigActionSelVM.SelectedIndexChanged += TrigActionSelVM_SelectedIndexChanged;
        }

        public void RefreshView()
        {
            // Force re-eval of all bindings
            //DataContext = null;
            //DataContext = trigBtnActVM;
            btnEditOpenTest.GetBindingExpression(Button.ContentProperty).UpdateTarget();
        }

        private void TrigActionSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                triggerSelectControl.TrigActionSelVM.SelectedIndex);
        }

        private void btnEditOpenTest_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new TriggerButtonBindingArgs(trigBtnActVM.Action.EventButton,
                !trigBtnActVM.Action.UseParentEventButton,
                trigBtnActVM.UpdateEventButton));
        }
    }
}
