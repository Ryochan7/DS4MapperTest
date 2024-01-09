using DS4MapperTest.ButtonActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;
using Nefarius.ViGEm.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.SteamControllerLibrary
{
    public class SteamControllerMapper : Mapper
    {
        private SteamControllerDevice device;
        private SteamControllerReader reader;

        private SteamControllerState currentMapperState;
        private SteamControllerState previousMapperState;

        private TouchEventFrame previousTouchFrameLeftPad;
        private TouchEventFrame previousTouchFrameRightPad;

        public override DeviceReaderBase BaseReader => reader;
        public override InputDeviceType DeviceType => InputDeviceType.SteamController;

        private StickDefinition lsDefintion;
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

        private double trackballAccel = 0.0;

        public SteamControllerMapper(SteamControllerDevice device, SteamControllerReader reader,
            AppGlobalData appGlobal)
        {
            this.device = device;
            this.reader = reader;
            this.appGlobal = appGlobal;

            deviceActionDefaults = new SteamControllerDeviceDefaults();

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
                elapsedReference: device.BaseElapsedReference, mouseScale: 0.012 * 1.1, mouseOffset: 0.4,
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

        public override void Start(ViGEmClient vigemTestClient, FakerInputHandler fakerInputHandler)
        {
            base.Start(vigemTestClient, fakerInputHandler);

            reader.Report += Reader_Report;
            reader.StartUpdate();
        }

        private void Reader_Report(SteamControllerReader sender, SteamControllerDevice device)
        {
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

                StickMapAction mapAction = currentLayer.stickActionDict["Stick"];
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

                TriggerMapAction trigMapAction = currentLayer.triggerActionDict["LT"];
                {
                    TriggerEventFrame eventFrame = new TriggerEventFrame
                    {
                        axisValue = currentMapperState.LT,
                        fullClick = currentMapperState.LTClick,
                    };
                    trigMapAction.Prepare(this, ref eventFrame);
                }
                if (trigMapAction.active) trigMapAction.Event(this);

                trigMapAction = currentLayer.triggerActionDict["RT"];
                {
                    TriggerEventFrame eventFrame = new TriggerEventFrame
                    {
                        axisValue = currentMapperState.RT,
                        fullClick = currentMapperState.RTClick,
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

                tempBtnAct = currentLayer.buttonActionDict["Back"];
                if (currentMapperState.Back || currentMapperState.Back != previousMapperState.Back)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Back);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["Start"];
                if (currentMapperState.Start || currentMapperState.Start != previousMapperState.Start)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Start);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["LShoulder"];
                if (currentMapperState.LB || currentMapperState.LB != previousMapperState.LB)
                {
                    tempBtnAct.Prepare(this, currentMapperState.LB);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["RShoulder"];
                if (currentMapperState.RB || currentMapperState.RB != previousMapperState.RB)
                {
                    tempBtnAct.Prepare(this, currentMapperState.RB);
                }

                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["LSClick"];
                if (currentMapperState.LSClick || currentMapperState.LSClick != previousMapperState.LSClick)
                {
                    tempBtnAct.Prepare(this, currentMapperState.LSClick);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["LeftGrip"];
                if (currentMapperState.LGrip || currentMapperState.LGrip != previousMapperState.LGrip)
                {
                    tempBtnAct.Prepare(this, currentMapperState.LGrip);
                }
                if (tempBtnAct.active) tempBtnAct.Event(this);

                tempBtnAct = currentLayer.buttonActionDict["RightGrip"];
                if (currentMapperState.RGrip || currentMapperState.RGrip != previousMapperState.RGrip)
                {
                    tempBtnAct.Prepare(this, currentMapperState.RGrip);
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

                tempBtnAct = currentLayer.buttonActionDict["Steam"];
                if (currentMapperState.Guide || currentMapperState.Guide != previousMapperState.Guide)
                {
                    tempBtnAct.Prepare(this, currentMapperState.Guide);
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
                case JoypadActionCodes.BtnStart:
                    result = currentMapperState.Start;
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
                case JoypadActionCodes.LTFullPull:
                    result = currentMapperState.LTClick;
                    break;
                case JoypadActionCodes.RTFullPull:
                    result = currentMapperState.RTClick;
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
                        result = currentMapperState.LB;
                        break;
                    case JoypadActionCodes.BtnRShoulder:
                        result = currentMapperState.RB;
                        break;
                    case JoypadActionCodes.BtnStart:
                        result = currentMapperState.Start;
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
                    case JoypadActionCodes.LTFullPull:
                        result = currentMapperState.LTClick;
                        break;
                    case JoypadActionCodes.RTFullPull:
                        result = currentMapperState.RTClick;
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
    }
}
