using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.TriggerActions;

namespace DS4MapperTest.ViewModels.TriggerActionPropViewModels
{
    public class TriggerDualStagePropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private TriggerDualStageAction action;
        public TriggerDualStageAction Action
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

        public string DeadZone
        {
            get => $"{action.DeadMod.DeadZone:N2}";
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

        public string AntiDeadZone
        {
            get => action.DeadMod.AntiDeadZone.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.DeadMod.AntiDeadZone = Math.Clamp(temp, 0.0, 1.0);
                    AntiDeadZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler AntiDeadZoneChanged;

        public string MaxZone
        {
            get => action.DeadMod.MaxZone.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.DeadMod.MaxZone = Math.Clamp(temp, 0.0, 1.0);
                    MaxZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler MaxZoneChanged;

        public string HipFireDelay
        {
            get => action.HipFireMS.ToString();
            set
            {
                if (int.TryParse(value, out int temp))
                {
                    action.HipFireMS = Math.Clamp(temp, 0, 10000);
                    HipFireDelayChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler HipFireDelayChanged;

        public bool ForceHipFireDelay
        {
            get => action.ForceHipTime;
            set
            {
                action.ForceHipTime = value;
                ForceHipFireDelayChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ForceHipFireDelayChanged;

        private int selectedDSModeIndex = 0;
        public int SelectedDSModeIndex
        {
            get => selectedDSModeIndex;
            set
            {
                if (selectedDSModeIndex == value) return;
                selectedDSModeIndex = value;
                SelectedDSModeIndexChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectedDSModeIndexChanged;

        public string FullBtnDisplayBind
        {
            get => action.FullPullActButton.DescribeActions(mapper);
        }

        public string SoftBtnDisplayBind
        {
            get => action.SoftPullActButton.DescribeActions(mapper);
        }

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightAntiDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.ANTIDEAD_ZONE);
        }
        public event EventHandler HighlightAntiDeadZoneChanged;

        public bool HighlightMaxZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.MAX_ZONE);
        }
        public event EventHandler HighlightMaxZoneChanged;

        public bool HighlightHipFireDelay
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.HIPFIRE_DELAY);
        }
        public event EventHandler HighlightHipFireDelayChanged;

        public bool HighlightForceHipFireDelay
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.FORCE_HIP_FIRE_TIME);
        }
        public event EventHandler HighlightForceHipFireDelayChanged;

        public bool HighlightDSMode
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.DUALSTAGE_MODE);
        }
        public event EventHandler HighlightDSModeChanged;

        public event EventHandler ActionPropertyChanged;

        public event EventHandler<TriggerMapAction> ActionChanged;

        private bool usingRealAction = true;

        public TriggerDualStagePropViewModel(Mapper mapper, TriggerMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TriggerDualStageAction;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TriggerDualStageAction baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TriggerDualStageAction;
                TriggerDualStageAction tempAction = new TriggerDualStageAction();
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

            NameChanged += TriggerDualStagePropViewModel_NameChanged;
            DeadZoneChanged += TriggerDualStagePropViewModel_DeadZoneChanged;
            AntiDeadZoneChanged += TriggerDualStagePropViewModel_AntiDeadZoneChanged;
            MaxZoneChanged += TriggerDualStagePropViewModel_MaxZoneChanged;
            HipFireDelayChanged += TriggerDualStagePropViewModel_HipFireDelayChanged;
            ForceHipFireDelayChanged += TriggerDualStagePropViewModel_ForceHipFireDelayChanged;
            SelectedDSModeIndexChanged += TriggerDualStagePropViewModel_SelectedDSModeIndexChanged;
        }

