using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest
{
    public abstract class ProfileDeviceSettings
    {
    }

    public class SteamControllerProfileDeviceSettings : ProfileDeviceSettings
    {
        public SteamControllerProfileDeviceSettings(Profile tempProfile)
        {
        }
    }

    public class DS4ProfileDeviceSettings : ProfileDeviceSettings
    {
        public DS4ProfileDeviceSettings(Profile tempProfile)
        {
        }
    }
}
