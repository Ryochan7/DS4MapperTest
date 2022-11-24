using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickModifiers;

namespace DS4MapperTest.TouchpadActions
{
    public class TouchpadActionPad : TouchpadMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";

            public const string PAD_DIR_UP = "DirUp";
            public const string PAD_DIR_DOWN = "DirDown";
            public const string PAD_DIR_LEFT = "DirLeft";
            public const string PAD_DIR_RIGHT = "DirRight";

            public const string PAD_DIR_UPLEFT = "DirUpLeft";
            public const string PAD_DIR_UPRIGHT = "DirUpRight";
            public const string PAD_DIR_DOWNLEFT = "DirDownLeft";
            public const string PAD_DIR_DOWNRIGHT = "DirDownRight";

            public const string OUTER_RING_BUTTON = "OuterRingButton";

            public const string PAD_MODE = "PadMode";
            public const string DEAD_ZONE_TYPE = "DeadZoneType";
            public const string DEAD_ZONE = "DeadZone";
            public const string MAX_ZONE = "MaxZone";
            public const string ROTATION = "Rotation";
            public const string DIAGONAL_RANGE = "DiagonalRange";
            public const string REQUIRES_CLICK = "RequiresClick";
            public const string DELAY_ENABLED = "DelayEnabled";
            public const string DELAY_TIME = "DelayTime";

