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
using DS4MapperTest.GyroActions;
using DS4MapperTest.ViewModels.GyroActionPropViewModels;
using static DS4MapperTest.Views.TouchpadActionPropControls.TouchpadActionPadPropControl;

namespace DS4MapperTest.Views.GyroActionPropControls
{
    /// <summary>
    /// Interaction logic for GyroDirSwipePropControl.xaml
    /// </summary>
    public partial class GyroDirSwipePropControl : UserControl
    {
        private GyroDirSwipeActionPropViewModel gyroDirSwipeVM;
        public GyroDirSwipeActionPropViewModel GyroDirSwipeVM => gyroDirSwipeVM;

        public event EventHandler<DirButtonBindingArgs> RequestFuncEditor;

        public event EventHandler<int> ActionTypeIndexChanged;

        public GyroDirSwipePropControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, GyroMapAction action)
        {
            gyroDirSwipeVM = new GyroDirSwipeActionPropViewModel(mapper, action);
            DataContext = gyroDirSwipeVM;

            gyroSelectControl.PostInit(mapper, action);
            gyroSelectControl.GyroActSelVM.SelectedIndexChanged += GyroActSelVM_SelectedIndexChanged; ;
        }

        public void RefreshView()
        {
            // Force re-eval of bindings
            DataContext = null;
            DataContext = gyroDirSwipeVM;
        }

        private void GyroActSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActionTypeIndexChanged?.Invoke(this,
                gyroSelectControl.GyroActSelVM.SelectedIndex);
        }

        private void BtnUpEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(gyroDirSwipeVM.Action.UsedEventsButtonsY[(int)GyroDirectionalSwipe.SwipeAxisYDir.Up],
                !gyroDirSwipeVM.Action.UseParentDataY[(int)GyroDirectionalSwipe.SwipeAxisYDir.Up],
                gyroDirSwipeVM.UpdateUpDirButton));
        }

        private void BtnDownEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(gyroDirSwipeVM.Action.UsedEventsButtonsY[(int)GyroDirectionalSwipe.SwipeAxisYDir.Down],
                !gyroDirSwipeVM.Action.UseParentDataY[(int)GyroDirectionalSwipe.SwipeAxisYDir.Down],
                gyroDirSwipeVM.UpdateDownDirButton));
        }

        private void BtnLeftEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(gyroDirSwipeVM.Action.UsedEventsButtonsX[(int)GyroDirectionalSwipe.SwipeAxisXDir.Left],
                !gyroDirSwipeVM.Action.UseParentDataX[(int)GyroDirectionalSwipe.SwipeAxisXDir.Left],
                gyroDirSwipeVM.UpdateLeftDirButton));
        }

        private void BtnRightEdit_Click(object sender, RoutedEventArgs e)
        {
            RequestFuncEditor?.Invoke(this,
                new DirButtonBindingArgs(gyroDirSwipeVM.Action.UsedEventsButtonsX[(int)GyroDirectionalSwipe.SwipeAxisXDir.Right],
                !gyroDirSwipeVM.Action.UseParentDataX[(int)GyroDirectionalSwipe.SwipeAxisXDir.Right],
                gyroDirSwipeVM.UpdateRightDirButton));
        }
    }
}
