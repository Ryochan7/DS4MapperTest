using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.ViewModels.TouchpadActionPropViewModels
{
    public class TouchpadDirSwipePropViewModel : TouchpadActionPropVMBase
    {
        private TouchpadDirectionalSwipe action;
        public TouchpadDirectionalSwipe Action
        {
            get => action;
        }

        public int DeadZoneX
        {
            get => action.swipeParams.deadzoneX;
            set
            {
                {
                    action.swipeParams.deadzoneX = Math.Clamp(value, 0, 10000);
                    DeadZoneXChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler DeadZoneXChanged;

        public int DeadZoneY
        {
            get => action.swipeParams.deadzoneY;
            set
            {
                {
                    action.swipeParams.deadzoneY = Math.Clamp(value, 0, 10000);
                    DeadZoneYChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler DeadZoneYChanged;

        public int DelayTime
        {
            get => action.swipeParams.delayTime;
            set
            {
                action.swipeParams.delayTime = Math.Clamp(value, 0, 10000);
                DelayTimeChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DelayTimeChanged;

        public string ActionUpBtnDisplayBind
        {
            get => action.UsedEventsButtonsY[(int)TouchpadDirectionalSwipe.SwipeAxisYDir.Up].DescribeActions(mapper);
        }

        public string ActionDownBtnDisplayBind
        {
            get => action.UsedEventsButtonsY[(int)TouchpadDirectionalSwipe.SwipeAxisYDir.Down].DescribeActions(mapper);
        }

        public string ActionLeftBtnDisplayBind
        {
            get => action.UsedEventsButtonsX[(int)TouchpadDirectionalSwipe.SwipeAxisXDir.Left].DescribeActions(mapper);
        }

        public string ActionRightBtnDisplayBind
        {
            get => action.UsedEventsButtonsX[(int)TouchpadDirectionalSwipe.SwipeAxisXDir.Right].DescribeActions(mapper);
        }

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadDirectionalSwipe.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZoneX
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_X);
        }
        public event EventHandler HighlightDeadZoneXChanged;

        public bool HighlightDeadZoneY
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_Y);
        }
        public event EventHandler HighlightDeadZoneYChanged;

        public bool HighlightDelayTime
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadDirectionalSwipe.PropertyKeyStrings.DELAY_TIME);
        }
        public event EventHandler HighlightDelayTimeChanged;

        public override event EventHandler ActionPropertyChanged;

        public TouchpadDirSwipePropViewModel(Mapper mapper, TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TouchpadDirectionalSwipe;
            this.baseAction = action;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TouchpadDirectionalSwipe baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TouchpadDirectionalSwipe;
                TouchpadDirectionalSwipe tempAction = new TouchpadDirectionalSwipe();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
                this.baseAction = this.action;
                usingRealAction = false;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }


            PrepareModel();

            NameChanged += TouchpadDirSwipePropViewModel_NameChanged;
            DeadZoneXChanged += TouchpadDirSwipePropViewModel_DeadZoneXChanged;
            DeadZoneYChanged += TouchpadDirSwipePropViewModel_DeadZoneYChanged;
            DelayTimeChanged += TouchpadDirSwipePropViewModel_DelayTimeChanged;
        }

        private void TouchpadDirSwipePropViewModel_DelayTimeChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadDirectionalSwipe.PropertyKeyStrings.DELAY_TIME))
            {
                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.DELAY_TIME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadDirectionalSwipe.PropertyKeyStrings.DELAY_TIME);
            HighlightDelayTimeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadDirSwipePropViewModel_DeadZoneYChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_Y))
            {
                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_Y);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_Y);
            HighlightDeadZoneYChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadDirSwipePropViewModel_DeadZoneXChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_X))
            {
                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_X);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_X);
            HighlightDeadZoneXChanged?.Invoke(this, EventArgs.Empty);
        }

        public void PrepareModel()
        {

        }

        private void TouchpadDirSwipePropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadDirectionalSwipe.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadDirectionalSwipe.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateUpDirButton(ButtonAction oldAction, ButtonAction newAction)
        {
            if (!usingRealAction)
            {
                ReplaceExistingLayerAction(this, EventArgs.Empty);
            }

            ExecuteInMapperThread(() =>
            {
                if (oldAction != null)
                {
                    oldAction.Release(mapper, ignoreReleaseActions: true);
                    action.UsedEventsButtonsY[(int)TouchpadDirectionalSwipe.SwipeAxisYDir.Up] = newAction;
                }

                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.PAD_DIR_UP);
                action.UseParentDataY[(int)TouchpadDirectionalSwipe.SwipeAxisYDir.Up] = false;
                action.RaiseNotifyPropertyChange(mapper, TouchpadDirectionalSwipe.PropertyKeyStrings.PAD_DIR_UP);
            });
        }

        public void UpdateDownDirButton(ButtonAction oldAction, ButtonAction newAction)
        {
            if (!usingRealAction)
            {
                ReplaceExistingLayerAction(this, EventArgs.Empty);
            }

            ExecuteInMapperThread(() =>
            {
                if (oldAction != null)
                {
                    oldAction.Release(mapper, ignoreReleaseActions: true);
                    action.UsedEventsButtonsY[(int)TouchpadDirectionalSwipe.SwipeAxisYDir.Down] = newAction;
                }

                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.PAD_DIR_DOWN);
                action.UseParentDataY[(int)TouchpadDirectionalSwipe.SwipeAxisYDir.Down] = false;
                action.RaiseNotifyPropertyChange(mapper, TouchpadDirectionalSwipe.PropertyKeyStrings.PAD_DIR_DOWN);
            });
        }

        public void UpdateLeftDirButton(ButtonAction oldAction, ButtonAction newAction)
        {
            if (!usingRealAction)
            {
                ReplaceExistingLayerAction(this, EventArgs.Empty);
            }

            ExecuteInMapperThread(() =>
            {
                if (oldAction != null)
                {
                    oldAction.Release(mapper, ignoreReleaseActions: true);
                    action.UsedEventsButtonsX[(int)TouchpadDirectionalSwipe.SwipeAxisXDir.Left] = newAction;
                }

                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.PAD_DIR_LEFT);
                action.UseParentDataX[(int)TouchpadDirectionalSwipe.SwipeAxisXDir.Left] = false;
                action.RaiseNotifyPropertyChange(mapper, TouchpadDirectionalSwipe.PropertyKeyStrings.PAD_DIR_LEFT);
            });
        }

        public void UpdateRightDirButton(ButtonAction oldAction, ButtonAction newAction)
        {
            if (!usingRealAction)
            {
                ReplaceExistingLayerAction(this, EventArgs.Empty);
            }

            ExecuteInMapperThread(() =>
            {
                if (oldAction != null)
                {
                    oldAction.Release(mapper, ignoreReleaseActions: true);
                    action.UsedEventsButtonsX[(int)TouchpadDirectionalSwipe.SwipeAxisXDir.Right] = newAction;
                }

                action.ChangedProperties.Add(TouchpadDirectionalSwipe.PropertyKeyStrings.PAD_DIR_RIGHT);
                action.UseParentDataX[(int)TouchpadDirectionalSwipe.SwipeAxisXDir.Right] = false;
                action.RaiseNotifyPropertyChange(mapper, TouchpadDirectionalSwipe.PropertyKeyStrings.PAD_DIR_RIGHT);
            });
        }
    }
}
