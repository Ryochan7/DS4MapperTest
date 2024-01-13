using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.ViewModels.Common;

namespace DS4MapperTest.ViewModels.TouchpadActionPropViewModels
{
    public class TouchpadActionPadPropViewModel : TouchpadActionPropVMBase
    {
        private TouchpadActionPad action;
        public TouchpadActionPad Action
        {
            get => action;
        }

        public string DeadZone
        {
            get => action.DeadMod.DeadZone.ToString();
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.DeadMod.DeadZone = Math.Clamp(temp, 0.0, 1.0);
                    DeadZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler DeadZoneChanged;

        public int DiagonalRange
        {
            get => action.DiagonalRange;
            set
            {
                if (action.DiagonalRange == value) return;
                action.DiagonalRange = value;
                DiagonalRangeChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DiagonalRangeChanged;

        public bool RequiresClick
        {
            get => action.RequiresClick;
            set
            {
                if (action.RequiresClick == value) return;
                action.RequiresClick = value;
                RequiresClickChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RequiresClickChanged;

        private List<PadModeItem> padModeItems;
        public List<PadModeItem> PadModeItems => padModeItems;

        private int selectedPadModeIndex = -1;
        public int SelectedPadModeIndex
        {
            get => selectedPadModeIndex;
            set
            {
                if (selectedPadModeIndex == value) return;
                selectedPadModeIndex = value;
                SelectedPadModeIndexChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectedPadModeIndexChanged;

        public bool ShowDiagonalPad
        {
            get => action.CurrentMode == TouchpadActionPad.DPadMode.EightWay ||
                action.CurrentMode == TouchpadActionPad.DPadMode.FourWayDiagonal;
        }
        public event EventHandler ShowDiagonalPadChanged;

        public bool ShowCardinalPad
        {
            get => action.CurrentMode == TouchpadActionPad.DPadMode.Standard ||
                action.CurrentMode == TouchpadActionPad.DPadMode.EightWay ||
                action.CurrentMode == TouchpadActionPad.DPadMode.FourWayCardinal;
        }
        public event EventHandler ShowCardinalPadChanged;

        public string ActionUpBtnDisplayBind
        {
            get => action.EventCodes4[(int)TouchpadActionPad.DpadDirections.Up].DescribeActions(mapper);
        }

        public string ActionDownBtnDisplayBind
        {
            get => action.EventCodes4[(int)TouchpadActionPad.DpadDirections.Down].DescribeActions(mapper);
        }

        public string ActionLeftBtnDisplayBind
        {
            get => action.EventCodes4[(int)TouchpadActionPad.DpadDirections.Left].DescribeActions(mapper);
        }

        public string ActionRightBtnDisplayBind
        {
            get => action.EventCodes4[(int)TouchpadActionPad.DpadDirections.Right].DescribeActions(mapper);
        }

        public string ActionUpLeftBtnDisplayBind
        {
            get => action.EventCodes4[(int)TouchpadActionPad.DpadDirections.UpLeft].DescribeActions(mapper);
        }

        public string ActionUpRightBtnDisplayBind
        {
            get => action.EventCodes4[(int)TouchpadActionPad.DpadDirections.UpRight].DescribeActions(mapper);
        }

        public string ActionDownLeftBtnDisplayBind
        {
            get => action.EventCodes4[(int)TouchpadActionPad.DpadDirections.DownLeft].DescribeActions(mapper);
        }

        public string ActionDownRightBtnDisplayBind
        {
            get => action.EventCodes4[(int)TouchpadActionPad.DpadDirections.DownRight].DescribeActions(mapper);
        }

        public bool UseOuterRing
        {
            get => action.UseRingButton;
            set
            {
                if (action.UseRingButton == value) return;
                action.UseRingButton = value;
                UseOuterRingChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UseOuterRingChanged;

        public bool OuterRingInvert
        {
            get => !action.UseAsOuterRing;
            set
            {
                if (action.UseAsOuterRing == !value) return;
                action.UseAsOuterRing = !value;
                OuterRingInvertChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OuterRingInvertChanged;

        public string OuterRingDeadZone
        {
            get => action.OuterRingDeadZone.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.OuterRingDeadZone = Math.Clamp(temp, 0, 10000);
                    OuterRingDeadZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler OuterRingDeadZoneChanged;

        private List<EnumChoiceSelection<OuterRingUseRange>> outerRingRangeChoiceItems =
            new List<EnumChoiceSelection<OuterRingUseRange>>()
            {
                new EnumChoiceSelection<OuterRingUseRange>("Only Active", OuterRingUseRange.OnlyActive),
                new EnumChoiceSelection<OuterRingUseRange>("Full Range", OuterRingUseRange.FullRange),
            };
        public List<EnumChoiceSelection<OuterRingUseRange>> OuterRingRangeChoiceItems => outerRingRangeChoiceItems;

        public OuterRingUseRange OuterRingRangeChoice
        {
            get => action.UsedOuterRingRange;
            set
            {
                action.UsedOuterRingRange = value;
                OuterRingRangeChoiceChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OuterRingRangeChoiceChanged;

        public string ActionRingDisplayBind
        {
            get
            {
                string result = "";
                if (action.RingButton != null)
                {
                    result = action.RingButton.DescribeActions(mapper);
                }

                return result;
            }
        }

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightDiagonalRange
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.DIAGONAL_RANGE);
        }
        public event EventHandler HighlightDiagonalRangeChanged;

        public bool HighlightRequiresClick
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.REQUIRES_CLICK);
        }
        public event EventHandler HighlightRequiresClickChanged;

        public bool HighlightPadMode
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.PAD_MODE);
        }
        public event EventHandler HighlightPadModeChanged;

        public bool HighlightUseOuterRing
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.USE_OUTER_RING);
        }
        public event EventHandler HighlightUseOuterRingChanged;

        public bool HighlightOuterRingInvert
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.USE_AS_OUTER_RING);
        }
        public event EventHandler HighlightOuterRingInvertChanged;

        public bool HighlightOuterRingDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.OUTER_RING_DEAD_ZONE);
        }
        public event EventHandler HighlightOuterRingDeadZoneChanged;

        public bool HighlightOuterRingRangeChoice
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.OUTER_RING_FULL_RANGE);
        }
        public event EventHandler HighlightOuterRingRangeChoiceChanged;

