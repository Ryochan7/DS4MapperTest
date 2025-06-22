using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.GyroActions;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest
{
    public abstract class DeviceActionDefaultsCreator
    {
        public struct TouchJoystickActionValues
        {
            public double deadZone;
            public double antiDeadZone;
            public double maxZone;

            public void Process(TouchpadStickAction action)
            {
                action.DeadMod.DeadZone = deadZone;
                action.DeadMod.AntiDeadZone = antiDeadZone;
                action.DeadMod.MaxZone = maxZone;

                action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE);
                action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.ANTIDEAD_ZONE);
                action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.MAX_ZONE);
            }
        }

        public struct TouchMouseActionValues
        {
            public int deadZone;

            public void Process(TouchpadMouse action)
            {
                action.DeadZone = deadZone;

                action.ChangedProperties.Add(TouchpadMouse.PropertyKeyStrings.DEAD_ZONE);
            }
        }

        public struct TouchMouseJoystickActionValues
        {
            public int deadZone;
            public int maxZone;

            public void Process(TouchpadMouseJoystick action)
            {
                action.MStickParams.deadZone = deadZone;
                action.MStickParams.maxZone = maxZone;

                action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.DEAD_ZONE);
                action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.MAX_ZONE);
            }
        }

        public struct TouchActionPadActionValues
        {
            public double deadZone;

            public void Process(TouchpadActionPad action)
            {
                action.DeadMod.DeadZone = deadZone;

                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.DEAD_ZONE);
            }
        }

        public struct TouchCircularActionValues
        {
            public double deadZone;
            public MapAction.HapticsIntensity hapticsIntensity;

            public void Process(TouchpadCircular action)
            {
                action.DeadMod.DeadZone = deadZone;
                action.ActionHapticsIntensity = hapticsIntensity;

                action.ChangedProperties.Add(TouchpadCircular.PropertyKeyStrings.HAPTICS_INTENSITY);

                //action.ChangedProperties.Add(TouchpadCircular.PropertyKeyStrings.DEAD_ZONE);
            }
        }

        public struct TouchDirectionalSwipeActionValues
        {
            public int deadZoneX;
            public int deadZoneY;
            public int delayTime;

            public void Process(TouchpadDirectionalSwipe action)
            {
                action.swipeParams.deadzoneX = deadZoneX;
                action.swipeParams.deadzoneY = deadZoneY;
                action.swipeParams.delayTime = delayTime;

                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_X);
                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_Y);
                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.DELAY_TIME);
            }
        }

        public struct StickTranslateActionValues
        {
            public double deadZone;
            public double antiDeadZone;
            public double maxZone;

            public StickTranslateActionValues()
            {
                maxZone = 1.0;
            }

            public void Process(StickTranslate action)
            {
                action.DeadMod.DeadZone = deadZone;
                action.DeadMod.AntiDeadZone = antiDeadZone;
                action.DeadMod.MaxZone = maxZone;

                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.DEAD_ZONE);
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.MAX_ZONE);
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.ANTIDEAD_ZONE);
            }
        }

        public struct StickMouseActionValues
        {
            public double deadZone;

            public void Process(StickMouse action)
            {
                action.DeadMod.DeadZone = deadZone;

                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.DEAD_ZONE);
            }
        }

        public struct StickPadActionActionValues
        {
            public double deadZone;

            public void Process(StickPadAction action)
            {
                action.DeadMod.DeadZone = deadZone;

                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.DEAD_ZONE);
            }
        }

        public struct StickCircularActionValues
        {
            public double deadZone;
            public MapAction.HapticsIntensity hapticsIntensity;

            public void Process(StickCircular action)
            {
                action.DeadMod.DeadZone = deadZone;
                action.ActionHapticsIntensity = hapticsIntensity;

                action.ChangedProperties.Add(StickCircular.PropertyKeyStrings.HAPTICS_INTENSITY);

                //action.ChangedProperties.Add(StickCircular.PropertyKeyStrings.DEAD_ZONE);
            }
        }

        public struct GyroMouseActionValues
        {
            public int deadzone;

            public void Process(GyroMouse action)
            {
                action.mouseParams.deadzone = deadzone;
            }
        }

        public abstract TouchJoystickActionValues GrabTouchJoystickDefaults();
        public abstract TouchMouseActionValues GrabTouchMouseDefaults();
        public abstract TouchMouseJoystickActionValues GrabTouchMouseJoystickDefaults();
        public abstract TouchActionPadActionValues GrabTouchActionPadDefaults();
        public abstract TouchCircularActionValues GrabTouchCircularActionDefaults();
        public abstract TouchDirectionalSwipeActionValues GetTouchDirectionSwipeActionDefaults();

        public abstract StickTranslateActionValues GrabStickTranslateActionDefaults();
        public abstract StickMouseActionValues GrabStickMouseActionDefaults();
        public abstract StickPadActionActionValues GrabStickPadActionActionDefaults();
        public abstract StickCircularActionValues GrabStickCircularActionDefaults();
        public abstract GyroMouseActionValues GrabGyroMouseActionDefaults();
    }

    public class DummyActionDefaultsCreator : DeviceActionDefaultsCreator
    {
        public override TouchDirectionalSwipeActionValues GetTouchDirectionSwipeActionDefaults()
        {
            TouchDirectionalSwipeActionValues result = new TouchDirectionalSwipeActionValues();
            return result;
        }

        public override StickMouseActionValues GrabStickMouseActionDefaults()
        {
            StickMouseActionValues result = new StickMouseActionValues();
            return result;
        }

        public override StickPadActionActionValues GrabStickPadActionActionDefaults()
        {
            StickPadActionActionValues result = new StickPadActionActionValues();
            return result;
        }

        public override StickTranslateActionValues GrabStickTranslateActionDefaults()
        {
            StickTranslateActionValues result = new StickTranslateActionValues();
            return result;
        }

        public override TouchActionPadActionValues GrabTouchActionPadDefaults()
        {
            TouchActionPadActionValues result = new TouchActionPadActionValues();
            return result;
        }

        public override TouchCircularActionValues GrabTouchCircularActionDefaults()
        {
            TouchCircularActionValues result = new TouchCircularActionValues()
            {
                hapticsIntensity = MapAction.HapticsIntensity.Medium,
            };

            return result;

        }

        public override TouchJoystickActionValues GrabTouchJoystickDefaults()
        {
            TouchJoystickActionValues result = new TouchJoystickActionValues()
            {
                deadZone = 0.1,
                maxZone = 1.0,
            };

            return result;
        }

        public override TouchMouseActionValues GrabTouchMouseDefaults()
        {
            TouchMouseActionValues result = new TouchMouseActionValues();
            return result;
        }

        public override TouchMouseJoystickActionValues GrabTouchMouseJoystickDefaults()
        {
            TouchMouseJoystickActionValues result = new TouchMouseJoystickActionValues();
            return result;
        }

        public override StickCircularActionValues GrabStickCircularActionDefaults()
        {
            StickCircularActionValues result = new StickCircularActionValues()
            {
                hapticsIntensity = MapAction.HapticsIntensity.Medium,
            };

            return result;
        }

        public override GyroMouseActionValues GrabGyroMouseActionDefaults()
        {
            GyroMouseActionValues result = new GyroMouseActionValues()
            {
                deadzone = 10,
            };

            return result;
        }
    }

    public class DS4ActionDefaultsCreator : DeviceActionDefaultsCreator
    {
        public override TouchDirectionalSwipeActionValues GetTouchDirectionSwipeActionDefaults()
        {
            TouchDirectionalSwipeActionValues result = new TouchDirectionalSwipeActionValues()
            {
                deadZoneX = 16,
                deadZoneY = 16,
                delayTime = 10,
            };

            return result;
        }

        public override StickMouseActionValues GrabStickMouseActionDefaults()
        {
            StickMouseActionValues result = new StickMouseActionValues()
            {
                deadZone = 0.05,
            };

            return result;
        }

        public override StickPadActionActionValues GrabStickPadActionActionDefaults()
        {
            StickPadActionActionValues result = new StickPadActionActionValues()
            {
                deadZone = 0.10,
            };

            return result;
        }

        public override StickTranslateActionValues GrabStickTranslateActionDefaults()
        {
            StickTranslateActionValues result = new StickTranslateActionValues()
            {
                deadZone = 0.10,
            };

            return result;
        }

        public override StickCircularActionValues GrabStickCircularActionDefaults()
        {
            StickCircularActionValues result = new StickCircularActionValues()
            {
                hapticsIntensity = MapAction.HapticsIntensity.Medium,
            };

            return result;
        }

        public override TouchActionPadActionValues GrabTouchActionPadDefaults()
        {
            TouchActionPadActionValues result = new TouchActionPadActionValues()
            {
                deadZone = 0.00,
            };

            return result;
        }

        public override TouchCircularActionValues GrabTouchCircularActionDefaults()
        {
            TouchCircularActionValues result = new TouchCircularActionValues()
            {
                deadZone = 0.2,
                hapticsIntensity = MapAction.HapticsIntensity.Medium,
            };

            return result;
        }

        public override TouchJoystickActionValues GrabTouchJoystickDefaults()
        {
            TouchJoystickActionValues result = new TouchJoystickActionValues()
            {
                deadZone = 0.05,
                antiDeadZone = 0.35,
                maxZone = 0.7,
            };

            return result;
        }

        public override TouchMouseActionValues GrabTouchMouseDefaults()
        {
            TouchMouseActionValues result = new TouchMouseActionValues()
            {
                deadZone = 0,
            };

            return result;
        }

        public override TouchMouseJoystickActionValues GrabTouchMouseJoystickDefaults()
        {
            TouchMouseJoystickActionValues result = new TouchMouseJoystickActionValues()
            {
                deadZone = 0,
                maxZone = 8,
            };

            return result;
        }

        public override GyroMouseActionValues GrabGyroMouseActionDefaults()
        {
            GyroMouseActionValues result = new GyroMouseActionValues()
            {
                deadzone = 10,
            };

            return result;
        }
    }

    public class Ultimate2WirelessDeviceDefaults: DummyActionDefaultsCreator
    {
        public override GyroMouseActionValues GrabGyroMouseActionDefaults()
        {
            GyroMouseActionValues result = base.GrabGyroMouseActionDefaults();
            result.deadzone = 20;

            return result;
        }

        public override StickPadActionActionValues GrabStickPadActionActionDefaults()
        {
            StickPadActionActionValues result = new StickPadActionActionValues()
            {
                deadZone = 0.10,
            };

            return result;
        }

        public override StickTranslateActionValues GrabStickTranslateActionDefaults()
        {
            StickTranslateActionValues result = new StickTranslateActionValues()
            {
                deadZone = 0.10,
                antiDeadZone = 0.20,
                maxZone = 0.95,
            };

            return result;
        }
    }

    public class SteamControllerDeviceDefaults : DummyActionDefaultsCreator
    {
        public override TouchJoystickActionValues GrabTouchJoystickDefaults()
        {
            TouchJoystickActionValues result = new TouchJoystickActionValues()
            {
                deadZone = 0.05,
                antiDeadZone = 0.0,
                maxZone = 0.7,
            };

            return result;
        }

        public override TouchMouseActionValues GrabTouchMouseDefaults()
        {
            TouchMouseActionValues result = new TouchMouseActionValues()
            {
                deadZone = 8,
            };

            return result;
        }

        public override TouchMouseJoystickActionValues GrabTouchMouseJoystickDefaults()
        {
            TouchMouseJoystickActionValues result = new TouchMouseJoystickActionValues()
            {
                deadZone = 70,
                maxZone = 430,
            };

            return result;
        }
    }
}
