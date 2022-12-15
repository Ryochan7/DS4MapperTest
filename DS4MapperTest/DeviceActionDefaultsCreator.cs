using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            public void Process(TouchpadMouseJoystick action)
            {
                action.MStickParams.deadZone = deadZone;

                action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.DEAD_ZONE);
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

            public void Process(TouchpadCircular action)
            {
                action.DeadMod.DeadZone = deadZone;

                //action.ChangedProperties.Add(TouchpadCircular.PropertyKeyStrings.DEAD_ZONE);
            }
        }

        public abstract TouchJoystickActionValues GrabTouchJoystickDefaults();
        public abstract TouchMouseActionValues GrabTouchMouseDefaults();
        public abstract TouchMouseJoystickActionValues GrabTouchMouseJoystickDefaults();
        public abstract TouchActionPadActionValues GrabTouchActionPadDefaults();
        public abstract TouchCircularActionValues GrabTouchCircularActionDefaults();
    }

    public class DummyActionDefaultsCreator : DeviceActionDefaultsCreator
    {
        public override TouchActionPadActionValues GrabTouchActionPadDefaults()
        {
            TouchActionPadActionValues result = new TouchActionPadActionValues();
            return result;
        }

        public override TouchCircularActionValues GrabTouchCircularActionDefaults()
        {
            TouchCircularActionValues result = new TouchCircularActionValues();
            return result;

        }

        public override TouchJoystickActionValues GrabTouchJoystickDefaults()
        {
            TouchJoystickActionValues result = new TouchJoystickActionValues();
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
    }

    public class DS4ActionDefaultsCreator : DeviceActionDefaultsCreator
    {
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
            };

            return result;
        }
    }
}
