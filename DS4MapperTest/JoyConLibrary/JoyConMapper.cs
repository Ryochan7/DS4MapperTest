using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.ScpVBus;
using DS4MapperTest.StickActions;
using DS4MapperTest.SwitchProLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DS4MapperTest.ScpVBus.Xbox360ScpOutDevice;

namespace DS4MapperTest.JoyConLibrary
{
    public class JoyConMapper : Mapper
    {
        private JoyConDevice device;
        private JoyConReader reader;

        private JoyConDevice secondJoyDevice;
        private JoyConReader secondJoyReader;

        public override InputDeviceType DeviceType => InputDeviceType.JoyCon;
        public override DeviceReaderBase BaseReader
        {
            get => reader;
        }

        // Have Mapper make copies of state structs for manipulation. Allow
        // device reader state structs to remain untouched
        private JoyConState currentMapperState;
        private JoyConState previousMapperState;

        private JoyConState currentMapperSecondaryState;
        private JoyConState previousMapperSecondaryState;

        private StickDefinition lsDefintion;
        private StickDefinition rsDefintion;
        private GyroSensDefinition gyroLSensDefinition;
        private GyroSensDefinition gyroRSensDefinition;

        public JoyConDevice JoyDevice => device;
        public JoyConDevice SecondaryJoyDevice => secondJoyDevice;
        public JoyConReader SecondJoyReader => secondJoyReader;

        private ReaderWriterLockSlim readerLock = new ReaderWriterLockSlim();

        [Flags]
        private enum JoyConSideUsed : ushort
        {
            None = 0x00,
            JoyConL = 0x01,
            JoyConR = 0x02,
        }

        // Used to keep track of which side has already gone
        // through the mapping routine. Helps run other side if primary device
        // is found before next report
        private JoyConSideUsed sideMapped;

        public JoyConMapper(JoyConDevice device, JoyConReader reader, AppGlobalData appGlobal)
        {
            this.device = device;
            this.reader = reader;
            this.appGlobal = appGlobal;
            this.baseDevice = device;

            // Only sync gamepad events when PrimaryDevice poll is read
            gamepadSync = false;

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
                new InputBindingMeta("LSideL", "SideL (Left)", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LSideR", "SideR (Left)", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RSideL", "SideL (Right)", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RSideR", "SideR (Right)", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LS", "Left Stick", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("RS", "Right Stick", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("DPad", "DPad", InputBindingMeta.InputControlType.DPad),
                new InputBindingMeta("GyroL", "Gyro (Left)", InputBindingMeta.InputControlType.Gyro),
                new InputBindingMeta("GyroR", "Gyro (Right)", InputBindingMeta.InputControlType.Gyro),
            };

            // Populate Input Binding dictionary
            bindingList.ForEach((item) => bindingDict.Add(item.id, item));

            device.SetOperational();

            if (device.SideType == JoyConSide.Left)
            {
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
                // Use current device for placeholder data. Will replace when secondary
                // device is detected
                rsDefintion = new StickDefinition(lxAxis, lyAxis, StickActionCodes.RS);
            }
            else if (device.SideType == JoyConSide.Right)
            {
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
                // Use current device for placeholder data. Will replace when secondary
                // device is detected
                lsDefintion = new StickDefinition(rxAxis, ryAxis, StickActionCodes.LS);
            }

            knownStickDefinitions.Add("LS", lsDefintion);
            knownStickDefinitions.Add("RS", rsDefintion);

            gyroLSensDefinition = new GyroSensDefinition()
            {
                //elapsedReference = 250.0,
                elapsedReference = device.BaseElapsedReference * 3.0,
                mouseCoefficient = 0.012,
                mouseOffset = 0.3,

                accelMinLeanX = -4000,
                accelMaxLeanX = 4000,
                accelMinLeanY = -4000,
                accelMaxLeanY = 4000,
                accelMinLeanZ = -4000,
                accelMaxLeanZ = 4000,
            };

            gyroRSensDefinition = new GyroSensDefinition()
            {
                //elapsedReference = 250.0,
                elapsedReference = device.BaseElapsedReference * 3.0,
                mouseCoefficient = 0.012,
                mouseOffset = 0.3,

                accelMinLeanX = -4000,
                accelMaxLeanX = 4000,
                accelMinLeanY = -4000,
                accelMaxLeanY = 4000,
                accelMinLeanZ = -4000,
                accelMaxLeanZ = 4000,
            };

            knownGyroSensDefinitions.Add("GyroL", gyroLSensDefinition);
            knownGyroSensDefinitions.Add("GyroR", gyroRSensDefinition);

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
                new ActionTriggerItem("LSideL", JoypadActionCodes.BtnLSideL),
                new ActionTriggerItem("LSideR", JoypadActionCodes.BtnLSideR),
                new ActionTriggerItem("RSideL", JoypadActionCodes.BtnRSideL),
                new ActionTriggerItem("RSideR", JoypadActionCodes.BtnLSideR),
                new ActionTriggerItem("DPad Up", JoypadActionCodes.BtnDPadUp),
                new ActionTriggerItem("DPad Down", JoypadActionCodes.BtnDPadDown),
                new ActionTriggerItem("DPad Left", JoypadActionCodes.BtnDPadLeft),
                new ActionTriggerItem("DPad Right", JoypadActionCodes.BtnDPadRight),
            };
        }

