using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.DPadActions
{
    public class DPadAction : DPadMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string PAD_MODE = "PadMode";
            public const string DELAY_TIME = "DelayTime";

            public const string PAD_DIR_UP = "DirUp";
            public const string PAD_DIR_DOWN = "DirDown";
            public const string PAD_DIR_LEFT = "DirLeft";
            public const string PAD_DIR_RIGHT = "DirRight";

            public const string PAD_DIR_UPLEFT = "DirUpLeft";
            public const string PAD_DIR_UPRIGHT = "DirUpRight";
            public const string PAD_DIR_DOWNLEFT = "DirDownLeft";
            public const string PAD_DIR_DOWNRIGHT = "DirDownRight";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.PAD_MODE,
            PropertyKeyStrings.DELAY_TIME,
            PropertyKeyStrings.PAD_DIR_UP,
            PropertyKeyStrings.PAD_DIR_DOWN,
            PropertyKeyStrings.PAD_DIR_LEFT,
            PropertyKeyStrings.PAD_DIR_RIGHT,
            PropertyKeyStrings.PAD_DIR_UPLEFT,
            PropertyKeyStrings.PAD_DIR_UPRIGHT,
            PropertyKeyStrings.PAD_DIR_DOWNLEFT,
            PropertyKeyStrings.PAD_DIR_DOWNRIGHT,
        };

        public enum DPadMode : uint
        {
            Standard,
            EightWay,
            FourWayCardinal,
            FourWayDiagonal,
        }

        public const string ACTION_TYPE_NAME = "DPadAction";

        // Cardinal direction codes. Use 0 for Centered and intermediate diagonals
        private int[] eventCodes = new int[9]
        {
            0,
            0,
            -128, // Right
            0,
            0,
            0, 0, 0,
            128, // Left
        };

        // Specify the input state of the button
        private bool inputStatus;

        // Cardinal direction codes. Use 0 for Centered
        //private OutputActionData[] eventCodes2 = new OutputActionData[13];
        private ButtonAction[] eventCodes4 = new ButtonAction[13];
        private ButtonAction[] usedEventList;

        // Allow individual ButtonAction instances to be overridden
        private ButtonAction[] usedEventButtonsList = new ButtonAction[13];

        private DPadMode currentMode = DPadMode.Standard;
        //private OutputActionData[] tmpCodes = new OutputActionData[2];
        private ButtonAction[] tmpCodes2 = new ButtonAction[2];
        private Dictionary<ButtonAction, DpadDirections> tmpActiveBtns = new Dictionary<ButtonAction, DpadDirections>();
        private int[] tmpBtnDirs = new int[2];
        private List<ButtonAction> removeBtnCandidates = new List<ButtonAction>();

        //public OutputActionData[] EventCodes2 { get => eventCodes2; }
        public DPadMode CurrentMode { get => currentMode; set => currentMode = value; }
        public ButtonAction[] EventCodes4 { get => eventCodes4; }

        private bool useParentData;
        private bool[] useParentDataDraft2 = new bool[13];
        public bool[] UsingParentActionButton => useParentDataDraft2;
        private bool useParentDelay;

        private double delayTime;
        public double DelayTime
        {
            get => delayTime;
            set => delayTime = value;
        }

        private Stopwatch delayStopWatch = new Stopwatch();

        public DPadAction()
        {
            usedEventList = eventCodes4;

            actionTypeName = ACTION_TYPE_NAME;

            FillDirectionButtons();
            usedEventList = eventCodes4;
        }

        public DPadAction(DPadAction parentAction)
        {
            if (parentAction != null)
            {
                useParentData = true;
                //usedEventList = parentAction.eventCodes4;

                // Grab references to parentAction ButtonAction instances
                int tempDir = (int)DpadDirections.Centered;
                foreach (ButtonAction tempAction in parentAction.eventCodes4)
                {
                    usedEventButtonsList[tempDir] = tempAction;
                    useParentDataDraft2[tempDir] = true;
                    tempDir++;
                }
                usedEventList = usedEventButtonsList;

                this.parentAction = parentAction;
                parentAction.hasLayeredAction = true;
                mappingId = parentAction.mappingId;
            }
            else
            {
                usedEventList = eventCodes4;
            }

            actionTypeName = ACTION_TYPE_NAME;
        }

        private void FillDirectionButtons()
        {
            for (int i = 0; i < eventCodes4.Length; i++)
            {
                //if (tempDir != AxisDirButton.AxisDirection.None)
                {
                    ButtonAction tempBtn = new ButtonAction();
                    eventCodes4[i] = tempBtn;
                }
            }
        }

        public override void Prepare(Mapper mapper, DpadDirections value, bool alterState = true)
        {
            if (value != previousDir)
            {
                bool changeDir = false;
                if (value == DpadDirections.Centered)
                {
                    changeDir = true;
                }
                else if (delayTime != 0.0)
                {
                    if (!delayStopWatch.IsRunning)
                    {
                        delayStopWatch.Restart();
                    }

                    if ((delayStopWatch.ElapsedMilliseconds * 0.001) >= delayTime)
                    {
                        changeDir = true;
                    }
                }
                else
                {
                    changeDir = true;
                }

                if (changeDir)
                {
                    currentDir = value;
                    active = true;
                    activeEvent = true;
                    inputStatus = value != DpadDirections.Centered;
                    delayStopWatch.Reset();
                }
            }

            if (alterState)
            {
                stateData.wasActive = stateData.state;
                stateData.state = active;
            }
        }

        public override unsafe void Event(Mapper mapper)
        {
            const int dpadUpDir = (int)DpadDirections.Up;
            const int dpadDownDir = (int)DpadDirections.Down;
            const int dpadLeftDir = (int)DpadDirections.Left;
            const int dpadRightDir = (int)DpadDirections.Right;

            int previousDirNum = (int)previousDir;
            int currentDirNum = (int)currentDir;

            int xorState = (int)(previousDir ^ currentDir);
            int btnRem = xorState & previousDirNum;
            int btnAdd = xorState & currentDirNum;

            // Release old buttons
            if (previousDir != DpadDirections.Centered)
            {
                bool onlyCardinal = previousDirNum % 3 != 0;
                //OutputActionData data = null;
                ButtonAction data = null;
                if (onlyCardinal)
                {
                    if (currentMode == DPadMode.Standard)
                    {
                        if ((previousDirNum & btnRem) != 0)
                        {
                            //int code = eventCodes[previousDirNum];
                            //data = eventCodes2[previousDirNum];
                            data = usedEventList[previousDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        if (previousDir != currentDir)
                        {
                            //data = eventCodes2[previousDirNum];
                            data = usedEventList[previousDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        if ((previousDir == DpadDirections.Up) &&
                            (currentDir != DpadDirections.Up && currentDir != DpadDirections.UpRight))
                        {
                            // Map Up or UpRight to output Up direction
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[previousDirNum];
                        }
                        else if ((previousDir == DpadDirections.Right) &&
                            (currentDir != DpadDirections.Right && currentDir != DpadDirections.DownRight))
                        {
                            // Map Right or DownRight to output Right direction
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[previousDirNum];
                        }
                        else if ((previousDir == DpadDirections.Down) &&
                            (currentDir != DpadDirections.Down && currentDir != DpadDirections.DownLeft))
                        {
                            // Map Down or DownLeft to output Down direction
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[previousDirNum];
                        }
                        else if ((previousDir == DpadDirections.Left) &&
                            (currentDir != DpadDirections.Left && currentDir != DpadDirections.UpLeft))
                        {
                            // Map Left or UpLeft to output Left direction
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[previousDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.FourWayDiagonal)
                    {
                        if (previousDir == DpadDirections.UpRight && currentDir != DpadDirections.UpRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[previousDirNum];
                        }
                        else if (previousDir == DpadDirections.DownRight && currentDir != DpadDirections.DownRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[previousDirNum];
                        }
                        else if (previousDir == DpadDirections.DownLeft && currentDir != DpadDirections.Down && currentDir != DpadDirections.DownLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[previousDirNum];
                        }
                        else if (previousDir == DpadDirections.UpLeft && currentDir != DpadDirections.UpLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[previousDirNum];
                        }
                    }

                    if (data != null)
                    {
                        data.Prepare(mapper, false);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(data, false);
                        if (!data.active)
                        {
                            tmpActiveBtns.Remove(data);
                        }
                    }
                }
                else
                {
                    int tempDir;
                    //int* codes = stackalloc int[2];
                    ///tmpCodes[0] = tmpCodes[1] = null;
                    tmpCodes2[0] = tmpCodes2[1] = null;
                    int i = 0;

                    if (currentMode == DPadMode.Standard)
                    {
                        if ((previousDir & DpadDirections.Up) != 0 && (dpadUpDir & btnRem) != 0)
                        {
                            tempDir = (int)DpadDirections.Up;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                        else if ((previousDir & DpadDirections.Down) != 0 && (dpadDownDir & btnRem) != 0)
                        {
                            tempDir = (int)DpadDirections.Down;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }

                        if ((previousDir & DpadDirections.Left) != 0 && (dpadLeftDir & btnRem) != 0)
                        {
                            tempDir = (int)DpadDirections.Left;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                        else if ((previousDir & DpadDirections.Right) != 0 && (dpadRightDir & btnRem) != 0)
                        {
                            tempDir = (int)DpadDirections.Right;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        if (previousDir != currentDir)
                        {
                            //tmpCodes[i] = eventCodes2[previousDirNum]; i++;
                            tmpCodes2[i] = usedEventList[previousDirNum]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        //if ((previousDir & DpadDirections.Up) != 0 && (dpadUpDir & btnRem) != 0)
                        if ((previousDir == DpadDirections.Up || previousDir == DpadDirections.UpRight) &&
                            (currentDir != DpadDirections.Up && currentDir != DpadDirections.UpRight))
                        {
                            // Map Up or UpRight to output Up direction
                            tempDir = (int)DpadDirections.Up;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                        //else if ((previousDir & DpadDirections.Down) != 0 && (dpadDownDir & btnRem) != 0)
                        else if ((previousDir == DpadDirections.Right || previousDir == DpadDirections.DownRight) &&
                            (currentDir != DpadDirections.Right && currentDir != DpadDirections.DownRight))
                        {
                            // Map Right or DownRight to output Right direction
                            tempDir = (int)DpadDirections.Right;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                        //else if ((previousDir & DpadDirections.Right) != 0 && (dpadRightDir & btnRem) != 0)
                        else if ((previousDir == DpadDirections.Down || previousDir == DpadDirections.DownLeft) &&
                            (currentDir != DpadDirections.Down && currentDir != DpadDirections.DownLeft))
                        {
                            // Map Down or DownLeft to output Down direction
                            tempDir = (int)DpadDirections.Down;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                        //if ((previousDir & DpadDirections.Left) != 0 && (dpadLeftDir & btnRem) != 0)
                        else if ((previousDir == DpadDirections.Left || previousDir == DpadDirections.UpLeft) &&
                            (currentDir != DpadDirections.Left && currentDir != DpadDirections.UpLeft))
                        {
                            // Map Left or UpLeft to output Left direction
                            tempDir = (int)DpadDirections.Left;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.FourWayDiagonal)
                    {
                        if (previousDir == DpadDirections.UpRight && currentDir != DpadDirections.UpRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            tmpCodes2[i] = usedEventList[previousDirNum]; i++;
                        }
                        else if (previousDir == DpadDirections.DownRight && currentDir != DpadDirections.DownRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            tmpCodes2[i] = usedEventList[previousDirNum]; i++;
                        }
                        else if (previousDir == DpadDirections.DownLeft && currentDir != DpadDirections.Down && currentDir != DpadDirections.DownLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            tmpCodes2[i] = usedEventList[previousDirNum]; i++;
                        }
                        else if (previousDir == DpadDirections.UpLeft && currentDir != DpadDirections.UpLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            tmpCodes2[i] = usedEventList[previousDirNum]; i++;
                        }
                    }

                    //if (tmpCodes[0] != null)
                    if (tmpCodes2[0] != null)
                    {
                        data = tmpCodes2[0];
                        data.Prepare(mapper, false);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(tmpCodes[0], false);

                        if (!data.active)
                        {
                            tmpActiveBtns.Remove(data);
                        }
                    }

                    if (tmpCodes2[1] != null)
                    {
                        data = tmpCodes2[1];
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

            // Activate new buttons
            if (currentDir != DpadDirections.Centered)
            {
                bool onlyCardinal = currentDirNum % 3 != 0;
                bool changedDir = previousDir != currentDir;
                //OutputActionData data = null;
                ButtonAction data = null;
                if (onlyCardinal)
                {
                    if (currentMode == DPadMode.Standard)
                    {
                        //if ((currentDirNum & btnAdd) != 0)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[currentDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        //if (currentDir != previousDir)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[currentDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        //if ((currentDirNum & btnAdd) != 0)
                        if (currentDir == DpadDirections.Up)// &&
                            //(previousDir != DpadDirections.Up && previousDir != DpadDirections.UpRight))
                        {
                            // Map Up or UpRight to output Up direction
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.Right)// &&
                            //(previousDir != DpadDirections.Right && previousDir != DpadDirections.DownRight))
                        {
                            // Map Right or DownRight to output Right direction
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.Down)// &&
                            //(previousDir != DpadDirections.Down && previousDir != DpadDirections.DownLeft))
                        {
                            // Map Down or DownLeft to output Down direction
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.Left)// &&
                            //(previousDir != DpadDirections.Left && previousDir != DpadDirections.UpLeft))
                        {
                            // Map Left or UpLeft to output Left direction
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[currentDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.FourWayDiagonal)
                    {
                        if (currentDir == DpadDirections.UpRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.DownRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.DownLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[currentDirNum];
                        }
                        else if (currentDir == DpadDirections.UpLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            data = usedEventList[currentDirNum];
                        }
                    }

                    if (data != null)
                    {
                        data.Prepare(mapper, true);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(data, true);

                        if (changedDir && !tmpActiveBtns.ContainsKey(data))
                        {
                            tmpActiveBtns.Add(data, currentDir);
                        }
                    }
                }
                else
                {
                    int tempDir;
                    //int* codes = stackalloc int[2];
                    //tmpCodes[0] = tmpCodes[1] = null;
                    tmpCodes2[0] = tmpCodes2[1] = null;
                    tmpBtnDirs[0] = tmpBtnDirs[1] = (int)DpadDirections.Centered;
                    int i = 0;

                    if (currentMode == DPadMode.Standard)
                    {
                        if ((currentDir & DpadDirections.Up) != 0)// && (dpadUpDir & btnAdd) != 0)
                        {
                            tempDir = (int)DpadDirections.Up;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                        else if ((currentDir & DpadDirections.Down) != 0)// && (dpadDownDir & btnAdd) != 0)
                        {
                            tempDir = (int)DpadDirections.Down;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }

                        if ((currentDir & DpadDirections.Left) != 0)// && (dpadLeftDir & btnAdd) != 0)
                        {
                            tempDir = (int)DpadDirections.Left;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                        else if ((currentDir & DpadDirections.Right) != 0)// && (dpadRightDir & btnAdd) != 0)
                        {
                            tempDir = (int)DpadDirections.Right;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        //if (previousDir != currentDir)
                        {
                            //tmpCodes[i] = eventCodes2[currentDirNum]; i++;
                            tmpBtnDirs[i] = currentDirNum;
                            tmpCodes2[i] = usedEventList[currentDirNum]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        //if ((currentDir & DpadDirections.Up) != 0 && (dpadUpDir & btnAdd) != 0)
                        if ((currentDir == DpadDirections.Up || currentDir == DpadDirections.UpRight))// &&
                            //(previousDir != DpadDirections.Up && previousDir != DpadDirections.UpRight))
                        {
                            // Map Up or UpRight to output Up direction
                            tempDir = (int)DpadDirections.Up;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                        //else if ((currentDir & DpadDirections.Down) != 0 && (dpadDownDir & btnAdd) != 0)
                        else if ((currentDir == DpadDirections.Right || currentDir == DpadDirections.DownRight))// &&
                            //(previousDir != DpadDirections.Right && previousDir != DpadDirections.DownRight))
                        {
                            // Map Right or DownRight to output Right direction
                            tempDir = (int)DpadDirections.Right;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                        //if ((currentDir & DpadDirections.Left) != 0 && (dpadLeftDir & btnAdd) != 0)
                        else if ((currentDir == DpadDirections.Down || currentDir == DpadDirections.DownLeft))// &&
                            //(previousDir != DpadDirections.Down && previousDir != DpadDirections.DownLeft))
                        {
                            // Map Down or DownLeft to output Down direction
                            tempDir = (int)DpadDirections.Down;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                        //else if ((currentDir & DpadDirections.Right) != 0 && (dpadRightDir & btnAdd) != 0)
                        else if ((currentDir == DpadDirections.Left || currentDir == DpadDirections.UpLeft))// &&
                            //(previousDir != DpadDirections.Left && previousDir != DpadDirections.UpLeft))
                        {
                            // Map Left or UpLeft to output Left direction
                            tempDir = (int)DpadDirections.Left;
                            //tmpCodes[i] = eventCodes2[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[tempDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.FourWayDiagonal)
                    {
                        if (currentDir == DpadDirections.UpRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            tempDir = (int)DpadDirections.UpRight;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[currentDirNum]; i++;
                        }
                        else if (currentDir == DpadDirections.DownRight)
                        {
                            //data = eventCodes2[currentDirNum];
                            tempDir = (int)DpadDirections.DownRight;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[currentDirNum]; i++;
                        }
                        else if (currentDir == DpadDirections.DownLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            tempDir = (int)DpadDirections.DownLeft;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[currentDirNum]; i++;
                        }
                        else if (currentDir == DpadDirections.UpLeft)
                        {
                            //data = eventCodes2[currentDirNum];
                            tempDir = (int)DpadDirections.UpLeft;
                            tmpBtnDirs[i] = tempDir;
                            tmpCodes2[i] = usedEventList[currentDirNum]; i++;
                        }
                    }

                    if (tmpCodes2[0] != null)
                    {
                        data = tmpCodes2[0];
                        data.Prepare(mapper, true);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(tmpCodes[0], true);

                        if (changedDir && !tmpActiveBtns.ContainsKey(data))
                        {
                            tmpActiveBtns.Add(data, (DpadDirections)tmpBtnDirs[0]);
                        }
                    }

                    if (tmpCodes2[1] != null)
                    {
                        data = tmpCodes2[1];
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
                        //data.Prepare(mapper, status);
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

            previousDir = currentDir;
            active = currentDir != DpadDirections.Centered || tmpActiveBtns.Count > 0;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            if (tmpActiveBtns.Count > 0)
            {
                foreach(KeyValuePair<ButtonAction, DpadDirections> pair in tmpActiveBtns)
                {
                    ButtonAction data = pair.Key;
                    DpadDirections dir = pair.Value;

                    if (data != null)
                    {
                        data.Release(mapper, resetState, ignoreReleaseActions);
                    }
                }

                tmpActiveBtns.Clear();
            }

            currentDir = DpadDirections.Centered;
            previousDir = currentDir;
            active = false;
            activeEvent = false;
            inputStatus = false;
            delayStopWatch.Reset();

            if (resetState)
            {
                stateData.Reset();
            }
        }

        //public override void Release(Mapper mapper, bool resetState = true)
        public void ReleaseOld(Mapper mapper, bool resetState = true)
        {
            if (currentDir != DpadDirections.Centered)
            {
                int currentDirNum = (int)currentDir;
                if (currentDirNum % 3 != 0)
                {
                    //int code = eventCodes[currentDirNum];
                    //OutputActionData data = eventCodes2[currentDirNum];
                    if (currentMode == DPadMode.Standard ||
                        currentMode == DPadMode.EightWay)
                    {
                        ButtonAction data = usedEventList[currentDirNum];
                        if (data != null)
                        {
                            data.Prepare(mapper, false);
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
                        ReleaseDiagonalEvents(mapper);
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        //OutputActionData data = eventCodes2[currentDirNum];
                        ButtonAction data = usedEventList[currentDirNum];
                        if (data != null)
                        {
                            data.Prepare(mapper, false);
                            data.Event(mapper);
                            //mapper.RunEventFromButton(data, false);
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        ReleaseFourWayCardinalEvents(mapper);
                    }
                }

                currentDir = DpadDirections.Centered;
                previousDir = currentDir;
                active = false;
                activeEvent = false;

                if (resetState)
                {
                    stateData.Reset();
                }
            }

            delayStopWatch.Reset();
        }


        public override void SoftRelease(Mapper mapper, MapAction checkAction,
            bool resetState = true)
        {
            if (tmpActiveBtns.Count > 0)
            {
                DPadAction checkDPadAction = checkAction as DPadAction;
                foreach (KeyValuePair<ButtonAction, DpadDirections> pair in tmpActiveBtns)
                {
                    ButtonAction data = pair.Key;
                    DpadDirections dir = pair.Value;
                    int dirNum = (int)dir;

                    if (data != null && !useParentDataDraft2[dirNum])
                    {
                        data.Release(mapper, resetState);
                    }
                    else if (data != null && checkDPadAction != null &&
                            checkDPadAction.usedEventList[dirNum] != data)
                    {
                        data.Release(mapper, resetState);
                    }
                }

                tmpActiveBtns.Clear();
            }

            currentDir = DpadDirections.Centered;
            previousDir = currentDir;
            active = false;
            activeEvent = false;
            inputStatus = false;

            if (!useParentDelay)
            {
                delayStopWatch.Reset();
            }

            if (resetState)
            {
                stateData.Reset();
            }
        }

        //public override void SoftRelease(Mapper mapper, MapAction checkAction,
        //    bool resetState = true)
        public void SoftReleaseOld(Mapper mapper, MapAction checkAction,
            bool resetState = true)
        {
            if (currentDir != DpadDirections.Centered)
            {
                DPadAction checkDPadAction = checkAction as DPadAction;
                int currentDirNum = (int)currentDir;
                if (currentDirNum % 3 != 0)
                {
                    //int code = eventCodes[currentDirNum];
                    //OutputActionData data = eventCodes2[currentDirNum];
                    if (currentMode == DPadMode.Standard ||
                        currentMode == DPadMode.EightWay)
                    {
                        ButtonAction data = usedEventList[currentDirNum];
                        if (data != null && !useParentDataDraft2[currentDirNum])
                        {
                            data.Prepare(mapper, false);
                            data.Event(mapper);
                            //mapper.RunEventFromButton(data, false);
                        }
                        else if (data != null && checkDPadAction != null &&
                            checkDPadAction.usedEventList[currentDirNum] != data)
                        {
                            data.Prepare(mapper, false);
                            data.Event(mapper);
                            //mapper.RunEventFromButton(data, false);
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        ReleaseFourWayCardinalEvents(mapper, checkSoft: true, checkDPadAction: checkDPadAction);
                    }
                }
                else
                {
                    if (currentMode == DPadMode.Standard)
                    {
                        ReleaseDiagonalEvents(mapper, checkSoft: true, checkDPadAction: checkDPadAction);
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        //OutputActionData data = eventCodes2[currentDirNum];
                        ButtonAction data = usedEventList[currentDirNum];
                        if (data != null && !useParentDataDraft2[currentDirNum])
                        {
                            data.Prepare(mapper, false);
                            data.Event(mapper);
                            //mapper.RunEventFromButton(data, false);
                        }
                        else if (data != null && checkDPadAction != null &&
                            checkDPadAction.usedEventList[currentDirNum] != data)
                        {
                            data.Prepare(mapper, false);
                            data.Event(mapper);
                            //mapper.RunEventFromButton(data, false);
                        }
                    }
                    else if (currentMode == DPadMode.FourWayCardinal)
                    {
                        ReleaseFourWayCardinalEvents(mapper, checkSoft: true, checkDPadAction: checkDPadAction);
                    }
                }

                currentDir = DpadDirections.Centered;
                previousDir = currentDir;
                active = false;
                activeEvent = false;

                if (resetState)
                {
                    stateData.Reset();
                }
            }
        }

        //private unsafe void ReleaseDiagonalEvents(Mapper mapper)
        private void ReleaseDiagonalEvents(Mapper mapper, bool checkSoft = false,
            DPadAction checkDPadAction = null)
        {
            int tempDir;
            int outDir = 0; int outDir2 = 0;
            //int* codes = stackalloc int[2];
            //tmpCodes[0] = tmpCodes[1] = null;
            tmpCodes2[0] = tmpCodes2[1] = null;
            int i = 0;
            if ((currentDir & DpadDirections.Up) != 0)
            {
                tempDir = (int)DpadDirections.Up;
                //tmpCodes[i] = eventCodes2[tempDir]; i++;
                tmpCodes2[i] = usedEventList[tempDir]; i++;
                outDir = tempDir;
            }
            else if ((currentDir & DpadDirections.Down) != 0)
            {
                tempDir = (int)DpadDirections.Down;
                //tmpCodes[i] = eventCodes2[tempDir]; i++;
                tmpCodes2[i] = usedEventList[tempDir]; i++;
                outDir = tempDir;
            }

            if ((currentDir & DpadDirections.Left) != 0)
            {
                tempDir = (int)DpadDirections.Left;
                //tmpCodes[i] = eventCodes2[tempDir]; i++;
                tmpCodes2[i] = usedEventList[tempDir]; i++;
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
                //tmpCodes[i] = eventCodes2[tempDir]; i++;
                tmpCodes2[i] = usedEventList[tempDir]; i++;
                if (i == 0)
                {
                    outDir = tempDir;
                }
                else
                {
                    outDir2 = tempDir;
                }
            }

            ButtonAction data = null;
            //if (tmpCodes[0] != null)
            if (tmpCodes2[0] != null)
            {
                data = tmpCodes2[0];
                if (!(checkSoft && useParentDataDraft2[outDir]))
                {
                    data.Prepare(mapper, false);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[0], false);
                }
                else if (checkSoft && checkDPadAction != null &&
                    checkDPadAction.usedEventList[outDir] != data)
                {
                    data.Prepare(mapper, false);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[0], false);
                }
            }
            
            if (tmpCodes2[1] != null)
            {
                data = tmpCodes2[1];
                if (!(checkSoft && useParentDataDraft2[outDir2]))
                {
                    data.Prepare(mapper, false);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[1], false);
                }
                else if (checkSoft && checkDPadAction != null &&
                    checkDPadAction.usedEventList[outDir2] != data)
                {
                    data.Prepare(mapper, false);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[1], false);
                }
            }
        }

        private void ReleaseFourWayCardinalEvents(Mapper mapper, bool checkSoft = false,
            DPadAction checkDPadAction = null)
        {
            ButtonAction data = null;
            //int currentDirNum = (int)currentDir;
            int outDir = 0;
            if (currentDir == DpadDirections.Up || currentDir == DpadDirections.UpRight)
            {
                outDir = (int)DpadDirections.Up;
                data = usedEventList[outDir];
            }
            else if (currentDir == DpadDirections.Right || currentDir == DpadDirections.DownRight)
            {
                outDir = (int)DpadDirections.Right;
                data = usedEventList[outDir];
            }
            else if (currentDir == DpadDirections.Down || currentDir == DpadDirections.DownLeft)
            {
                outDir = (int)DpadDirections.Down;
                data = usedEventList[outDir];
            }
            else if (currentDir == DpadDirections.Left || currentDir == DpadDirections.UpLeft)
            {
                outDir = (int)DpadDirections.Left;
                data = usedEventList[outDir];
            }

            if (data != null)
            {
                if (!(checkSoft && useParentDataDraft2[outDir]))
                {
                    data.Prepare(mapper, false);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[0], false);
                }
                else if (checkSoft && checkDPadAction != null &&
                    checkDPadAction.usedEventList[outDir] != data)
                {
                    data.Prepare(mapper, false);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[0], false);
                }
            }
        }

        public override DPadMapAction DuplicateAction()
        {
            return new DPadAction(this);
        }

        public override void SoftCopyFromParent(DPadMapAction parentAction)
        {
            if (parentAction is DPadAction tempDpadAction)
            {
                base.SoftCopyFromParent(parentAction);

                useParentData = true;

                this.parentAction = parentAction;
                tempDpadAction.hasLayeredAction = true;
                mappingId = tempDpadAction.mappingId;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch(parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempDpadAction.name;
                            break;
                        case PropertyKeyStrings.PAD_MODE:
                            currentMode = tempDpadAction.CurrentMode;
                            break;
                        case PropertyKeyStrings.PAD_DIR_UP:
                            {
                                int tempDir = (int)DpadDirections.Up;
                                usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWN:
                            {
                                int tempDir = (int)DpadDirections.Down;
                                usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_LEFT:
                            {
                                int tempDir = (int)DpadDirections.Left;
                                usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_RIGHT:
                            {
                                int tempDir = (int)DpadDirections.Right;
                                usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_UPLEFT:
                            {
                                int tempDir = (int)DpadDirections.UpLeft;
                                usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_UPRIGHT:
                            {
                                int tempDir = (int)DpadDirections.UpRight;
                                usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWNLEFT:
                            {
                                int tempDir = (int)DpadDirections.DownLeft;
                                usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWNRIGHT:
                            {
                                int tempDir = (int)DpadDirections.DownRight;
                                usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.DELAY_TIME:
                            {
                                delayTime = tempDpadAction.delayTime;
                                // Copy ref to parent action Stopwatch
                                delayStopWatch = tempDpadAction.delayStopWatch;
                                useParentDelay = true;
                            }

                            break;
                        default:
                            break;
                    }
                }

                //if (changedProperties.Contains(PropertyKeyStrings.NAME))
                //{
                //    name = tempDpadAction.name;
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.PAD_MODE))
                //{
                //    currentMode = tempDpadAction.CurrentMode;
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.PAD_DIR_UP))
                //{
                //    int tempDir = (int)DpadDirections.Up;
                //    usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.PAD_DIR_DOWN))
                //{
                //    int tempDir = (int)DpadDirections.Down;
                //    usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.PAD_DIR_LEFT))
                //{
                //    int tempDir = (int)DpadDirections.Left;
                //    usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.PAD_DIR_RIGHT))
                //{
                //    int tempDir = (int)DpadDirections.Right;
                //    usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.PAD_DIR_UPLEFT))
                //{
                //    int tempDir = (int)DpadDirections.UpLeft;
                //    usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.PAD_DIR_UPRIGHT))
                //{
                //    int tempDir = (int)DpadDirections.UpRight;
                //    usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.PAD_DIR_DOWNLEFT))
                //{
                //    int tempDir = (int)DpadDirections.DownLeft;
                //    usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.PAD_DIR_DOWNRIGHT))
                //{
                //    int tempDir = (int)DpadDirections.DownRight;
                //    usedEventButtonsList[tempDir] = tempDpadAction.eventCodes4[tempDir];
                //}
            }
        }
    }
}
