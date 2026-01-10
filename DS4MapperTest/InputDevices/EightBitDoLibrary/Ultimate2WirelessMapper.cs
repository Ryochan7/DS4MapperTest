using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;
using Nefarius.ViGEm.Client;

namespace DS4MapperTest.InputDevices.EightBitDoLibrary
{
    internal class Ultimate2WirelessMapper : Mapper
    {
        private Ultimate2WirelessDevice device;
        private Ultimate2WirelessReader reader;

        public override InputDeviceType DeviceType => InputDeviceType.EightBitDoUltimate2Wireless;
        public override DeviceReaderBase BaseReader
        {
            get => reader;
        }

        private Ultimate2WirelessState currentMapperState;
        private Ultimate2WirelessState previousMapperState;

        private StickDefinition lsDefintion;
        private StickDefinition rsDefintion;
        private TriggerDefinition leftTriggerDefinition;
        private TriggerDefinition rightTriggerDefinition;
        private GyroSensDefinition gyroSensDefinition;

        public Ultimate2WirelessMapper(Ultimate2WirelessDevice device, Ultimate2WirelessReader reader,
            AppGlobalData appGlobal)
        {
            this.appGlobal = appGlobal;
            this.device = device;
            this.baseDevice = device;
            this.reader = reader;

            bindingList = new List<InputBindingMeta>()
            {
                new InputBindingMeta("A", "A", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("B", "B", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("X", "X", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Y", "Y", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LB", "LB", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RB", "RB", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LT", "LT", InputBindingMeta.InputControlType.Trigger),
                new InputBindingMeta("RT", "RT", InputBindingMeta.InputControlType.Trigger),
                new InputBindingMeta("LSClick", "LSClick", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RSClick", "RSClick", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("L4", "L4", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("R4", "R4", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("PL", "PL", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("PR", "PR", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Minus", "Minus", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Plus", "Plus", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Guide", "Guide", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LS", "Left Stick", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("RS", "Right Stick", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("DPad", "DPad", InputBindingMeta.InputControlType.DPad),
                new InputBindingMeta("Gyro", "Gyro", InputBindingMeta.InputControlType.Gyro),
            };

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
                hard_min = 0,
                hard_max = 255,
                hard_invert = true,
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

            gyroSensDefinition = new GyroSensDefinition()
            {
                //elapsedReference = 125.0,
                elapsedReference = device.BaseElapsedReference,
                mouseCoefficient = 0.012,
                mouseOffset = 0.2,

                accelMinLeanX = -Ultimate2WirelessState.Ult2Motion.ACC_RES_PER_G,
                accelMaxLeanX = Ultimate2WirelessState.Ult2Motion.ACC_RES_PER_G,
                accelMinLeanY = -Ultimate2WirelessState.Ult2Motion.ACC_RES_PER_G,
                accelMaxLeanY = Ultimate2WirelessState.Ult2Motion.ACC_RES_PER_G,
                accelMinLeanZ = -Ultimate2WirelessState.Ult2Motion.ACC_RES_PER_G,
                accelMaxLeanZ = Ultimate2WirelessState.Ult2Motion.ACC_RES_PER_G,
            };

            knownStickDefinitions.Add("LS", lsDefintion);
            knownStickDefinitions.Add("RS", rsDefintion);
            knownTriggerDefinitions.Add("LT", leftTriggerDefinition);
            knownTriggerDefinitions.Add("RT", rightTriggerDefinition);
            knownGyroSensDefinitions.Add("Gyro", gyroSensDefinition);

            actionTriggerItems.Clear();
            actionTriggerItems = new List<ActionTriggerItem>()
            {
                new ActionTriggerItem("Always On", JoypadActionCodes.AlwaysOn),
                new ActionTriggerItem("A", JoypadActionCodes.BtnSouth),
                new ActionTriggerItem("B", JoypadActionCodes.BtnEast),
                new ActionTriggerItem("X", JoypadActionCodes.BtnWest),
                new ActionTriggerItem("Y", JoypadActionCodes.BtnNorth),
                new ActionTriggerItem("L1", JoypadActionCodes.BtnLShoulder),
                new ActionTriggerItem("R2", JoypadActionCodes.BtnRShoulder),
                new ActionTriggerItem("LT", JoypadActionCodes.AxisLTrigger),
                new ActionTriggerItem("RT", JoypadActionCodes.AxisRTrigger),
                new ActionTriggerItem("LSClick", JoypadActionCodes.BtnThumbL),
                new ActionTriggerItem("RSClick", JoypadActionCodes.BtnThumbR),

                new ActionTriggerItem("L4", JoypadActionCodes.BtnLSideL),
                new ActionTriggerItem("R4", JoypadActionCodes.BtnLSideR),
                new ActionTriggerItem("PL", JoypadActionCodes.BtnLGrip),
                new ActionTriggerItem("PR", JoypadActionCodes.BtnRGrip),

                new ActionTriggerItem("Share", JoypadActionCodes.BtnSelect),
                new ActionTriggerItem("Options", JoypadActionCodes.BtnStart),
                new ActionTriggerItem("Guide", JoypadActionCodes.BtnHome),
                new ActionTriggerItem("DPad Up", JoypadActionCodes.BtnDPadUp),
                new ActionTriggerItem("DPad Down", JoypadActionCodes.BtnDPadDown),
                new ActionTriggerItem("DPad Left", JoypadActionCodes.BtnDPadLeft),
                new ActionTriggerItem("DPad Right", JoypadActionCodes.BtnDPadRight),
            };
        }

        public override void Start(ViGEmClient vigemTestClient, VirtualKBMBase eventInputHandler)
        {
            base.Start(vigemTestClient, eventInputHandler);

            reader.Report += Reader_Report;
            reader.StartUpdate();
        }

        private void Reader_Report(Ultimate2WirelessReader sender, Ultimate2WirelessDevice device)
        {
            while (pauseMapper)
            {
                Thread.SpinWait(500);
            }

            mapperActionActive = true;

            ref Ultimate2WirelessState current = ref device.CurrentStateRef;
            ref Ultimate2WirelessState previous = ref device.PreviousStateRef;

            // Copy state struct data for later mapper manipulation. Leave
            // device state instance alone
            currentMapperState = device.CurrentState;

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

                TriggerMapAction trigMapAction = currentLayer.triggerActionDict["LT"];
                {
                    TriggerEventFrame eventFrame = new TriggerEventFrame
                    {
                        axisValue = currentMapperState.LT,
                    };
                    trigMapAction.Prepare(this, ref eventFrame);
                }
                if (trigMapAction.active) trigMapAction.Event(this);

                trigMapAction = currentLayer.triggerActionDict["RT"];
                {
                    TriggerEventFrame eventFrame = new TriggerEventFrame
                    {
                        axisValue = currentMapperState.RT,
                    };
                    trigMapAction.Prepare(this, ref eventFrame);
                }
                if (trigMapAction.active) trigMapAction.Event(this);

                ButtonMapAction tempBtnAct = currentLayer.buttonActionDict["A"];
                if (currentMapperState.A || currentMapperState.A != previousMapperState.A)
                {
                    tempBtnAct.Prepare(this, currentMapperState.A);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["B"];
                if (currentMapperState.B || currentMapperState.B != previousMapperState.B)
                {
                    tempBtnAct.Prepare(this, currentMapperState.B);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["X"];
                if (currentMapperState.X || currentMapperState.X != previousMapperState.X)
                {
                    tempBtnAct.Prepare(this, currentMapperState.X);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Y"];
                if (currentMapperState.Y || currentMapperState.Y != previousMapperState.Y)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Y);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["LB"];
                if (currentMapperState.LB || currentMapperState.LB != previousMapperState.LB)
                {
                    tempBtnAct.Prepare(this, currentMapperState.LB);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["RB"];
                if (currentMapperState.RB || currentMapperState.RB != previousMapperState.RB)
                {
                    tempBtnAct.Prepare(this, currentMapperState.RB);
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

                tempBtnAct = currentLayer.buttonActionDict["L4"];
                if (currentMapperState.L4 || currentMapperState.L4 != previousMapperState.L4)
                {
                    tempBtnAct.Prepare(this, currentMapperState.L4);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["R4"];
                if (currentMapperState.R4 || currentMapperState.R4 != previousMapperState.R4)
                {
                    tempBtnAct.Prepare(this, currentMapperState.R4);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["PL"];
                if (currentMapperState.PL || currentMapperState.PL != previousMapperState.PL)
                {
                    tempBtnAct.Prepare(this, currentMapperState.PL);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["PR"];
                if (currentMapperState.PR || currentMapperState.PR != previousMapperState.PR)
                {
                    tempBtnAct.Prepare(this, currentMapperState.PR);
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
                }

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

        public override void EstablishForceFeedback()
        {
            if (outputControlType == OutputContType.Xbox360)
            {
                outputForceFeedbackDel = (sender, e) =>
                {
                    device.FeedbackStateRef.LeftHeavy = e.LargeMotor;
                    device.FeedbackStateRef.RightLight = e.SmallMotor;
                    device.RumbleDirty = true;
                    //rumbleDirty = true;
                    //reader.WriteRumbleReport();
                };
            }
        }

        public override bool IsButtonActive(JoypadActionCodes code)
        {
            bool result = false;
            result = CheckButtonActive(code, result);
            return result;
        }

        public override bool IsButtonsActiveDraft(IEnumerable<JoypadActionCodes> codes, bool andEval = true)
        {
            bool result = false;
            foreach (JoypadActionCodes code in codes)
            {
                bool btnActive = CheckButtonActive(code, result);
                if (andEval && !btnActive)
                {
                    // All buttons in the list must be active
                    result = false;
                    break;
                }
                else if (!andEval && btnActive)
                {
                    // Only care if any button in the list is active
                    result = true;
                    break;
                }
                else
                {
                    result = btnActive;
                }
            }

            return result;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckButtonActive(JoypadActionCodes code, bool result)
        {
            switch (code)
            {
                case JoypadActionCodes.AlwaysOn:
                    result = true;
                    break;
                case JoypadActionCodes.BtnSouth:
                    result = currentMapperState.A;
                    break;
                case JoypadActionCodes.BtnEast:
                    result = currentMapperState.B;
                    break;
                case JoypadActionCodes.BtnNorth:
                    result = currentMapperState.Y;
                    break;
                case JoypadActionCodes.BtnWest:
                    result = currentMapperState.X;
                    break;
                case JoypadActionCodes.BtnLShoulder:
                    result = currentMapperState.LB;
                    break;
                case JoypadActionCodes.BtnRShoulder:
                    result = currentMapperState.RB;
                    break;
                case JoypadActionCodes.BtnSelect:
                    result = currentMapperState.Minus;
                    break;
                case JoypadActionCodes.BtnStart:
                    result = currentMapperState.Plus;
                    break;
                case JoypadActionCodes.BtnMode:
                    result = currentMapperState.Guide;
                    break;
                case JoypadActionCodes.AxisLTrigger:
                    result = currentMapperState.LT > 0;
                    break;
                case JoypadActionCodes.AxisRTrigger:
                    result = currentMapperState.RT > 0;
                    break;
                case JoypadActionCodes.BtnThumbL:
                    result = currentMapperState.LSClick;
                    break;
                case JoypadActionCodes.BtnThumbR:
                    result = currentMapperState.RSClick;
                    break;
                case JoypadActionCodes.BtnLSideL:
                    result = currentMapperState.L4;
                    break;
                case JoypadActionCodes.BtnLSideR:
                    result = currentMapperState.R4;
                    break;
                case JoypadActionCodes.BtnLGrip:
                    result = currentMapperState.PL;
                    break;
                case JoypadActionCodes.BtnRGrip:
                    result = currentMapperState.PR;
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
                default:
                    break;
            }

            return result;
        }
    }
}
