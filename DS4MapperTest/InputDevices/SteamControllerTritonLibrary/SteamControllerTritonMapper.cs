using DS4MapperTest.ButtonActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;
using Nefarius.ViGEm.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS4MapperTest.InputDevices.SteamControllerTritonLibrary
{
    public class SteamControllerTritonMapper : Mapper
    {
        private SteamControllerTritonDevice device;
        private SteamControllerTritonReader reader;

        private SteamControllerState currentMapperState;
        private SteamControllerState previousMapperState;

        private TouchEventFrame previousTouchFrameLeftPad;
        private TouchEventFrame previousTouchFrameRightPad;

        public override DeviceReaderBase BaseReader => reader;
        public override InputDeviceType DeviceType => InputDeviceType.SteamControllerTriton;

        private StickDefinition lsDefintion;
        private StickDefinition rsDefintion;
        private TouchpadDefinition leftPadDefiniton;
        private TouchpadDefinition rightPadDefinition;
        private TriggerDefinition leftTriggerDefinition;
        private TriggerDefinition rightTriggerDefinition;
        private GyroSensDefinition gyroSensDefinition;

        private const short STICK_MAX = 30000;
        private const short STICK_MIN = -30000;

        private const int TRACKBALL_INIT_FRICTION = 10;
        private const int TRACKBALL_JOY_FRICTION = 7;
        private const int TRACKBALL_MASS = 45;
        private const double TRACKBALL_RADIUS = 0.0245;
        private double TRACKBALL_INERTIA = 2.0 * (TRACKBALL_MASS * TRACKBALL_RADIUS * TRACKBALL_RADIUS) / 5.0;
        //private double TRACKBALL_SCALE = 0.000023;
        private double TRACKBALL_SCALE = 0.000023;

        private const double TRACKPAD_MOUSE_POWER = 1.40;
        private const double TRACKPAD_MOUSE_DISPLACEMENT = 375.0;

        private double trackballAccel = 0.0;
        private bool hapticsEvent;
        private double pendingHapticsLeftAmpRatio = 0.0;
        private double pendingHapticsRightAmpRatio = 0.0;
        private double pendingRumbleLeftAmpRatio = 0.0;
        private double pendingRumbleRightAmpRatio = 0.0;

        private SteamControllerTritonDevice.HapticFeedbackInfo hapticsInfo =
            new SteamControllerTritonDevice.HapticFeedbackInfo();
        private Stopwatch standBySw = new Stopwatch();

        public SteamControllerTritonMapper(SteamControllerTritonDevice device, SteamControllerTritonReader reader,
            AppGlobalData appGlobal)
        {
            this.device = device;
            this.baseDevice = device;
            this.reader = reader;
            this.appGlobal = appGlobal;

            deviceActionDefaults = new SteamControllerTritonDeviceDefaults();

            bindingList = new List<InputBindingMeta>()
            {
                new InputBindingMeta("A", "A", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("B", "B", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("X", "X", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Y", "Y", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Select", "Select", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Start", "Start", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("L1", "L1", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("R1", "R1", InputBindingMeta.InputControlType.Button),

                new InputBindingMeta("L2", "L2", InputBindingMeta.InputControlType.Trigger),
                new InputBindingMeta("R2", "R2", InputBindingMeta.InputControlType.Trigger),

                new InputBindingMeta("L3", "L3", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("R3", "R3", InputBindingMeta.InputControlType.Button),

                new InputBindingMeta("Steam", "Steam", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("QAM", "QAM", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LS", "LS", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("RS", "RS", InputBindingMeta.InputControlType.Stick),

                new InputBindingMeta("L4", "L4", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("R4", "R4", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("L5", "L5", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("R5", "R5", InputBindingMeta.InputControlType.Button),

                new InputBindingMeta("LSTouch", "LS Touch", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RSTouch", "RS Touch", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LeftGripSense", "Left GripSense", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RightGripSense", "Right GripSense", InputBindingMeta.InputControlType.Button),

                new InputBindingMeta("DPad", "DPad", InputBindingMeta.InputControlType.DPad),

                new InputBindingMeta("LeftTouchpad", "Left Touchpad", InputBindingMeta.InputControlType.Touchpad),
                new InputBindingMeta("RightTouchpad", "Right Touchpad", InputBindingMeta.InputControlType.Touchpad),
                new InputBindingMeta("LeftPadClick", "Left Pad Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LeftPadTouch", "Left Pad Touch", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RightPadClick", "Right Pad Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RightPadTouch", "Right Pad Touch", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Gyro", "Gyro", InputBindingMeta.InputControlType.Gyro),
            };

            // Populate Input Binding dictionary
            bindingList.ForEach((item) => bindingDict.Add(item.id, item));

            //trackballAccel = TRACKBALL_RADIUS * TRACKBALL_INIT_FRICTION / TRACKBALL_INERTIA;
            trackballAccel = TRACKBALL_RADIUS * TRACKBALL_JOY_FRICTION / TRACKBALL_INERTIA;

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

            StickDefinition.StickAxisData rxAxis = new StickDefinition.StickAxisData
            {
                min = -30000,
                max = 30000,
                mid = 0,
                hard_max = 32767,
                hard_min = -32767,
            };
            StickDefinition.StickAxisData ryAxis = new StickDefinition.StickAxisData
            {
                min = -30000,
                max = 30000,
                mid = 0,
                hard_max = 32767,
                hard_min = -32767,
            };
            //StickDefinition lsDefintion = new StickDefinition(STICK_MIN, STICK_MAX, STICK_NEUTRAL, StickActionCodes.LS);
            rsDefintion = new StickDefinition(lxAxis, lyAxis, StickActionCodes.RS);

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
                elapsedReference: device.BaseElapsedReference, mouseScale: 0.012 * 1.1, mouseOffset: 0.4,
                trackballScale: 0.000023);
            leftPadDefiniton.throttleRelMouse = true;
            leftPadDefiniton.throttleRelMousePower = TRACKPAD_MOUSE_POWER;
            leftPadDefiniton.throttleRelMouseZone = TRACKPAD_MOUSE_DISPLACEMENT;

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
                elapsedReference: device.BaseElapsedReference, mouseScale: 0.012 * 1.1, mouseOffset: 0.4,
                trackballScale: 0.000023);
            rightPadDefinition.throttleRelMouse = true;
            rightPadDefinition.throttleRelMousePower = TRACKPAD_MOUSE_POWER;
            rightPadDefinition.throttleRelMouseZone = TRACKPAD_MOUSE_DISPLACEMENT;

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

            knownStickDefinitions.Add("LS", lsDefintion);
            knownStickDefinitions.Add("RS", rsDefintion);
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
                new ActionTriggerItem("L1", JoypadActionCodes.BtnLShoulder),
                new ActionTriggerItem("R1", JoypadActionCodes.BtnRShoulder),
                new ActionTriggerItem("L2", JoypadActionCodes.AxisLTrigger),
                new ActionTriggerItem("R2", JoypadActionCodes.AxisRTrigger),
                
                new ActionTriggerItem("L3", JoypadActionCodes.BtnThumbL),
                new ActionTriggerItem("R3", JoypadActionCodes.BtnThumbR),

                new ActionTriggerItem("L4", JoypadActionCodes.BtnLGrip),
                new ActionTriggerItem("R4", JoypadActionCodes.BtnRGrip),
                new ActionTriggerItem("L5", JoypadActionCodes.BtnLGrip2),
                new ActionTriggerItem("R5", JoypadActionCodes.BtnRGrip2),

                new ActionTriggerItem("Left Touchpad Touch", JoypadActionCodes.LPadTouch),
                new ActionTriggerItem("Left Touchpad Touch", JoypadActionCodes.LPadTouch),
                new ActionTriggerItem("Right Touchpad Touch", JoypadActionCodes.RPadTouch),
                new ActionTriggerItem("Right Touchpad Click", JoypadActionCodes.RPadClick),

                new ActionTriggerItem("Select", JoypadActionCodes.BtnSelect),
                new ActionTriggerItem("Start", JoypadActionCodes.BtnStart),
                new ActionTriggerItem("Steam", JoypadActionCodes.BtnHome),
                new ActionTriggerItem("QAM", JoypadActionCodes.BtnMode),

                new ActionTriggerItem("LS Touch", JoypadActionCodes.BtnMode2),
                new ActionTriggerItem("RS Touch", JoypadActionCodes.BtnMode3),

                new ActionTriggerItem("Left GripSense", JoypadActionCodes.BtnLSideL),
                new ActionTriggerItem("Right GripSense", JoypadActionCodes.BtnRSideR),

                new ActionTriggerItem("DPad Up", JoypadActionCodes.BtnDPadUp),
                new ActionTriggerItem("DPad Down", JoypadActionCodes.BtnDPadDown),
                new ActionTriggerItem("DPad Left", JoypadActionCodes.BtnDPadLeft),
                new ActionTriggerItem("DPad Right", JoypadActionCodes.BtnDPadRight),
            };
        }

        public override void Start(ViGEmClient vigemTestClient,
            VirtualKBMBase eventInputHandler, VirtualKBMMapping eventInputMapping)
        {
            base.Start(vigemTestClient, eventInputHandler, eventInputMapping);

            reader.Report += Reader_Report;
            reader.StartUpdate();
        }

        private void Reader_Report(SteamControllerTritonReader sender, SteamControllerTritonDevice device)
        {
            while (pauseMapper)
            {
                Thread.SpinWait(500);
            }

            mapperActionActive = true;

            ref SteamControllerState current = ref device.CurrentStateRef;
            ref SteamControllerState previous = ref device.PreviousStateRef;

            // Copy state struct data for later mapper manipulation. Leave
            // device state instance alone
            currentMapperState = device.CurrentState;

            // Apply rotation for Left Touchpad
            int leftRotation = device.NativeDeviceOptions.LeftTouchpadRotation;
            if (leftRotation != 0)
            {
                currentMapperState.LeftPad.Rotate(leftRotation * Math.PI / 180.0);
            }

            // Apply rotation for Right Touchpad
            int rightRotation = device.NativeDeviceOptions.RightTouchpadRotation;
            if (rightRotation != 0)
            {
                currentMapperState.RightPad.Rotate(rightRotation * Math.PI / 180.0);
            }

            //currentMapperState.LeftPad.Rotate(-18.0 * Math.PI / 180.0);
            //currentMapperState.RightPad.Rotate(8.0 * Math.PI / 180.0);

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
                    int LX = Math.Clamp(currentMapperState.LX, STICK_MIN, STICK_MAX);
                    int LY = Math.Clamp(currentMapperState.LY, STICK_MIN, STICK_MAX);
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
                    int RX = Math.Clamp(currentMapperState.RX, STICK_MIN, STICK_MAX);
                    int RY = Math.Clamp(currentMapperState.RY, STICK_MIN, STICK_MAX);
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

                tempBtnAct = currentLayer.buttonActionDict["Select"];
                if (currentMapperState.Select || currentMapperState.Select != previousMapperState.Select)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Select);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Start"];
                if (currentMapperState.Start || currentMapperState.Start != previousMapperState.Start)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Start);
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


                tempBtnAct = currentLayer.buttonActionDict["L5"];
                if (currentMapperState.L5 || currentMapperState.L5 != previousMapperState.L5)
                {
                    tempBtnAct.Prepare(this, currentMapperState.L5);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["R5"];
                if (currentMapperState.R5 || currentMapperState.R5 != previousMapperState.R5)
                {
                    tempBtnAct.Prepare(this, currentMapperState.R5);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);


                tempBtnAct = currentLayer.buttonActionDict["LeftPadClick"];
                if (currentMapperState.LeftPad.Click || currentMapperState.LeftPad.Click != previousMapperState.LeftPad.Click)
                {
                    tempBtnAct.Prepare(this, currentMapperState.LeftPad.Click);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["RightPadClick"];
                if (currentMapperState.RightPad.Click || currentMapperState.RightPad.Click != previousMapperState.RightPad.Click)
                {
                    tempBtnAct.Prepare(this, currentMapperState.RightPad.Click);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["LeftPadTouch"];
                if (currentMapperState.LeftPad.Touch || currentMapperState.LeftPad.Touch != previousMapperState.LeftPad.Touch)
                {
                    tempBtnAct.Prepare(this, currentMapperState.LeftPad.Touch);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["RightPadTouch"];
                if (currentMapperState.RightPad.Touch || currentMapperState.RightPad.Touch != previousMapperState.RightPad.Touch)
                {
                    tempBtnAct.Prepare(this, currentMapperState.RightPad.Touch);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Steam"];
                if (currentMapperState.Steam || currentMapperState.Steam != previousMapperState.Steam)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Steam);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["QAM"];
                if (currentMapperState.QAM || currentMapperState.QAM != previousMapperState.QAM)
                {
                    tempBtnAct.Prepare(this, currentMapperState.QAM);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["LSTouch"];
                if (currentMapperState.LSTouch || currentMapperState.LSTouch != previousMapperState.LSTouch)
                {
                    tempBtnAct.Prepare(this, currentMapperState.LSTouch);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["RSTouch"];
                if (currentMapperState.RSTouch || currentMapperState.RSTouch != previousMapperState.RSTouch)
                {
                    tempBtnAct.Prepare(this, currentMapperState.RSTouch);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["LeftGripSense"];
                if (currentMapperState.LeftGripSenseTouch || currentMapperState.LeftGripSenseTouch != previousMapperState.LeftGripSenseTouch)
                {
                    tempBtnAct.Prepare(this, currentMapperState.LeftGripSenseTouch);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["RightGripSense"];
                if (currentMapperState.RightGripSenseTouch || currentMapperState.RightGripSenseTouch != previousMapperState.RightGripSenseTouch)
                {
                    tempBtnAct.Prepare(this, currentMapperState.RightGripSenseTouch);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                TouchpadMapAction tempTouchAction = currentLayer.touchpadActionDict["LeftTouchpad"];
                //if (currentMapperState.LeftPad.Touch || currentMapperState.LeftPad.Touch != previousMapperState.LeftPad.Touch)
                {
                    //Trace.WriteLine($"{currentMapperState.LeftPad.X} {currentMapperState.LeftPad.Y}");
                    TouchEventFrame eventFrame = new TouchEventFrame
                    {
                        X = Math.Clamp(currentMapperState.LeftPad.X, (short)-32768, (short)32767),
                        Y = Math.Clamp(currentMapperState.LeftPad.Y, (short)-32768, (short)32767),
                        Touch = currentMapperState.LeftPad.Touch,
                        Click = currentMapperState.LeftPad.Click,
                        numTouches = 1,
                        timeElapsed = currentMapperState.timeElapsed,
                    };

                    tempTouchAction.Prepare(this, ref eventFrame);
                    if (tempTouchAction.active) tempTouchAction.Event(this);

                    previousTouchFrameLeftPad = eventFrame;
                }

                {
                    tempTouchAction = currentLayer.touchpadActionDict["RightTouchpad"];
                    //if (currentMapperState.RightPad.Touch || currentMapperState.RightPad.Touch != previousMapperState.RightPad.Touch)
                    TouchEventFrame eventFrame = new TouchEventFrame
                    {
                        X = Math.Clamp(currentMapperState.RightPad.X, (short)-32768, (short)32767),
                        Y = Math.Clamp(currentMapperState.RightPad.Y, (short)-32768, (short)32767),
                        Touch = currentMapperState.RightPad.Touch,
                        Click = currentMapperState.RightPad.Click,
                        numTouches = 1,
                        timeElapsed = currentMapperState.timeElapsed,
                    };

                    //Trace.WriteLine($"RY: {eventFrame.Y}");
                    tempTouchAction.Prepare(this, ref eventFrame);
                    if (tempTouchAction.active) tempTouchAction.Event(this);

                    previousTouchFrameRightPad = eventFrame;
                }

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
                        elapsedReference = 125.0,
                        //elapsedReference = device.BaseElapsedReference,
                    };

                    gyroAct.Prepare(this, ref mouseFrame);
                    if (gyroAct.active)
                    {
                        gyroAct.Event(this);
                    }
                }

                gamepadSync = intermediateState.Dirty;

                ProcessSyncEvents();

                ProcessActionSetLayerChecks();

                // Prefer haptics event over rumble
                if (hapticsEvent)
                {
                    reader.WriteHapticsReport();
                    hapticsEvent = false;

                    bool rumbleActive = device.currentLeftAmpRatio != 0.0 || device.currentRightAmpRatio != 0.0;
                    if (rumbleActive)
                    {
                        device.ResetRumbleData();
                    }

                    if (standBySw.IsRunning)
                    {
                        standBySw.Reset();
                    }
                }
                else if (device.rumbleDirty)
                {
                    reader.WriteRumbleReport();
                    device.rumbleDirty = false;

                    bool rumbleActive = device.currentLeftAmpRatio != 0.0 ||
                        device.currentRightAmpRatio != 0.0;
                    if (rumbleActive)
                    {
                        standBySw.Restart();
                    }
                    else
                    {
                        standBySw.Reset();
                    }
                }
                else if (!device.rumbleDirty)
                {
                    bool rumbleActive = device.currentLeftAmpRatio != 0.0 ||
                        device.currentRightAmpRatio != 0.0;
                    if (standBySw.ElapsedMilliseconds >= 3000L && rumbleActive)
                    {
                        // Write new rumble report before currently running rumble ends
                        reader.WriteRumbleReport();
                        standBySw.Restart();
                    }
                    else
                    {
                        //Trace.WriteLine("FAIL NOW");
                    }

                    if (!rumbleActive && standBySw.IsRunning)
                    {
                        standBySw.Reset();
                    }
                }

                hapticsEvent = false;
                device.rumbleDirty = false;
                pendingHapticsLeftAmpRatio = pendingHapticsRightAmpRatio = 0.0;
                pendingRumbleLeftAmpRatio = pendingRumbleRightAmpRatio = 0.0;

                // Make copy of state data as the previous state
                previousMapperState = currentMapperState;

                mapperActionActive = false;

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
                    device.rumbleDirty = true;
                    // Wait until next gamepad poll finished before pushing rumble state
                    //reader.WriteRumbleReport();
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
                    result = currentMapperState.L1;
                    break;
                case JoypadActionCodes.BtnRShoulder:
                    result = currentMapperState.R1;
                    break;
                case JoypadActionCodes.BtnSelect:
                    result = currentMapperState.Select;
                    break;
                case JoypadActionCodes.BtnStart:
                    result = currentMapperState.Start;
                    break;
                case JoypadActionCodes.BtnHome:
                    result = currentMapperState.Steam;
                    break;
                case JoypadActionCodes.BtnMode:
                    result = currentMapperState.QAM;
                    break;
                case JoypadActionCodes.AxisLTrigger:
                    result = currentMapperState.L2 > 0;
                    break;
                case JoypadActionCodes.AxisRTrigger:
                    result = currentMapperState.R2 > 0;
                    break;
                case JoypadActionCodes.BtnThumbL:
                    result = currentMapperState.L3;
                    break;
                case JoypadActionCodes.BtnThumbR:
                    result = currentMapperState.R3;
                    break;
                case JoypadActionCodes.LPadTouch:
                    result = currentMapperState.LeftPad.Touch;
                    break;
                case JoypadActionCodes.RPadTouch:
                    result = currentMapperState.RightPad.Touch;
                    break;
                case JoypadActionCodes.LPadClick:
                    result = currentMapperState.LeftPad.Click;
                    break;
                case JoypadActionCodes.RPadClick:
                    result = currentMapperState.RightPad.Click;
                    break;
                case JoypadActionCodes.BtnLGrip:
                    result = currentMapperState.L4;
                    break;
                case JoypadActionCodes.BtnRGrip:
                    result = currentMapperState.R4;
                    break;
                case JoypadActionCodes.BtnLGrip2:
                    result = currentMapperState.L5;
                    break;
                case JoypadActionCodes.BtnRGrip2:
                    result = currentMapperState.R5;
                    break;
                case JoypadActionCodes.BtnMode2:
                    result = currentMapperState.LSTouch;
                    break;
                case JoypadActionCodes.BtnMode3:
                    result = currentMapperState.RSTouch;
                    break;
                case JoypadActionCodes.BtnLSideL:
                    result = currentMapperState.LeftGripSenseTouch;
                    break;
                case JoypadActionCodes.BtnRSideR:
                    result = currentMapperState.RightGripSenseTouch;
                    break;
                case JoypadActionCodes.BtnDPadUp:
                    result = currentMapperState.DPadUp;
                    break;
                case JoypadActionCodes.BtnDPadDown:
                    result = currentMapperState.DPadDown;
                    break;
                case JoypadActionCodes.BtnDPadLeft:
                    result = currentMapperState.DPadLeft;
                    break;
                case JoypadActionCodes.BtnDPadRight:
                    result = currentMapperState.DPadRight;
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
                        result = currentMapperState.L1;
                        break;
                    case JoypadActionCodes.BtnRShoulder:
                        result = currentMapperState.R1;
                        break;
                    case JoypadActionCodes.BtnSelect:
                        result = currentMapperState.Select;
                        break;
                    case JoypadActionCodes.BtnStart:
                        result = currentMapperState.Start;
                        break;
                    case JoypadActionCodes.BtnHome:
                        result = currentMapperState.Steam;
                        break;
                    case JoypadActionCodes.BtnMode:
                        result = currentMapperState.QAM;
                        break;
                    case JoypadActionCodes.AxisLTrigger:
                        result = currentMapperState.L2 > 0;
                        break;
                    case JoypadActionCodes.AxisRTrigger:
                        result = currentMapperState.R2 > 0;
                        break;
                    case JoypadActionCodes.BtnThumbL:
                        result = currentMapperState.L3;
                        break;
                    case JoypadActionCodes.BtnThumbR:
                        result = currentMapperState.R3;
                        break;
                    case JoypadActionCodes.LPadTouch:
                        result = currentMapperState.LeftPad.Touch;
                        break;
                    case JoypadActionCodes.RPadTouch:
                        result = currentMapperState.RightPad.Touch;
                        break;
                    case JoypadActionCodes.LPadClick:
                        result = currentMapperState.LeftPad.Click;
                        break;
                    case JoypadActionCodes.RPadClick:
                        result = currentMapperState.RightPad.Click;
                        break;
                    case JoypadActionCodes.BtnLGrip:
                        result = currentMapperState.L4;
                        break;
                    case JoypadActionCodes.BtnRGrip:
                        result = currentMapperState.R4;
                        break;
                    case JoypadActionCodes.BtnLGrip2:
                        result = currentMapperState.L5;
                        break;
                    case JoypadActionCodes.BtnRGrip2:
                        result = currentMapperState.R5;
                        break;
                    case JoypadActionCodes.BtnMode2:
                        result = currentMapperState.LSTouch;
                        break;
                    case JoypadActionCodes.BtnMode3:
                        result = currentMapperState.RSTouch;
                        break;
                    case JoypadActionCodes.BtnLSideL:
                        result = currentMapperState.LeftGripSenseTouch;
                        break;
                    case JoypadActionCodes.BtnRSideR:
                        result = currentMapperState.RightGripSenseTouch;
                        break;
                    case JoypadActionCodes.BtnDPadUp:
                        result = currentMapperState.DPadUp;
                        break;
                    case JoypadActionCodes.BtnDPadDown:
                        result = currentMapperState.DPadDown;
                        break;
                    case JoypadActionCodes.BtnDPadLeft:
                        result = currentMapperState.DPadLeft;
                        break;
                    case JoypadActionCodes.BtnDPadRight:
                        result = currentMapperState.DPadRight;
                        break;
                    default:
                        break;
                }

                if (andEval && !result)
                {
                    // All buttons in the list must be active
                    break;
                }
                else if (!andEval && result)
                {
                    // Only care if any button in the list is active
                    break;
                }
            }

            return result;
        }

        public override ref TouchEventFrame GetPreviousTouchEventFrame(TouchpadActionCodes padID)
        {
            switch (padID)
            {
                case TouchpadActionCodes.Touch1:
                    return ref previousTouchFrameLeftPad;
                case TouchpadActionCodes.Touch2:
                    return ref previousTouchFrameRightPad;
                default:
                    break;
            }

            return ref previousTouchFrameLeftPad;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckLeftHapticSide(double ratio, MapAction.HapticsSide side,
            bool checkDefault = true)
        {
            if ((checkDefault && side == MapAction.HapticsSide.Default) ||
                side == MapAction.HapticsSide.Left ||
                side == MapAction.HapticsSide.All)
            {
                if (pendingHapticsLeftAmpRatio < ratio)
                {
                    pendingHapticsLeftAmpRatio = ratio;

                    device.hapticInfo.leftActuatorAmpRatio = ratio;
                    device.hapticInfo.countLeft = 1;
                    device.hapticInfo.dirty = true;
                    hapticsEvent = true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckRightHapticSide(double ratio, MapAction.HapticsSide side,
            bool checkDefault = true)
        {
            if ((checkDefault && side == MapAction.HapticsSide.Default) ||
                side == MapAction.HapticsSide.Right ||
                side == MapAction.HapticsSide.All)
            {
                if (pendingHapticsRightAmpRatio < ratio)
                {
                    pendingHapticsRightAmpRatio = ratio;

                    device.hapticInfo.rightActuatorAmpRatio = ratio;
                    device.hapticInfo.countRight = 1;
                    device.hapticInfo.dirty = true;
                    hapticsEvent = true;
                }
            }
        }

        public override void SetFeedback(string mappingId, double ratio,
            MapAction.HapticsSide side = MapAction.HapticsSide.Default)
        {
            unchecked
            {
                switch (mappingId)
                {
                    case "LS":
                        CheckLeftHapticSide(ratio, side, true);
                        CheckRightHapticSide(ratio, side, false);
                        //hapticAmps[0] = 1.0;
                        //device.hapticsLeftAmpRatio = ratio;
                        //device.hapticsPeriodLeftRatio = 1.0;
                        //device.hapticsDurationLeft = 0.004;
                        break;
                    case "RS":
                        CheckLeftHapticSide(ratio, side, false);
                        CheckRightHapticSide(ratio, side, true);
                        //hapticAmps[0] = 1.0;
                        //device.hapticsLeftAmpRatio = ratio;
                        //device.hapticsPeriodLeftRatio = 1.0;
                        //device.hapticsDurationLeft = 0.004;
                        break;
                    case "LeftTouchpad":
                        CheckLeftHapticSide(ratio, side);
                        CheckRightHapticSide(ratio, side, false);
                        //device.hapticInfo.leftActuatorAmpRatio = ratio;
                        //device.hapticInfo.countLeft = 1;
                        break;
                    case "RightTouchpad":
                        CheckLeftHapticSide(ratio, side, false);
                        CheckRightHapticSide(ratio, side);
                        //device.hapticInfo.rightActuatorAmpRatio = ratio;
                        //device.hapticInfo.countRight = 1;
                        break;
                    case "A":
                    case "B":
                    case "X":
                    case "Y":
                    case "Select":
                    case "Start":
                    case "Steam":
                    case "QAM":
                    case "L3":
                    case "R3":
                    case "L4":
                    case "R4":
                    case "L5":
                    case "R5":
                    case "DPadUp":
                    case "DPadDown":
                    case "DPadLeft":
                    case "DPadRight":

                        CheckLeftHapticSide(ratio, side, true);
                        CheckRightHapticSide(ratio, side, true);
                        //device.hapticInfo.leftActuatorAmpRatio = ratio;
                        //device.hapticInfo.rightActuatorAmpRatio = ratio;
                        //device.hapticInfo.countLeft = 1;
                        //device.hapticInfo.countRight = 1;
                        break;
                    case "L1":
                    case "L2":
                        CheckLeftHapticSide(ratio, side, true);
                        CheckRightHapticSide(ratio, side, false);
                        //device.hapticInfo.leftActuatorAmpRatio = ratio;
                        //device.hapticInfo.countLeft = 1;
                        break;
                    case "R1":
                    case "R2":
                        CheckLeftHapticSide(ratio, side);
                        CheckRightHapticSide(ratio, side, true);
                        //device.hapticInfo.rightActuatorAmpRatio = ratio;
                        //device.hapticInfo.countRight = 1;
                        break;

                    default: break;
                }
            }
        }

        public override void SetRumble(double ratioLeft, double ratioRight)
        {
            bool changed = false;
            if (pendingRumbleLeftAmpRatio < ratioLeft)
            {
                device.currentLeftAmpRatio = ratioLeft;
                changed = true;
            }

            if (pendingRumbleRightAmpRatio < ratioRight)
            {
                device.currentRightAmpRatio = ratioRight;
                changed = true;
            }

            if (changed)
            {
                device.rumbleDirty = true;
            }

            //device.hapticInfo.leftActuatorAmpRatio = ratioLeft;
            //device.hapticInfo.leftPeriodRatio = ratioLeft;
            //device.hapticInfo.rightActuatorAmpRatio = ratioRight;
            //device.hapticInfo.rightPeriodRatio = ratioRight;
            //device.hapticInfo.
        }
    }
}
