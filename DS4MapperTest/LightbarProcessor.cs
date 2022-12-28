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
        private bool useOverrideColor;
        public bool UserOverrideColor
        {
            get => useOverrideColor;
            set => useOverrideColor = value;
        }
        private DS4Color overrideColor = new DS4Color(0, 0, 0);
        public DS4Color OverrideColor => overrideColor;
        public ref DS4Color OverrideColorRef
        {
            get => ref overrideColor;
        }

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
            DS4Color useColor = new DS4Color();
            bool updateColor = false;

            switch (profile.LightbarSettings.Mode)
            {
                case LightbarMode.SolidColor:
                    {
                        if (useOverrideColor)
                        {
                            if (!device.LightbarColor.Equals(overrideColor))
                            {
                                useColor = overrideColor;
                                updateColor = true;
                            }
                        }
                        else if (!device.LightbarColor.Equals(profile.LightbarSettings.SolidColor))
                        {
                            useColor = profile.LightbarSettings.SolidColor;
                            updateColor = true;
                        }
                    }

                    break;
                default: break;
            }

            if (updateColor)
            {
                device.SetLightbarColor(ref useColor);
                device.HapticsDirty = true;
            }
        }

        public void UpdateLightbarDS(DualSense.DualSenseDevice device, Profile profile)
        {
            DS4Color useColor = new DS4Color();
            bool updateColor = false;

            switch (profile.LightbarSettings.Mode)
            {
                case LightbarMode.SolidColor:
                    {
                        if (useOverrideColor)
                        {
                            if (!device.LightbarColor.Equals(overrideColor))
                            {
                                useColor = overrideColor;
                                updateColor = true;
                            }
                        }
                        else if (!device.LightbarColor.Equals(profile.LightbarSettings.SolidColor))
                        {
                            useColor = profile.LightbarSettings.SolidColor;
                            updateColor = true;
                        }
                    }

                    break;
                default: break;
            }

            if (updateColor)
            {
                device.SetLightbarColor(ref useColor);
                device.HapticsDirty = true;
            }
        }
    }
}
