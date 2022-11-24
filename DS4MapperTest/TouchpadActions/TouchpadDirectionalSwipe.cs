using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;

namespace DS4MapperTest.TouchpadActions
{
    public struct TouchpadDirectionalSwipeParams
    {
        public int deadzoneX;
        public int deadzoneY;
        public int delayTime;
    }

    public class TouchpadDirectionalSwipe : TouchpadMapAction
    {
        public enum SwipeAxisXDir
        {
            Centered,
            Left,
            Right,
        }

        public enum SwipeAxisYDir
        {
            Centered,
            Up,
            Down,
        }

        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE_X = "DeadZoneX";
            public const string DEAD_ZONE_Y = "DeadZoneY";
            public const string DELAY_TIME = "DelayTime";

            public const string PAD_DIR_UP = "DirUp";
            public const string PAD_DIR_DOWN = "DirDown";
            public const string PAD_DIR_LEFT = "DirLeft";
            public const string PAD_DIR_RIGHT = "DirRight";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.DEAD_ZONE_X,
            PropertyKeyStrings.DEAD_ZONE_Y,
            PropertyKeyStrings.DELAY_TIME,

            PropertyKeyStrings.PAD_DIR_UP,
            PropertyKeyStrings.PAD_DIR_DOWN,
            PropertyKeyStrings.PAD_DIR_LEFT,
            PropertyKeyStrings.PAD_DIR_RIGHT,
        };

        private ButtonAction[] usedEventsButtonsX =
           new ButtonAction[3];

        private ButtonAction[] usedEventsButtonsY =
            new ButtonAction[3];

        private Dictionary<ButtonAction, SwipeAxisXDir> tmpActiveBtnsX =
            new Dictionary<ButtonAction, SwipeAxisXDir>();

        private Dictionary<ButtonAction, SwipeAxisYDir> tmpActiveBtnsY =
            new Dictionary<ButtonAction, SwipeAxisYDir>();

        private bool[] useParentDataX = new bool[3];
        public bool[] UseParentDataX => useParentDataX;

        private bool[] useParentDataY = new bool[3];
        public bool[] UseParentDataY => useParentDataY;

        private SwipeAxisXDir previousXDir;
        private SwipeAxisYDir previousYDir;

        private SwipeAxisXDir currentXDir;
        private SwipeAxisYDir currentYDir;

        private DateTime initialTimeX;
        private DateTime initialTimeY;

        private bool xSwipeActive;
        private bool ySwipeActive;

        private List<ButtonAction> removeBtnCandidates = new List<ButtonAction>();

        public TouchpadDirectionalSwipeParams swipeParams;

        public ButtonAction[] UsedEventsButtonsX
        {
            get => usedEventsButtonsX;
        }

        public ButtonAction[] UsedEventsButtonsY
        {
            get => usedEventsButtonsY;
        }

        public TouchpadDirectionalSwipe()
        {
            swipeParams = new TouchpadDirectionalSwipeParams()
            {
                deadzoneX = 250,
                deadzoneY = 250,
                delayTime = 30,
            };

            FillDirectionalButtons();
        }

        private void FillDirectionalButtons()
        {
            usedEventsButtonsX[0] = new ButtonAction();
            usedEventsButtonsX[1] = new ButtonAction();
            usedEventsButtonsX[2] = new ButtonAction();

            usedEventsButtonsY[0] = new ButtonAction();
            usedEventsButtonsY[1] = new ButtonAction();
            usedEventsButtonsY[2] = new ButtonAction();
        }