        public override event EventHandler ActionPropertyChanged;

        public TouchpadActionPadPropViewModel(Mapper mapper, TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TouchpadActionPad;
            this.baseAction = action;

            padModeItems = new List<PadModeItem>();

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TouchpadActionPad baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TouchpadActionPad;
                TouchpadActionPad tempAction = new TouchpadActionPad();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.EditLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
                this.baseAction = this.action;
                usingRealAction = false;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            PrepareModel();

            NameChanged += TouchpadActionPadPropViewModel_NameChanged;
            DeadZoneChanged += TouchpadActionPadPropViewModel_DeadZoneChanged;
            DiagonalRangeChanged += TouchpadActionPadPropViewModel_DiagonalRangeChanged;
            RequiresClickChanged += TouchpadActionPadPropViewModel_RequiresClickChanged;
            SelectedPadModeIndexChanged += ChangeStickPadMode;
            SelectedPadModeIndexChanged += TouchpadActionPadPropViewModel_SelectedPadModeIndexChanged;
            OuterRingRangeChoiceChanged += TouchpadActionPadPropViewModel_OuterRingRangeChoiceChanged;
        }

        private void TouchpadActionPadPropViewModel_OuterRingRangeChoiceChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.OUTER_RING_FULL_RANGE))
            {
                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.OUTER_RING_FULL_RANGE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadActionPad.PropertyKeyStrings.OUTER_RING_FULL_RANGE);
            HighlightOuterRingRangeChoiceChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadActionPadPropViewModel_SelectedPadModeIndexChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.PAD_MODE))
            {
                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.PAD_MODE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadActionPad.PropertyKeyStrings.PAD_MODE);
            HighlightPadModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ChangeStickPadMode(object sender, EventArgs e)
        {
            action.CurrentMode = padModeItems[selectedPadModeIndex].DPadMode;

            ShowCardinalPadChanged?.Invoke(this, EventArgs.Empty);
            ShowDiagonalPadChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadActionPadPropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadActionPad.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadActionPadPropViewModel_DiagonalRangeChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.DIAGONAL_RANGE))
            {
                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.DIAGONAL_RANGE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadActionPad.PropertyKeyStrings.DIAGONAL_RANGE);
            HighlightDiagonalRangeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadActionPadPropViewModel_RequiresClickChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.REQUIRES_CLICK))
            {
                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.REQUIRES_CLICK);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadActionPad.PropertyKeyStrings.REQUIRES_CLICK);
            HighlightRequiresClickChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadActionPadPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadActionPad.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadActionPad.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PrepareModel()
        {
            padModeItems.AddRange(new PadModeItem[]
            {
                new PadModeItem("Standard", TouchpadActionPad.DPadMode.Standard),
                new PadModeItem("Eight Way", TouchpadActionPad.DPadMode.EightWay),
                new PadModeItem("Four Way Cardinal", TouchpadActionPad.DPadMode.FourWayCardinal),
                new PadModeItem("Four Way Diagonal", TouchpadActionPad.DPadMode.FourWayDiagonal),
            });

            int index = padModeItems.FindIndex((item) => item.DPadMode == action.CurrentMode);
            if (index >= 0)
            {
                selectedPadModeIndex = index;
            }
        }

        public void UpdateUpDirAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.EventCodes4[(int)TouchpadActionPad.DpadDirections.Up] = newAction;
                }

                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.PAD_DIR_UP);
                action.UseParentActionButton[(int)TouchpadActionPad.DpadDirections.Up] = false;
            });


        }

        public void UpdateDownDirAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.EventCodes4[(int)TouchpadActionPad.DpadDirections.Down] = newAction;
                }

                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.PAD_DIR_DOWN);
                action.UseParentActionButton[(int)TouchpadActionPad.DpadDirections.Down] = false;
            });
        }

        public void UpdateLeftDirAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.EventCodes4[(int)TouchpadActionPad.DpadDirections.Left] = newAction;
                }

                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.PAD_DIR_LEFT);
                action.UseParentActionButton[(int)TouchpadActionPad.DpadDirections.Left] = false;
            });
        }

        public void UpdateRightAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.EventCodes4[(int)TouchpadActionPad.DpadDirections.Right] = newAction;
                }
                
                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.PAD_DIR_RIGHT);
                action.UseParentActionButton[(int)TouchpadActionPad.DpadDirections.Right] = false;
            });
        }

        public void UpdateDownLeftAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.EventCodes4[(int)TouchpadActionPad.DpadDirections.DownLeft] = newAction;
                }

                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.PAD_DIR_DOWNLEFT);
                action.UseParentActionButton[(int)TouchpadActionPad.DpadDirections.DownLeft] = false;
            });
        }

        public void UpdateDownRightAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.EventCodes4[(int)TouchpadActionPad.DpadDirections.DownRight] = newAction;
                }

                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.PAD_DIR_DOWNRIGHT);
                action.UseParentActionButton[(int)TouchpadActionPad.DpadDirections.DownRight] = false;
            });
        }

        public void UpdateUpLeftAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.EventCodes4[(int)TouchpadActionPad.DpadDirections.UpLeft] = newAction;
                }

                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.PAD_DIR_UPLEFT);
                action.UseParentActionButton[(int)TouchpadActionPad.DpadDirections.UpLeft] = false;
            });
        }

        public void UpdateUpRightAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.EventCodes4[(int)TouchpadActionPad.DpadDirections.UpRight] = newAction;
                }

                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.PAD_DIR_UPRIGHT);
                action.UseParentActionButton[(int)TouchpadActionPad.DpadDirections.UpRight] = false;
            });
        }

        public void UpdateRingButton(ButtonAction oldAction, ButtonAction newAction)
        {
            if (!usingRealAction)
            {
                ReplaceExistingLayerAction(this, EventArgs.Empty);
            }

            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            //ExecuteInMapperThread(() =>
            mapper.ProcessMappingChangeAction(() =>
            {
                if (oldAction != null)
                {
                    oldAction.Release(mapper, ignoreReleaseActions: true);
                    action.RingButton = newAction as AxisDirButton;
                }

                action.ChangedProperties.Add(TouchpadActionPad.PropertyKeyStrings.OUTER_RING_BUTTON);
                action.UseParentRingButton = false;

                resetEvent.Set();
            });

            resetEvent.Wait();
        }
    }

    public class PadModeItem
    {
        private string displayName;
        public string DisplayName
        {
            get => displayName;
        }

        private TouchpadActionPad.DPadMode dpadMode = TouchpadActionPad.DPadMode.Standard;
        public TouchpadActionPad.DPadMode DPadMode
        {
            get => dpadMode;
            set
            {
                if (dpadMode == value) return;
                dpadMode = value;
                DPadModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DPadModeChanged;

        public PadModeItem(string displayName, TouchpadActionPad.DPadMode dpadMode)
        {
            this.displayName = displayName;
            this.dpadMode = dpadMode;
        }
    }
}
