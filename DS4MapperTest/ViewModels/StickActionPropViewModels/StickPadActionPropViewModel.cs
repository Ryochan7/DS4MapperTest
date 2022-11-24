using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.ViewModels.Common;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.StickActions;
using DS4MapperTest.StickModifiers;

namespace DS4MapperTest.ViewModels.StickActionPropViewModels
{
    public class StickPadActionPropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private StickPadAction action;
        public StickPadAction Action
        {
            get => action;
        }

        public string Name
        {
            get => action.Name;
            set
            {
                if (action.Name == value) return;
                action.Name = value;
                NameChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler NameChanged;

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
            get => action.CurrentMode == StickPadAction.DPadMode.EightWay ||
                action.CurrentMode == StickPadAction.DPadMode.FourWayDiagonal;
        }
        public event EventHandler ShowDiagonalPadChanged;

        public bool ShowCardinalPad
        {
            get => action.CurrentMode == StickPadAction.DPadMode.Standard ||
                action.CurrentMode == StickPadAction.DPadMode.EightWay ||
                action.CurrentMode == StickPadAction.DPadMode.FourWayCardinal;
        }
        public event EventHandler ShowCardinalPadChanged;

        private List<EnumChoiceSelection<StickDeadZone.DeadZoneTypes>> deadZoneModesChoices =
            new List<EnumChoiceSelection<StickDeadZone.DeadZoneTypes>>()
            {
                new EnumChoiceSelection<StickDeadZone.DeadZoneTypes>("Radial", StickDeadZone.DeadZoneTypes.Radial),
                new EnumChoiceSelection<StickDeadZone.DeadZoneTypes>("Bowtie", StickDeadZone.DeadZoneTypes.Bowtie),
            };

        public List<EnumChoiceSelection<StickDeadZone.DeadZoneTypes>> DeadZoneModesChoices => deadZoneModesChoices;

        public StickDeadZone.DeadZoneTypes DeadZoneType
        {
            get => action.DeadMod.DeadZoneType;
            set
            {
                action.DeadMod.DeadZoneType = value;
                DeadZoneTypeChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeadZoneTypeChanged;

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

        public string ActionUpBtnDisplayBind
        {
            get => action.EventCodes4[(int)StickPadAction.DpadDirections.Up].DescribeActions(mapper);
        }

        public string ActionDownBtnDisplayBind
        {
            get => action.EventCodes4[(int)StickPadAction.DpadDirections.Down].DescribeActions(mapper);
        }

        public string ActionLeftBtnDisplayBind
        {
            get => action.EventCodes4[(int)StickPadAction.DpadDirections.Left].DescribeActions(mapper);
        }

        public string ActionRightBtnDisplayBind
        {
            get => action.EventCodes4[(int)StickPadAction.DpadDirections.Right].DescribeActions(mapper);
        }

        public string ActionUpLeftBtnDisplayBind
        {
            get => action.EventCodes4[(int)StickPadAction.DpadDirections.UpLeft].DescribeActions(mapper);
        }

        public string ActionUpRightBtnDisplayBind
        {
            get => action.EventCodes4[(int)StickPadAction.DpadDirections.UpRight].DescribeActions(mapper);
        }

        public string ActionDownLeftBtnDisplayBind
        {
            get => action.EventCodes4[(int)StickPadAction.DpadDirections.DownLeft].DescribeActions(mapper);
        }

        public string ActionDownRightBtnDisplayBind
        {
            get => action.EventCodes4[(int)StickPadAction.DpadDirections.DownRight].DescribeActions(mapper);
        }

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickPadAction.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightPadMode
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickPadAction.PropertyKeyStrings.PAD_MODE);
        }
        public event EventHandler HighlightPadModeChanged;

        public bool HighlightDiagonalRange
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickPadAction.PropertyKeyStrings.DIAGONAL_RANGE);
        }
        public event EventHandler HighlightDiagonalRangeChanged;

        public bool HighlightDeadZoneType
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickPadAction.PropertyKeyStrings.DEAD_ZONE_TYPE);
        }
        public event EventHandler HighlightDeadZoneTypeChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickPadAction.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public event EventHandler ActionPropertyChanged;
        public event EventHandler<StickMapAction> ActionChanged;

        private bool usingRealAction = false;

        public StickPadActionPropViewModel(Mapper mapper, StickMapAction action)
        {
            this.mapper = mapper;
            this.action = action as StickPadAction;
            padModeItems = new List<PadModeItem>();

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                StickPadAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as StickPadAction;
                StickPadAction tempAction = new StickPadAction();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            PrepareModel();

            NameChanged += StickPadActionPropViewModel_NameChanged;
            DeadZoneChanged += StickPadActionPropViewModel_DeadZoneChanged;
            DeadZoneTypeChanged += StickPadActionPropViewModel_DeadZoneTypeChanged;
            SelectedPadModeIndexChanged += ChangeStickPadMode;
            SelectedPadModeIndexChanged += StickPadActionPropViewModel_SelectedPadModeIndexChanged;
        }

        private void StickPadActionPropViewModel_DeadZoneTypeChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickPadAction.PropertyKeyStrings.DEAD_ZONE_TYPE))
            {
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.DEAD_ZONE_TYPE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.DEAD_ZONE_TYPE);
            HighlightDeadZoneTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickPadActionPropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickPadAction.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ChangeStickPadMode(object sender, EventArgs e)
        {
            action.CurrentMode = padModeItems[selectedPadModeIndex].DPadMode;

            ShowCardinalPadChanged?.Invoke(this, EventArgs.Empty);
            ShowDiagonalPadChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickPadActionPropViewModel_SelectedPadModeIndexChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickPadAction.PropertyKeyStrings.PAD_MODE))
            {
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.PAD_MODE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.PAD_MODE);
            HighlightPadModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickPadActionPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickPadAction.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

                mapper.QueueEvent(() =>
                {
                    this.action.ParentAction.Release(mapper, ignoreReleaseActions: true);

                    mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddStickAction(this.action);
                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        mapper.ActionProfile.CurrentActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.SyncActions();
                        mapper.ActionProfile.CurrentActionSet.ClearCompositeLayerActions();
                        mapper.ActionProfile.CurrentActionSet.PrepareCompositeLayer();
                    }

                    resetEvent.Set();
                });

                resetEvent.Wait();

                usingRealAction = true;

                ActionChanged?.Invoke(this, action);
            }
        }

        private void PrepareModel()
        {
            padModeItems.AddRange(new PadModeItem[]
            {
                new PadModeItem("Standard", StickPadAction.DPadMode.Standard),
                new PadModeItem("Eight Way", StickPadAction.DPadMode.EightWay),
                new PadModeItem("Four Way Cardinal", StickPadAction.DPadMode.FourWayCardinal),
                new PadModeItem("Four Way Diagonal", StickPadAction.DPadMode.FourWayDiagonal),
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
                oldAction.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)StickPadAction.DpadDirections.Up] = newAction as AxisDirButton;
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.PAD_DIR_UP);
                this.action.UsingParentActionButton[(int)StickPadAction.DpadDirections.Up] = false;
                action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.PAD_DIR_UP);
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
                oldAction.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)StickPadAction.DpadDirections.Down] = newAction as AxisDirButton;
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.PAD_DIR_DOWN);
                this.action.UsingParentActionButton[(int)StickPadAction.DpadDirections.Down] = false;
                action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.PAD_DIR_DOWN);
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
                oldAction.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)StickPadAction.DpadDirections.Left] = newAction as AxisDirButton;
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.PAD_DIR_LEFT);
                this.action.UsingParentActionButton[(int)StickPadAction.DpadDirections.Left] = false;
                action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.PAD_DIR_LEFT);
            });
        }

        public void UpdateRightDirAction(ButtonAction oldAction, ButtonAction newAction)
        {
            if (!usingRealAction)
            {
                ReplaceExistingLayerAction(this, EventArgs.Empty);
            }

            ExecuteInMapperThread(() =>
            {
                oldAction.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)StickPadAction.DpadDirections.Right] = newAction as AxisDirButton;
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.PAD_DIR_RIGHT);
                this.action.UsingParentActionButton[(int)StickPadAction.DpadDirections.Right] = false;
                action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.PAD_DIR_RIGHT);
            });
        }

        public void UpdateUpLeftDirAction(ButtonAction oldAction, ButtonAction newAction)
        {
            if (!usingRealAction)
            {
                ReplaceExistingLayerAction(this, EventArgs.Empty);
            }

            ExecuteInMapperThread(() =>
            {
                oldAction.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)StickPadAction.DpadDirections.UpLeft] = newAction as AxisDirButton;
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.PAD_DIR_UPLEFT);
                this.action.UsingParentActionButton[(int)StickPadAction.DpadDirections.UpLeft] = false;
                action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.PAD_DIR_UPLEFT);
            });
        }

        public void UpdateUpRightDirAction(ButtonAction oldAction, ButtonAction newAction)
        {
            if (!usingRealAction)
            {
                ReplaceExistingLayerAction(this, EventArgs.Empty);
            }

            ExecuteInMapperThread(() =>
            {
                oldAction.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)StickPadAction.DpadDirections.UpRight] = newAction as AxisDirButton;
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.PAD_DIR_UPRIGHT);
                this.action.UsingParentActionButton[(int)StickPadAction.DpadDirections.UpRight] = false;
                action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.PAD_DIR_UPRIGHT);
            });
        }

        public void UpdateDownLeftDirAction(ButtonAction oldAction, ButtonAction newAction)
        {
            if (!usingRealAction)
            {
                ReplaceExistingLayerAction(this, EventArgs.Empty);
            }

            ExecuteInMapperThread(() =>
            {
                oldAction.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)StickPadAction.DpadDirections.DownLeft] = newAction as AxisDirButton;
                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.PAD_DIR_DOWNLEFT);
                this.action.UsingParentActionButton[(int)StickPadAction.DpadDirections.DownLeft] = false;
                action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.PAD_DIR_DOWNLEFT);
            });
        }

        public void UpdateDownRightDirAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.EventCodes4[(int)StickPadAction.DpadDirections.DownRight] = newAction as AxisDirButton;
                }

                action.ChangedProperties.Add(StickPadAction.PropertyKeyStrings.PAD_DIR_DOWNRIGHT);
                this.action.UsingParentActionButton[(int)StickPadAction.DpadDirections.DownRight] = false;
                action.RaiseNotifyPropertyChange(mapper, StickPadAction.PropertyKeyStrings.PAD_DIR_DOWNRIGHT);
            });
        }

        protected void ExecuteInMapperThread(Action tempAction)
        {
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            mapper.QueueEvent(() =>
            {
                tempAction?.Invoke();

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

        private StickPadAction.DPadMode dpadMode = StickPadAction.DPadMode.Standard;
        public StickPadAction.DPadMode DPadMode
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

        public PadModeItem(string displayName, StickPadAction.DPadMode dpadMode)
        {
            this.displayName = displayName;
            this.dpadMode = dpadMode;
        }
    }
}
