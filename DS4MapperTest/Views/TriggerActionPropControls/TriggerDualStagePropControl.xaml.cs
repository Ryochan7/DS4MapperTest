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
using DS4MapperTest.ActionUtil;

namespace DS4MapperTest.Views.TriggerActionPropControls
{
    /// <summary>
    /// Interaction logic for TriggerDualStagePropControl.xaml
    /// </summary>
    public partial class TriggerDualStagePropControl : UserControl
    {
        public class DualStageBindingArgs : EventArgs
        {
            private AxisDirButton pullBtn;
            public AxisDirButton PullBtn => pullBtn;

            private bool realAction;
            public bool RealAction => realAction;

            public delegate void UpdateActionHandler(ButtonAction oldAction, ButtonAction newAction);
            private UpdateActionHandler updateActHandler;
            public UpdateActionHandler UpdateActHandler => updateActHandler;

            public DualStageBindingArgs(AxisDirButton pullBtn, bool realAction, UpdateActionHandler updateActDel)
            {
                this.pullBtn = pullBtn;
                this.realAction = realAction;
                this.updateActHandler = updateActDel;
            }
        }

        private TriggerDualStagePropViewModel trigDualStagePropVM;
        public TriggerDualStagePropViewModel TrigDualStagePropVM => trigDualStagePropVM;

        public event EventHandler<int> ActionTypeIndexChanged;
        public event EventHandler<DualStageBindingArgs> RequestFuncEditor;

        public TriggerDualStagePropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TriggerMapAction action)
        {
            trigDualStagePropVM = new TriggerDualStagePropViewModel(mapper, action);
            DataContext = trigDualStagePropVM;

            triggerSelectControl.PostInit(mapper, action);
            triggerSelectControl.TrigActionSelVM.SelectedIndexChanged += TrigActionSelVM_SelectedIndexChanged;
        }

        public void RefreshView()
        {
            btnEditOpenSoftTest.GetBindingExpression(Button.ContentProperty).UpdateTarget();
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
                new DualStageBindingArgs(trigDualStagePropVM.Action.FullPullActButton,
                !trigDualStagePropVM.Action.UseParentFullPullBtn,
                trigDualStagePropVM.UpdateFullPullAction));
        }

        private void btnEditOpenSoftTest_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DualStageBindingArgs(trigDualStagePropVM.Action.SoftPullActButton,
                !trigDualStagePropVM.Action.UseParentSoftPullBtn,
                trigDualStagePropVM.UpdateSoftPullAction));
        }
    }
}