        public override void Start(X360BusDevice busDevice, FakerInputHandler fakerInputHandler)
        {
            base.Start(busDevice, fakerInputHandler);

            reader.Report += Reader_Report;
            reader.LeftStickCalibUpdated += Reader_LeftStickCalibUpdated;
            reader.RightStickCalibUpdated += Reader_RightStickCalibUpdated;
            reader.StartUpdate();
        }

        private void Reader_LeftStickCalibUpdated(object sender, JoyConDevice currentDevice)
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

        private void Reader_RightStickCalibUpdated(object sender, JoyConDevice currentDevice)
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

        private void Reader_Report(JoyConReader sender, JoyConDevice device)
        {
            using WriteLocker locker = new WriteLocker(readerLock);

            ref JoyConState current = ref device.ClothOff;
            //ref JoyConState previous = ref device.ClothOff2;

            // Copy state struct data for later mapper manipulation. Leave
            // device state instance alone
            //currentMapperState = current;
            mouseX = mouseY = 0.0;

            unchecked
            {
                CopyStateData(device, ref current);

                if (this.device == device)
                {
                    //outputController?.ResetReport();
                    //intermediateState = new IntermediateState();
                }

                gamepadSync = false;

                //if (!device.PrimaryDevice)
                //{
                //    return;
                //}

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

                if (device.SideType == JoyConSide.Left)
                {
                    //if (device.PrimaryDevice && !sideMapped.HasFlag(JoyConSideUsed.JoyConL))
                    //{
                    //    Trace.WriteLine("DID NOT GET REPORT L");
                    //}
                    //else if (device.SideType == JoyConSide.Left)
                    //{
                    //    Trace.WriteLine("GOT REPORT L");
                    //}

                    bool currentDev = device.SideType == JoyConSide.Left;
                    StickMapAction mapAction = currentLayer.stickActionDict["LS"];
                    //if ((currentMapperState.LX != previousMapperState.LX) || (currentMapperState.LY != previousMapperState.LY))
                    {
                        mapAction.Prepare(this, currentMapperState.LX, currentMapperState.LY);
                    }

                    if (mapAction.active)
                    {
                        mapAction.Event(this);
                    }

                    ButtonMapAction tempBtnAct = currentLayer.buttonActionDict["Minus"];
                    if (currentMapperState.Minus || currentMapperState.Minus != previousMapperState.Minus)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.Minus);
                    }

                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["LShoulder"];
                    if (currentMapperState.LShoulder || currentMapperState.LShoulder != previousMapperState.LShoulder)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.LShoulder);
                    }

                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["ZL"];
                    if (currentMapperState.ZL || currentMapperState.ZL != previousMapperState.ZL)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.ZL);
                    }
                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["LSClick"];
                    if (currentMapperState.LSClick || currentMapperState.LSClick != previousMapperState.LSClick)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.LSClick);
                    }
                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["Capture"];
                    if (currentMapperState.Capture || currentMapperState.Capture != previousMapperState.Capture)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.Capture);
                    }
                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["LSideL"];
                    if (currentMapperState.SideL || currentMapperState.SideL != previousMapperState.SideL)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.SideL);
                    }
                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["LSideR"];
                    if (currentMapperState.SideR || currentMapperState.SideR != previousMapperState.SideR)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.SideR);
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

                    GyroMapAction gyroAct = currentLayer.gyroActionDict["GyroL"];
                    // Skip if duration is less than 10 ms
                    //if (currentMapperState.timeElapsed > 0.01)
                    bool runPrepareAction = (gyroAct.OnlyOnPrimary && currentDev);
                    //if (!gyroAct.OnlyOnPrimary || runPrepareAction)
                    {
                        GyroEventFrame mouseFrame = new GyroEventFrame
                        {
                            GyroYaw = currentMapperState.MotionL.GyroYaw,
                            GyroPitch = currentMapperState.MotionL.GyroPitch,
                            GyroRoll = currentMapperState.MotionL.GyroRoll,
                            AngGyroYaw = currentMapperState.MotionL.AngGyroYaw,
                            AngGyroPitch = currentMapperState.MotionL.AngGyroPitch,
                            AngGyroRoll = currentMapperState.MotionL.AngGyroRoll,
                            AccelX = currentMapperState.MotionL.AccelX,
                            AccelY = currentMapperState.MotionL.AccelY,
                            AccelZ = currentMapperState.MotionL.AccelZ,
                            AccelXG = currentMapperState.MotionL.AccelXG,
                            AccelYG = currentMapperState.MotionL.AccelYG,
                            AccelZG = currentMapperState.MotionL.AccelZG,
                            timeElapsed = currentMapperState.timeElapsed,
                            //elapsedReference = 66.7,
                            elapsedReference = device.BaseElapsedReference * 3,
                        };

                        //if (currentDev || runPrepareAction)
                        {
                            gyroAct.Prepare(this, ref mouseFrame);
                        }

                        if (gyroAct.active)
                        {
                            gyroAct.Event(this);
                        }

                        //sender.CombLatency = 0;
                    }

                    sideMapped |= JoyConSideUsed.JoyConL;
                }

                if (device.SideType == JoyConSide.Right)
                {
                    bool currentDev = device.SideType == JoyConSide.Right;
                    StickMapAction mapAction = currentLayer.stickActionDict["RS"];

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

                    tempBtnAct = currentLayer.buttonActionDict["Plus"];
                    if (currentMapperState.Plus || currentMapperState.Plus != previousMapperState.Plus)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.Plus);
                    }

                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["RShoulder"];
                    if (currentMapperState.RShoulder || currentMapperState.RShoulder != previousMapperState.RShoulder)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.RShoulder);
                    }

                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["ZR"];
                    if (currentMapperState.ZR || currentMapperState.ZR != previousMapperState.ZR)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.ZR);
                    }
                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["RSClick"];
                    if (currentMapperState.RSClick || currentMapperState.RSClick != previousMapperState.RSClick)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.RSClick);
                    }
                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["Home"];
                    if (currentMapperState.Home || currentMapperState.Home != previousMapperState.Home)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.Home);
                    }
                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["RSideL"];
                    if (currentMapperState.RightSideL || currentMapperState.RightSideL != previousMapperState.RightSideL)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.RightSideL);
                    }
                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["RSideR"];
                    if (currentMapperState.RightSideR || currentMapperState.RightSideR != previousMapperState.RightSideR)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.RightSideR);
                    }
                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    GyroMapAction gyroAct = currentLayer.gyroActionDict["GyroR"];
                    // Skip if duration is less than 10 ms
                    //if (currentMapperState.timeElapsed > 0.01)
                    bool runPrepareAction = (gyroAct.OnlyOnPrimary && currentDev);
                    //if (!gyroAct.OnlyOnPrimary || runPrepareAction)
                    {
                        GyroEventFrame mouseFrame = new GyroEventFrame
                        {
                            GyroYaw = currentMapperState.MotionR.GyroYaw,
                            GyroPitch = currentMapperState.MotionR.GyroPitch,
                            GyroRoll = currentMapperState.MotionR.GyroRoll,
                            AngGyroYaw = currentMapperState.MotionR.AngGyroYaw,
                            AngGyroPitch = currentMapperState.MotionR.AngGyroPitch,
                            AngGyroRoll = currentMapperState.MotionR.AngGyroRoll,
                            AccelX = currentMapperState.MotionR.AccelX,
                            AccelY = currentMapperState.MotionR.AccelY,
                            AccelZ = currentMapperState.MotionR.AccelZ,
                            AccelXG = currentMapperState.MotionR.AccelXG,
                            AccelYG = currentMapperState.MotionR.AccelYG,
                            AccelZG = currentMapperState.MotionR.AccelZG,
                            timeElapsed = currentMapperState.timeElapsedR,
                            //elapsedReference = 66.7,
                            elapsedReference = device.BaseElapsedReference * 3,
                        };

                        //if (currentDev || runPrepareAction)
                        {
                            gyroAct.Prepare(this, ref mouseFrame);
                        }

                        if (gyroAct.active)
                        {
                            gyroAct.Event(this);
                        }

                        //sender.CombLatency = 0;
                    }

                    sideMapped |= JoyConSideUsed.JoyConR;
                }

                if (device.PrimaryDevice && intermediateState.Dirty)
                {
                    gamepadSync = true;
                }

                ProcessSyncEvents();

                ProcessActionSetLayerChecks();

                if (this.device == device)
                {
                    // Make copy of state data as the previous state
                    previousMapperState = currentMapperState;

                    //intermediateState = new IntermediateState();
                    sideMapped = JoyConSideUsed.None;
                }

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
                if (device != null)
                {
                    Xbox360FeedbackReceivedEventHandler tempDel = (Xbox360ScpOutDevice sender, byte large, byte small, int idx) =>
                    {
                        device.currentLeftAmpRatio = large / 255.0;
                        device.currentRightAmpRatio = small / 255.0;
                        using (WriteLocker locker = new WriteLocker(device.rumbleDataLock))
                        {
                            device.rumbleDirty = true;
                        }

                        //Trace.WriteLine($"TEST {e.LargeMotor} {e.SmallMotor}");
                        reader.WriteRumbleReport();
                    };
                    outputControllerSCP.forceFeedbacksDict.Add(device.Index, tempDel);
                }
                
                if (secondJoyDevice != null)
                {
                    Xbox360FeedbackReceivedEventHandler tempDel = (Xbox360ScpOutDevice sender, byte large, byte small, int idx) =>
                    {
                        secondJoyDevice.currentLeftAmpRatio = large / 255.0;
                        secondJoyDevice.currentRightAmpRatio = small / 255.0;
                        using (WriteLocker locker = new WriteLocker(secondJoyDevice.rumbleDataLock))
                        {
                            secondJoyDevice.rumbleDirty = true;
                        }
                        secondJoyReader.WriteRumbleReport();
                    };
                    outputControllerSCP.forceFeedbacksDict.Add(secondJoyDevice.Index, tempDel);
                }
            }
        }

        private void EstablishSecondaryForceFeedback()
        {
            Xbox360FeedbackReceivedEventHandler tempDel = (Xbox360ScpOutDevice sender, byte large, byte small, int idx) =>
            {
                secondJoyDevice.currentLeftAmpRatio = large / 255.0;
                secondJoyDevice.currentRightAmpRatio = small / 255.0;
                using (WriteLocker locker = new WriteLocker(secondJoyDevice.rumbleDataLock))
                {
                    secondJoyDevice.rumbleDirty = true;
                }
                secondJoyReader.WriteRumbleReport();
            };
            outputControllerSCP.forceFeedbacksDict.Add(secondJoyDevice.Index, tempDel);
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
    
        public void AssignSecondaryJoyCon(JoyConDevice device, JoyConReader reader)
        {
            secondJoyDevice = device;
            secondJoyReader = reader;

            secondJoyReader.Report += Reader_Report;
            secondJoyReader.LeftStickCalibUpdated += Reader_LeftStickCalibUpdated;
            secondJoyReader.RightStickCalibUpdated += Reader_RightStickCalibUpdated;
            secondJoyReader.StartUpdate();

            secondJoyDevice.Removal += SecondaryDeviceRemoval;

            if (actionProfile.OutputGamepadSettings.ForceFeedbackEnabled &&
                outputControlType == OutputContType.Xbox360 &&
                !outputControllerSCP.forceFeedbacksDict.ContainsKey(secondJoyDevice.Index))
            {
                EstablishSecondaryForceFeedback();
                //outputForceFeedbackSecondDel = (sender, e) =>
                //{
                //    secondJoyDevice.currentLeftAmpRatio = e.LargeMotor / 255.0;
                //    secondJoyDevice.currentRightAmpRatio = e.SmallMotor / 255.0;
                //    reader.WriteRumbleReport();
                //};
            }

            QueueEvent(() =>
            {
                if (device.SideType == JoyConSide.Left)
                {
                    // Replace placeholder data with real device data
                    lsDefintion.xAxis.min = (short)device.leftStickXData.min;
                    lsDefintion.xAxis.max = (short)device.leftStickXData.max;
                    lsDefintion.xAxis.mid = (short)device.leftStickXData.mid;
                    lsDefintion.xAxis.PostInit();

                    // Replace placeholder data with real device data
                    lsDefintion.yAxis.min = (short)device.leftStickYData.min;
                    lsDefintion.yAxis.max = (short)device.leftStickYData.max;
                    lsDefintion.yAxis.mid = (short)device.leftStickYData.mid;
                    lsDefintion.yAxis.PostInit();
                }
                else if (device.SideType == JoyConSide.Right)
                {
                    // Replace placeholder data with real device data
                    rsDefintion.xAxis.min = (short)device.rightStickXData.min;
                    rsDefintion.xAxis.max = (short)device.rightStickXData.max;
                    rsDefintion.xAxis.mid = (short)device.rightStickXData.mid;
                    rsDefintion.xAxis.PostInit();

                    // Replace placeholder data with real device data
                    rsDefintion.yAxis.min = (short)device.rightStickYData.min;
                    rsDefintion.yAxis.max = (short)device.rightStickYData.max;
                    rsDefintion.yAxis.mid = (short)device.rightStickYData.mid;
                    rsDefintion.yAxis.PostInit();
                }

                if (actionProfile.OutputGamepadSettings.ForceFeedbackEnabled &&
                    outputControlType == OutputContType.Xbox360 &&
                    outputControllerSCP.forceFeedbacksDict.ContainsKey(secondJoyDevice.Index))
                {
                    HookSecondaryFeedback();
                }

                //if (actionProfile.OutputGamepadSettings.ForceFeedbackEnabled &&
                //    outputControlType == OutputContType.Xbox360 &&
                //    outputForceFeedbackSecondDel != null)
                //{
                //    (outputController as IXbox360Controller).FeedbackReceived += outputForceFeedbackSecondDel;
                //}
            });
        }

        private void SecondaryDeviceRemoval(object sender, EventArgs e)
        {
            using WriteLocker locker = new WriteLocker(readerLock);

            secondJoyReader.Report -= Reader_Report;

            secondJoyDevice = null;
            secondJoyReader = null;
        }

        public override void HookFeedback()
        {
            if (actionProfile.OutputGamepadSettings.ForceFeedbackEnabled &&
                outputControlType == OutputContType.Xbox360)
            {
                if (outputControllerSCP.forceFeedbacksDict.TryGetValue(device.Index, out Xbox360FeedbackReceivedEventHandler tempDel))
                {
                    outputControllerSCP.FeedbackReceived += tempDel;
                }
                //if (outputForceFeedbackDel != null)
                //{
                //    (outputController as IXbox360Controller).FeedbackReceived += outputForceFeedbackDel;
                //}

                HookSecondaryFeedback();                
            }
        }

        public override void RemoveFeedback()
        {
            outputControllerSCP.RemoveFeedbacks();
        }

        private void HookSecondaryFeedback()
        {
            if (outputControllerSCP.forceFeedbacksDict.TryGetValue(secondJoyDevice.Index, out Xbox360FeedbackReceivedEventHandler tempDel))
            {
                outputControllerSCP.FeedbackReceived += tempDel;
            }
            //if (outputForceFeedbackSecondDel != null)
            //{
            //    (outputController as IXbox360Controller).FeedbackReceived += outputForceFeedbackSecondDel;
            //}
        }

        private void CopyStateData(JoyConDevice device, ref JoyConState srcState)
        {
            if (device.SideType == JoyConSide.Left)
            {
                currentMapperState.LX = srcState.LX;
                currentMapperState.LY = srcState.LY;
                currentMapperState.LSClick = srcState.LSClick;
                currentMapperState.Minus = srcState.Minus;
                currentMapperState.LShoulder = srcState.LShoulder;
                currentMapperState.ZL = srcState.ZL;
                currentMapperState.SideL = srcState.SideL;
                currentMapperState.SideR = srcState.SideR;
                currentMapperState.Capture = srcState.Capture;

                currentMapperState.DpadUp = srcState.DpadUp;
                currentMapperState.DpadDown = srcState.DpadDown;
                currentMapperState.DpadLeft = srcState.DpadLeft;
                currentMapperState.DpadRight = srcState.DpadRight;

                currentMapperState.timeElapsed = srcState.timeElapsed;
                currentMapperState.Motion = srcState.Motion;
                currentMapperState.MotionL = srcState.Motion;
            }
            else if (device.SideType == JoyConSide.Right)
            {
                currentMapperState.RX = srcState.RX;
                currentMapperState.RY = srcState.RY;
                currentMapperState.RSClick = srcState.RSClick;
                currentMapperState.Plus = srcState.Plus;
                currentMapperState.RShoulder = srcState.RShoulder;
                currentMapperState.ZR = srcState.ZR;
                currentMapperState.RightSideL = srcState.SideL;
                currentMapperState.RightSideR = srcState.SideR;
                currentMapperState.Home = srcState.Home;

                currentMapperState.A = srcState.A;
                currentMapperState.B = srcState.B;
                currentMapperState.X = srcState.X;
                currentMapperState.Y = srcState.Y;

                currentMapperState.timeElapsed = srcState.timeElapsed;
                currentMapperState.timeElapsedR = srcState.timeElapsed;
                currentMapperState.Motion = srcState.Motion;
                currentMapperState.MotionR = srcState.Motion;
            }
        }

        public override void Stop(bool finalSync = false)
        {
            //if (outputController != null &&
            //    outputForceFeedbackSecondDel != null)
            //{
            //    (outputController as IXbox360Controller).FeedbackReceived -= outputForceFeedbackSecondDel;
            //}

            base.Stop(finalSync);

            if (secondJoyDevice != null)
            {
                secondJoyReader.StopUpdate();
                secondJoyDevice.Detach();
                //secondJoyDevice.RaiseRemoval();
                secondJoyDevice = null;
                //secondJoyReader.StopUpdate();
            }
        }
    }
}
