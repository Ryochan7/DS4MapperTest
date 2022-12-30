using DS4MapperTest.DS4Library;
using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
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
        public int rainbowSecondsCycle = 1;
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

        private double rainbowCounter;
        private DateTime oldCheckDateTime;

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
                case LightbarMode.Rainbow:
                    {
                        DateTime now = DateTime.UtcNow;
                        double cycles = profile.LightbarSettings.rainbowSecondsCycle;
                        if (now >= oldCheckDateTime + TimeSpan.FromMilliseconds(10.0))
                        {
                            int diffMs = now.Subtract(oldCheckDateTime).Milliseconds;
                            oldCheckDateTime = now;

                            rainbowCounter += 360.0 * (double)(diffMs / 1000.0 / cycles);
                            rainbowCounter = rainbowCounter % 360.0;
                            useColor = HSVToRGB((float)rainbowCounter, 1.0f, 1.0f);
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
                case LightbarMode.Rainbow:
                    {
                        DateTime now = DateTime.UtcNow;
                        double cycles = profile.LightbarSettings.rainbowSecondsCycle;
                        if (now >= oldCheckDateTime + TimeSpan.FromMilliseconds(10.0))
                        {
                            int diffMs = now.Subtract(oldCheckDateTime).Milliseconds;
                            oldCheckDateTime = now;

                            rainbowCounter += 360.0 * (double)(diffMs / 1000.0 / cycles);
                            rainbowCounter = rainbowCounter % 360.0;
                            useColor = HSVToRGB((float)rainbowCounter, 1.0f, 1.0f);
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

        public DS4Color HSVToRGB(float hue, float sat, float val)
        {
            // Adapted from equation for HSV to RGB documented on
            // https://www.rapidtables.com/convert/color/hsv-to-rgb.html
            DS4Color result = new DS4Color();
            double C = sat * val;
            double hPrime = (hue / 60.0) % 6.0;
            double X = C * (1 - (Math.Abs((hPrime) % 2 - 1)));
            double m = val - C;
            double rPrime = 0.0, gPrime = 0.0, bPrime = 0.0;
#if DEBUG
            Trace.WriteLine($"H {hue} H'{hPrime} C{C} X{X}");
#endif

            switch((int)hPrime)
            {
                case 0:
                    rPrime = C; gPrime = X; bPrime = 0;
                    break;
                case 1:
                    rPrime = X; gPrime = C; bPrime = 0;
                    break;
                case 2:
                    rPrime = 0; gPrime = C; bPrime = X;
                    break;
                case 3:
                    rPrime = 0; gPrime = X; bPrime = C;
                    break;
                case 4:
                    rPrime = X; gPrime = 0; bPrime = C;
                    break;
                case 5:
                    rPrime = C; gPrime = 0; bPrime = X;
                    break;
                default: break;
            }

            result.red = (byte)((rPrime + m) * 255);
            result.green = (byte)((gPrime + m) * 255);
            result.green = (byte)((bPrime + m) * 255);
            return result;
        }
    }
}
