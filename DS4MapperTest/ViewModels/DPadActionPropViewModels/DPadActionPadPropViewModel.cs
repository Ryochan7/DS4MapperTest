using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.StickActions;

namespace DS4MapperTest.ViewModels.DPadActionPropViewModels
{
    public class DPadActionPadPropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private DPadAction action;
        public DPadAction Action
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
            get => action.CurrentMode == DPadAction.DPadMode.EightWay ||
                action.CurrentMode == DPadAction.DPadMode.FourWayDiagonal;
        }
        public event EventHandler ShowDiagonalPadChanged;

        public bool ShowCardinalPad
        {
            get => action.CurrentMode == DPadAction.DPadMode.Standard ||
                action.CurrentMode == DPadAction.DPadMode.EightWay ||
                action.CurrentMode == DPadAction.DPadMode.FourWayCardinal;
        }
        public event EventHandler ShowCardinalPadChanged;


        public string ActionUpBtnDisplayBind
        {
            get => action.EventCodes4[(int)DpadDirections.Up].DescribeActions(mapper);
        }

        public string ActionDownBtnDisplayBind
        {
            get => action.EventCodes4[(int)DpadDirections.Down].DescribeActions(mapper);
        }

        public string ActionLeftBtnDisplayBind
        {
            get => action.EventCodes4[(int)DpadDirections.Left].DescribeActions(mapper);
        }

        public string ActionRightBtnDisplayBind
        {
            get => action.EventCodes4[(int)DpadDirections.Right].DescribeActions(mapper);
        }

        public string ActionUpLeftBtnDisplayBind
        {
            get => action.EventCodes4[(int)DpadDirections.UpLeft].DescribeActions(mapper);
        }

        public string ActionUpRightBtnDisplayBind
        {
            get => action.EventCodes4[(int)DpadDirections.UpRight].DescribeActions(mapper);
        }

        public string ActionDownLeftBtnDisplayBind
        {
            get => action.EventCodes4[(int)DpadDirections.DownLeft].DescribeActions(mapper);
        }

        public string ActionDownRightBtnDisplayBind
        {
            get => action.EventCodes4[(int)DpadDirections.DownRight].DescribeActions(mapper);
        }

        public string DelayTime
        {
            get => action.DelayTime.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    if (action.DelayTime == temp) return;
                    action.DelayTime = Math.Clamp(temp, 0.0, 3600.0);
                    DelayTimeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler DelayTimeChanged;


        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(DPadAction.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightPadMode
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(DPadAction.PropertyKeyStrings.PAD_MODE);
        }
        public event EventHandler HighlightPadModeChanged;

        public bool HighlightDelayTime
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(DPadAction.PropertyKeyStrings.DELAY_TIME);
        }
        public event EventHandler HighlightDelayTimeChanged;

        public event EventHandler ActionPropertyChanged;
        public event EventHandler<DPadMapAction> ActionChanged;

        private bool usingRealAction = false;

        public DPadActionPadPropViewModel(Mapper mapper, DPadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as DPadAction;
            padModeItems = new List<PadModeItem>();
            usingRealAction = true;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                DPadAction baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as DPadAction;
                DPadAction tempAction = new DPadAction();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.EditLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
                usingRealAction = false;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            PrepareModel();

            NameChanged += DPadActionPropViewModel_NameChanged;
            SelectedPadModeIndexChanged += ChangeDPadMode;
            SelectedPadModeIndexChanged += StickPadActionPropViewModel_SelectedPadModeIndexChanged;
            DelayTimeChanged += DPadActionPadPropViewModel_DelayTimeChanged;
        }

        private void DPadActionPadPropViewModel_DelayTimeChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(DPadAction.PropertyKeyStrings.DELAY_TIME))
            {
                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.DELAY_TIME);
            }

            action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.DELAY_TIME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void DPadActionPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(DPadAction.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ChangeDPadMode(object sender, EventArgs e)
        {
            action.CurrentMode = padModeItems[selectedPadModeIndex].DPadMode;

            ShowCardinalPadChanged?.Invoke(this, EventArgs.Empty);
            ShowDiagonalPadChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickPadActionPropViewModel_SelectedPadModeIndexChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(DPadAction.PropertyKeyStrings.PAD_MODE))
            {
                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.PAD_MODE);
            }

            action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.PAD_MODE);
            HighlightPadModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

                mapper.QueueEvent(() =>
                {
                    this.action.ParentAction.Release(mapper, ignoreReleaseActions: true);

                    mapper.EditLayer.AddDPadAction(this.action);
                    if (mapper.EditActionSet.UsingCompositeLayer)
                    {
                        mapper.EditActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.EditLayer.SyncActions();
                        mapper.EditActionSet.ClearCompositeLayerActions();
                        mapper.EditActionSet.PrepareCompositeLayer();
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
                new PadModeItem("Standard", DPadAction.DPadMode.Standard),
                new PadModeItem("Eight Way", DPadAction.DPadMode.EightWay),
                new PadModeItem("Four Way Cardinal", DPadAction.DPadMode.FourWayCardinal),
                new PadModeItem("Four Way Diagonal", DPadAction.DPadMode.FourWayDiagonal),
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
                oldAction?.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)DpadDirections.Up] = newAction as ButtonAction;
                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.PAD_DIR_UP);
                this.action.UsingParentActionButton[(int)DpadDirections.Up] = false;
                action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.PAD_DIR_UP);
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
                oldAction?.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)DpadDirections.Down] = newAction as ButtonAction;
                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.PAD_DIR_DOWN);
                this.action.UsingParentActionButton[(int)DpadDirections.Down] = false;
                action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.PAD_DIR_DOWN);
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
                oldAction?.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)DpadDirections.Left] = newAction as ButtonAction;
                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.PAD_DIR_LEFT);
                this.action.UsingParentActionButton[(int)DpadDirections.Left] = false;
                action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.PAD_DIR_LEFT);
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
                oldAction?.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)DpadDirections.Right] = newAction as ButtonAction;
                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.PAD_DIR_RIGHT);
                this.action.UsingParentActionButton[(int)DpadDirections.Right] = false;
                action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.PAD_DIR_RIGHT);
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
                oldAction?.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)DpadDirections.UpLeft] = newAction as ButtonAction;
                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.PAD_DIR_UPLEFT);
                this.action.UsingParentActionButton[(int)DpadDirections.UpLeft] = false;
                action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.PAD_DIR_UPLEFT);
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
                oldAction?.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)DpadDirections.UpRight] = newAction as ButtonAction;
                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.PAD_DIR_UPRIGHT);
                this.action.UsingParentActionButton[(int)DpadDirections.UpRight] = false;
                action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.PAD_DIR_UPRIGHT);
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
                oldAction?.Release(mapper, ignoreReleaseActions: true);

                action.EventCodes4[(int)DpadDirections.DownLeft] = newAction as ButtonAction;
                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.PAD_DIR_DOWNLEFT);
                this.action.UsingParentActionButton[(int)DpadDirections.DownLeft] = false;
                action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.PAD_DIR_DOWNLEFT);
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
                    action.EventCodes4[(int)DpadDirections.DownRight] = newAction as ButtonAction;
                }

                action.ChangedProperties.Add(DPadAction.PropertyKeyStrings.PAD_DIR_DOWNRIGHT);
                this.action.UsingParentActionButton[(int)DpadDirections.DownRight] = false;
                action.RaiseNotifyPropertyChange(mapper, DPadAction.PropertyKeyStrings.PAD_DIR_DOWNRIGHT);
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

        private DPadAction.DPadMode dpadMode = DPadAction.DPadMode.Standard;
        public DPadAction.DPadMode DPadMode
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

        public PadModeItem(string displayName, DPadAction.DPadMode dpadMode)
        {
            this.displayName = displayName;
            this.dpadMode = dpadMode;
        }
    }
}
