using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest
{
    public static class ProfileDeviceSettingsFactory
    {
        public static ProfileDeviceSettings CreateDeviceSettings(InputDeviceType deviceType,
            Profile tempProfile)
        {
            ProfileDeviceSettings result = null;
            switch(deviceType)
            {
                case InputDeviceType.SteamController:
                    {
                        result = new SteamControllerProfileDeviceSettings(tempProfile);
                    }

                    break;
                case InputDeviceType.DS4:
                    {
                        result = new DS4ProfileDeviceSettings(tempProfile);
                    }

                    break;
                default: break;
            }
            return result;
        }
    }
}
