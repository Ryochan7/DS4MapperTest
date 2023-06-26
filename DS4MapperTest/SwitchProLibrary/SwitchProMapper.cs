using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickActions;
using Nefarius.ViGEm.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.SwitchProLibrary
{
    public class SwitchProMapper : Mapper
    {
        private SwitchProDevice device;
        private SwitchProReader reader;

        public override InputDeviceType DeviceType => InputDeviceType.SwitchPro;
        public override DeviceReaderBase BaseReader
        {
            get => reader;
        }

        // Have Mapper make copies of state structs for manipulation. Allow
        // device reader state structs to remain untouched
        private SwitchProState currentMapperState;
        private SwitchProState previousMapperState;

        private StickDefinition lsDefintion;
        private StickDefinition rsDefintion;
        private GyroSensDefinition gyroSensDefinition;

        public SwitchProMapper(SwitchProDevice device, SwitchProReader reader, AppGlobalData appGlobal)
        {
            this.device = device;
            this.reader = reader;
            this.appGlobal = appGlobal;

            bindingList = new List<InputBindingMeta>()
            {
                new InputBindingMeta("A", "A", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("B", "B", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("X", "X", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Y", "Y", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Minus", "Minus", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Plus", "Plus", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LShoulder", "LShoulder", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RShoulder", "RShoulder", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LSClick", "LS Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RSClick", "RS Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("ZL", "ZL", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("ZR", "ZR", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Capture", "Capture", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Home", "Home", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LS", "Left Stick", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("RS", "Right Stick", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("DPad", "DPad", InputBindingMeta.InputControlType.DPad),
                new InputBindingMeta("Gyro", "Gyro", InputBindingMeta.InputControlType.Gyro),
            };

            // Populate Input Binding dictionary
            bindingList.ForEach((item) => bindingDict.Add(item.id, item));

            device.SetOperational();

            StickDefinition.StickAxisData lxAxis = new StickDefinition.StickAxisData
            {
                min = (short)device.leftStickXData.min,
                max = (short)device.leftStickXData.max,
                mid = (short)device.leftStickXData.mid,
            };
            lxAxis.PostInit();
            StickDefinition.StickAxisData lyAxis = new StickDefinition.StickAxisData
            {
                min = (short)device.leftStickYData.min,
                max = (short)device.leftStickYData.max,
                mid = (short)device.leftStickYData.mid,
            };
            lyAxis.PostInit();
            //StickDefinition lsDefintion = new StickDefinition(STICK_MIN, STICK_MAX, STICK_NEUTRAL, StickActionCodes.LS);
            lsDefintion = new StickDefinition(lxAxis, lyAxis, StickActionCodes.LS);

            StickDefinition.StickAxisData rxAxis = new StickDefinition.StickAxisData
            {
                min = (short)device.rightStickXData.min,
                max = (short)device.rightStickXData.max,
                mid = (short)device.rightStickXData.mid,
            };
            rxAxis.PostInit();
            StickDefinition.StickAxisData ryAxis = new StickDefinition.StickAxisData
            {
                min = (short)device.rightStickYData.min,
                max = (short)device.rightStickYData.max,
                mid = (short)device.rightStickYData.mid,
            };
            ryAxis.PostInit();
            rsDefintion = new StickDefinition(rxAxis, ryAxis, StickActionCodes.RS);

            gyroSensDefinition = new GyroSensDefinition()
            {
                //elapsedReference = 250.0,
                elapsedReference = device.BaseElapsedReference * 3.0,
                mouseCoefficient = 0.012,
                mouseOffset = 0.7,

                accelMinLeanX = -4000,
                accelMaxLeanX = 4000,
                accelMinLeanY = -4000,
                accelMaxLeanY = 4000,
                accelMinLeanZ = -4000,
                accelMaxLeanZ = 4000,
            };

            knownStickDefinitions.Add("LS", lsDefintion);
            knownStickDefinitions.Add("RS", rsDefintion);
            knownGyroSensDefinitions.Add("Gyro", gyroSensDefinition);

            actionTriggerItems.Clear();
            actionTriggerItems = new List<ActionTriggerItem>()
            {
                new ActionTriggerItem("Always On", JoypadActionCodes.AlwaysOn),
                new ActionTriggerItem("A", JoypadActionCodes.BtnEast),
                new ActionTriggerItem("B", JoypadActionCodes.BtnSouth),
                new ActionTriggerItem("X", JoypadActionCodes.BtnNorth),
                new ActionTriggerItem("Y", JoypadActionCodes.BtnWest),
                new ActionTriggerItem("LShoulder", JoypadActionCodes.BtnLShoulder),
                new ActionTriggerItem("RShoulder", JoypadActionCodes.BtnRShoulder),
                new ActionTriggerItem("ZL", JoypadActionCodes.AxisLTrigger),
                new ActionTriggerItem("ZR", JoypadActionCodes.AxisRTrigger),
                new ActionTriggerItem("LSClick", JoypadActionCodes.BtnThumbL),
                new ActionTriggerItem("RSClick", JoypadActionCodes.BtnThumbR),
                new ActionTriggerItem("Minus", JoypadActionCodes.BtnSelect),
                new ActionTriggerItem("Plus", JoypadActionCodes.BtnStart),
                new ActionTriggerItem("Home", JoypadActionCodes.BtnHome),
                new ActionTriggerItem("Capture", JoypadActionCodes.BtnCapture),
                new ActionTriggerItem("DPad Up", JoypadActionCodes.BtnDPadUp),
                new ActionTriggerItem("DPad Down", JoypadActionCodes.BtnDPadDown),
                new ActionTriggerItem("DPad Left", JoypadActionCodes.BtnDPadLeft),
                new ActionTriggerItem("DPad Right", JoypadActionCodes.BtnDPadRight),
            };
        }

        public override void Start(ViGEmClient vigemTestClient, FakerInputHandler fakerInputHandler)
        {
            base.Start(vigemTestClient, fakerInputHandler);

            reader.Report += Reader_Report;
            reader.LeftStickCalibUpdated += Reader_LeftStickCalibUpdated;
            reader.RightStickCalibUpdated += Reader_RightStickCalibUpdated;
            reader.StartUpdate();
        }

        private void Reader_LeftStickCalibUpdated(object sender, SwitchProDevice currentDevice)
        {
            // Replace placeholder data with real device data
            lsDefintion.xAxis.min = (short)currentDevice.leftStickXData.min;
            lsDefintion.xAxis.max = (short)currentDevice.leftStickXData.max;
            lsDefintion.xAxis.mid = (short)currentDevice.leftStickXData.mid;
            lsDefintion.xAxis.PostInit();

            // Replace placeholder data with real device data
            lsDefintion.yAxis.min = (short)currentDevice.leftStickYData.min;
            lsDefintion.yAxis.max = (short)currentDevice.leftStickYData.max;
            lsDefintion.yAxis.mid = (short)currentDevice.leftStickYData.mid;
            lsDefintion.yAxis.PostInit();
        }

        private void Reader_RightStickCalibUpdated(object sender, SwitchProDevice currentDevice)
        {
            // Replace placeholder data with real device data
            rsDefintion.xAxis.min = (short)currentDevice.rightStickXData.min;
            rsDefintion.xAxis.max = (short)currentDevice.rightStickXData.max;
            rsDefintion.xAxis.mid = (short)currentDevice.rightStickXData.mid;
            rsDefintion.xAxis.PostInit();

            // Replace placeholder data with real device data
            rsDefintion.yAxis.min = (short)currentDevice.rightStickYData.min;
            rsDefintion.yAxis.max = (short)currentDevice.rightStickYData.max;
            rsDefintion.yAxis.mid = (short)currentDevice.rightStickYData.mid;
            rsDefintion.yAxis.PostInit();
        }

        private void Reader_Report(SwitchProReader sender, SwitchProDevice device)
        {
            ref SwitchProState current = ref device.ClothOff;
            //ref SwitchProState previous = ref device.ClothOff2;

            // Copy state struct data for later mapper manipulation. Leave
            // device state instance alone
            currentMapperState = current;

            mouseX = mouseY = 0.0;

            unchecked
            {
                outputController?.ResetReport();

                intermediateState = new IntermediateState();
                currentLatency = currentMapperState.timeElapsed;
                currentRate = 1.0 / currentLatency;

                ProcessReleaseEvents();
                ProcessCycleChecks();

                ActionLayer currentLayer = actionProfile.CurrentActionSet.CurrentActionLayer;

                if (currentLayer.actionSetActionDict.TryGetValue($"{actionProfile.CurrentActionSet.ActionButtonId}",
                    out ButtonMapAction currentSetAction))
                {
                    currentSetAction.Prepare(this, true);
                    if (currentSetAction.active)
                    {
                        currentSetAction.Event(this);
                    }
                }

                StickMapAction mapAction = currentLayer.stickActionDict["LS"];
                //if ((currentMapperState.LX != previousMapperState.LX) || (currentMapperState.LY != previousMapperState.LY))
                {
                    mapAction.Prepare(this, currentMapperState.LX, currentMapperState.LY);
                }

                if (mapAction.active)
                {
                    mapAction.Event(this);
                }

                mapAction = currentLayer.stickActionDict["RS"];
                //Console.WriteLine(currentMapperState.RY);
                //if ((currentMapperState.RX != previousMapperState.RX) || (currentMapperState.RY != previousMapperState.RY))
                {
                    mapAction.Prepare(this, currentMapperState.RX, currentMapperState.RY);
                }

                if (mapAction.active)
                {
                    mapAction.Event(this);
                }

                ButtonMapAction tempBtnAct = currentLayer.buttonActionDict["B"];
                if (currentMapperState.B || currentMapperState.B != previousMapperState.B)
                {
                    tempBtnAct.Prepare(this, currentMapperState.B);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["A"];
                if (currentMapperState.A || currentMapperState.A != previousMapperState.A)
                {
                    tempBtnAct.Prepare(this, currentMapperState.A);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Y"];
                if (currentMapperState.Y || currentMapperState.Y != previousMapperState.Y)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Y);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["X"];
                if (currentMapperState.X || currentMapperState.X != previousMapperState.X)
                {
                    tempBtnAct.Prepare(this, currentMapperState.X);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Minus"];
                if (currentMapperState.Minus || currentMapperState.Minus != previousMapperState.Minus)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Minus);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Plus"];
                if (currentMapperState.Plus || currentMapperState.Plus != previousMapperState.Plus)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Plus);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["LShoulder"];
                if (currentMapperState.LShoulder || currentMapperState.LShoulder != previousMapperState.LShoulder)
                {
                    tempBtnAct.Prepare(this, currentMapperState.LShoulder);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["RShoulder"];
                if (currentMapperState.RShoulder || currentMapperState.RShoulder != previousMapperState.RShoulder)
                {
                    tempBtnAct.Prepare(this, currentMapperState.RShoulder);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["ZL"];
                if (currentMapperState.ZL || currentMapperState.ZL != previousMapperState.ZL)
                {
                    tempBtnAct.Prepare(this, currentMapperState.ZL);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["ZR"];
                if (currentMapperState.ZR || currentMapperState.ZR != previousMapperState.ZR)
                {
                    tempBtnAct.Prepare(this, currentMapperState.ZR);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["LSClick"];
                if (currentMapperState.LSClick || currentMapperState.LSClick != previousMapperState.LSClick)
                {
                    tempBtnAct.Prepare(this, currentMapperState.LSClick);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["RSClick"];
                if (currentMapperState.RSClick || currentMapperState.RSClick != previousMapperState.RSClick)
                {
                    tempBtnAct.Prepare(this, currentMapperState.RSClick);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Capture"];
                if (currentMapperState.Capture || currentMapperState.Capture != previousMapperState.Capture)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Capture);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Home"];
                if (currentMapperState.Home || currentMapperState.Home != previousMapperState.Home)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Home);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                DpadDirections currentDpad = DpadDirections.Centered;

                if (currentMapperState.DpadUp)
                    currentDpad |= DpadDirections.Up;
                if (currentMapperState.DpadRight)
                    currentDpad |= DpadDirections.Right;
                if (currentMapperState.DpadDown)
                    currentDpad |= DpadDirections.Down;
                if (currentMapperState.DpadLeft)
                    currentDpad |= DpadDirections.Left;

                DPadMapAction dpadMapAction = currentLayer.dpadActionDict["DPad"];
                dpadMapAction.Prepare(this, currentDpad);
                if (dpadMapAction.active)
                    dpadMapAction.Event(this);

                GyroMapAction gyroAct = currentLayer.gyroActionDict["Gyro"];
                // Skip if duration is less than 10 ms
                //if (currentMapperState.timeElapsed > 0.01)
                {
                    GyroEventFrame mouseFrame = new GyroEventFrame
                    {
                        GyroYaw = currentMapperState.Motion.GyroYaw,
                        GyroPitch = currentMapperState.Motion.GyroPitch,
                        GyroRoll = currentMapperState.Motion.GyroRoll,
                        AngGyroYaw = currentMapperState.Motion.AngGyroYaw,
                        AngGyroPitch = currentMapperState.Motion.AngGyroPitch,
                        AngGyroRoll = currentMapperState.Motion.AngGyroRoll,
                        AccelX = currentMapperState.Motion.AccelX,
                        AccelY = currentMapperState.Motion.AccelY,
                        AccelZ = currentMapperState.Motion.AccelZ,
                        AccelXG = currentMapperState.Motion.AccelXG,
                        AccelYG = currentMapperState.Motion.AccelYG,
                        AccelZG = currentMapperState.Motion.AccelZG,
                        timeElapsed = currentLatency,
                        //elapsedReference = 66.7,
                        elapsedReference = device.BaseElapsedReference * 3,
                    };

                    gyroAct.Prepare(this, ref mouseFrame);
                    if (gyroAct.active)
                    {
                        gyroAct.Event(this);
                    }

                    //sender.CombLatency = 0;
                }

                gamepadSync = intermediateState.Dirty;

                ProcessSyncEvents();

                ProcessActionSetLayerChecks();

                // Make copy of state data as the previous state
                previousMapperState = currentMapperState;

                if (hasInputEvts)
                {
                    ProcessQueuedActions();
                }
            }
        }

        public override void EstablishForceFeedback()
        {
            if (outputControlType == OutputContType.Xbox360)
            {
                outputForceFeedbackDel = (sender, e) =>
                {
                    device.currentLeftAmpRatio = e.LargeMotor / 255.0;
                    device.currentRightAmpRatio = e.SmallMotor / 255.0;
                    reader.WriteRumbleReport();
                };
            }
        }

        public override bool IsButtonActive(JoypadActionCodes code)
        {
            bool result = false;
            switch (code)
            {
                case JoypadActionCodes.AlwaysOn:
                    result = true;
                    break;
                case JoypadActionCodes.BtnSouth:
                    result = currentMapperState.B;
                    break;
                case JoypadActionCodes.BtnEast:
                    result = currentMapperState.A;
                    break;
                case JoypadActionCodes.BtnNorth:
                    result = currentMapperState.X;
                    break;
                case JoypadActionCodes.BtnWest:
                    result = currentMapperState.Y;
                    break;
                case JoypadActionCodes.BtnLShoulder:
                    result = currentMapperState.LShoulder;
                    break;
                case JoypadActionCodes.BtnRShoulder:
                    result = currentMapperState.RShoulder;
                    break;
                case JoypadActionCodes.BtnStart:
                    result = currentMapperState.Plus;
                    break;
                case JoypadActionCodes.BtnMode:
                    result = currentMapperState.Minus;
                    break;
                case JoypadActionCodes.BtnHome:
                    result = currentMapperState.Home;
                    break;
                case JoypadActionCodes.AxisLTrigger:
                    result = currentMapperState.ZL;
                    break;
                case JoypadActionCodes.AxisRTrigger:
                    result = currentMapperState.ZR;
                    break;
                default:
                    break;
            }

            return result;
        }

        public override bool IsButtonsActiveDraft(IEnumerable<JoypadActionCodes> codes, bool andEval = true)
        {
            bool result = false;
            foreach (JoypadActionCodes code in codes)
            {
                switch (code)
                {
                    case JoypadActionCodes.AlwaysOn:
                        result = true;
                        break;
                    case JoypadActionCodes.BtnSouth:
                        result = currentMapperState.B;
                        break;
                    case JoypadActionCodes.BtnEast:
                        result = currentMapperState.A;
                        break;
                    case JoypadActionCodes.BtnNorth:
                        result = currentMapperState.X;
                        break;
                    case JoypadActionCodes.BtnWest:
                        result = currentMapperState.Y;
                        break;
                    case JoypadActionCodes.BtnLShoulder:
                        result = currentMapperState.LShoulder;
                        break;
                    case JoypadActionCodes.BtnRShoulder:
                        result = currentMapperState.RShoulder;
                        break;
                    case JoypadActionCodes.BtnStart:
                        result = currentMapperState.Plus;
                        break;
                    case JoypadActionCodes.BtnSelect:
                        result = currentMapperState.Minus;
                        break;
                    case JoypadActionCodes.BtnHome:
                        result = currentMapperState.Home;
                        break;
                    case JoypadActionCodes.BtnCapture:
                        result = currentMapperState.Capture;
                        break;
                    case JoypadActionCodes.AxisLTrigger:
                        result = currentMapperState.ZL;
                        break;
                    case JoypadActionCodes.AxisRTrigger:
                        result = currentMapperState.ZR;
                        break;
                    default:
                        break;
                }

                if (andEval && !result)
                {
                    break;
                }
                else if (!andEval && result)
                {
                    break;
                }
            }

            return result;
        }
    }
}
