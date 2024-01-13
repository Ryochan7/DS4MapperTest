using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;

namespace DS4MapperUnitTests
{
    internal class TestMapper : Mapper
    {
        private StickDefinition lsDefintion;
        private TouchpadDefinition leftPadDefiniton;
        private TouchpadDefinition rightPadDefinition;
        private TriggerDefinition leftTriggerDefinition;
        private TriggerDefinition rightTriggerDefinition;
        private GyroSensDefinition gyroSensDefinition;

        public Dictionary<string, StickDefinition> KnownStickDefinitions => knownStickDefinitions;
        public Dictionary<string, TriggerDefinition> KnownTriggerDefinitions => knownTriggerDefinitions;
        public Dictionary<string, TouchpadDefinition> KnownTouchpadDefinitions => knownTouchpadDefinitions;
        public Dictionary<string, GyroSensDefinition> KnownGyroSensDefinitions => knownGyroSensDefinitions;

        public override DeviceReaderBase BaseReader => throw new NotImplementedException();

        public TestMapper()
        {
            bindingList = new List<InputBindingMeta>()
            {
                new InputBindingMeta("A", "A", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("B", "B", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("X", "X", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Y", "Y", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Back", "Back", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Start", "Start", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LShoulder", "Left Shoulder", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RShoulder", "Right Shoulder", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LSClick", "Stick Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LeftGrip", "Left Grip", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RightGrip", "Right Grip", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LT", "Left Trigger", InputBindingMeta.InputControlType.Trigger),
                new InputBindingMeta("RT", "Right Trigger", InputBindingMeta.InputControlType.Trigger),
                new InputBindingMeta("Steam", "Steam", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Stick", "Stick", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("LeftTouchpad", "Left Touchpad", InputBindingMeta.InputControlType.Touchpad),
                new InputBindingMeta("RightTouchpad", "Right Touchpad", InputBindingMeta.InputControlType.Touchpad),
                new InputBindingMeta("LeftPadClick", "Left Pad Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RightPadClick", "Right Pad Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Gyro", "Gyro", InputBindingMeta.InputControlType.Gyro),
            };

            // Populate Input Binding dictionary
            bindingList.ForEach((item) => bindingDict.Add(item.id, item));

            //trackballAccel = TRACKBALL_RADIUS * TRACKBALL_INIT_FRICTION / TRACKBALL_INERTIA;
            //trackballAccel = TRACKBALL_RADIUS * TRACKBALL_JOY_FRICTION / TRACKBALL_INERTIA;

            StickDefinition.StickAxisData lxAxis = new StickDefinition.StickAxisData
            {
                min = -30000,
                max = 30000,
                mid = 0,
                hard_max = 32767,
                hard_min = -32767,
            };
            StickDefinition.StickAxisData lyAxis = new StickDefinition.StickAxisData
            {
                min = -30000,
                max = 30000,
                mid = 0,
                hard_max = 32767,
                hard_min = -32767,
            };
            //StickDefinition lsDefintion = new StickDefinition(STICK_MIN, STICK_MAX, STICK_NEUTRAL, StickActionCodes.LS);
            lsDefintion = new StickDefinition(lxAxis, lyAxis, StickActionCodes.LS);

            TouchpadDefinition.TouchAxisData lpadXAxis = new TouchpadDefinition.TouchAxisData
            {
                min = -32768,
                max = 32767,
                mid = 0,

                hard_max = 32767,
                hard_min = -32768,
            };
            lpadXAxis.PostInit();

            TouchpadDefinition.TouchAxisData lpadYAxis = new TouchpadDefinition.TouchAxisData
            {
                min = -32768,
                max = 32767,
                mid = 0,

                hard_max = 32767,
                hard_min = -32768,
            };
            lpadYAxis.PostInit();

            leftPadDefiniton = new TouchpadDefinition(lpadXAxis, lpadYAxis, TouchpadActionCodes.TouchL,
                elapsedReference: 8.0, mouseScale: 0.012 * 1.1, mouseOffset: 0.4,
                trackballScale: 0.000023);
            leftPadDefiniton.throttleRelMouse = true;
            leftPadDefiniton.throttleRelMousePower = 1.428;
            leftPadDefiniton.throttleRelMouseZone = 10;

            TouchpadDefinition.TouchAxisData rpadXAxis = new TouchpadDefinition.TouchAxisData
            {
                min = -32768,
                max = 32767,
                mid = 0,

                hard_max = 32767,
                hard_min = -32768,
            };
            rpadXAxis.PostInit();

            TouchpadDefinition.TouchAxisData rpadYAxis = new TouchpadDefinition.TouchAxisData
            {
                min = -32768,
                max = 32767,
                mid = 0,

                hard_max = 32767,
                hard_min = -32768,
            };
            rpadYAxis.PostInit();

            rightPadDefinition = new TouchpadDefinition(rpadXAxis, rpadYAxis, TouchpadActionCodes.TouchR,
                elapsedReference: 8.0, mouseScale: 0.012 * 1.1, mouseOffset: 0.4,
                trackballScale: 0.000023);
            rightPadDefinition.throttleRelMouse = true;
            rightPadDefinition.throttleRelMousePower = 1.428;
            rightPadDefinition.throttleRelMouseZone = 10;

            TriggerDefinition.TriggerAxisData ltAxis = new TriggerDefinition.TriggerAxisData
            {
                min = 0,
                max = 255,
                hasClickButton = true,
                fullClickBtnCode = JoypadActionCodes.LTFullPull,
            };

            leftTriggerDefinition = new TriggerDefinition(ltAxis, TriggerActionCodes.LeftTrigger);

            // Copy struct
            TriggerDefinition.TriggerAxisData rtAxis = new TriggerDefinition.TriggerAxisData
            {
                min = 0,
                max = 255,
                hasClickButton = true,
                fullClickBtnCode = JoypadActionCodes.RTFullPull,
            };

            rightTriggerDefinition = new TriggerDefinition(rtAxis, TriggerActionCodes.LeftTrigger);

            gyroSensDefinition = new GyroSensDefinition()
            {
                elapsedReference = 125.0,
                mouseCoefficient = 0.025,
                mouseOffset = 0.3,

                accelMinLeanX = -16384,
                accelMaxLeanX = 16384,
                accelMinLeanY = -16384,
                accelMaxLeanY = 16384,
                accelMinLeanZ = -16384,
                accelMaxLeanZ = 16384,
            };

            knownStickDefinitions.Add("Stick", lsDefintion);
            knownTriggerDefinitions.Add("LT", leftTriggerDefinition);
            knownTriggerDefinitions.Add("RT", rightTriggerDefinition);
            knownTouchpadDefinitions.Add("LeftTouchpad", leftPadDefiniton);
            knownTouchpadDefinitions.Add("RightTouchpad", rightPadDefinition);
            knownGyroSensDefinitions.Add("Gyro", gyroSensDefinition);

            actionTriggerItems.Clear();
            actionTriggerItems = new List<ActionTriggerItem>()
            {
                new ActionTriggerItem("Always On", JoypadActionCodes.AlwaysOn),
                new ActionTriggerItem("A", JoypadActionCodes.BtnSouth),
                new ActionTriggerItem("B", JoypadActionCodes.BtnEast),
                new ActionTriggerItem("X", JoypadActionCodes.BtnWest),
                new ActionTriggerItem("Y", JoypadActionCodes.BtnNorth),
                new ActionTriggerItem("Left Bumper", JoypadActionCodes.BtnLShoulder),
                new ActionTriggerItem("Right Bumper", JoypadActionCodes.BtnRShoulder),
                new ActionTriggerItem("Left Trigger", JoypadActionCodes.AxisLTrigger),
                new ActionTriggerItem("Right Trigger", JoypadActionCodes.AxisRTrigger),
                new ActionTriggerItem("Left Grip", JoypadActionCodes.BtnLGrip),
                new ActionTriggerItem("Right Grip", JoypadActionCodes.BtnRGrip),
                new ActionTriggerItem("Stick Click", JoypadActionCodes.BtnThumbL),
                new ActionTriggerItem("Left Touchpad Touch", JoypadActionCodes.LPadTouch),
                new ActionTriggerItem("Left Touchpad Touch", JoypadActionCodes.LPadTouch),
                new ActionTriggerItem("Right Touchpad Touch", JoypadActionCodes.RPadTouch),
                new ActionTriggerItem("Right Touchpad Click", JoypadActionCodes.RPadClick),

                new ActionTriggerItem("Back", JoypadActionCodes.BtnSelect),
                new ActionTriggerItem("Start", JoypadActionCodes.BtnStart),
                new ActionTriggerItem("Steam", JoypadActionCodes.BtnMode),
            };
        }

        public override void EstablishForceFeedback()
        {
            throw new NotImplementedException();
        }

        public override bool IsButtonActive(JoypadActionCodes code)
        {
            throw new NotImplementedException();
        }

        public override bool IsButtonsActiveDraft(IEnumerable<JoypadActionCodes> codes, bool andEval = true)
        {
            throw new NotImplementedException();
        }
    }
}