        private void TriggerDualStagePropViewModel_ForceHipFireDelayChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.FORCE_HIP_FIRE_TIME))
            {
                action.ChangedProperties.Add(TriggerDualStageAction.PropertyKeyStrings.FORCE_HIP_FIRE_TIME);
            }

            action.RaiseNotifyPropertyChange(mapper, TriggerDualStageAction.PropertyKeyStrings.FORCE_HIP_FIRE_TIME);
            HighlightForceHipFireDelayChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TriggerDualStagePropViewModel_SelectedDSModeIndexChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.DUALSTAGE_MODE))
            {
                action.ChangedProperties.Add(TriggerDualStageAction.PropertyKeyStrings.DUALSTAGE_MODE);
            }

            action.RaiseNotifyPropertyChange(mapper, TriggerDualStageAction.PropertyKeyStrings.DUALSTAGE_MODE);
            HighlightDSModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

                mapper.ProcessMappingChangeAction(() =>
                {
                    this.action.ParentAction?.Release(mapper, ignoreReleaseActions: true);

                    mapper.EditLayer.AddTriggerAction(this.action);
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

        private void TriggerDualStagePropViewModel_HipFireDelayChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.HIPFIRE_DELAY))
            {
                action.ChangedProperties.Add(TriggerDualStageAction.PropertyKeyStrings.HIPFIRE_DELAY);
            }

            action.RaiseNotifyPropertyChange(mapper, TriggerDualStageAction.PropertyKeyStrings.HIPFIRE_DELAY);
            HighlightHipFireDelayChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TriggerDualStagePropViewModel_MaxZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.MAX_ZONE))
            {
                action.ChangedProperties.Add(TriggerDualStageAction.PropertyKeyStrings.MAX_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TriggerDualStageAction.PropertyKeyStrings.MAX_ZONE);
            HighlightMaxZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TriggerDualStagePropViewModel_AntiDeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.ANTIDEAD_ZONE))
            {
                action.ChangedProperties.Add(TriggerDualStageAction.PropertyKeyStrings.ANTIDEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TriggerDualStageAction.PropertyKeyStrings.ANTIDEAD_ZONE);
            HighlightAntiDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TriggerDualStagePropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(TriggerDualStageAction.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TriggerDualStageAction.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TriggerDualStagePropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TriggerDualStageAction.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(TriggerDualStageAction.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TriggerDualStageAction.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PrepareModel()
        {
            switch(action.TriggerStateMode)
            {
                case TriggerDualStageAction.DualStageMode.Threshold:
                    selectedDSModeIndex = 0;
                    break;
                case TriggerDualStageAction.DualStageMode.ExclusiveButtons:
                    selectedDSModeIndex = 1;
                    break;
                case TriggerDualStageAction.DualStageMode.HairTrigger:
                    selectedDSModeIndex = 2;
                    break;
                case TriggerDualStageAction.DualStageMode.HipFire:
                    selectedDSModeIndex = 3;
                    break;
                case TriggerDualStageAction.DualStageMode.HipFireExclusiveButtons:
                    selectedDSModeIndex = 4;
                    break;
                default:
                    break;
            }
        }

        public void UpdateFullPullAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.FullPullActButton = newAction as AxisDirButton;
                }

                action.ChangedProperties.Add(TriggerDualStageAction.PropertyKeyStrings.FULLPULL_BUTTON);
                action.UseParentFullPullBtn = false;
                action.RaiseNotifyPropertyChange(mapper, TriggerDualStageAction.PropertyKeyStrings.FULLPULL_BUTTON);

                resetEvent.Set();
            });

            resetEvent.Wait();
        }

        public void UpdateSoftPullAction(ButtonAction oldAction, ButtonAction newAction)
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
                    action.SoftPullActButton = newAction as AxisDirButton;
                }

                action.ChangedProperties.Add(TriggerDualStageAction.PropertyKeyStrings.SOFTPULL_BUTTON);
                action.UseParentSoftPullBtn = false;
                action.RaiseNotifyPropertyChange(mapper, TriggerDualStageAction.PropertyKeyStrings.SOFTPULL_BUTTON);

                resetEvent.Set();
            });

            resetEvent.Wait();
        }
    }
}
