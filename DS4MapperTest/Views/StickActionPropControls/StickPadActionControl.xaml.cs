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
using DS4MapperTest.ViewModels.StickActionPropViewModels;
using DS4MapperTest.StickActions;
using DS4MapperTest.ButtonActions;

namespace DS4MapperTest.Views.StickActionPropControls
{
    /// <summary>
    /// Interaction logic for StickPadActionControl.xaml
    /// </summary>
    public partial class StickPadActionControl : UserControl
    {
        public class DirButtonBindingArgs : EventArgs
        {
            private AxisDirButton dirBtn;
            public AxisDirButton DirBtn => dirBtn;

            private bool realAction;
            public bool RealAction => realAction;

            public delegate void UpdateActionHandler(ButtonAction oldAction, ButtonAction newAction);
            private UpdateActionHandler updateActHandler;
            public UpdateActionHandler UpdateActHandler => updateActHandler;

            public DirButtonBindingArgs(AxisDirButton dirBtn, bool realAction, UpdateActionHandler updateActDel)
            {
                this.dirBtn = dirBtn;
                this.realAction = realAction;
                this.updateActHandler = updateActDel;
            }
        }

        private StickPadActionPropViewModel stickPadActVM;
        public StickPadActionPropViewModel StickPadActVM => stickPadActVM;

        public event EventHandler<int> ActionTypeIndexChanged;
        public event EventHandler<DirButtonBindingArgs> RequestFuncEditor;

        public StickPadActionControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, StickMapAction action)
        {
            stickPadActVM = new StickPadActionPropViewModel(mapper, action);
            DataContext = stickPadActVM;

            stickSelectControl.PostInit(mapper, action);
            stickSelectControl.StickActSelVM.SelectedIndexChanged += StickActSelVM_SelectedIndexChanged;
        }

        public void RefreshView()
        {
            // Force re-eval of bindings
            DataContext = null;
            DataContext = stickPadActVM;
        }

        private void StickActSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                stickSelectControl.StickActSelVM.SelectedIndex);
        }

        private void btnUpEdit_Click(object sender, RoutedEventArgs e)
        {
            bool fuckery = stickPadActVM.Action.UsingParentActionButton[(int)StickPadAction.DpadDirections.Up];

            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(stickPadActVM.Action.EventCodes4[(int)StickPadAction.DpadDirections.Up],
                !stickPadActVM.Action.UsingParentActionButton[(int)StickPadAction.DpadDirections.Up],
                stickPadActVM.UpdateUpDirAction));
        }

        private void btnDownEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(stickPadActVM.Action.EventCodes4[(int)StickPadAction.DpadDirections.Down],
                !stickPadActVM.Action.UsingParentActionButton[(int)StickPadAction.DpadDirections.Down],
                stickPadActVM.UpdateDownDirAction));
        }

        private void btnLeftEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(stickPadActVM.Action.EventCodes4[(int)StickPadAction.DpadDirections.Left],
                !stickPadActVM.Action.UsingParentActionButton[(int)StickPadAction.DpadDirections.Left],
                stickPadActVM.UpdateLeftDirAction));
        }

        private void btnRightEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(stickPadActVM.Action.EventCodes4[(int)StickPadAction.DpadDirections.Right],
                !stickPadActVM.Action.UsingParentActionButton[(int)StickPadAction.DpadDirections.Right],
                stickPadActVM.UpdateRightDirAction));
        }

        private void btnUpLeftEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(stickPadActVM.Action.EventCodes4[(int)StickPadAction.DpadDirections.UpLeft],
                !stickPadActVM.Action.UsingParentActionButton[(int)StickPadAction.DpadDirections.UpLeft],
                stickPadActVM.UpdateUpLeftDirAction));
        }

        private void btnUpRightEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(stickPadActVM.Action.EventCodes4[(int)StickPadAction.DpadDirections.UpRight],
                !stickPadActVM.Action.UsingParentActionButton[(int)StickPadAction.DpadDirections.UpRight],
                stickPadActVM.UpdateUpRightDirAction));
        }

        private void btnDownLeftEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(stickPadActVM.Action.EventCodes4[(int)StickPadAction.DpadDirections.DownLeft],
                !stickPadActVM.Action.UsingParentActionButton[(int)StickPadAction.DpadDirections.DownLeft],
                stickPadActVM.UpdateDownLeftDirAction));
        }

        private void btnDownRightEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(stickPadActVM.Action.EventCodes4[(int)StickPadAction.DpadDirections.DownRight],
                !stickPadActVM.Action.UsingParentActionButton[(int)StickPadAction.DpadDirections.DownRight],
                stickPadActVM.UpdateDownRightDirAction));
        }
    }
}
