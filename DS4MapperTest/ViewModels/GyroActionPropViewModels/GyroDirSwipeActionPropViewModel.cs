using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ViewModels.Common;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ViewModels.GyroActionPropViewModels
{
    public class GyroDirSwipeActionPropViewModel : GyroActionPropVMBase
    {
        protected GyroDirectionalSwipe action;
        public GyroDirectionalSwipe Action
        {
            get => action;
        }

        private List<GyroTriggerButtonItem> triggerButtonItems;
        public List<GyroTriggerButtonItem> TriggerButtonItems => triggerButtonItems;

        public string GyroTriggerString
        {
            get
            {
                List<string> tempList = new List<string>();
                foreach (JoypadActionCodes code in action.swipeParams.gyroTriggerButtons)
                {
                    GyroTriggerButtonItem tempItem =
                        triggerButtonItems.Find((item) => item.Code == code);

                    if (tempItem != null)
                    {
                        tempList.Add(tempItem.DisplayString);
                    }
                }

                if (tempList.Count == 0)
                {
                    tempList.Add(DEFAULT_EMPTY_TRIGGER_STR);
                }

                string result = string.Join(", ", tempList);
                return result;
            }
        }
        public event EventHandler GyroTriggerStringChanged;

        public bool GyroTriggerCondChoice
        {
            get => action.swipeParams.andCond;
            set
            {
                if (action.swipeParams.andCond == value) return;
                action.swipeParams.andCond = value;
                GyroTriggerCondChoiceChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler GyroTriggerCondChoiceChanged;

        public bool GyroTriggerActivates
        {
            get => action.swipeParams.triggerActivates;
            set
            {
                if (action.swipeParams.triggerActivates == value) return;
                action.swipeParams.triggerActivates = value;
                GyroTriggerActivatesChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler GyroTriggerActivatesChanged;

        public double DeadZoneX
        {
            get => action.swipeParams.deadzoneX;
            set
            {
                {
                    action.swipeParams.deadzoneX = Math.Clamp(value, 0.0, 4000.0);
                    DeadZoneXChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler DeadZoneXChanged;

        public double DeadZoneY
        {
            get => action.swipeParams.deadzoneY;
            set
            {
                {
                    action.swipeParams.deadzoneY = Math.Clamp(value, 0.0, 4000.0);
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
            get => action.UsedEventsButtonsY[(int)GyroDirectionalSwipe.SwipeAxisYDir.Up].DescribeActions(mapper);
        }

        public string ActionDownBtnDisplayBind
        {
            get => action.UsedEventsButtonsY[(int)GyroDirectionalSwipe.SwipeAxisYDir.Down].DescribeActions(mapper);
        }

        public string ActionLeftBtnDisplayBind
        {
            get => action.UsedEventsButtonsX[(int)GyroDirectionalSwipe.SwipeAxisXDir.Left].DescribeActions(mapper);
        }

        public string ActionRightBtnDisplayBind
        {
            get => action.UsedEventsButtonsX[(int)GyroDirectionalSwipe.SwipeAxisXDir.Right].DescribeActions(mapper);
        }

        public bool HighlightName
        {
            get => baseAction.ParentAction == null ||
                baseAction.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZoneX
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_X);
        }
        public event EventHandler HighlightDeadZoneXChanged;

        public bool HighlightDeadZoneY
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_Y);
        }
        public event EventHandler HighlightDeadZoneYChanged;

        public bool HighlightDelayTime
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.DELAY_TIME);
        }
        public event EventHandler HighlightDelayTimeChanged;

        public bool HighlightGyroTriggerCond
        {
            get => action.ParentAction == null ||
                baseAction.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_EVAL_COND);
        }
        public event EventHandler HighlightGyroTriggerCondChanged;

        public bool HighlightGyroTriggerActivates
        {
            get => action.ParentAction == null ||
                baseAction.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_ACTIVATE);
        }
        public event EventHandler HighlightGyroTriggerActivatesChanged;

        public bool HighlightGyroTriggers
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_BUTTONS);
        }
        public event EventHandler HighlightGyroTriggersChanged;

        public override event EventHandler ActionPropertyChanged;

        public GyroDirSwipeActionPropViewModel(Mapper mapper, GyroMapAction action)
        {
            this.mapper = mapper;
            this.action = action as GyroDirectionalSwipe;
            this.baseAction = action;
            triggerButtonItems = new List<GyroTriggerButtonItem>();

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                GyroDirectionalSwipe baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as GyroDirectionalSwipe;
                GyroDirectionalSwipe tempAction = new GyroDirectionalSwipe();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.EditLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
                this.baseAction = tempAction;
                usingRealAction = false;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            PopulateModel();

            NameChanged += GyroDirSwipeActionPropViewModel_NameChanged;
            DeadZoneXChanged += GyroDirSwipeActionPropViewModel_DeadZoneXChanged;
            DeadZoneYChanged += GyroDirSwipeActionPropViewModel_DeadZoneYChanged;
            DelayTimeChanged += GyroDirSwipeActionPropViewModel_DelayTimeChanged;
            GyroTriggerCondChoiceChanged += GyroDirSwipeActionPropViewModel_GyroTriggerCondChoiceChanged;
            GyroTriggerActivatesChanged += GyroDirSwipeActionPropViewModel_TriggerActivatesChanged;
        }

        private void GyroDirSwipeActionPropViewModel_GyroTriggerCondChoiceChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_EVAL_COND))
            {
                this.action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_EVAL_COND);
            }

            action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_EVAL_COND);
            HighlightGyroTriggerCondChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroDirSwipeActionPropViewModel_TriggerActivatesChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_ACTIVATE))
            {
                action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_ACTIVATE);
            }

            action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_ACTIVATE);
            HighlightGyroTriggerActivatesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroDirSwipeActionPropViewModel_DelayTimeChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.DELAY_TIME))
            {
                action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.DELAY_TIME);
            }

            action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.DELAY_TIME);
            HighlightDelayTimeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroDirSwipeActionPropViewModel_DeadZoneYChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_Y))
            {
                action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_Y);
            }

            action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_Y);
            HighlightDeadZoneYChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroDirSwipeActionPropViewModel_DeadZoneXChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_X))
            {
                action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_X);
            }

            action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.DEAD_ZONE_X);
            HighlightDeadZoneXChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroDirSwipeActionPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PopulateModel()
        {
            foreach (ActionTriggerItem item in mapper.ActionTriggerItems)
            {
                triggerButtonItems.Add(new GyroTriggerButtonItem(item.DisplayName, item.Code));
            }

            foreach (JoypadActionCodes code in action.swipeParams.gyroTriggerButtons)
            {
                GyroTriggerButtonItem tempItem = triggerButtonItems.Find((item) => item.Code == code);
                if (tempItem != null)
                {
                    tempItem.Enabled = true;
                }
            }

            triggerButtonItems.ForEach((item) =>
            {
                item.EnabledChanged += GyroTriggerItem_EnabledChanged;
            });
        }

        private void GyroTriggerItem_EnabledChanged(object sender, EventArgs e)
        {
            GyroTriggerButtonItem tempItem = sender as GyroTriggerButtonItem;

            // Convert current array to temp List for convenience
            List<JoypadActionCodes> tempList = action.swipeParams.gyroTriggerButtons.ToList();

            // Add or remove code based on current enabled status
            if (tempItem.Enabled)
            {
                tempList.Add(tempItem.Code);
            }
            else
            {
                tempList.Remove(tempItem.Code);
            }

            if (!action.ChangedProperties.Contains(GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_BUTTONS))
            {
                action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_BUTTONS);
            }

            ExecuteInMapperThread(() =>
            {
                // Convert to array and save to action
                action.swipeParams.gyroTriggerButtons = tempList.ToArray();
                action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.TRIGGER_BUTTONS);
            });

            HighlightGyroTriggersChanged?.Invoke(this, EventArgs.Empty);
            GyroTriggerStringChanged?.Invoke(this, EventArgs.Empty);
            ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
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
                    action.UsedEventsButtonsY[(int)GyroDirectionalSwipe.SwipeAxisYDir.Up] = newAction;
                }

                action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.PAD_DIR_UP);
                action.UseParentDataY[(int)GyroDirectionalSwipe.SwipeAxisYDir.Up] = false;
                action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.PAD_DIR_UP);
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
                    action.UsedEventsButtonsY[(int)GyroDirectionalSwipe.SwipeAxisYDir.Down] = newAction;
                }

                action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.PAD_DIR_DOWN);
                action.UseParentDataY[(int)GyroDirectionalSwipe.SwipeAxisYDir.Down] = false;
                action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.PAD_DIR_DOWN);
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
                    action.UsedEventsButtonsX[(int)GyroDirectionalSwipe.SwipeAxisXDir.Left] = newAction;
                }

                action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.PAD_DIR_LEFT);
                action.UseParentDataX[(int)GyroDirectionalSwipe.SwipeAxisXDir.Left] = false;
                action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.PAD_DIR_LEFT);
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
                    action.UsedEventsButtonsX[(int)GyroDirectionalSwipe.SwipeAxisXDir.Right] = newAction;
                }

                action.ChangedProperties.Add(GyroDirectionalSwipe.PropertyKeyStrings.PAD_DIR_RIGHT);
                action.UseParentDataX[(int)GyroDirectionalSwipe.SwipeAxisXDir.Right] = false;
                action.RaiseNotifyPropertyChange(mapper, GyroDirectionalSwipe.PropertyKeyStrings.PAD_DIR_RIGHT);
            });
        }
    }
}