        public override void Prepare(Mapper mapper, ref TouchEventFrame touchFrame, bool alterState = true)
        {
            if (!touchFrame.Touch)
            {
                currentXDir = SwipeAxisXDir.Centered;
                currentYDir = SwipeAxisYDir.Centered;
                bool changeDirX = previousXDir != currentXDir;
                bool changeDirY = previousYDir != currentYDir;
                if (changeDirX || changeDirY)
                {
                    active = activeEvent = true;
                }
                else
                {
                    active = activeEvent = false;
                }

                return;
            }

            ref TouchEventFrame previousTouchFrame =
                ref mapper.GetPreviousTouchEventFrame(touchpadDefinition.touchCode);

            int dx = previousTouchFrame.X - touchFrame.X;
            if (Math.Abs(dx) > swipeParams.deadzoneX)
            {
                if (dx > 0)
                {
                    currentXDir = SwipeAxisXDir.Right;
                }
                else
                {
                    currentXDir = SwipeAxisXDir.Left;
                }

                if (previousXDir != currentXDir)
                {
                    initialTimeX = DateTime.Now;
                    xSwipeActive = swipeParams.delayTime == 0;
                }

                active = true;
            }
            else
            {
                currentXDir = SwipeAxisXDir.Centered;
                if (previousXDir != currentXDir)
                {
                    active = true;
                }
            }

            int dy = previousTouchFrame.Y - touchFrame.Y;
            if (Math.Abs(dy) > swipeParams.deadzoneY)
            {
                if (dy > 0)
                {
                    currentYDir = SwipeAxisYDir.Up;
                }
                else
                {
                    currentYDir = SwipeAxisYDir.Down;
                }

                if (previousYDir != currentYDir)
                {
                    initialTimeY = DateTime.Now;
                    ySwipeActive = swipeParams.delayTime == 0;
                }

                active = true;
            }
            else
            {
                currentYDir = SwipeAxisYDir.Centered;
                if (previousYDir != currentYDir)
                {
                    active = true;
                }
            }

            activeEvent = true;
        }

