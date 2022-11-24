using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.StickModifiers;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.ActionUtil;

namespace DS4MapperTest.GyroActions
{
    public struct GyroPadParams
    {
        public JoypadActionCodes[] gyroTriggerButtons;
        public bool andCond;
        public bool triggerActivates;
    }

    public class GyroPadAction : GyroMapAction
    {
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

        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string PAD_MODE = "PadMode";

            public const string PAD_DIR_UP = "DirUp";
            public const string PAD_DIR_DOWN = "DirDown";
            public const string PAD_DIR_LEFT = "DirLeft";
            public const string PAD_DIR_RIGHT = "DirRight";

            public const string PAD_DIR_UPLEFT = "DirUpLeft";
            public const string PAD_DIR_UPRIGHT = "DirUpRight";
            public const string PAD_DIR_DOWNLEFT = "DirDownLeft";
            public const string PAD_DIR_DOWNRIGHT = "DirDownRight";

            public const string TRIGGER_BUTTONS = "Triggers";
            public const string TRIGGER_ACTIVATE = "TriggersActivate";
            public const string TRIGGER_EVAL_COND = "TriggersEvalCond";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.PAD_MODE,
            PropertyKeyStrings.PAD_DIR_UP,
            PropertyKeyStrings.PAD_DIR_DOWN,
            PropertyKeyStrings.PAD_DIR_LEFT,
            PropertyKeyStrings.PAD_DIR_RIGHT,
            PropertyKeyStrings.PAD_DIR_UPLEFT,
            PropertyKeyStrings.PAD_DIR_UPRIGHT,
            PropertyKeyStrings.PAD_DIR_DOWNLEFT,
            PropertyKeyStrings.PAD_DIR_DOWNRIGHT,
            PropertyKeyStrings.TRIGGER_BUTTONS,
            PropertyKeyStrings.TRIGGER_ACTIVATE,
            PropertyKeyStrings.TRIGGER_EVAL_COND,
        };

        private const int MAX_ACCEL_LEAN_X = 16384;
        private const int MIN_ACCEL_LEAN_X = -16384;
        private const int MAX_ACCEL_LEAN_Y = MAX_ACCEL_LEAN_X;
        private const int MIN_ACCEL_LEAN_Y = MIN_ACCEL_LEAN_X;

        public const string ACTION_TYPE_NAME = "GyroPadAction";

        private StickDeadZone deadMod;
        public StickDeadZone DeadMod { get => deadMod; }

        private AxisDirButton[] eventCodes4 = new AxisDirButton[13];
        public AxisDirButton[] EventCodes4 { get => eventCodes4; }
        private AxisDirButton[] tmpBtnActions = new AxisDirButton[2];

        private DpadDirections currentDir;
        private DpadDirections previousDir;

        private DPadMode currentMode = DPadMode.Standard;
        public DPadMode CurrentMode { get => currentMode; set => currentMode = value; }
        public GyroPadParams padParams;

        private Dictionary<AxisDirButton, DpadDirections> tmpActiveBtns = new Dictionary<AxisDirButton, DpadDirections>();
        private int[] tmpBtnDirs = new int[2];
        private List<AxisDirButton> removeBtnCandidates = new List<AxisDirButton>();
        private bool[] useParentDataDraft2 = new bool[13];

        private double xNorm = 0.0, yNorm = 0.0;
        private double prevXNorm = 0.0, prevYNorm = 0.0;

        public GyroPadAction()
        {
            actionTypeName = ACTION_TYPE_NAME;
            deadMod = new StickDeadZone(0.30, 0.30, 0.9, 0.9, 0.0, 0.0);
            deadMod.CircleDead = false;
        }

