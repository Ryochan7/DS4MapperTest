using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.StickModifiers;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.ButtonActions;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Diagnostics;

namespace DS4MapperTest.StickActions
{
    public class StickPadAction : StickMapAction
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

            public const string USE_OUTER_RING = "UseOuterRing";
            public const string OUTER_RING_DEAD_ZONE = "OuterRingDeadZone";
            public const string USE_AS_OUTER_RING = "UseAsOuterRing";
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

        public const string ACTION_TYPE_NAME = "StickPadAction";

        // Cardinal direction codes. Use 0 for Centered and intermediate diagonals
        //private OutputActionData[] eventCodes = new OutputActionData[13];
        //private ActionFunc[] eventCodes3 = new ActionFunc[13];
        private AxisDirButton[] eventCodes4 = new AxisDirButton[13];
        private AxisDirButton[] usedFuncList;

        // Allow individual AxisDirButton instances to be overridden
        private AxisDirButton[] usedEventButtonsList = new AxisDirButton[13];

        private StickDeadZone deadMod;
        //private StickDefinition stickDefinition;
        private DPadMode currentMode = DPadMode.Standard;
        private double xNorm = 0.0, yNorm = 0.0;
        private double prevXNorm = 0.0, prevYNorm = 0.0;
        //public bool active;
        //public bool activeEvent;
        private DpadDirections currentDir;
        private DpadDirections previousDir;
        // Specify the input state of the button
        private bool inputStatus;
        //private OutputActionData[] tmpCodes = new OutputActionData[2];
        //private ActionFunc[] tmpFuncs = new ActionFunc[2];
        private AxisDirButton[] tmpBtnActions = new AxisDirButton[2];
        private Dictionary<AxisDirButton, DpadDirections> tmpActiveBtns = new Dictionary<AxisDirButton, DpadDirections>();
        private int[] tmpBtnDirs = new int[2];
        private List<AxisDirButton> removeBtnCandidates = new List<AxisDirButton>();

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
        public AxisDirButton RingButton
        {
            get => ringButton;
            set => ringButton = value;
        }
        public bool UseAsOuterRing { get => outerRing; set => outerRing = value; }
        public bool UseRingButton { get => useRingButton; set => useRingButton = value; }
        public double OuterRingDeadZone { get => outerRingDeadZone; set => outerRingDeadZone = value; }

        public StickDeadZone DeadMod { get => deadMod; }
        //public OutputActionData[] EventCodes { get => eventCodes; }
        //public ActionFunc[] EventCodes3 { get => eventCodes3; }
        public DPadMode CurrentMode { get => currentMode; set => currentMode = value; }
        public AxisDirButton[] EventCodes4 { get => eventCodes4; set => eventCodes4 = value; }

        // + = Clockwise. - = Counter-clockwise
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


        protected StickPadAction parentPadAction;
        protected bool useParentActions;
        protected bool[] useParentDataDraft2 = new bool[13];
        public bool[] UsingParentActionButton => useParentDataDraft2;
        private bool useParentRingButton;

        public StickPadAction()
        {
            actionTypeName = ACTION_TYPE_NAME;
            stickDefinition = new StickDefinition(new StickDefinition.StickAxisData(), new StickDefinition.StickAxisData(), StickActionCodes.Empty);
            deadMod = new StickDeadZone(0.30, 1.0, 0.0);
            //deadMod.CircleDead = true;
            deadMod.DeadZoneType = StickDeadZone.DeadZoneTypes.Radial;
            FillDirectionButtons();
        }