        public override void Event(Mapper mapper)
        {
            bool dirXChange = previousXDir != currentXDir;
            bool dirYChange = previousYDir != currentYDir;

            if (dirXChange && previousXDir != SwipeAxisXDir.Centered)
            {
                ButtonAction data = null;
                int dirNum = (int)previousXDir;
                data = usedEventsButtonsX[dirNum];

                if (data != null && data.active)
                {
                    data.Prepare(mapper, false);
                    data.Event(mapper);

                    if (!data.active)
                    {
                        tmpActiveBtnsX.Remove(data);
                    }
                }
            }

            if (dirYChange && previousYDir != SwipeAxisYDir.Centered)
            {
                ButtonAction data = null;
                int dirNum = (int)previousYDir;
                data = usedEventsButtonsY[dirNum];

                if (data != null && data.active)
                {
                    data.Prepare(mapper, false);
                    data.Event(mapper);

                    if (!data.active)
                    {
                        tmpActiveBtnsY.Remove(data);
                    }
                }
            }

            if (currentXDir != SwipeAxisXDir.Centered)
            {
                ButtonAction data = null;
                int dirNum = (int)previousXDir;
                data = usedEventsButtonsX[dirNum];

                if (data != null)
                {
                    bool swipeActive = xSwipeActive ||
                        (xSwipeActive = initialTimeX + TimeSpan.FromMilliseconds(swipeParams.delayTime) < DateTime.Now);

                    if (swipeActive)
                    {
                        data.Prepare(mapper, true);
                        data.Event(mapper);

                        if (dirXChange && !tmpActiveBtnsX.ContainsKey(data))
                        {
                            tmpActiveBtnsX.Add(data, currentXDir);
                        }
                    }
                }
            }
            else if (dirXChange)
            {
                if (tmpActiveBtnsX.Count > 0)
                {
                    foreach (KeyValuePair<ButtonAction, SwipeAxisXDir> pair in tmpActiveBtnsX)
                    {
                        ButtonAction data = pair.Key;
                        if (data != null)
                        {
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
                        tmpActiveBtnsX.Remove(removeBtn);
                    }

                    removeBtnCandidates.Clear();
                }
            }

            if (currentYDir != SwipeAxisYDir.Centered)
            {
                ButtonAction data = null;
                int dirNum = (int)previousYDir;
                data = usedEventsButtonsY[dirNum];

                if (data != null)
                {
                    bool swipeActive = ySwipeActive ||
                        (ySwipeActive = initialTimeY + TimeSpan.FromMilliseconds(swipeParams.delayTime) < DateTime.Now);

                    if (swipeActive)
                    {
                        data.Prepare(mapper, true);
                        data.Event(mapper);

                        if (dirYChange && !tmpActiveBtnsY.ContainsKey(data))
                        {
                            tmpActiveBtnsY.Add(data, currentYDir);
                        }
                    }
                }
            }
            else if (dirYChange)
            {
                if (tmpActiveBtnsY.Count > 0)
                {
                    foreach (KeyValuePair<ButtonAction, SwipeAxisYDir> pair in tmpActiveBtnsY)
                    {
                        ButtonAction data = pair.Key;
                        if (data != null)
                        {
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
                        tmpActiveBtnsY.Remove(removeBtn);
                    }

                    removeBtnCandidates.Clear();
                }
            }

            previousXDir = currentXDir;
            previousYDir = currentYDir;

            active = currentXDir != SwipeAxisXDir.Centered ||
                currentYDir != SwipeAxisYDir.Centered ||
                tmpActiveBtnsX.Count > 0 || tmpActiveBtnsY.Count > 0;

            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            if (active)
            {
                foreach (KeyValuePair<ButtonAction, SwipeAxisXDir> pair in tmpActiveBtnsX)
                {
                    ButtonAction data = pair.Key;
                    if (data != null)
                    {
                        data.Release(mapper, resetState, ignoreReleaseActions);
                    }
                }

                tmpActiveBtnsX.Clear();

                foreach (KeyValuePair<ButtonAction, SwipeAxisYDir> pair in tmpActiveBtnsY)
                {
                    ButtonAction data = pair.Key;
                    if (data != null)
                    {
                        data.Release(mapper, resetState, ignoreReleaseActions);
                    }
                }

                tmpActiveBtnsY.Clear();
            }

            active = false;
            activeEvent = false;
            previousXDir = currentXDir = SwipeAxisXDir.Centered;
            xSwipeActive = ySwipeActive = false;

            if (resetState)
            {
                stateData.Reset();
            }
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            if (active)
            {
                TouchpadDirectionalSwipe swipeAction = checkAction as TouchpadDirectionalSwipe;
                foreach (KeyValuePair<ButtonAction, SwipeAxisXDir> pair in tmpActiveBtnsX)
                {
                    ButtonAction data = pair.Key;
                    SwipeAxisXDir dir = pair.Value;
                    int dirNum = (int)dir;

                    if (data != null && !useParentDataX[dirNum])
                    {
                        data.Release(mapper, resetState);
                    }
                    else if (data != null && swipeAction != null &&
                            swipeAction.usedEventsButtonsX[dirNum] != data)
                    {
                        data.Release(mapper, resetState);
                    }
                }

                tmpActiveBtnsX.Clear();

                foreach (KeyValuePair<ButtonAction, SwipeAxisYDir> pair in tmpActiveBtnsY)
                {
                    ButtonAction data = pair.Key;
                    SwipeAxisYDir dir = pair.Value;
                    int dirNum = (int)dir;

                    if (data != null && !useParentDataY[dirNum])
                    {
                        data.Release(mapper, resetState);
                    }
                    else if (data != null && swipeAction != null &&
                            swipeAction.usedEventsButtonsY[dirNum] != data)
                    {
                        data.Release(mapper, resetState);
                    }
                }

                tmpActiveBtnsY.Clear();
            }

            active = false;
            activeEvent = false;
            previousXDir = currentXDir = SwipeAxisXDir.Centered;
            xSwipeActive = ySwipeActive = false;

            if (resetState)
            {
                stateData.Reset();
            }
        }

        public override void SoftCopyFromParent(TouchpadMapAction parentAction)
        {
            if (parentAction is TouchpadDirectionalSwipe tempSwipeAction)
            {
                base.SoftCopyFromParent(parentAction);

                tempSwipeAction.NotifyPropertyChanged += TempSwipeAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch (parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempSwipeAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE_X:
                            swipeParams.deadzoneX = tempSwipeAction.swipeParams.deadzoneX;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE_Y:
                            swipeParams.deadzoneY = tempSwipeAction.swipeParams.deadzoneY;
                            break;
                        case PropertyKeyStrings.DELAY_TIME:
                            swipeParams.delayTime = tempSwipeAction.swipeParams.delayTime;
                            break;
                        case PropertyKeyStrings.PAD_DIR_UP:
                            {
                                usedEventsButtonsY[(int)SwipeAxisYDir.Up] =
                                    tempSwipeAction.usedEventsButtonsY[(int)SwipeAxisYDir.Up];
                                useParentDataY[(int)SwipeAxisYDir.Up] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_DOWN:
                            {
                                usedEventsButtonsY[(int)SwipeAxisYDir.Down] =
                                    tempSwipeAction.usedEventsButtonsY[(int)SwipeAxisYDir.Down];
                                useParentDataY[(int)SwipeAxisYDir.Down] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_LEFT:
                            {
                                usedEventsButtonsX[(int)SwipeAxisXDir.Left] =
                                    tempSwipeAction.usedEventsButtonsY[(int)SwipeAxisXDir.Left];
                                useParentDataX[(int)SwipeAxisXDir.Left] = true;
                            }

                            break;
                        case PropertyKeyStrings.PAD_DIR_RIGHT:
                            {
                                usedEventsButtonsX[(int)SwipeAxisXDir.Right] =
                                    tempSwipeAction.usedEventsButtonsY[(int)SwipeAxisXDir.Right];
                                useParentDataX[(int)SwipeAxisXDir.Right] = true;
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempSwipeAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
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

            TouchpadDirectionalSwipe tempSwipeAction = parentAction as TouchpadDirectionalSwipe;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempSwipeAction.name;
                    break;
                case PropertyKeyStrings.DEAD_ZONE_X:
                    swipeParams.deadzoneX = tempSwipeAction.swipeParams.deadzoneX;
                    break;
                case PropertyKeyStrings.DEAD_ZONE_Y:
                    swipeParams.deadzoneY = tempSwipeAction.swipeParams.deadzoneY;
                    break;
                case PropertyKeyStrings.DELAY_TIME:
                    swipeParams.delayTime = tempSwipeAction.swipeParams.delayTime;
                    break;
                case PropertyKeyStrings.PAD_DIR_UP:
                    {
                        usedEventsButtonsY[(int)SwipeAxisYDir.Up] =
                            tempSwipeAction.usedEventsButtonsY[(int)SwipeAxisYDir.Up];
                        useParentDataY[(int)SwipeAxisYDir.Up] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_DOWN:
                    {
                        usedEventsButtonsY[(int)SwipeAxisYDir.Down] =
                            tempSwipeAction.usedEventsButtonsY[(int)SwipeAxisYDir.Down];
                        useParentDataY[(int)SwipeAxisYDir.Down] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_LEFT:
                    {
                        usedEventsButtonsX[(int)SwipeAxisXDir.Left] =
                            tempSwipeAction.usedEventsButtonsY[(int)SwipeAxisXDir.Left];
                        useParentDataX[(int)SwipeAxisXDir.Left] = true;
                    }

                    break;
                case PropertyKeyStrings.PAD_DIR_RIGHT:
                    {
                        usedEventsButtonsX[(int)SwipeAxisXDir.Right] =
                            tempSwipeAction.usedEventsButtonsY[(int)SwipeAxisXDir.Right];
                        useParentDataX[(int)SwipeAxisXDir.Right] = true;
                    }

                    break;
                default:
                    break;
            }
        }
    }
}
