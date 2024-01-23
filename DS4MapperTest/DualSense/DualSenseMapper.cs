﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.DS4Library;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.ScpVBus;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;
using static DS4MapperTest.ScpVBus.Xbox360ScpOutDevice;

namespace DS4MapperTest.DualSense
{
    public class DualSenseMapper : Mapper
    {
        private DualSenseDevice device;
        private DualSenseReader reader;
        private LightbarProcessor lightProcess = new LightbarProcessor();
        public override InputDeviceType DeviceType => InputDeviceType.DualSense;

        public override DeviceReaderBase BaseReader
        {
            get => reader;
        }

        private DualSenseState currentMapperState;
        private DualSenseState previousMapperState;

        private TouchEventFrame previousTouchFramePad;

        private StickDefinition lsDefintion;
        private StickDefinition rsDefintion;
        private TriggerDefinition leftTriggerDefinition;
        private TriggerDefinition rightTriggerDefinition;
        private TouchpadDefinition cpadDefinition;
        private GyroSensDefinition gyroSensDefinition;

        public DualSenseMapper(DualSenseDevice device, DualSenseReader reader, AppGlobalData appGlobal)
        {
            this.appGlobal = appGlobal;
            this.device = device;
            this.reader = reader;
            this.baseDevice = device;

            deviceActionDefaults = new DS4ActionDefaultsCreator();

            bindingList = new List<InputBindingMeta>()
            {
                new InputBindingMeta("Cross", "Cross", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Circle", "Circle", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Square", "Square", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Triangle", "Triangle", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("L1", "L1", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("R1", "R1", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("L2", "L2", InputBindingMeta.InputControlType.Trigger),
                new InputBindingMeta("R2", "R2", InputBindingMeta.InputControlType.Trigger),
                new InputBindingMeta("L3", "L3", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("R3", "R3", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Create", "Create", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Options", "Options", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("PS", "PS", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Mute", "Mute", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("TouchClick", "Touch Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LS", "Left Stick", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("RS", "Right Stick", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("DPad", "DPad", InputBindingMeta.InputControlType.DPad),
                new InputBindingMeta("Gyro", "Gyro", InputBindingMeta.InputControlType.Gyro),
                new InputBindingMeta("Touchpad", "Touchpad", InputBindingMeta.InputControlType.Touchpad),
            };

            if (device.SubType == DualSenseDevice.DeviceSubType.DSEdge)
            {
                bindingList.AddRange(new InputBindingMeta[] {
                    new InputBindingMeta("FnL", "Function L", InputBindingMeta.InputControlType.Button),
                    new InputBindingMeta("FnR", "Function R", InputBindingMeta.InputControlType.Button),
                    new InputBindingMeta("BLP", "Paddle L", InputBindingMeta.InputControlType.Button),
                    new InputBindingMeta("BRP", "Paddle R", InputBindingMeta.InputControlType.Button),
                });
            }

            // Populate Input Binding dictionary
            bindingList.ForEach((item) => bindingDict.Add(item.id, item));

            StickDefinition.StickAxisData lxAxis = new StickDefinition.StickAxisData
            {
                min = 0,
                max = 255,
                mid = 128,
                hard_min = 0,
                hard_max = 255,
            };
            lxAxis.PostInit();
            StickDefinition.StickAxisData lyAxis = new StickDefinition.StickAxisData
            {
                min = 0,
                max = 255,
                mid = 128,
                invert = true,
                hard_min = 0,
                hard_max = 255,
            };
            lyAxis.PostInit();

            StickDefinition.StickAxisData rxAxis = lxAxis;
            StickDefinition.StickAxisData ryAxis = lyAxis;
            //StickDefinition lsDefintion = new StickDefinition(STICK_MIN, STICK_MAX, STICK_NEUTRAL, StickActionCodes.LS);
            lsDefintion = new StickDefinition(lxAxis, lyAxis, StickActionCodes.LS);
            rsDefintion = new StickDefinition(rxAxis, ryAxis, StickActionCodes.RS);

            TriggerDefinition.TriggerAxisData ltAxis = new TriggerDefinition.TriggerAxisData
            {
                min = 0,
                max = 255,
            };

            leftTriggerDefinition = new TriggerDefinition(ltAxis, TriggerActionCodes.LeftTrigger);

            // Copy struct
            TriggerDefinition.TriggerAxisData rtAxis = new TriggerDefinition.TriggerAxisData
            {
                min = 0,
                max = 255,
            };

            rightTriggerDefinition = new TriggerDefinition(rtAxis, TriggerActionCodes.RightTrigger);

            TouchpadDefinition.TouchAxisData cpadXAxis = new TouchpadDefinition.TouchAxisData
            {
                min = 0,
                max = 1920,
                mid = 1920 / 2,

                hard_min = 0,
                hard_max = 1920,
            };
            cpadXAxis.PostInit();

            TouchpadDefinition.TouchAxisData cpadYAxis = new TouchpadDefinition.TouchAxisData
            {
                min = 0,
                max = 1079,
                mid = 1079 / 2,

                hard_min = 0,
                hard_max = 1079,
            };
            cpadYAxis.PostInit();

            cpadDefinition = new TouchpadDefinition(cpadXAxis, cpadYAxis,
                TouchpadActionCodes.Touch, elapsedReference: device.BaseElapsedReference,
                mouseScale: 1.0, mouseOffset: 0.015, trackballScale: 0.004);

            gyroSensDefinition = new GyroSensDefinition()
            {
                //elapsedReference = 250.0,
                elapsedReference = device.BaseElapsedReference,
                mouseCoefficient = 0.009,
                mouseOffset = 0.15,

                accelMinLeanX = -8192,
                accelMaxLeanX = 8192,
                accelMinLeanY = -8192,
                accelMaxLeanY = 8192,
                accelMinLeanZ = -8192,
                accelMaxLeanZ = 8192,
            };

            knownStickDefinitions.Add("LS", lsDefintion);
            knownStickDefinitions.Add("RS", rsDefintion);
            knownTriggerDefinitions.Add("L2", leftTriggerDefinition);
            knownTriggerDefinitions.Add("R2", rightTriggerDefinition);
            knownTouchpadDefinitions.Add("Touchpad", cpadDefinition);
            knownGyroSensDefinitions.Add("Gyro", gyroSensDefinition);

            actionTriggerItems.Clear();
            actionTriggerItems = new List<ActionTriggerItem>()
            {
                new ActionTriggerItem("Always On", JoypadActionCodes.AlwaysOn),
                new ActionTriggerItem("Cross", JoypadActionCodes.BtnSouth),
                new ActionTriggerItem("Circle", JoypadActionCodes.BtnEast),
                new ActionTriggerItem("Square", JoypadActionCodes.BtnWest),
                new ActionTriggerItem("Triangle", JoypadActionCodes.BtnNorth),
                new ActionTriggerItem("L1", JoypadActionCodes.BtnLShoulder),
                new ActionTriggerItem("R2", JoypadActionCodes.BtnRShoulder),
                new ActionTriggerItem("L2", JoypadActionCodes.AxisLTrigger),
                new ActionTriggerItem("R2", JoypadActionCodes.AxisRTrigger),
                new ActionTriggerItem("L3", JoypadActionCodes.BtnThumbL),
                new ActionTriggerItem("R3", JoypadActionCodes.BtnThumbR),
                new ActionTriggerItem("Touchpad Touch", JoypadActionCodes.LPadTouch),
                new ActionTriggerItem("Touchpad Click", JoypadActionCodes.LPadClick),
                new ActionTriggerItem("Create", JoypadActionCodes.BtnSelect),
                new ActionTriggerItem("Options", JoypadActionCodes.BtnStart),
                new ActionTriggerItem("PS", JoypadActionCodes.BtnHome),
                new ActionTriggerItem("Mute", JoypadActionCodes.BtnMode),
                new ActionTriggerItem("DPad Up", JoypadActionCodes.BtnDPadUp),
                new ActionTriggerItem("DPad Down", JoypadActionCodes.BtnDPadDown),
                new ActionTriggerItem("DPad Left", JoypadActionCodes.BtnDPadLeft),
                new ActionTriggerItem("DPad Right", JoypadActionCodes.BtnDPadRight),
            };

            if (device.SubType == DualSenseDevice.DeviceSubType.DSEdge)
            {
                actionTriggerItems.AddRange(new ActionTriggerItem[]
                {
                    new ActionTriggerItem("FnL", JoypadActionCodes.BtnMode2),
                    new ActionTriggerItem("FnR", JoypadActionCodes.BtnMode3),
                    new ActionTriggerItem("BLP", JoypadActionCodes.BtnLGrip),
                    new ActionTriggerItem("BLR", JoypadActionCodes.BtnRGrip),
                });
            }
        }

        public override void Start(X360BusDevice busDevice, FakerInputHandler fakerInputHandler)
        {
            PostProfileChange += DualSenseMapper_PostProfileChange;
            lightProcess.Reset();

            base.Start(busDevice, fakerInputHandler);
            // Update current lightbar status before sending first output packet
            lightProcess.UpdateLightbarDS(device, actionProfile);

            reader.Report += Reader_Report;
            reader.StartUpdate();
        }

        private void DualSenseMapper_PostProfileChange(object sender, EventArgs e)
        {
            if (actionProfile.LightbarSettings.Mode == LightbarMode.SolidColor)
            {
                DS4Color tempColor = actionProfile.LightbarSettings.SolidColor;
                device.SetLightbarColor(ref tempColor);
            }
        }

        private void Reader_Report(DualSenseReader sender, DualSenseDevice device)
        {
            while (pauseMapper)
            {
                Thread.SpinWait(500);
            }

            mapperActionActive = true;

            ref DualSenseState current = ref device.CurrentStateRef;
            //ref DualSenseState previous = ref device.PreviousStateRef;

            // Copy state struct data for later mapper manipulation. Leave
            // device state instance alone
            currentMapperState = device.CurrentState;

            mouseX = mouseY = 0.0;

            unchecked
            {
                //outputController?.ResetReport();

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
                    //Trace.WriteLine($"{currentMapperState.LX} {currentMapperState.LY}");
                    int LX = Math.Clamp(currentMapperState.LX, lsDefintion.xAxis.min, lsDefintion.xAxis.max);
                    int LY = Math.Clamp(currentMapperState.LY, lsDefintion.yAxis.min, lsDefintion.yAxis.max);
                    LY = AxisScale(LY, true, lsDefintion.yAxis);
                    mapAction.Prepare(this, LX, LY);
                }

                if (mapAction.active)
                {
                    mapAction.Event(this);
                }

                mapAction = currentLayer.stickActionDict["RS"];
                //if ((currentMapperState.RX != previousMapperState.RX) || (currentMapperState.RY != previousMapperState.RY))
                {
                    //Trace.WriteLine($"{currentMapperState.RX} {currentMapperState.RY}");
                    int RX = Math.Clamp(currentMapperState.RX, rsDefintion.xAxis.min, rsDefintion.xAxis.max);
                    int RY = Math.Clamp(currentMapperState.RY, rsDefintion.yAxis.min, rsDefintion.yAxis.max);
                    RY = AxisScale(RY, true, rsDefintion.yAxis);
                    mapAction.Prepare(this, RX, RY);
                }

                if (mapAction.active)
                {
                    mapAction.Event(this);
                }


                TriggerMapAction trigMapAction = currentLayer.triggerActionDict["L2"];
                {
                    TriggerEventFrame eventFrame = new TriggerEventFrame
                    {
                        axisValue = currentMapperState.L2,
                    };
                    trigMapAction.Prepare(this, ref eventFrame);
                }
                if (trigMapAction.active) trigMapAction.Event(this);

                trigMapAction = currentLayer.triggerActionDict["R2"];
                {
                    TriggerEventFrame eventFrame = new TriggerEventFrame
                    {
                        axisValue = currentMapperState.R2,
                    };
                    trigMapAction.Prepare(this, ref eventFrame);
                }
                if (trigMapAction.active) trigMapAction.Event(this);


                ButtonMapAction tempBtnAct = currentLayer.buttonActionDict["Cross"];
                if (currentMapperState.Cross || currentMapperState.Cross != previousMapperState.Cross)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Cross);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Circle"];
                if (currentMapperState.Circle || currentMapperState.Circle != previousMapperState.Circle)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Circle);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Square"];
                if (currentMapperState.Square || currentMapperState.Square != previousMapperState.Square)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Square);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Triangle"];
                if (currentMapperState.Triangle || currentMapperState.Triangle != previousMapperState.Triangle)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Triangle);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["L1"];
                if (currentMapperState.L1 || currentMapperState.L1 != previousMapperState.L1)
                {
                    tempBtnAct.Prepare(this, currentMapperState.L1);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["R1"];
                if (currentMapperState.R1 || currentMapperState.R1 != previousMapperState.R1)
                {
                    tempBtnAct.Prepare(this, currentMapperState.R1);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);


                tempBtnAct = currentLayer.buttonActionDict["L3"];
                if (currentMapperState.L3 || currentMapperState.L3 != previousMapperState.L3)
                {
                    tempBtnAct.Prepare(this, currentMapperState.L3);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["R3"];
                if (currentMapperState.R3 || currentMapperState.R3 != previousMapperState.R3)
                {
                    tempBtnAct.Prepare(this, currentMapperState.R3);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Create"];
                if (currentMapperState.Create || currentMapperState.Create != previousMapperState.Create)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Create);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Options"];
                if (currentMapperState.Options || currentMapperState.Options != previousMapperState.Options)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Options);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["PS"];
                if (currentMapperState.PS || currentMapperState.PS != previousMapperState.PS)
                {
                    tempBtnAct.Prepare(this, currentMapperState.PS);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["TouchClick"];
                if (currentMapperState.TouchClickButton || currentMapperState.TouchClickButton != previousMapperState.TouchClickButton)
                {
                    tempBtnAct.Prepare(this, currentMapperState.TouchClickButton);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Mute"];
                if (currentMapperState.Mute || currentMapperState.Mute != previousMapperState.Mute)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Mute);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                // Check state of extra DualSense Edge buttons
                if (device.SubType == DualSenseDevice.DeviceSubType.DSEdge)
                {
                    tempBtnAct = currentLayer.buttonActionDict["FnL"];
                    if (currentMapperState.FnL || currentMapperState.FnL != previousMapperState.FnL)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.FnL);
                    }

                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["FnR"];
                    if (currentMapperState.FnR || currentMapperState.FnR != previousMapperState.FnR)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.FnR);
                    }

                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["BLP"];
                    if (currentMapperState.BLP || currentMapperState.BLP != previousMapperState.BLP)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.BLP);
                    }

                    if (tempBtnAct.active) tempBtnAct.Event(this);

                    tempBtnAct = currentLayer.buttonActionDict["BRP"];
                    if (currentMapperState.BRP || currentMapperState.BRP != previousMapperState.BRP)
                    {
                        tempBtnAct.Prepare(this, currentMapperState.BRP);
                    }

                    if (tempBtnAct.active) tempBtnAct.Event(this);
                }

                DpadDirections currentDpad =
                    DpadDirections.Centered;

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
                    GyroEventFrame gyroFrame = new GyroEventFrame
                    {
                        GyroYaw = currentMapperState.Motion.GyroYaw,
                        GyroPitch = (short)-currentMapperState.Motion.GyroPitch,
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
                        elapsedReference = gyroSensDefinition.elapsedReference,
                    };

                    gyroAct.Prepare(this, ref gyroFrame);
                    if (gyroAct.active)
                    {
                        gyroAct.Event(this);
                    }

                    //sender.CombLatency = 0;
                }

                TouchpadMapAction tempTouchAction = currentLayer.touchpadActionDict["Touchpad"];
                //if (current.TouchPacketNum != previousMapperState.TouchPacketNum)
                //if (currentMapperState.LeftPad.Touch || currentMapperState.LeftPad.Touch != previousMapperState.LeftPad.Touch)
                {
                    //Trace.WriteLine($"{currentMapperState.LeftPad.X} {currentMapperState.LeftPad.Y}");
                    TouchEventFrame eventFrame = new TouchEventFrame
                    {
                        X = Math.Clamp(currentMapperState.Touch1.X, (short)0, (short)DualSenseState.TouchInfo.TOUCHPAD_MAX_X),
                        X2 = Math.Clamp(currentMapperState.Touch2.X, (short)0, (short)DualSenseState.TouchInfo.TOUCHPAD_MAX_X),
                        Y = Math.Clamp(TouchpadAxisScale(currentMapperState.Touch1.Y, true, cpadDefinition.yAxis),
                            (short)0, (short)DualSenseState.TouchInfo.TOUCHPAD_MAX_Y),
                        Y2 = Math.Clamp(TouchpadAxisScale(currentMapperState.Touch2.Y, true, cpadDefinition.yAxis),
                            (short)0, (short)DualSenseState.TouchInfo.TOUCHPAD_MAX_Y),
                        Touch = currentMapperState.Touch1.Touch,
                        numTouches = currentMapperState.NumTouches,
                        timeElapsed = currentMapperState.timeElapsed,
                        passDelta = current.TouchPacketNum == previousMapperState.TouchPacketNum,
                    };

                    //Trace.WriteLine($"{eventFrame.X} {eventFrame.Y} deltax:{eventFrame.X - previousTouchFramePad.X} | deltay:{eventFrame.Y - previousTouchFramePad.Y}");
                    //Trace.WriteLine("BACON");

                    tempTouchAction.Prepare(this, ref eventFrame);
                    if (tempTouchAction.active) tempTouchAction.Event(this);

                    previousTouchFramePad = eventFrame;
                }
            }

            lightProcess.UpdateLightbarDS(device, actionProfile);

            gamepadSync = intermediateState.Dirty;

            ProcessSyncEvents();

            ProcessActionSetLayerChecks();

            // Make copy of state data as the previous state
            previousMapperState = currentMapperState;

            mapperActionActive = false;

            if (hasInputEvts)
            {
                ProcessQueuedActions();
            }
        }

        private short AxisScale(int value, bool flip, StickDefinition.StickAxisData axisData)
        {
            unchecked
            {
                double temp = (value - axisData.min) * axisData.reciprocalInputResolution;
                if (flip) temp = (temp - 0.5f) * -1.0f + 0.5f;
                return (short)((axisData.max - axisData.min) * temp + axisData.min);
            }
        }

        private short TouchpadAxisScale(int value, bool flip, TouchpadDefinition.TouchAxisData axisData)
        {
            unchecked
            {
                double temp = (value - axisData.min) * axisData.reciprocalInputResolution;
                if (flip) temp = (temp - 0.5f) * -1.0f + 0.5f;
                return (short)((axisData.max - axisData.min) * temp + axisData.min);
            }
        }

        public override ref TouchEventFrame GetPreviousTouchEventFrame(TouchpadActionCodes padID)
        {
            switch (padID)
            {
                case TouchpadActionCodes.Touch1:
                    return ref previousTouchFramePad;
                default:
                    break;
            }

            return ref previousTouchFramePad;
        }

        public override bool IsButtonActive(JoypadActionCodes code)
        {
            bool result = false;
            switch (code)
            {
                case JoypadActionCodes.AlwaysOn:
                    result = true;
                    break;
                case JoypadActionCodes.BtnNorth:
                    result = currentMapperState.Triangle;
                    break;
                case JoypadActionCodes.BtnSouth:
                    result = currentMapperState.Cross;
                    break;
                case JoypadActionCodes.BtnEast:
                    result = currentMapperState.Circle;
                    break;
                case JoypadActionCodes.BtnWest:
                    result = currentMapperState.Square;
                    break;
                case JoypadActionCodes.BtnLShoulder:
                    result = currentMapperState.L1;
                    break;
                case JoypadActionCodes.BtnRShoulder:
                    result = currentMapperState.R1;
                    break;
                case JoypadActionCodes.BtnThumbL:
                    result = currentMapperState.L3;
                    break;
                case JoypadActionCodes.BtnThumbR:
                    result = currentMapperState.R3;
                    break;
                case JoypadActionCodes.BtnSelect:
                    result = currentMapperState.Create;
                    break;
                case JoypadActionCodes.BtnStart:
                    result = currentMapperState.Options;
                    break;
                case JoypadActionCodes.BtnMode:
                    result = currentMapperState.Mute;
                    break;
                case JoypadActionCodes.BtnMode2:
                    result = currentMapperState.FnL;
                    break;
                case JoypadActionCodes.BtnMode3:
                    result = currentMapperState.FnR;
                    break;
                case JoypadActionCodes.BtnLGrip:
                    result = currentMapperState.BLP;
                    break;
                case JoypadActionCodes.BtnRGrip:
                    result = currentMapperState.BRP;
                    break;
                case JoypadActionCodes.BtnDPadUp:
                    result = currentMapperState.DpadUp;
                    break;
                case JoypadActionCodes.BtnDPadDown:
                    result = currentMapperState.DpadDown;
                    break;
                case JoypadActionCodes.BtnDPadLeft:
                    result = currentMapperState.DpadLeft;
                    break;
                case JoypadActionCodes.BtnDPadRight:
                    result = currentMapperState.DpadRight;
                    break;
                case JoypadActionCodes.LPadClick:
                    result = currentMapperState.TouchClickButton;
                    break;
                case JoypadActionCodes.LPadTouch:
                    result = currentMapperState.NumTouches > 0;
                    break;

                default: break;
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
                    case JoypadActionCodes.BtnNorth:
                        result = currentMapperState.Triangle;
                        break;
                    case JoypadActionCodes.BtnSouth:
                        result = currentMapperState.Cross;
                        break;
                    case JoypadActionCodes.BtnEast:
                        result = currentMapperState.Circle;
                        break;
                    case JoypadActionCodes.BtnWest:
                        result = currentMapperState.Square;
                        break;
                    case JoypadActionCodes.BtnLShoulder:
                        result = currentMapperState.L1;
                        break;
                    case JoypadActionCodes.BtnRShoulder:
                        result = currentMapperState.R1;
                        break;
                    case JoypadActionCodes.BtnThumbL:
                        result = currentMapperState.L3;
                        break;
                    case JoypadActionCodes.BtnThumbR:
                        result = currentMapperState.R3;
                        break;
                    case JoypadActionCodes.BtnSelect:
                        result = currentMapperState.Create;
                        break;
                    case JoypadActionCodes.BtnStart:
                        result = currentMapperState.Options;
                        break;
                    case JoypadActionCodes.BtnHome:
                        result = currentMapperState.PS;
                        break;
                    case JoypadActionCodes.BtnMode:
                        result = currentMapperState.Mute;
                        break;
                    case JoypadActionCodes.BtnMode2:
                        result = currentMapperState.FnL;
                        break;
                    case JoypadActionCodes.BtnMode3:
                        result = currentMapperState.FnR;
                        break;
                    case JoypadActionCodes.BtnLGrip:
                        result = currentMapperState.BLP;
                        break;
                    case JoypadActionCodes.BtnRGrip:
                        result = currentMapperState.BRP;
                        break;
                    case JoypadActionCodes.BtnDPadUp:
                        result = currentMapperState.DpadUp;
                        break;
                    case JoypadActionCodes.BtnDPadDown:
                        result = currentMapperState.DpadDown;
                        break;
                    case JoypadActionCodes.BtnDPadLeft:
                        result = currentMapperState.DpadLeft;
                        break;
                    case JoypadActionCodes.BtnDPadRight:
                        result = currentMapperState.DpadRight;
                        break;
                    case JoypadActionCodes.LPadClick:
                        result = currentMapperState.TouchClickButton;
                        break;
                    case JoypadActionCodes.LPadTouch:
                        result = currentMapperState.NumTouches > 0;
                        break;

                    default: break;
                }
            }

            return result;
        }

        public override void EstablishForceFeedback()
        {
            if (outputControlType == OutputContType.Xbox360)
            {
                Xbox360FeedbackReceivedEventHandler tempDel = (Xbox360ScpOutDevice sender, byte large, byte small, int idx) =>
                {
                    device.FeedbackStateRef.LeftHeavy = large;
                    device.FeedbackStateRef.RightLight = small;
                    device.HapticsDirty = true;
                    //reader.WriteRumbleReport();
                };
                outputControllerSCP.forceFeedbacksDict.Add(device.Index, tempDel);
            }
        }
    }
}