        public StickPadAction(StickDefinition stickDefinition)
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.stickDefinition = stickDefinition;
            deadMod = new StickDeadZone(0.30, 1.0, 0.0);
            //deadMod.CircleDead = true;
            deadMod.DeadZoneType = StickDeadZone.DeadZoneTypes.Radial;
            FillDirectionButtons();
        }

        public StickPadAction(StickPadAction parentAction)
        {
            actionTypeName = ACTION_TYPE_NAME;
            if (parentAction != null)
            {
                this.parentAction = parentAction;
                parentAction.hasLayeredAction = true;
                parentPadAction = parentAction;
                //this.stickDefinition = parentAction.stickDefinition;
                this.stickDefinition = new StickDefinition(parentAction.stickDefinition);
                this.deadMod = new StickDeadZone(parentAction.deadMod);
                mappingId = parentAction.mappingId;
                useParentActions = true;

                // Grab references to parentAction AxisDirButton instances
                int tempDir = (int)DpadDirections.Centered;
                foreach (AxisDirButton tempAction in parentAction.eventCodes4)
                {
                    usedEventButtonsList[tempDir] = tempAction;
                    eventCodes4[tempDir] = tempAction;
                    useParentDataDraft2[tempDir] = true;
                    tempDir++;
                }

                useParentRingButton = true;
                //usedFuncList = usedEventButtonsList;
            }
        }

        private void FillDirectionButtons()
        {
            AxisDirButton.AxisDirection[] axisDirs = new AxisDirButton.AxisDirection[13]
            {
                AxisDirButton.AxisDirection.None, AxisDirButton.AxisDirection.YNeg, AxisDirButton.AxisDirection.XPos,
                AxisDirButton.AxisDirection.XY, AxisDirButton.AxisDirection.YPos, AxisDirButton.AxisDirection.None,
                AxisDirButton.AxisDirection.XY, AxisDirButton.AxisDirection.None, AxisDirButton.AxisDirection.XNeg,
                AxisDirButton.AxisDirection.XY, AxisDirButton.AxisDirection.None, AxisDirButton.AxisDirection.None,
                AxisDirButton.AxisDirection.XY,
            };
            for (int i = 0; i < eventCodes4.Length; i++)
            {
                AxisDirButton.AxisDirection tempDir = axisDirs[i];
                //if (tempDir != AxisDirButton.AxisDirection.None)
                {
                    AxisDirButton tempBtn = new AxisDirButton();
                    tempBtn.Direction = axisDirs[i];
                    eventCodes4[i] = tempBtn;
                }
            }
        }

        public override void Prepare(Mapper mapper, int axisXVal, int axisYVal, bool alterState = true)
        {
            xNorm = 0.0; yNorm = 0.0;

            if (rotation != 0)
            {
                StickMethods.RotatedCoordinates(rotation, axisXVal, axisYVal,
                    stickDefinition, out axisXVal, out axisYVal);
            }

            int axisXMid = stickDefinition.xAxis.mid, axisYMid = stickDefinition.yAxis.mid;
            int axisXDir = axisXVal - axisXMid, axisYDir = axisYVal - axisYMid;
            bool xNegative = axisXDir < 0;
            bool yNegative = axisYDir < 0;
            int maxDirX = (!xNegative ? stickDefinition.xAxis.max : stickDefinition.xAxis.min) - axisXMid;
            int maxDirY = (!yNegative ? stickDefinition.yAxis.max : stickDefinition.yAxis.min) - axisYMid;
            bool inSafeZone;
            deadMod.CalcOutValues(axisXDir, axisYDir, maxDirX,
                    maxDirY, out xNorm, out yNorm);
            inSafeZone = deadMod.inSafeZone;

            if (inSafeZone)
            {
                //Trace.WriteLine($"IN SAFE ZONE {deadMod.DeadZone}");
                //Console.WriteLine("SHEIT: {0} ({1}) {2} ({3}", axisXVal, xNorm, axisYVal, yNorm);
                //if (currentMode == DPadMode.Standard || currentMode == DPadMode.EightWay)
                {
                    DetermineDirection();
                    //Console.WriteLine(currentDir.ToString());
                }
                //else
                //{

                //}

                if (useParentActions)
                {
                    //usedFuncList = parentPadAction.eventCodes4;
                    usedFuncList = eventCodes4;
                    usedRingButton = parentPadAction.ringButton;
                }
                else
                {
                    usedFuncList = eventCodes4;
                    usedRingButton = ringButton;
                }
            }
            else
            {
                // Console.WriteLine("CENTER THIS");
                currentDir = DpadDirections.Centered;

            }

            //if (currentDir != previousDir)
            {
                //Console.WriteLine("ACTIVATE EVENT");
                active = true;
                activeEvent = true;
                inputStatus = currentDir != DpadDirections.Centered;
            }
        }

        public override unsafe void Event(Mapper mapper)
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
                    AxisDirButton data = null;
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
                        data.PrepareAnalog(mapper, 0.0, 0.0);
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
                    AxisDirButton data = null;
                    if (tmpBtnActions[0] != null)
                    {
                        data = tmpBtnActions[0];
                        data.PrepareAnalog(mapper, 0.0, 0.0);
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
                        data.PrepareAnalog(mapper, 0.0, 0.0);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(tmpCodes[1], false);

                        if (!data.active)
                        {
                            tmpActiveBtns.Remove(data);
                        }
                    }
                }
            }

            //bool checkRingButton = useRingButton;
            //if (parentAction != null)
            //{
            //    checkRingButton = parentPadAction.useRingButton;
            //}

            if (useRingButton && usedRingButton != null)
            //if (checkRingButton)
            {
                //Trace.WriteLine("IN CHECK");
                double dist = Math.Sqrt((xNorm * xNorm) + (yNorm * yNorm));
                bool activeMod = outerRing ? (dist > outerRingDeadZone ? true : false) :
                    (dist > 0.0 && dist <= outerRingDeadZone ? true : false);

                //ringButton.PrepareAnalog(mapper, dist);
                //if (ringButton.active)
                //{
                //    ringButton.Event(mapper);
                //}

                // Treat as boolean button for now
                usedRingButton.Prepare(mapper, activeMod);
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
                    AxisDirButton data = null;
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
                        data.PrepareAnalog(mapper, ButtonAxisValue(data), ButtonAxisUnitValue(data));
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
                    AxisDirButton data = null;

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
                        data.PrepareAnalog(mapper, ButtonAxisValue(data), ButtonAxisUnitValue(data));
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
                        data.PrepareAnalog(mapper, ButtonAxisValue(data), ButtonAxisUnitValue(data));
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
                foreach (KeyValuePair<AxisDirButton, DpadDirections> pair in tmpActiveBtns)
                {
                    AxisDirButton data = pair.Key;
                    if (data != null)
                    {
                        //data.PrepareAnalog(mapper, ButtonAxisValue(data));
                        data.PrepareAnalog(mapper, 0.0, 0.0);
                        data.Event(mapper);

                        if (!data.active)
                        {
                            removeBtnCandidates.Add(data);
                        }
                    }
                }

                foreach(AxisDirButton removeBtn in removeBtnCandidates)
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
            bool ringBtnActive = usedRingButton != null && usedRingButton.active;
            active = currentDir != DpadDirections.Centered || ringBtnActive ||
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

                foreach (KeyValuePair<AxisDirButton, DpadDirections> pair in tmpActiveBtns)
                {
                    AxisDirButton data = pair.Key;
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

            if (resetState)
            {
                stateData.Reset();
            }
        }

        public void ReleaseOld(Mapper mapper, bool resetState = true)
        {
            if (currentDir != DpadDirections.Centered)
            {
                if (useRingButton && usedRingButton != null)
                {
                    usedRingButton.Release(mapper, resetState);
                }

                int currentDirNum = (int)currentDir;
                if (currentDirNum % 3 != 0)
                {
                    if (currentMode == DPadMode.Standard ||
                        currentMode == DPadMode.EightWay)
                    {
                        //int code = eventCodes[currentDirNum];
                        //OutputActionData data = eventCodes[currentDirNum];
                        AxisDirButton data = usedFuncList[currentDirNum];
                        if (data != null)
                        {
                            data.PrepareAnalog(mapper, 0.0, 0.0);
                            data.Event(mapper);
                            //mapper.RunEventFromButton(data, false);
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        ReleaseFourWayCardinalEvents(mapper);
                    }
                }
                else
                {
                    if (currentMode == DPadMode.Standard)
                    {
                        ReleaseStandardDiagonalEvents(mapper);
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        AxisDirButton data = usedFuncList[currentDirNum];
                        if (data != null)
                        {
                            data.PrepareAnalog(mapper, 0.0, 0.0);
                            data.Event(mapper);
                        }
                        //OutputActionData data = eventCodes[currentDirNum];
                        //mapper.RunEventFromButton(data, false);
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        ReleaseFourWayCardinalEvents(mapper);
                    }
                }

                currentDir = DpadDirections.Centered;
                previousDir = currentDir;
            }

            if (resetState)
            {
                stateData.Reset();
            }
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction,
            bool resetState = true)
        {
            if (active || tmpActiveBtns.Count > 0)
            {
                StickPadAction checkStickAction = checkAction as StickPadAction;

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

                foreach (KeyValuePair<AxisDirButton, DpadDirections> pair in tmpActiveBtns)
                {
                    AxisDirButton data = pair.Key;
                    DpadDirections dir = pair.Value;

                    int dirNum = (int)dir;

                    if (data != null && !useParentDataDraft2[dirNum])
                    {
                        data.Release(mapper, resetState);
                    }
                    else if (data != null && checkStickAction != null &&
                        checkStickAction.usedFuncList[dirNum] != data)
                    {
                        data.Release(mapper, resetState);
                    }
                }

                currentDir = DpadDirections.Centered;
                previousDir = currentDir;
            }

            inputStatus = false;

            if (resetState)
            {
                stateData.Reset();
            }
        }

        public void SoftReleaseOld(Mapper mapper, MapAction checkAction,
            bool resetState = true)
        {
            if (currentDir != DpadDirections.Centered)
            {
                StickPadAction checkStickAction = checkAction as StickPadAction;

                if (useRingButton && usedRingButton != null &&
                    usedRingButton.active)
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

                int currentDirNum = (int)currentDir;
                if (currentDirNum % 3 != 0)
                {
                    if (currentMode == DPadMode.Standard ||
                        currentMode == DPadMode.EightWay)
                    {
                        //int code = eventCodes[currentDirNum];
                        //OutputActionData data = eventCodes[currentDirNum];
                        AxisDirButton data = usedFuncList[currentDirNum];
                        if (data != null && !useParentDataDraft2[currentDirNum])
                        {
                            data.PrepareAnalog(mapper, 0.0, 0.0);
                            data.Event(mapper);
                            //mapper.RunEventFromButton(data, false);
                        }
                        else if (data != null && checkStickAction != null &&
                            checkStickAction.usedFuncList[currentDirNum] != data)
                        {
                            data.PrepareAnalog(mapper, 0.0, 0.0);
                            data.Event(mapper);
                            //mapper.RunEventFromButton(data, false);
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        ReleaseFourWayCardinalEvents(mapper, true, checkStickAction);
                    }
                }
                else
                {
                    if (currentMode == DPadMode.Standard)
                    {
                        ReleaseStandardDiagonalEvents(mapper, checkSoft: true, checkStickAction: checkStickAction);
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        AxisDirButton data = usedFuncList[currentDirNum];
                        if (data != null && !useParentDataDraft2[currentDirNum])
                        {
                            data.PrepareAnalog(mapper, 0.0, 0.0);
                            data.Event(mapper);
                        }
                        else if (data != null && checkStickAction != null &&
                            checkStickAction.usedFuncList[currentDirNum] != data)
                        {
                            data.PrepareAnalog(mapper, 0.0, 0.0);
                            data.Event(mapper);
                        }

                        //OutputActionData data = eventCodes[currentDirNum];
                        //mapper.RunEventFromButton(data, false);
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        ReleaseFourWayCardinalEvents(mapper, true, checkStickAction);
                    }
                }

                currentDir = DpadDirections.Centered;
                previousDir = currentDir;
            }

            if (resetState)
            {
                stateData.Reset();
            }
        }

        //private unsafe void ReleaseStandardDiagonalEvents(Mapper mapper)
        private void ReleaseStandardDiagonalEvents(Mapper mapper, bool checkSoft=false,
            StickPadAction checkStickAction = null)
        {
            int tempDir;
            int outDir = 0; int outDir2 = 0;
            //int* codes = stackalloc int[2];
            //tmpCodes[0] = tmpCodes[1] = null;
            tmpBtnActions[0] = tmpBtnActions[1] = null;
            int i = 0;
            if ((currentDir & DpadDirections.Up) != 0)
            {
                tempDir = (int)DpadDirections.Up;
                //tmpCodes[i] = eventCodes[tempDir]; i++;
                tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                outDir = tempDir;
            }
            else if ((currentDir & DpadDirections.Down) != 0)
            {
                tempDir = (int)DpadDirections.Down;
                //tmpCodes[i] = eventCodes[tempDir]; i++;
                tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                outDir = tempDir;
            }

            if ((currentDir & DpadDirections.Left) != 0)
            {
                tempDir = (int)DpadDirections.Left;
                //tmpCodes[i] = eventCodes[tempDir]; i++;
                tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                if (i == 0)
                {
                    outDir = tempDir;
                }
                else
                {
                    outDir2 = tempDir;
                }
            }
            else if ((currentDir & DpadDirections.Right) != 0)
            {
                tempDir = (int)DpadDirections.Right;
                //tmpCodes[i] = eventCodes[tempDir]; i++;
                tmpBtnActions[i] = usedFuncList[tempDir]; i++;
                if (i == 0)
                {
                    outDir = tempDir;
                }
                else
                {
                    outDir2 = tempDir;
                }
            }

            AxisDirButton data = null;
            if (tmpBtnActions[0] != null)
            {
                data = tmpBtnActions[0];

                if (!(checkSoft && useParentDataDraft2[outDir]))
                {
                    data.PrepareAnalog(mapper, 0.0, 0.0);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[0], false);
                }
                else if (checkSoft && checkStickAction != null &&
                    checkStickAction.usedFuncList[outDir] != data)
                {
                    data.PrepareAnalog(mapper, 0.0, 0.0);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[0], false);
                }
            }

            //if (tmpCodes[1] != null)
            if (tmpBtnActions[1] != null)
            {
                data = tmpBtnActions[1];

                if (!(checkSoft && useParentDataDraft2[outDir2]))
                {
                    data.PrepareAnalog(mapper, 0.0, 0.0);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[1], false);
                }
                else if (checkSoft && checkStickAction != null &&
                    checkStickAction.usedFuncList[outDir2] != data)
                {
                    data.PrepareAnalog(mapper, 0.0, 0.0);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[1], false);
                }
            }
        }

        private void ReleaseFourWayCardinalEvents(Mapper mapper, bool checkSoft=false, StickPadAction checkStickAction = null)
        {
            AxisDirButton data = null;
            int currentDirNum = (int)currentDir;
            int outDir = 0;
            if (currentDir == DpadDirections.Up || currentDir == DpadDirections.UpRight)
            {
                outDir = (int)DpadDirections.Up;
                data = usedFuncList[outDir];
            }
            else if (currentDir == DpadDirections.Right || currentDir == DpadDirections.DownRight)
            {
                outDir = (int)DpadDirections.Right;
                data = usedFuncList[outDir];
            }
            else if (currentDir == DpadDirections.Down || currentDir == DpadDirections.DownLeft)
            {
                outDir = (int)DpadDirections.Down;
                data = usedFuncList[outDir];
            }
            else if (currentDir == DpadDirections.Left || currentDir == DpadDirections.UpLeft)
            {
                outDir = (int)DpadDirections.Left;
                data = usedFuncList[outDir];
            }

            if (data != null)
            {
                if (!(checkSoft && useParentDataDraft2[outDir]))
                {
                    data.PrepareAnalog(mapper, 0.0, 0.0);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(data, false);
                }
                else if (checkSoft && checkStickAction != null &&
                    checkStickAction.usedFuncList[outDir] != data)
                {
                    data.PrepareAnalog(mapper, 0.0, 0.0);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(data, false);
                }
            }
        }

        private void DetermineDirection()
        {
            if (xNorm == 0.0 && yNorm == 0.0)
            {
                currentDir = DpadDirections.Centered;
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
                        currentDir = DpadDirections.Up;
                    }
                    else if (angle > upLeftEnd || angle < upRightBegin)
                    {
                        currentDir = DpadDirections.Up;
                    }
                    else if (angle >= upRightBegin && angle < rightBegin)
                    {
                        currentDir = DpadDirections.UpRight;
                    }
                    else if (angle >= rightBegin && angle < downRightBegin)
                    {
                        currentDir = DpadDirections.Right;
                    }
                    else if (angle >= downRightBegin && angle < downBegin)
                    {
                        currentDir = DpadDirections.DownRight;
                    }
                    else if (angle >= downBegin && angle < downLeftBegin)
                    {
                        currentDir = DpadDirections.Down;
                    }
                    else if (angle >= downLeftBegin && angle < leftBegin)
                    {
                        currentDir = DpadDirections.DownLeft;
                    }
                    else if (angle >= leftBegin && angle < upLeftBegin)
                    {
                        currentDir = DpadDirections.Left;
                    }
                    else if (angle >= upLeftBegin && angle <= upLeftEnd)
                    {
                        currentDir = DpadDirections.UpLeft;
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
                        currentDir = DpadDirections.Up;
                    }
                    else if (angle > upRightBegin && angle < downRightBegin)
                    {
                        currentDir = DpadDirections.UpRight;
                    }
                    else if (angle >= downRightBegin && angle < downLeftBegin)
                    {
                        currentDir = DpadDirections.DownRight;
                    }
                    else if (angle >= downLeftBegin && angle < upLeftBegin)
                    {
                        currentDir = DpadDirections.DownLeft;
                    }
                    else if (angle >= upLeftBegin && angle < 360)
                    {
                        currentDir = DpadDirections.UpLeft;
                    }
                }
            }

            //Trace.WriteLine(currentDir);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double ButtonAxisValue(AxisDirButton button)
        {
            double result = 0.0;
            switch (button.Direction)
            {
                case AxisDirButton.AxisDirection.None:
                    break;
                case AxisDirButton.AxisDirection.XNeg:
                    result = xNorm;
                    break;
                case AxisDirButton.AxisDirection.XPos:
                    result = xNorm;
                    break;
                case AxisDirButton.AxisDirection.YNeg:
                    result = yNorm;
                    break;
                case AxisDirButton.AxisDirection.YPos:
                    result = yNorm;
                    break;
                case AxisDirButton.AxisDirection.XY:
                    result = Math.Sqrt((xNorm * xNorm) + (yNorm * yNorm));
                    break;
                default:
                    break;
            }

            result = Math.Abs(result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double ButtonAxisUnitValue(AxisDirButton button)
        {
            double result = 0.0;

            double angle = Math.Atan2(-yNorm, xNorm);
            double angCos = Math.Abs(Math.Cos(angle)),
                angSin = Math.Abs(Math.Sin(angle));

            switch (button.Direction)
            {
                case AxisDirButton.AxisDirection.None:
                    break;
                case AxisDirButton.AxisDirection.XNeg:
                    result = angCos;
                    break;
                case AxisDirButton.AxisDirection.XPos:
                    result = angCos;
                    break;
                case AxisDirButton.AxisDirection.YNeg:
                    result = angSin;
                    break;
                case AxisDirButton.AxisDirection.YPos:
                    result = angSin;
                    break;
                case AxisDirButton.AxisDirection.XY:
                    // Treat XY as a single unit vector
                    result = 1.0;
                    break;
                default:
                    break;
            }

            result = Math.Abs(result);
            return result;
        }

        public override StickMapAction DuplicateAction()
        {
            return new StickPadAction(this);
        }

        public override void SoftCopyFromParent(StickMapAction parentAction)
        {
            if (parentAction is StickPadAction tempPadAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                tempPadAction.hasLayeredAction = true;
                parentPadAction = tempPadAction;

                this.stickDefinition = new StickDefinition(tempPadAction.stickDefinition);
                mappingId = tempPadAction.mappingId;
                useParentActions = true;

                tempPadAction.NotifyPropertyChanged += TempPadAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch(parentPropType)
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
                            currentMode = tempPadAction.CurrentMode;
                            break;
                        case PropertyKeyStrings.PAD_DIR_UP:
                            {
                                int tempDir = (int)DpadDirections.Up;
                                eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWN:
                            {
                                int tempDir = (int)DpadDirections.Down;
                                eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_LEFT:
                            {
                                int tempDir = (int)DpadDirections.Left;
                                eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_RIGHT:
                            {
                                int tempDir = (int)DpadDirections.Right;
                                eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_UPLEFT:
                            {
                                int tempDir = (int)DpadDirections.UpLeft;
                                eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_UPRIGHT:
                            {
                                int tempDir = (int)DpadDirections.UpRight;
                                eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWNLEFT:
                            {
                                int tempDir = (int)DpadDirections.DownLeft;
                                eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWNRIGHT:
                            {
                                int tempDir = (int)DpadDirections.DownRight;
                                eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
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
                        case PropertyKeyStrings.ROTATION:
                            rotation = tempPadAction.rotation;
                            break;
                        case PropertyKeyStrings.DIAGONAL_RANGE:
                            diagonalRange = tempPadAction.diagonalRange;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE_TYPE:
                            deadMod.DeadZoneType = tempPadAction.deadMod.DeadZoneType;
                            break;
                        default:
                            break;
                    }
                }

                // Grab references to parentAction AxisDirButton instances
                /*int tempDir = (int)DpadDirections.Centered;
                foreach (AxisDirButton tempAction in parentAction.eventCodes4)
                {
                    usedEventButtonsList[tempDir] = tempAction;
                    tempDir++;
                }
                usedFuncList = usedEventButtonsList;
                */
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

            StickPadAction tempPadAction = parentAction as StickPadAction;

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
                    currentMode = tempPadAction.CurrentMode;
                    break;
                case PropertyKeyStrings.PAD_DIR_UP:
                    {
                        int tempDir = (int)DpadDirections.Up;
                        eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_DOWN:
                    {
                        int tempDir = (int)DpadDirections.Down;
                        eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_LEFT:
                    {
                        int tempDir = (int)DpadDirections.Left;
                        eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_RIGHT:
                    {
                        int tempDir = (int)DpadDirections.Right;
                        eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_UPLEFT:
                    {
                        int tempDir = (int)DpadDirections.UpLeft;
                        eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_UPRIGHT:
                    {
                        int tempDir = (int)DpadDirections.UpRight;
                        eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_DOWNLEFT:
                    {
                        int tempDir = (int)DpadDirections.DownLeft;
                        eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_DOWNRIGHT:
                    {
                        int tempDir = (int)DpadDirections.DownRight;
                        eventCodes4[tempDir] = tempPadAction.eventCodes4[tempDir];
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
                case PropertyKeyStrings.ROTATION:
                    rotation = tempPadAction.rotation;
                    break;
                case PropertyKeyStrings.DIAGONAL_RANGE:
                    diagonalRange = tempPadAction.diagonalRange;
                    break;
                case PropertyKeyStrings.DEAD_ZONE_TYPE:
                    deadMod.DeadZoneType = tempPadAction.deadMod.DeadZoneType;
                    break;
                default:
                    break;
            }
        }
    }
}