            public const string USE_OUTER_RING = "UseOuterRing";
            public const string OUTER_RING_DEAD_ZONE = "OuterRingDeadZone";
            public const string USE_AS_OUTER_RING = "UseAsOuterRing";
            public const string OUTER_RING_FULL_RANGE = "OuterRingFullRange";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.PAD_MODE,
            PropertyKeyStrings.DEAD_ZONE_TYPE,
            PropertyKeyStrings.DEAD_ZONE,
            PropertyKeyStrings.MAX_ZONE,
            PropertyKeyStrings.PAD_DIR_UP,
            PropertyKeyStrings.PAD_DIR_DOWN,
            PropertyKeyStrings.PAD_DIR_LEFT,
            PropertyKeyStrings.PAD_DIR_RIGHT,
            PropertyKeyStrings.PAD_DIR_UPLEFT,
            PropertyKeyStrings.PAD_DIR_UPRIGHT,
            PropertyKeyStrings.PAD_DIR_DOWNLEFT,
            PropertyKeyStrings.PAD_DIR_DOWNRIGHT,
            PropertyKeyStrings.OUTER_RING_BUTTON,
            PropertyKeyStrings.USE_OUTER_RING,
            PropertyKeyStrings.OUTER_RING_DEAD_ZONE,
            PropertyKeyStrings.USE_AS_OUTER_RING,
            PropertyKeyStrings.ROTATION,
            PropertyKeyStrings.DIAGONAL_RANGE,
            PropertyKeyStrings.REQUIRES_CLICK,
            PropertyKeyStrings.DELAY_ENABLED,
            PropertyKeyStrings.DELAY_TIME,
        };

        public enum DPadMode : uint
        {
            Standard,
            EightWay,
            FourWayCardinal,
            FourWayDiagonal,
        }

        public enum DpadDirections : uint
        {
            Centered,
            Up = 1,
            Right = 2,
            UpRight = 3,
            Down = 4,
            DownRight = 6,
            Left = 8,
            UpLeft = 9,
            DownLeft = 12,
        }

        public const string ACTION_TYPE_NAME = "TouchActionPadAction";

        // Allow individual AxisDirButton instances to be overridden
        private ButtonAction[] usedEventButtonsList = new ButtonAction[13];
        private ButtonAction[] usedFuncList;

        private DPadMode currentMode = DPadMode.Standard;
        private double xNorm = 0.0, yNorm = 0.0;
        private double prevXNorm = 0.0, prevYNorm = 0.0;
        private double fullXNorm = 0.0, fullYNorm = 0.0;
        private double ringDistance = 0.0;
        private DpadDirections currentDir;
        private DpadDirections previousDir;
        // Specify the input state of the button
        private bool inputStatus;
        private ButtonAction[] tmpBtnActions = new ButtonAction[2];
        private Dictionary<ButtonAction, DpadDirections> tmpActiveBtns = new Dictionary<ButtonAction, DpadDirections>();
        private int[] tmpBtnDirs = new int[2];
        private List<ButtonAction> removeBtnCandidates = new List<ButtonAction>();

        /* Possibly group values in a class */
        /// <summary>
        /// Virtual direction button that takes the vector magnitude as its value
        /// </summary>
        //private AxisDirButton ringButton = new AxisDirButton(new OutputActionData(OutputActionData.ActionType.Keyboard, KeyInterop.VirtualKeyFromKey(Key.Z), 0));
        private AxisDirButton ringButton = new AxisDirButton();
        private AxisDirButton usedRingButton = null;

        /// <summary>
        /// Used to determine outer ring mode or inner ring mode. Will change to using an Enum later
        /// </summary>
        private bool outerRing;
        /// <summary>
        /// Specify whether to interpret a ring binging at all
        /// </summary>
        private bool useRingButton;

        private const double DEFAULT_OUTER_RING_DEAD_ZONE = 0.7;
        /// <summary>
        /// Displacement threshold when a ring binding should execute
        /// </summary>
        private double outerRingDeadZone = 1.0;
        private OuterRingUseRange usedOuterRingRange;
        public AxisDirButton RingButton
        {
            get => ringButton;
            set => ringButton = value;
        }
        public bool UseAsOuterRing { get => outerRing; set => outerRing = value; }
        public bool UseRingButton { get => useRingButton; set => useRingButton = value; }
        public double OuterRingDeadZone { get => outerRingDeadZone; set => outerRingDeadZone = value; }
        public OuterRingUseRange UsedOuterRingRange { get => usedOuterRingRange; set => usedOuterRingRange = value; }

        public DPadMode CurrentMode { get => currentMode; set => currentMode = value; }
        public ButtonAction[] EventCodes4 { get => usedEventButtonsList; set => usedEventButtonsList = value; }

        private int rotation;
        public int Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        private const int DEFAULT_DIAGONAL_RANGE = 45;
        private int diagonalRange = DEFAULT_DIAGONAL_RANGE;
        public int DiagonalRange
        {
            get => diagonalRange;
            set => diagonalRange = value;
        }

        private StickDeadZone deadMod;
        public StickDeadZone DeadMod
        {
            get => deadMod;
        }

        private bool requiresClick;
        public bool RequiresClick
        {
            get => requiresClick;
            set => requiresClick = value;
        }

        private bool[] useParentDataDraft2 = new bool[13];
        public bool[] UseParentActionButton => useParentDataDraft2;
        private bool useParentRingButton;
        public bool UseParentRingButton
        {
            get => useParentRingButton;
            set => useParentRingButton = value;
        }

        private bool useParentDelay;

        private bool delayEnabled;
        public bool DelayEnabled
        {
            get => delayEnabled; set => delayEnabled = value;
        }

        private double delayTime;
        public double DelayTime
        {
            get => delayTime;
            set => delayTime = value;
        }

        private Stopwatch delayStopWatch = new Stopwatch();

        public TouchpadActionPad()
        {
            actionTypeName = ACTION_TYPE_NAME;

            FillDirectionalButtons();
            deadMod = new StickDeadZone(0.0, 1.0, 0.0);
        }

        public override void Prepare(Mapper mapper, ref TouchEventFrame touchFrame, bool alterState = true)
        {
            xNorm = 0.0; yNorm = 0.0;

            int axisXVal = touchFrame.X;
            int axisYVal = touchFrame.Y;
            if (rotation != 0)
            {
                TouchpadMethods.RotatedCoordinates(rotation, axisXVal, axisYVal,
                    touchpadDefinition, out axisXVal, out axisYVal);
            }

            int axisXMid = touchpadDefinition.xAxis.mid, axisYMid = touchpadDefinition.yAxis.mid;
            int axisXDir = axisXVal - axisXMid, axisYDir = axisYVal - axisYMid;
            bool xNegative = axisXDir < 0;
            bool yNegative = axisYDir < 0;
            int maxDirX = (!xNegative ? touchpadDefinition.xAxis.max : touchpadDefinition.xAxis.min) - axisXMid;
            int maxDirY = (!yNegative ? touchpadDefinition.yAxis.max : touchpadDefinition.yAxis.min) - axisYMid;

            bool deadInput = true;
            if (requiresClick && touchFrame.Click)
            {
                deadMod.CalcOutValues(axisXDir, axisYDir, maxDirX, maxDirY, out xNorm, out yNorm);
                deadInput = false;
            }
            else if (!requiresClick)
            {
                deadMod.CalcOutValues(axisXDir, axisYDir, maxDirX, maxDirY, out xNorm, out yNorm);
                deadInput = false;
            }
            else
            {
                xNorm = yNorm = 0.0;
            }

            fullXNorm = axisXDir / (double)maxDirX;
            fullYNorm = axisYDir / (double)maxDirY;

            if (usedOuterRingRange == OuterRingUseRange.OnlyActive)
            {
                ringDistance = Math.Sqrt((xNorm * xNorm) + (yNorm * yNorm));
            }
            else
            {
                ringDistance = Math.Sqrt((fullXNorm * fullXNorm) + (fullYNorm * fullYNorm));
            }

            //deadMod.CalcOutValues(axisXDir, axisYDir, maxDirX, maxDirY, out xNorm, out yNorm);
            //xNorm = axisXVal / (double)maxDirX;
            //yNorm = axisYVal / (double)maxDirY;

            //if (xNegative) xNorm *= -1.0;
            //if (yNegative) yNorm *= -1.0;

            //if (currentMode == DPadMode.Standard || currentMode == DPadMode.EightWay)
            {
                if (delayEnabled || deadInput)
                {
                    currentDir = DetermineDirection();
                }
                else
                {
                    DpadDirections tempDir = DetermineDirection();
                    if (currentDir != tempDir)
                    {
                        if (!delayStopWatch.IsRunning)
                        {
                            delayStopWatch.Restart();
                        }

                        if ((delayStopWatch.ElapsedMilliseconds * 0.001) >= delayTime)
                        {
                            currentDir = tempDir;
                            delayStopWatch.Reset();
                        }
                    }
                    else
                    {
                        if (delayStopWatch.IsRunning)
                        {
                            delayStopWatch.Reset();
                        }
                    }
                }
            }

            active = true;
            activeEvent = true;
            inputStatus = currentDir != DpadDirections.Centered;
            usedFuncList = usedEventButtonsList;
            usedRingButton = ringButton;
        }

        public override void Event(Mapper mapper)
        {
            int previousDirNum = (int)previousDir;
            //Console.WriteLine("DIRS: Previous: {0} Current: {1}", previousDir, currentDir);

            // Release old buttons
            if (previousDir != DpadDirections.Centered)
            {
                bool onlyCardinal = previousDirNum % 3 != 0;
                //Console.WriteLine("lkjdfslkjdfslkjdfs {0}", (previousDir & currentDir) != 0);
                if (onlyCardinal)
                {
                    //OutputActionData data = null;
                    ButtonAction data = null;
                    //double axisval = 0.0;
                    if (currentMode == DPadMode.Standard)
                    {
                        if ((previousDir & currentDir) == 0)
                        {
                            //Console.WriteLine("REMOVE CARDINAL");
                            //int code = eventCodes[previousDirNum];
                            data = usedFuncList[previousDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        if (previousDir != currentDir)
                        {
                            data = usedFuncList[previousDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        if ((previousDir == DpadDirections.Up) &&
                            (currentDir != DpadDirections.Up && currentDir != DpadDirections.UpRight))
                        {
                            // Map Up or UpRight to output Up direction
                            //data = eventCodes2[currentDirNum];
                            data = usedFuncList[previousDirNum];
                        }
                        else if ((previousDir == DpadDirections.Right) &&
                            (currentDir != DpadDirections.Right && currentDir != DpadDirections.DownRight))
                        {
                            // Map Right or DownRight to output Right direction
                            //data = eventCodes2[currentDirNum];
                            data = usedFuncList[previousDirNum];
                        }
                        else if ((previousDir == DpadDirections.Down) &&
                            (currentDir != DpadDirections.Down && currentDir != DpadDirections.DownLeft))
                        {
                            // Map Down or DownLeft to output Down direction
                            //data = eventCodes2[currentDirNum];
                            data = usedFuncList[previousDirNum];
                        }
                        else if ((previousDir == DpadDirections.Left) &&
                            (currentDir != DpadDirections.Left && currentDir != DpadDirections.UpLeft))
                        {
                            // Map Left or UpLeft to output Left direction
                            //data = eventCodes2[currentDirNum];
                            data = usedFuncList[previousDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.FourWayDiagonal)
                    {
                        if (previousDir == DpadDirections.UpRight && currentDir != DpadDirections.UpRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedFuncList[previousDirNum];
                        }
                        else if (previousDir == DpadDirections.DownRight && currentDir != DpadDirections.DownRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedFuncList[previousDirNum];
                        }
                        else if (previousDir == DpadDirections.DownLeft && currentDir != DpadDirections.DownLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedFuncList[previousDirNum];
                        }
                        else if (previousDir == DpadDirections.UpLeft && currentDir != DpadDirections.UpLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedFuncList[previousDirNum];
                        }
                    }

                    if (data != null)
                    {
                        //data.PrepareAnalog(mapper, 0.0);
                        data.Prepare(mapper, false);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(data, false);

                        if (!data.active)
                        {
                            tmpActiveBtns.Remove(data);
                        }
                    }
                }
                else if (!onlyCardinal)
                {
                    int tempDir;
                    //int* codes = stackalloc int[2];
                    //tmpCodes[0] = tmpCodes[1] = null;
                    tmpBtnActions[0] = tmpBtnActions[1] = null;
                    int i = 0;

                    if (currentMode == DPadMode.Standard)
                    {
                        if ((previousDir & DpadDirections.Up) != 0 && (currentDir & DpadDirections.Up) == 0)
                        {
                            tempDir = (int)DpadDirections.Up;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if ((previousDir & DpadDirections.Down) != 0 && (currentDir & DpadDirections.Down) == 0)
                        {
                            tempDir = (int)DpadDirections.Down;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }

                        if ((previousDir & DpadDirections.Left) != 0 && (currentDir & DpadDirections.Left) == 0)
                        {
                            tempDir = (int)DpadDirections.Left;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if ((previousDir & DpadDirections.Right) != 0 && (currentDir & DpadDirections.Right) == 0)
                        {
                            tempDir = (int)DpadDirections.Right;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        if (previousDir != currentDir)
                        {
                            //tmpCodes[i] = eventCodes[(int)previousDir]; i++;
                            tmpBtnActions[i] = usedFuncList[(int)previousDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        //if ((previousDir & DpadDirections.Up) != 0 && (currentDir & DpadDirections.Up) == 0)
                        if ((previousDir == DpadDirections.Up || previousDir == DpadDirections.UpRight) &&
                            (currentDir != DpadDirections.Up && currentDir != DpadDirections.UpRight))
                        {
                            // Map Up or UpRight to output Up direction
                            tempDir = (int)DpadDirections.Up;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        //else if ((previousDir & DpadDirections.Down) != 0 && (currentDir & DpadDirections.Down) == 0)
                        else if ((previousDir == DpadDirections.Right || previousDir == DpadDirections.DownRight) &&
                            (currentDir != DpadDirections.Right && currentDir != DpadDirections.DownRight))
                        {
                            // Map Right or DownRight to output Right direction
                            tempDir = (int)DpadDirections.Right;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        //if ((previousDir & DpadDirections.Left) != 0 && (currentDir & DpadDirections.Left) == 0)
                        else if ((previousDir == DpadDirections.Down || previousDir == DpadDirections.DownLeft) &&
                            (currentDir != DpadDirections.Down && currentDir != DpadDirections.DownLeft))
                        {
                            // Map Down or DownLeft to output Down direction
                            tempDir = (int)DpadDirections.Down;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        //else if ((previousDir & DpadDirections.Right) != 0 && (currentDir & DpadDirections.Right) == 0)
                        else if ((previousDir == DpadDirections.Left || previousDir == DpadDirections.UpLeft) &&
                            (currentDir != DpadDirections.Left && currentDir != DpadDirections.UpLeft))
                        {
                            tempDir = (int)DpadDirections.Left;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.FourWayDiagonal)
                    {
                        if (previousDir == DpadDirections.UpRight && currentDir != DpadDirections.UpRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            tempDir = (int)DpadDirections.UpRight;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if (previousDir == DpadDirections.DownRight && currentDir != DpadDirections.DownRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            tempDir = (int)DpadDirections.DownRight;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if (previousDir == DpadDirections.DownLeft && currentDir != DpadDirections.DownLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            tempDir = (int)DpadDirections.DownLeft;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if (previousDir == DpadDirections.UpLeft && currentDir != DpadDirections.UpLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            tempDir = (int)DpadDirections.UpLeft;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                    }

                    //if (tmpCodes[0] != null)
                    ButtonAction data = null;
                    if (tmpBtnActions[0] != null)
                    {
                        data = tmpBtnActions[0];
                        data.Prepare(mapper, false);
                        //data.PrepareAnalog(mapper, 0.0);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(tmpCodes[0], false);

                        if (!data.active)
                        {
                            tmpActiveBtns.Remove(data);
                        }
                    }

                    if (tmpBtnActions[1] != null)
                    {
                        data = tmpBtnActions[1];
                        //data.PrepareAnalog(mapper, 0.0);
                        data.Prepare(mapper, false);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(tmpCodes[1], false);

                        if (!data.active)
                        {
                            tmpActiveBtns.Remove(data);
                        }
                    }
                }
            }


            if (useRingButton && usedRingButton != null)
            //if (checkRingButton)
            {
                //Trace.WriteLine("IN CHECK");
                //double dist = Math.Sqrt((xNorm * xNorm) + (yNorm * yNorm));
                bool activeMod = outerRing ? (ringDistance > outerRingDeadZone ? true : false) :
                    (ringDistance > 0.0 && ringDistance <= outerRingDeadZone ? true : false);
                double tempRingDistance = activeMod ? ringDistance : 0.0;
                double tempRingUnit = activeMod ? 1.0 : 0.0;

                //ringButton.PrepareAnalog(mapper, dist);
                //if (ringButton.active)
                //{
                //    ringButton.Event(mapper);
                //}

                // Treat as boolean button for now
                usedRingButton.PrepareAnalog(mapper, tempRingDistance, tempRingUnit);
                //usedRingButton.PrepareAnalog(mapper, dist);
                if (usedRingButton.active)
                {
                    usedRingButton.Event(mapper);
                }
            }

            // Activate new buttons
            int currentDirNum = (int)currentDir;
            if (currentDir != DpadDirections.Centered)
            {
                bool onlyCardinal = currentDirNum % 3 != 0;
                bool changedDir = previousDir != currentDir;
                if (onlyCardinal)
                {
                    //Console.WriteLine("ACTIVATE CARDINAL");
                    //int code = eventCodes[currentDirNum];
                    //OutputActionData data = null;
                    ButtonAction data = null;
                    if (currentMode == DPadMode.Standard)
                    {
                        //if ((previousDir & currentDir) == 0)
                        //{
                        data = usedFuncList[currentDirNum];
                        //}
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        //if (currentDir != previousDir)
                        //{
                        data = usedFuncList[currentDirNum];
                        //}
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        //if ((currentDirNum & btnAdd) != 0)
                        if (currentDir == DpadDirections.Up)// &&
                                                            //(previousDir != DpadDirections.Up && previousDir != DpadDirections.UpRight))
                        {
                            data = usedFuncList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.Right)// &&
                                                                    //(previousDir != DpadDirections.Right && previousDir != DpadDirections.DownRight))
                        {
                            data = usedFuncList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.Down)// &&
                                                                   //(previousDir != DpadDirections.Down && previousDir != DpadDirections.DownLeft))
                        {
                            data = usedFuncList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.Left)// &&
                                                                   //(previousDir != DpadDirections.Left && previousDir != DpadDirections.UpLeft))
                        {
                            data = usedFuncList[currentDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.FourWayDiagonal)
                    {
                        if (currentDir == DpadDirections.UpRight)
                        {
                            data = usedFuncList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.DownRight)
                        {
                            data = usedFuncList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.DownLeft)
                        {
                            data = usedFuncList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.UpLeft)
                        {
                            data = usedFuncList[currentDirNum];
                        }
                    }

                    if (data != null)
                    {
                        //data.PrepareAnalog(mapper, ButtonAxisValue(data));
                        data.Prepare(mapper, true);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(data, true);

                        if (changedDir && !tmpActiveBtns.ContainsKey(data))
                        {
                            tmpActiveBtns.Add(data, currentDir);
                        }
                    }
                }
                else if (!onlyCardinal)
                {
                    int tempDir;
                    //int* codes = stackalloc int[2];
                    tmpBtnActions[0] = tmpBtnActions[1] = null;
                    int i = 0;
                    ButtonAction data = null;

                    if (currentMode == DPadMode.Standard)
                    {
                        if ((currentDir & DpadDirections.Up) != 0)// && (previousDir & DpadDirections.Up) == 0)
                        {
                            tempDir = (int)DpadDirections.Up;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if ((currentDir & DpadDirections.Down) != 0)// && (previousDir & DpadDirections.Down) == 0)
                        {
                            tempDir = (int)DpadDirections.Down;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }

                        if ((currentDir & DpadDirections.Left) != 0)// && (previousDir & DpadDirections.Left) == 0)
                        {
                            tempDir = (int)DpadDirections.Left;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if ((currentDir & DpadDirections.Right) != 0)// && (previousDir & DpadDirections.Right) == 0)
                        {
                            tempDir = (int)DpadDirections.Right;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        //if (currentDir != previousDir)
                        //{
                        //tmpCodes[i] = eventCodes[(int)currentDir]; i++;
                        tmpBtnDirs[i] = (int)currentDir;
                        tmpBtnActions[i] = usedFuncList[(int)currentDir]; i++;
                        //}
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        if (currentDir == DpadDirections.Up || currentDir == DpadDirections.UpRight)
                        {
                            // Map Up or UpRight to output Up direction
                            tempDir = (int)DpadDirections.Up;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        //else if ((currentDir & DpadDirections.Down) != 0)// && (previousDir & DpadDirections.Down) == 0)
                        else if (currentDir == DpadDirections.Right || currentDir == DpadDirections.DownRight)
                        {
                            // Map Right or DownRight to output Right direction
                            tempDir = (int)DpadDirections.Right;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if (currentDir == DpadDirections.Down || currentDir == DpadDirections.DownLeft)
                        {
                            // Map Down or DownLeft to output Down direction
                            tempDir = (int)DpadDirections.Down;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if (currentDir == DpadDirections.Left || currentDir == DpadDirections.UpLeft)
                        {
                            // Map Left or UpLeft to output Left direction
                            tempDir = (int)DpadDirections.Left;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.FourWayDiagonal)
                    {
                        if (currentDir == DpadDirections.UpRight || currentDir == DpadDirections.UpRight)
                        {
                            tempDir = (int)DpadDirections.UpRight;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if (currentDir == DpadDirections.DownRight)
                        {
                            tempDir = (int)DpadDirections.DownRight;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if (currentDir == DpadDirections.DownLeft)
                        {
                            tempDir = (int)DpadDirections.DownLeft;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                        else if (currentDir == DpadDirections.UpLeft)
                        {
                            tempDir = (int)DpadDirections.UpLeft;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                        }
                    }


                    //if (tmpCodes[0] != null)
                    if (tmpBtnActions[0] != null)
                    {
                        data = tmpBtnActions[0];
                        //data.PrepareAnalog(mapper, ButtonAxisValue(data));
                        data.Prepare(mapper, true);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(tmpCodes[0], true);

                        if (changedDir && !tmpActiveBtns.ContainsKey(data))
                        {
                            tmpActiveBtns.Add(data, (DpadDirections)tmpBtnDirs[0]);
                        }
                    }

                    if (tmpBtnActions[1] != null)
                    {
                        data = tmpBtnActions[1];
                        //data.PrepareAnalog(mapper, ButtonAxisValue(data));
                        data.Prepare(mapper, true);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(tmpCodes[1], true);

                        if (changedDir && !tmpActiveBtns.ContainsKey(data))
                        {
                            tmpActiveBtns.Add(data, (DpadDirections)tmpBtnDirs[1]);
                        }
                    }
                }
            }
            else if (tmpActiveBtns.Count > 0)
            {
                foreach (KeyValuePair<ButtonAction, DpadDirections> pair in tmpActiveBtns)
                {
                    ButtonAction data = pair.Key;
                    if (data != null)
                    {
                        //data.PrepareAnalog(mapper, ButtonAxisValue(data));
                        //data.PrepareAnalog(mapper, 0.0);
                        data.Prepare(mapper, false);
                        data.Event(mapper);

                        if (!data.active)
                        {
                            removeBtnCandidates.Add(data);
                        }
                    }
                }

                foreach (ButtonAction removeBtn in removeBtnCandidates)
                {
                    tmpActiveBtns.Remove(removeBtn);
                }

                removeBtnCandidates.Clear();
            }
            //else
            //{
            //    AxisDirButton data = null;
            //    if (tmpBtnActions[0] != null)
            //    {
            //        data = tmpBtnActions[0];
            //        data.PrepareAnalog(mapper, 0.0);
            //        data.Event(mapper);
            //        //mapper.RunEventFromButton(tmpCodes[0], false);
            //    }

            //    if (tmpBtnActions[1] != null)
            //    {
            //        data = tmpBtnActions[1];
            //        data.PrepareAnalog(mapper, 0.0);
            //        data.Event(mapper);
            //        //mapper.RunEventFromButton(tmpCodes[1], false);
            //    }
            //}

            prevXNorm = xNorm; prevYNorm = yNorm;
            previousDir = currentDir;
            //bool ringBtnActive = usedRingButton != null && usedRingButton.active;
            //active = currentDir != DpadDirections.Centered || ringBtnActive ||
            //    tmpActiveBtns.Count > 0;
            active = currentDir != DpadDirections.Centered ||
                tmpActiveBtns.Count > 0;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            if (active || tmpActiveBtns.Count > 0)
            {
                if (useRingButton && usedRingButton != null)
                {
                    usedRingButton.Release(mapper, resetState, ignoreReleaseActions);
                }

                foreach (KeyValuePair<ButtonAction, DpadDirections> pair in tmpActiveBtns)
                {
                    ButtonAction data = pair.Key;
                    //DpadDirections dir = pair.Value;

                    if (data != null)
                    {
                        data.Release(mapper, resetState, ignoreReleaseActions);
                    }
                }

                currentDir = DpadDirections.Centered;
                previousDir = currentDir;
            }

            inputStatus = false;
            if (delayStopWatch.IsRunning)
            {
                delayStopWatch.Reset();
            }

            if (resetState)
            {
                stateData.Reset();
            }
        }


        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            //Release(mapper, resetState);
            //base.SoftRelease(mapper, checkAction, resetState);
            if (active || tmpActiveBtns.Count > 0)
            {
                TouchpadActionPad checkStickAction = checkAction as TouchpadActionPad;

                if (useRingButton && usedRingButton != null)
                {
                    if (!useParentRingButton)
                    {
                        usedRingButton.Release(mapper, resetState);
                    }
                    else if (checkStickAction.usedRingButton != usedRingButton)
                    {
                        usedRingButton.Release(mapper, resetState);
                    }
                }

                foreach (KeyValuePair<ButtonAction, DpadDirections> pair in tmpActiveBtns)
                {
                    ButtonAction data = pair.Key;
                    DpadDirections dir = pair.Value;

                    int dirNum = (int)dir;
                    if (data != null && !useParentDataDraft2[dirNum])
                    {
                        data.Release(mapper, resetState);
                    }
                    else if (data != null && checkStickAction != null &&
                        checkStickAction.usedEventButtonsList[dirNum] != data)
                    {
                        data.Release(mapper, resetState);
                    }
                }

                currentDir = DpadDirections.Centered;
                previousDir = currentDir;
            }

            if (!useParentDelay && delayStopWatch.IsRunning)
            {
                delayStopWatch.Reset();
            }

            inputStatus = false;

            if (resetState)
            {
                stateData.Reset();
            }
        }

        private void FillDirectionalButtons()
        {
            for (int i = 0; i < usedEventButtonsList.Length; i++)
            {
                //if (tempDir != AxisDirButton.AxisDirection.None)
                {
                    ButtonAction tempBtn = new ButtonAction();
                    usedEventButtonsList[i] = tempBtn;
                }
            }
        }

        private DpadDirections DetermineDirection()
        {
            DpadDirections resultDir = DpadDirections.Centered;
            if (xNorm == 0.0 && yNorm == 0.0)
            {
                resultDir = DpadDirections.Centered;
            }
            else
            {
                if (currentMode != DPadMode.FourWayDiagonal)
                {
                    /*const double CARDINAL_RANGE = 45.0;
                const double DIAGONAL_RANGE = 45.0;
                //const double CARDINAL_HALF_RANGE = CARDINAL_RANGE / 2.0;
                const double CARDINAL_HALF_RANGE = 22.5;

                const double upLeftEnd = 360 - CARDINAL_HALF_RANGE;
                const double upRightBegin = CARDINAL_HALF_RANGE;
                const double rightBegin = upRightBegin + DIAGONAL_RANGE;
                const double downRightBegin = rightBegin + CARDINAL_RANGE;
                const double downBegin = downRightBegin + DIAGONAL_RANGE;
                const double downLeftBegin = downBegin + CARDINAL_RANGE;
                const double leftBegin = downLeftBegin + DIAGONAL_RANGE;
                const double upLeftBegin = leftBegin + CARDINAL_RANGE;
                */

                    double currentDiagonalRange = diagonalRange;
                    double currentCardinalRange = 90.0 - currentDiagonalRange;
                    //const double CARDINAL_HALF_RANGE = currentCardinalRange / 2.0;
                    double CARDINAL_HALF_RANGE = currentCardinalRange / 2.0;

                    double upLeftEnd = 360 - CARDINAL_HALF_RANGE;
                    double upRightBegin = CARDINAL_HALF_RANGE;
                    double rightBegin = upRightBegin + currentDiagonalRange;
                    double downRightBegin = rightBegin + currentCardinalRange;
                    double downBegin = downRightBegin + currentDiagonalRange;
                    double downLeftBegin = downBegin + currentCardinalRange;
                    double leftBegin = downLeftBegin + currentDiagonalRange;
                    double upLeftBegin = leftBegin + currentCardinalRange;

                    double angleRad = Math.Atan2(xNorm, yNorm);
                    double angle = (angleRad >= 0 ? angleRad : (2 * Math.PI + angleRad)) * 180 / Math.PI;

                    //Trace.WriteLine(angle);
                    if (angle == 0.0)
                    {
                        resultDir = DpadDirections.Up;
                    }
                    else if (angle > upLeftEnd || angle < upRightBegin)
                    {
                        resultDir = DpadDirections.Up;
                    }
                    else if (angle >= upRightBegin && angle < rightBegin)
                    {
                        resultDir = DpadDirections.UpRight;
                    }
                    else if (angle >= rightBegin && angle < downRightBegin)
                    {
                        resultDir = DpadDirections.Right;
                    }
                    else if (angle >= downRightBegin && angle < downBegin)
                    {
                        resultDir = DpadDirections.DownRight;
                    }
                    else if (angle >= downBegin && angle < downLeftBegin)
                    {
                        resultDir = DpadDirections.Down;
                    }
                    else if (angle >= downLeftBegin && angle < leftBegin)
                    {
                        resultDir = DpadDirections.DownLeft;
                    }
                    else if (angle >= leftBegin && angle < upLeftBegin)
                    {
                        resultDir = DpadDirections.Left;
                    }
                    else if (angle >= upLeftBegin && angle <= upLeftEnd)
                    {
                        resultDir = DpadDirections.UpLeft;
                    }
                }
                else if (currentMode == DPadMode.FourWayDiagonal)
                {
                    double currentDiagonalRange = 90;
                    double currentCardinalRange = 90.0 - currentDiagonalRange;
                    //const double CARDINAL_HALF_RANGE = currentCardinalRange / 2.0;
                    double CARDINAL_HALF_RANGE = currentCardinalRange / 2.0;

                    double upRightBegin = 0;
                    double downRightBegin = upRightBegin + currentDiagonalRange;
                    double downLeftBegin = downRightBegin + currentDiagonalRange;
                    double upLeftBegin = downLeftBegin + currentDiagonalRange;

                    double angleRad = Math.Atan2(xNorm, yNorm);
                    double angle = (angleRad >= 0 ? angleRad : (2 * Math.PI + angleRad)) * 180 / Math.PI;

                    if (angle == 0.0)
                    {
                        resultDir = DpadDirections.Up;
                    }
                    else if (angle > upRightBegin && angle < downRightBegin)
                    {
                        resultDir = DpadDirections.UpRight;
                    }
                    else if (angle >= downRightBegin && angle < downLeftBegin)
                    {
                        resultDir = DpadDirections.DownRight;
                    }
                    else if (angle >= downLeftBegin && angle < upLeftBegin)
                    {
                        resultDir = DpadDirections.DownLeft;
                    }
                    else if (angle >= upLeftBegin && angle < 360)
                    {
                        resultDir = DpadDirections.UpLeft;
                    }
                }
            }

            return resultDir;

            //Trace.WriteLine(currentDir);
        }

        public override void SoftCopyFromParent(TouchpadMapAction parentAction)
        {
            if (parentAction is TouchpadActionPad tempPadAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                mappingId = tempPadAction.mappingId;

                this.touchpadDefinition = new TouchpadDefinition(tempPadAction.touchpadDefinition);
                mappingId = tempPadAction.mappingId;

                tempPadAction.NotifyPropertyChanged += TempPadAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch (parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempPadAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE:
                            deadMod.DeadZone = tempPadAction.deadMod.DeadZone;
                            break;
                        case PropertyKeyStrings.MAX_ZONE:
                            deadMod.MaxZone = tempPadAction.deadMod.MaxZone;
                            break;
                        case PropertyKeyStrings.PAD_MODE:
                            currentMode = tempPadAction.currentMode;
                            break;
                        case PropertyKeyStrings.DIAGONAL_RANGE:
                            diagonalRange = tempPadAction.diagonalRange;
                            break;
                        case PropertyKeyStrings.REQUIRES_CLICK:
                            requiresClick = tempPadAction.requiresClick;
                            break;
                        case PropertyKeyStrings.ROTATION:
                            rotation = tempPadAction.rotation;
                            break;
                        case PropertyKeyStrings.PAD_DIR_UP:
                            {
                                int tempDir = (int)DpadDirections.Up;
                                usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWN:
                            {
                                int tempDir = (int)DpadDirections.Down;
                                usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_LEFT:
                            {
                                int tempDir = (int)DpadDirections.Left;
                                usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_RIGHT:
                            {
                                int tempDir = (int)DpadDirections.Right;
                                usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_UPLEFT:
                            {
                                int tempDir = (int)DpadDirections.UpLeft;
                                usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_UPRIGHT:
                            {
                                int tempDir = (int)DpadDirections.UpRight;
                                usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWNLEFT:
                            {
                                int tempDir = (int)DpadDirections.DownLeft;
                                usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWNRIGHT:
                            {
                                int tempDir = (int)DpadDirections.DownRight;
                                usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.OUTER_RING_BUTTON:
                            ringButton = tempPadAction.ringButton;
                            useParentRingButton = true;
                            break;
                        case PropertyKeyStrings.USE_OUTER_RING:
                            useRingButton = tempPadAction.useRingButton;
                            break;
                        case PropertyKeyStrings.OUTER_RING_DEAD_ZONE:
                            outerRingDeadZone = tempPadAction.outerRingDeadZone;
                            break;
                        case PropertyKeyStrings.USE_AS_OUTER_RING:
                            outerRing = tempPadAction.outerRing;
                            break;
                        case PropertyKeyStrings.OUTER_RING_FULL_RANGE:
                            usedOuterRingRange = tempPadAction.usedOuterRingRange;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE_TYPE:
                            deadMod.DeadZoneType = tempPadAction.deadMod.DeadZoneType;
                            break;
                        case PropertyKeyStrings.DELAY_ENABLED:
                            delayEnabled = tempPadAction.delayEnabled;
                            useParentDelay = true;
                            break;
                        case PropertyKeyStrings.DELAY_TIME:
                            {
                                delayTime = tempPadAction.delayTime;
                                // Copy ref to parent action Stopwatch
                                delayStopWatch = tempPadAction.delayStopWatch;
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempPadAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
        {
            CascadePropertyChange(e.Mapper, e.PropertyName);
        }

        protected override void CascadePropertyChange(Mapper mapper, string propertyName)
        {
            if (changedProperties.Contains(propertyName))
            {
                // Property already overrridden in action. Leave
                return;
            }
            else if (parentAction == null)
            {
                // No parent action. Leave
                return;
            }

            TouchpadActionPad tempPadAction = parentAction as TouchpadActionPad;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempPadAction.name;
                    break;
                case PropertyKeyStrings.DEAD_ZONE:
                    deadMod.DeadZone = tempPadAction.deadMod.DeadZone;
                    break;
                case PropertyKeyStrings.MAX_ZONE:
                    deadMod.MaxZone = tempPadAction.deadMod.MaxZone;
                    break;
                case PropertyKeyStrings.PAD_MODE:
                    currentMode = tempPadAction.currentMode;
                    break;
                case PropertyKeyStrings.DIAGONAL_RANGE:
                    diagonalRange = tempPadAction.diagonalRange;
                    break;
                case PropertyKeyStrings.REQUIRES_CLICK:
                    requiresClick = tempPadAction.requiresClick;
                    break;
                case PropertyKeyStrings.ROTATION:
                    rotation = tempPadAction.rotation;
                    break;
                case PropertyKeyStrings.PAD_DIR_UP:
                    {
                        int tempDir = (int)DpadDirections.Up;
                        usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_DOWN:
                    {
                        int tempDir = (int)DpadDirections.Down;
                        usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_LEFT:
                    {
                        int tempDir = (int)DpadDirections.Left;
                        usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_RIGHT:
                    {
                        int tempDir = (int)DpadDirections.Right;
                        usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_UPLEFT:
                    {
                        int tempDir = (int)DpadDirections.UpLeft;
                        usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_UPRIGHT:
                    {
                        int tempDir = (int)DpadDirections.UpRight;
                        usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_DOWNLEFT:
                    {
                        int tempDir = (int)DpadDirections.DownLeft;
                        usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_DOWNRIGHT:
                    {
                        int tempDir = (int)DpadDirections.DownRight;
                        usedEventButtonsList[tempDir] = tempPadAction.usedEventButtonsList[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.OUTER_RING_BUTTON:
                    ringButton = tempPadAction.ringButton;
                    useParentRingButton = true;
                    break;
                case PropertyKeyStrings.USE_OUTER_RING:
                    useRingButton = tempPadAction.useRingButton;
                    break;
                case PropertyKeyStrings.OUTER_RING_DEAD_ZONE:
                    outerRingDeadZone = tempPadAction.outerRingDeadZone;
                    break;
                case PropertyKeyStrings.USE_AS_OUTER_RING:
                    outerRing = tempPadAction.outerRing;
                    break;
                case PropertyKeyStrings.OUTER_RING_FULL_RANGE:
                    usedOuterRingRange = tempPadAction.usedOuterRingRange;
                    break;
                case PropertyKeyStrings.DEAD_ZONE_TYPE:
                    deadMod.DeadZoneType = tempPadAction.deadMod.DeadZoneType;
                    break;
                case PropertyKeyStrings.DELAY_ENABLED:
                    delayEnabled = tempPadAction.delayEnabled;
                    useParentDelay = true;
                    break;
                case PropertyKeyStrings.DELAY_TIME:
                    {
                        delayTime = tempPadAction.delayTime;
                        // Copy ref to parent action Stopwatch
                        delayStopWatch = tempPadAction.delayStopWatch;
                    }

                    break;
                default:
                    break;
            }
        }
    }
}
