using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.StickActions;
using DS4MapperTest.ViewModels.DPadActionPropViewModels;
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

namespace DS4MapperTest.Views.DPadActionPropControls
{
    /// <summary>
    /// Interaction logic for DPadActionPadPropControl.xaml
    /// </summary>
    public partial class DPadActionPadPropControl : UserControl
    {
        public class DPadDirButtonBindingArgs : EventArgs
        {
            private ButtonAction dirBtn;
            public ButtonAction DirBtn => dirBtn;

            private bool realAction;
            public bool RealAction => realAction;

            public delegate void UpdateActionHandler(ButtonAction oldAction, ButtonAction newAction);
            private UpdateActionHandler updateActHandler;
            public UpdateActionHandler UpdateActHandler => updateActHandler;

            public DPadDirButtonBindingArgs(ButtonAction dirBtn, bool realAction, UpdateActionHandler updateActDel)
            {
                this.dirBtn = dirBtn;
                this.realAction = realAction;
                this.updateActHandler = updateActDel;
            }
        }

        private DPadActionPadPropViewModel dpadActVM;
        public DPadActionPadPropViewModel DPadActVM => dpadActVM;

        public event EventHandler<int> ActionTypeIndexChanged;
        public event EventHandler<DPadDirButtonBindingArgs> RequestFuncEditor;

        public DPadActionPadPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, DPadMapAction action)
        {
            dpadActVM = new DPadActionPadPropViewModel(mapper, action);
            DataContext = dpadActVM;

            dpadSelectControl.PostInit(mapper, action);
            dpadSelectControl.DPadActSelVM.SelectedIndexChanged += DPadActSelVM_SelectedIndexChanged;
        }

        public void RefreshView()
        {
            // Force re-eval of bindings
            DataContext = null;
            DataContext = dpadActVM;
        }

        private void DPadActSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                dpadSelectControl.DPadActSelVM.SelectedIndex);
        }

        private void btnUpEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DPadDirButtonBindingArgs(dpadActVM.Action.EventCodes4[(int)DpadDirections.Up],
                !dpadActVM.Action.UsingParentActionButton[(int)DpadDirections.Up],
                dpadActVM.UpdateUpDirAction));
        }

        private void btnDownEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DPadDirButtonBindingArgs(dpadActVM.Action.EventCodes4[(int)DpadDirections.Down],
                !dpadActVM.Action.UsingParentActionButton[(int)DpadDirections.Down],
                dpadActVM.UpdateDownDirAction));
        }

        private void btnLeftEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DPadDirButtonBindingArgs(dpadActVM.Action.EventCodes4[(int)DpadDirections.Left],
                !dpadActVM.Action.UsingParentActionButton[(int)DpadDirections.Left],
                dpadActVM.UpdateLeftDirAction));
        }

        private void btnRightEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DPadDirButtonBindingArgs(dpadActVM.Action.EventCodes4[(int)DpadDirections.Right],
                !dpadActVM.Action.UsingParentActionButton[(int)DpadDirections.Right],
                dpadActVM.UpdateRightDirAction));
        }

        private void btnUpLeftEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DPadDirButtonBindingArgs(dpadActVM.Action.EventCodes4[(int)DpadDirections.UpLeft],
                !dpadActVM.Action.UsingParentActionButton[(int)DpadDirections.UpLeft],
                dpadActVM.UpdateUpLeftDirAction));
        }

        private void btnUpRightEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DPadDirButtonBindingArgs(dpadActVM.Action.EventCodes4[(int)DpadDirections.UpRight],
                !dpadActVM.Action.UsingParentActionButton[(int)DpadDirections.UpRight],
                dpadActVM.UpdateUpRightDirAction));
        }

        private void btnDownLeftEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DPadDirButtonBindingArgs(dpadActVM.Action.EventCodes4[(int)DpadDirections.DownLeft],
                !dpadActVM.Action.UsingParentActionButton[(int)DpadDirections.DownLeft],
                dpadActVM.UpdateDownLeftDirAction));
        }

        private void btnDownRightEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DPadDirButtonBindingArgs(dpadActVM.Action.EventCodes4[(int)DpadDirections.DownRight],
                !dpadActVM.Action.UsingParentActionButton[(int)DpadDirections.DownRight],
                dpadActVM.UpdateDownRightDirAction));
        }
    }
}
