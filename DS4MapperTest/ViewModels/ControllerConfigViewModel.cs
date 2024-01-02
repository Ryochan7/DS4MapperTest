using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.SteamControllerLibrary;

namespace DS4MapperTest.ViewModels
{
    public class ControllerConfigViewModel
    {
        private SteamControllerDevice device;
        public SteamControllerDevice Device
        {
            get => device;
        }

        private SteamControllerControllerOptions controlOptions;
        public SteamControllerControllerOptions ControlOptions
        {
            get => controlOptions;
        }

        public ControllerConfigViewModel(SteamControllerDevice device)
        {
            this.device = device;
            this.controlOptions = device.NativeDeviceOptions;
        }
    }
}
