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
using System.Windows.Shapes;
using DS4MapperTest.ViewModels;
using DS4MapperTest.SteamControllerLibrary;

namespace DS4MapperTest
{
    /// <summary>
    /// Interaction logic for ControllerConfigWin.xaml
    /// </summary>
    public partial class ControllerConfigWin : Window
    {
        private bool dirty;
        private ControllerConfigViewModel controlConfigVM;

        public ControllerConfigWin()
        {
            InitializeComponent();
        }

        public void PostInit(SteamControllerDevice device)
        {
            controlConfigVM = new ControllerConfigViewModel(device);

            steamControllerTabItem.DataContext = controlConfigVM;
            controlConfigVM.ControlOptions.LeftTouchpadRotationChanged += ControlOptions_OptionChanged;
            controlConfigVM.ControlOptions.RightTouchpadRotationChanged += ControlOptions_OptionChanged;
        }

        private void ControlOptions_OptionChanged(object sender, EventArgs e)
        {
            dirty = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (dirty)
            {
                AppGlobalDataSingleton.Instance.SaveControllerDeviceSettings(controlConfigVM.Device,
                    controlConfigVM.Device.DeviceOptions);
            }

            controlConfigVM.ControlOptions.LeftTouchpadRotationChanged -= ControlOptions_OptionChanged;
            controlConfigVM.ControlOptions.RightTouchpadRotationChanged -= ControlOptions_OptionChanged;

            steamControllerTabItem.DataContext = null;
        }
    }
}
