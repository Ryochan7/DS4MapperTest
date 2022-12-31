using DS4MapperTest.DS4Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DS4MapperTest
{
    public enum LightbarMode : ushort
    {
        SolidColor,
        Rainbow,
        Flashing,
        Pulse,
        Battery,
        Passthru,
    }

    public class LightbarSettings
    {
        public const int RAINBOW_SECONDS_CYCLE_DEFAULT = 5;

        public LightbarMode Mode = LightbarMode.SolidColor;
        public DS4Library.DS4Color SolidColor = new DS4Library.DS4Color();
        public DS4Library.DS4Color FlashColor = new DS4Library.DS4Color();
        public DS4Library.DS4Color BatteryFullColor = new DS4Library.DS4Color();
        public int rainbowSecondsCycle = RAINBOW_SECONDS_CYCLE_DEFAULT;
        public DS4Library.DS4Color PulseColor = new DS4Library.DS4Color();
    }

    public class LightbarProcessor
    {
        enum FadeDirection : byte
        {
            In,
            Out,
        }

        private const int PULSE_FLASH_DURATION = 2000;
        private const double PULSE_FLASH_SEGMENTS = PULSE_FLASH_DURATION / 40;

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
        private FadeDirection fadedirection;
        private Stopwatch fadewatch = new Stopwatch();

        public void Reset()
        {
            useOverrideColor = false;
            overrideColor = new DS4Color(0, 0, 0);
            rainbowCounter = 0;
            oldCheckDateTime = DateTime.UtcNow;
            fadedirection = FadeDirection.In;
            fadewatch.Reset();
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
                case LightbarMode.Rainbow:
                    {
                        DateTime now = DateTime.UtcNow;
                        double secsCycle = profile.LightbarSettings.rainbowSecondsCycle;
                        if (secsCycle > 0)
                        {
                            if (now >= oldCheckDateTime + TimeSpan.FromMilliseconds(10.0))
                            {
                                int diffMs = now.Subtract(oldCheckDateTime).Milliseconds;
                                oldCheckDateTime = now;

                                rainbowCounter += 360.0 * (double)(diffMs / 1000.0 / secsCycle);
                                rainbowCounter = rainbowCounter % 360.0;
                                useColor = HSVToRGB((float)rainbowCounter, 1.0f, 1.0f);
                                updateColor = true;
                            }
                        }
                        else
                        {
                            if (!device.LightbarColor.Equals(profile.LightbarSettings.SolidColor))
                            {
                                useColor = profile.LightbarSettings.SolidColor;
                                updateColor = true;
                            }
                        }
                    }

                    break;
                case LightbarMode.Pulse:
                    {
                        double ratio = 0.0;

                        if (!fadewatch.IsRunning)
                        {
                            fadewatch.Restart();
                            switch(fadedirection)
                            {
                                case FadeDirection.In:
                                    fadedirection = FadeDirection.Out;
                                    break;
                                case FadeDirection.Out:
                                    fadedirection = FadeDirection.In;
                                    break;
                                default: break;
                            }

                            ratio = fadedirection == FadeDirection.Out ? 100.0 : 0.0;
                        }
                        else
                        {
                            long elapsed = fadewatch.ElapsedMilliseconds;
                            if (fadedirection == FadeDirection.In)
                            {
                                if (elapsed < PULSE_FLASH_DURATION)
                                {
                                    elapsed = elapsed / 40;
                                    if (elapsed > PULSE_FLASH_SEGMENTS)
                                        elapsed = (long)PULSE_FLASH_SEGMENTS;
                                    ratio = 100.0 * (elapsed / PULSE_FLASH_SEGMENTS);
                                }
                                else
                                {
                                    ratio = 100.0;
                                    fadewatch.Stop();
                                }
                            }
                            else
                            {
                                if (elapsed < PULSE_FLASH_DURATION)
                                {
                                    elapsed = elapsed / 40;
                                    if (elapsed > PULSE_FLASH_SEGMENTS)
                                        elapsed = (long)PULSE_FLASH_SEGMENTS;
                                    ratio = (0 - 100.0) * (elapsed / PULSE_FLASH_SEGMENTS) + 100.0;
                                }
                                else
                                {
                                    ratio = 0.0;
                                    fadewatch.Stop();
                                }
                            }

                            useColor = RatioColor(profile.LightbarSettings.PulseColor,
                                ratio / 100.0);
                            if (!device.LightbarColor.Equals(useColor))
                            {
                                updateColor = true;
                            }
                        }
                    }

                    break;
                case LightbarMode.Battery:
                    {
                        useColor =
                            RatioColor(profile.LightbarSettings.BatteryFullColor,
                            device.Battery / 100.0);

                        if (!device.LightbarColor.Equals(useColor))
                        {
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
                        double secsCycle = profile.LightbarSettings.rainbowSecondsCycle;
                        if (secsCycle > 0)
                        {
                            if (now >= oldCheckDateTime + TimeSpan.FromMilliseconds(10.0))
                            {
                                int diffMs = now.Subtract(oldCheckDateTime).Milliseconds;
                                oldCheckDateTime = now;

                                rainbowCounter += 360.0 * (double)(diffMs / 1000.0 / secsCycle);
                                rainbowCounter = rainbowCounter % 360.0;
                                useColor = HSVToRGB((float)rainbowCounter, 1.0f, 1.0f);
                                updateColor = true;
                            }
                        }
                        else
                        {
                            if (!device.LightbarColor.Equals(profile.LightbarSettings.SolidColor))
                            {
                                useColor = profile.LightbarSettings.SolidColor;
                                updateColor = true;
                            }
                        }
                    }

                    break;
                case LightbarMode.Battery:
                    {
                        useColor =
                            RatioColor(profile.LightbarSettings.BatteryFullColor,
                            device.Battery / 100.0);

                        if (!device.LightbarColor.Equals(useColor))
                        {
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

        // Convert RGB value from DS4Color to HSV. Alter V value and
        // return converted color as RGB DS4Color struct
        private DS4Color RatioColor(DS4Color color, double ratio)
        {
            // Adapted from equation for RGB to HSV documented on
            // https://www.rapidtables.com/convert/color/rgb-to-hsv.html
            double rPrime = color.red / 255.0;
            double gPrime = color.green / 255.0;
            double bPrime = color.blue / 255.0;
            double max = Math.Max(rPrime, Math.Max(gPrime, bPrime));
            double min = Math.Min(rPrime, Math.Min(gPrime, bPrime));

            double delta = max - min;
            double hue = 0.0;
            double sat = 0.0;
            double val = 0.0;
            if (delta == 0)
            {
                hue = 0;
            }
            else if (max == rPrime)
            {
                hue = 60.0 * ((gPrime - bPrime) / delta % 6);
            }
            else if (max == gPrime)
            {
                hue = 60.0 * ((bPrime - rPrime) / delta + 2);
            }
            else if (max == bPrime)
            {
                hue = 60.0 * ((rPrime - gPrime) / delta + 4);
            }

            sat = max == 0 ? 0 : (delta / max);
            val = max;
            double updatedVal = val * ratio;
            //Trace.WriteLine($"H{hue} S{sat} B{updatedVal}");
            return HSVToRGB((float)hue, (float)sat, (float)updatedVal);
        }

        //public Color RGBToHSV(ref DS4Color color)
        //{
        //    DS4Color result = new DS4Color();
        //    return result;
        //    double ratioR = color.red / 255.0;
        //    double ratioG = color.green / 255.0;
        //    double ratioB = color.blue / 255.0;

        //    double max = Math.Max(ratioR, Math.Max(ratioG, ratioB));
        //    double min = Math.Min(ratioR, Math.Min(ratioG, ratioB));
        //    double C = max - min;

        //}

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
            result.blue = (byte)((bPrime + m) * 255);
            return result;
        }
    }
}
