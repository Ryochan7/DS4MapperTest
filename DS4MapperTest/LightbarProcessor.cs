using DS4MapperTest.DS4Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest
{
    public enum LightbarMode : ushort
    {
        SolidColor,
        Rainbow,
        Flashing,
        Passthru,
    }

    public class LightbarSettings
    {
        public LightbarMode Mode = LightbarMode.SolidColor;
        public DS4Library.DS4Color SolidColor = new DS4Library.DS4Color();
        public DS4Library.DS4Color FlashColor = new DS4Library.DS4Color();
    }

    public class LightbarProcessor
    {
        public void UpdateLightbar(InputDeviceBase device, Profile profile)
        {
            switch(profile.LightbarSettings.Mode)
            {
                case LightbarMode.SolidColor:
                    {

                    }

                    break;
                default: break;
            }
        }

        public void UpdateLightbarDS4(DS4Device device, Profile profile)
        {
            switch (profile.LightbarSettings.Mode)
            {
                case LightbarMode.SolidColor:
                    {
                        if (!device.LightbarColor.Equals(profile.LightbarSettings.SolidColor))
                        {
                            device.SetLightbarColor(ref profile.LightbarSettings.SolidColor);
                            device.HapticsDirty = true;
                        }
                    }

                    break;
                default: break;
            }
        }

        public void UpdateLightbarDS(DualSense.DualSenseDevice device, Profile profile)
        {
            switch (profile.LightbarSettings.Mode)
            {
                case LightbarMode.SolidColor:
                    {
                        if (!device.LightbarColor.Equals(profile.LightbarSettings.SolidColor))
                        {
                            device.SetLightbarColor(ref profile.LightbarSettings.SolidColor);
                            device.HapticsDirty = true;
                        }
                    }

                    break;
                default: break;
            }
        }
    }
}
