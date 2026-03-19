using DS4MapperTest.ButtonActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.ViewModels.TouchpadActionPropViewModels;
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

namespace DS4MapperTest.Views.TouchpadActionPropControls
{
    /// <summary>
    /// Interaction logic for TouchpadStickActionPropControl.xaml
    /// </summary>
    public partial class TouchpadStickActionPropControl : UserControl
    {
        public class DirButtonBindingArgs : EventArgs
        {
            private ButtonAction dirBtn;
            public ButtonAction DirBtn => dirBtn;

            private bool realAction = false;
            public bool RealAction => realAction;

            public delegate void UpdateActionHandler(ButtonAction oldAction, ButtonAction newAction);
            private UpdateActionHandler updateActHandler;
            public UpdateActionHandler UpdateActHandler => updateActHandler;

            public DirButtonBindingArgs(ButtonAction dirBtn, bool realAction = false, UpdateActionHandler updateActDel = null)
            {
                this.dirBtn = dirBtn;
                this.realAction = realAction;
                this.updateActHandler = updateActDel;
            }
        }

        private TouchpadStickActionPropViewModel touchStickPropVM;
        public TouchpadStickActionPropViewModel TouchStickPropVM => touchStickPropVM;

        public event EventHandler<DirButtonBindingArgs> RequestFuncEditor;

        public TouchpadStickActionPropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TouchpadMapAction action)
        {
            touchStickPropVM = new TouchpadStickActionPropViewModel(mapper, action);

            DataContext = touchStickPropVM;
        }

        public void RefreshView()
        {
            // Force re-eval of bindings
            DataContext = null;
            DataContext = touchStickPropVM;
        }

        private void btnEditTest_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(touchStickPropVM.Action.RingButton,
                !touchStickPropVM.Action.UseParentRingButton,
                touchStickPropVM.UpdateRingButton));
        }
    }
}