        public GyroPadAction(GyroPadParams padParams)
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.padParams = padParams;
            deadMod = new StickDeadZone(0.30, 0.30, 0.9, 0.9, 0.0, 0.0);
            deadMod.CircleDead = false;
        }

        public GyroPadAction(GyroPadAction parentAction)
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.parentAction = parentAction;
            this.padParams = parentAction.padParams;
        }

        public override void Prepare(Mapper mapper, ref GyroEventFrame gyroFrame, bool alterState = true)
        {
            xNorm = 0.0; yNorm = 0.0;
            //Console.WriteLine("{0} {1}", gyroFrame.AccelX, gyroFrame.AccelY);

            JoypadActionCodes[] tempTriggerButtons = padParams.gyroTriggerButtons;
            //if (tempTriggerButtons != JoypadActionCodes.Empty)
            {
                bool triggerButtonActive = mapper.IsButtonsActiveDraft(padParams.gyroTriggerButtons, padParams.andCond);
                if (!padParams.triggerActivates && triggerButtonActive)
                {
                    prevXNorm = xNorm; prevYNorm = yNorm;
                    xNorm = yNorm = 0.0;
                    currentDir = DpadDirections.Centered;
                    active = true;
                    activeEvent = true;
                    return;
                }
                else if (padParams.triggerActivates && !triggerButtonActive)
                {
                    prevXNorm = xNorm; prevYNorm = yNorm;
                    xNorm = yNorm = 0.0;
                    currentDir = DpadDirections.Centered;
                    active = true;
                    activeEvent = true;
                    return;
                }
            }

            int axisMid = 0;
            int axisXDir = gyroFrame.AccelX - axisMid, axisYDir = gyroFrame.AccelY - axisMid;

            bool xNegative = gyroFrame.AccelX < 0;
            bool yNegative = gyroFrame.AccelY < 0;
            int maxDirX = (!xNegative ? gyroSensDefinition.accelMaxLeanX : gyroSensDefinition.accelMinLeanX) - axisMid;
            int maxDirY = (!yNegative ? gyroSensDefinition.accelMaxLeanY : gyroSensDefinition.accelMinLeanY) - axisMid;
            bool inSafeZone;
            deadMod.CalcOutValues(axisXDir, axisYDir, maxDirX,
                    maxDirY, out xNorm, out yNorm);
            inSafeZone = deadMod.inSafeZone;

            if (inSafeZone)
            {
                DetermineDirection();
            }
            else
            {
                currentDir = DpadDirections.Centered;
            }

            active = true;
            activeEvent = true;
        }

        public override void Event(Mapper mapper)
        {
            int previousDirNum = (int)previousDir;
            //Console.WriteLine("DIRS: Previous: {0} Current: {1}", previousDir, currentDir);
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
                            data = eventCodes4[previousDirNum];
                        }
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        if (previousDir != currentDir)
                        {
                            data = eventCodes4[previousDirNum];
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
                            tmpBtnActions[i] = eventCodes4[tempDir]; i++;
                        }
                        else if ((previousDir & DpadDirections.Down) != 0 && (currentDir & DpadDirections.Down) == 0)
                        {
                            tempDir = (int)DpadDirections.Down;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = eventCodes4[tempDir]; i++;
                        }

                        if ((previousDir & DpadDirections.Left) != 0 && (currentDir & DpadDirections.Left) == 0)
                        {
                            tempDir = (int)DpadDirections.Left;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = eventCodes4[tempDir]; i++;
                        }
                        else if ((previousDir & DpadDirections.Right) != 0 && (currentDir & DpadDirections.Right) == 0)
                        {
                            tempDir = (int)DpadDirections.Right;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnActions[i] = eventCodes4[tempDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        if (previousDir != currentDir)
                        {
                            //tmpCodes[i] = eventCodes[(int)previousDir]; i++;
                            tmpBtnActions[i] = eventCodes4[(int)previousDir]; i++;
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
                        data = eventCodes4[currentDirNum];
                        //}
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        //if (currentDir != previousDir)
                        //{
                        data = eventCodes4[currentDirNum];
                        //}
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
                    tmpBtnDirs[0] = tmpBtnDirs[1] = (int)DpadDirections.Centered;
                    int i = 0;
                    AxisDirButton data = null;

                    if (currentMode == DPadMode.Standard)
                    {
                        if ((currentDir & DpadDirections.Up) != 0)// && (previousDir & DpadDirections.Up) == 0)
                        {
                            tempDir = (int)DpadDirections.Up;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = eventCodes4[tempDir]; i++;
                        }
                        else if ((currentDir & DpadDirections.Down) != 0)// && (previousDir & DpadDirections.Down) == 0)
                        {
                            tempDir = (int)DpadDirections.Down;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = eventCodes4[tempDir]; i++;
                        }

                        if ((currentDir & DpadDirections.Left) != 0)// && (previousDir & DpadDirections.Left) == 0)
                        {
                            tempDir = (int)DpadDirections.Left;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = eventCodes4[tempDir]; i++;
                        }
                        else if ((currentDir & DpadDirections.Right) != 0)// && (previousDir & DpadDirections.Right) == 0)
                        {
                            tempDir = (int)DpadDirections.Right;
                            //tmpCodes[i] = eventCodes[tempDir]; i++;
                            tmpBtnDirs[i] = tempDir;
                            tmpBtnActions[i] = eventCodes4[tempDir]; i++;
                        }
                    }
                    else if (currentMode == DPadMode.EightWay)
                    {
                        //if (currentDir != previousDir)
                        //{
                        //tmpCodes[i] = eventCodes[(int)currentDir]; i++;
                        tmpBtnDirs[i] = currentDirNum;
                        tmpBtnActions[i] = eventCodes4[(int)currentDir]; i++;
                        //}
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
                        //data.Prepare(mapper, status);
                        data.Prepare(mapper, false);
                        data.Event(mapper);

                        if (!data.active)
                        {
                            removeBtnCandidates.Add(data);
                        }
                    }
                }

                foreach (AxisDirButton removeBtn in removeBtnCandidates)
                {
                    tmpActiveBtns.Remove(removeBtn);
                }

                removeBtnCandidates.Clear();
            }
            /*else
            {
                AxisDirButton data = null;
                if (tmpBtnActions[0] != null)
                {
                    data = tmpBtnActions[0];
                    data.PrepareAnalog(mapper, 0.0);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[0], false);
                }

                if (tmpBtnActions[1] != null)
                {
                    data = tmpBtnActions[1];
                    data.PrepareAnalog(mapper, 0.0);
                    data.Event(mapper);
                    //mapper.RunEventFromButton(tmpCodes[1], false);
                }
            }
            */

            prevXNorm = xNorm; prevYNorm = yNorm;
            previousDir = currentDir;
            active = currentDir != DpadDirections.Centered;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            if (tmpActiveBtns.Count > 0)
            {
                foreach (KeyValuePair<AxisDirButton, DpadDirections> pair in tmpActiveBtns)
                {
                    AxisDirButton data = pair.Key;
                    DpadDirections dir = pair.Value;

                    if (data != null)
                    {
                        data.Release(mapper, resetState, ignoreReleaseActions);
                    }
                }

                tmpActiveBtns.Clear();
            }

            if (resetState)
            {
                stateData.Reset();
            }

            currentDir = DpadDirections.Centered;
            previousDir = currentDir;
            active = false;
            activeEvent = false;
            //inputStatus = false;
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            if (tmpActiveBtns.Count > 0)
            {
                GyroPadAction gyroPadAction = checkAction as GyroPadAction;
                foreach (KeyValuePair<AxisDirButton, DpadDirections> pair in tmpActiveBtns)
                {
                    AxisDirButton data = pair.Key;
                    DpadDirections dir = pair.Value;
                    int dirNum = (int)dir;

                    if (data != null && !useParentDataDraft2[dirNum])
                    {
                        data.Release(mapper, resetState);
                    }
                    else if (data != null && gyroPadAction != null &&
                            gyroPadAction.eventCodes4[dirNum] != data)
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
            //inputStatus = false;

            if (resetState)
            {
                stateData.Reset();
            }
        }

        public void ReleaseOld(Mapper mapper, bool resetState = true)
        {
            if (currentDir != DpadDirections.Centered)
            {
                int currentDirNum = (int)currentDir;
                if (currentDirNum % 3 != 0)
                {
                    //int code = eventCodes[currentDirNum];
                    //OutputActionData data = eventCodes[currentDirNum];
                    AxisDirButton data = eventCodes4[currentDirNum];
                    if (data != null)
                    {
                        data.PrepareAnalog(mapper, 0.0, 0.0);
                        data.Event(mapper);
                        //mapper.RunEventFromButton(data, false);
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
                        AxisDirButton data = eventCodes4[currentDirNum];
                        if (data != null)
                        {
                            data.PrepareAnalog(mapper, 0.0, 0.0);
                            data.Event(mapper);
                        }
                        //OutputActionData data = eventCodes[currentDirNum];
                        //mapper.RunEventFromButton(data, false);
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

        public override void BlankEvent(Mapper mapper)
        {
            Release(mapper);
        }

        private unsafe void ReleaseStandardDiagonalEvents(Mapper mapper)
        {
            int tempDir;
            //int* codes = stackalloc int[2];
            //tmpCodes[0] = tmpCodes[1] = null;
            tmpBtnActions[0] = tmpBtnActions[1] = null;
            int i = 0;
            if ((currentDir & DpadDirections.Up) != 0)
            {
                tempDir = (int)DpadDirections.Up;
                //tmpCodes[i] = eventCodes[tempDir]; i++;
                tmpBtnActions[i] = eventCodes4[tempDir]; i++;
            }
            else if ((currentDir & DpadDirections.Down) != 0)
            {
                tempDir = (int)DpadDirections.Down;
                //tmpCodes[i] = eventCodes[tempDir]; i++;
                tmpBtnActions[i] = eventCodes4[tempDir]; i++;
            }

            if ((currentDir & DpadDirections.Left) != 0)
            {
                tempDir = (int)DpadDirections.Left;
                //tmpCodes[i] = eventCodes[tempDir]; i++;
                tmpBtnActions[i] = eventCodes4[tempDir]; i++;
            }
            else if ((currentDir & DpadDirections.Right) != 0)
            {
                tempDir = (int)DpadDirections.Right;
                //tmpCodes[i] = eventCodes[tempDir]; i++;
                tmpBtnActions[i] = eventCodes4[tempDir]; i++;
            }

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

            //if (tmpCodes[1] != null)
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

        private void DetermineDirection()
        {
            if (xNorm == 0.0 && yNorm == 0.0)
            {
                currentDir = DpadDirections.Centered;
            }
            else
            {
                const double CARDINAL_RANGE = 45.0;
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

                double angleRad = Math.Atan2(xNorm, yNorm);
                double angle = (angleRad >= 0 ? angleRad : (2 * Math.PI + angleRad)) * 180 / Math.PI;

                //Console.WriteLine(angle);
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

            //Console.WriteLine(currentDir);
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

        public override GyroMapAction DuplicateAction()
        {
            return new GyroPadAction(this);
        }

        public override void SoftCopyFromParent(GyroMapAction parentAction)
        {
            if (parentAction is GyroPadAction tempGyroPadAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                tempGyroPadAction.hasLayeredAction = true;
                mappingId = tempGyroPadAction.mappingId;

                tempGyroPadAction.NotifyPropertyChanged += TempGyroPadAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch (parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempGyroPadAction.name;
                            break;
                        case PropertyKeyStrings.PAD_MODE:
                            currentMode = tempGyroPadAction.CurrentMode;
                            break;
                        case PropertyKeyStrings.PAD_DIR_UP:
                            {
                                int tempDir = (int)DpadDirections.Up;
                                eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWN:
                            {
                                int tempDir = (int)DpadDirections.Down;
                                eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_LEFT:
                            {
                                int tempDir = (int)DpadDirections.Left;
                                eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_RIGHT:
                            {
                                int tempDir = (int)DpadDirections.Right;
                                eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_UPLEFT:
                            {
                                int tempDir = (int)DpadDirections.UpLeft;
                                eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_UPRIGHT:
                            {
                                int tempDir = (int)DpadDirections.UpRight;
                                eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWNLEFT:
                            {
                                int tempDir = (int)DpadDirections.DownLeft;
                                eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWNRIGHT:
                            {
                                int tempDir = (int)DpadDirections.DownRight;
                                eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                                useParentDataDraft2[tempDir] = true;
                            }

                            break;
                        case PropertyKeyStrings.TRIGGER_BUTTONS:
                            padParams.gyroTriggerButtons = tempGyroPadAction.padParams.gyroTriggerButtons;
                            break;
                        case PropertyKeyStrings.TRIGGER_ACTIVATE:
                            padParams.triggerActivates = tempGyroPadAction.padParams.triggerActivates;
                            break;
                        case PropertyKeyStrings.TRIGGER_EVAL_COND:
                            padParams.andCond = tempGyroPadAction.padParams.andCond;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempGyroPadAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
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

            GyroPadAction tempGyroPadAction = parentAction as GyroPadAction;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempGyroPadAction.name;
                    break;
                case PropertyKeyStrings.PAD_MODE:
                    currentMode = tempGyroPadAction.CurrentMode;
                    break;
                case PropertyKeyStrings.PAD_DIR_UP:
                    {
                        int tempDir = (int)DpadDirections.Up;
                        eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_DOWN:
                    {
                        int tempDir = (int)DpadDirections.Down;
                        eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_LEFT:
                    {
                        int tempDir = (int)DpadDirections.Left;
                        eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_RIGHT:
                    {
                        int tempDir = (int)DpadDirections.Right;
                        eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_UPLEFT:
                    {
                        int tempDir = (int)DpadDirections.UpLeft;
                        eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_UPRIGHT:
                    {
                        int tempDir = (int)DpadDirections.UpRight;
                        eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_DOWNLEFT:
                    {
                        int tempDir = (int)DpadDirections.DownLeft;
                        eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_DOWNRIGHT:
                    {
                        int tempDir = (int)DpadDirections.DownRight;
                        eventCodes4[tempDir] = tempGyroPadAction.eventCodes4[tempDir];
                        useParentDataDraft2[tempDir] = true;
                    }

                    break;
                case PropertyKeyStrings.TRIGGER_BUTTONS:
                    padParams.gyroTriggerButtons = tempGyroPadAction.padParams.gyroTriggerButtons;
                    break;
                case PropertyKeyStrings.TRIGGER_ACTIVATE:
                    padParams.triggerActivates = tempGyroPadAction.padParams.triggerActivates;
                    break;
                case PropertyKeyStrings.TRIGGER_EVAL_COND:
                    padParams.andCond = tempGyroPadAction.padParams.andCond;
                    break;
                default:
                    break;
            }
        }
    }
}
