using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.StickActions;
using DS4MapperTest.ViewModels.Common;

namespace DS4MapperTest.ViewModels.StickActionPropViewModels
{
    public class StickTranslatePropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private StickTranslate action;
        public StickTranslate Action
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

        private OutputStickSelectionItemList outputStickHolder = new OutputStickSelectionItemList();
        public OutputStickSelectionItemList OutputStickHolder => outputStickHolder;

        private int outputStickIndex = -1;
        public int OutputStickIndex
        {
            get => outputStickIndex;
            set
            {
                outputStickIndex = value;
                OutputStickIndexChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OutputStickIndexChanged;

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

        public string AntiDeadZone
        {
            get => action.DeadMod.AntiDeadZone.ToString();
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

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightOutputStick
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.OUTPUT_STICK);
        }
        public event EventHandler HighlightOutputStickChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightAntiDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.ANTIDEAD_ZONE);
        }
        public event EventHandler HighlightAntiDeadZoneChanged;

        public bool HighlightMaxZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.MAX_ZONE);
        }
        public event EventHandler HighlightMaxZoneChanged;

        public event EventHandler ActionPropertyChanged;
        public event EventHandler<StickMapAction> ActionChanged;

        private bool replacedAction = false;

        public StickTranslatePropViewModel(Mapper mapper, StickMapAction action)
        {
            this.mapper = mapper;
            this.action = action as StickTranslate;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                StickTranslate baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as StickTranslate;
                StickTranslate tempAction = new StickTranslate();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            PrepareModel();

            NameChanged += StickTranslatePropViewModel_NameChanged;
            OutputStickIndexChanged += ChangeOutputStickMode;
            OutputStickIndexChanged += StickTranslatePropViewModel_OutputStickIndexChanged;
            DeadZoneChanged += StickTranslatePropViewModel_DeadZoneChanged;
            AntiDeadZoneChanged += StickTranslatePropViewModel_AntiDeadZoneChanged;
            MaxZoneChanged += StickTranslatePropViewModel_MaxZoneChanged;
        }

        private void ChangeOutputStickMode(object sender, EventArgs e)
        {
            if (outputStickIndex == -1) return;

            OutputStickSelectionItem item = outputStickHolder.OutputStickItems[outputStickIndex];
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            mapper.QueueEvent(() =>
            {
                action.Release(mapper, ignoreReleaseActions: true);

                action.OutputAction.Reset();
                action.OutputAction.Prepare(MapperUtil.OutputActionData.ActionType.GamepadControl, 0);
                action.OutputAction.StickCode = item.Code;

                resetEvent.Set();
            });

            resetEvent.Wait();
        }

        private void StickTranslatePropViewModel_MaxZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.MAX_ZONE))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.MAX_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.MAX_ZONE);
            HighlightMaxZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_AntiDeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.ANTIDEAD_ZONE))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.ANTIDEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.ANTIDEAD_ZONE);
            HighlightAntiDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_OutputStickIndexChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.OUTPUT_STICK))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.OUTPUT_STICK);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.OUTPUT_STICK);
            HighlightOutputStickChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!replacedAction)
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
                    }

                    resetEvent.Set();
                });

                resetEvent.Wait();

                replacedAction = true;

                ActionChanged?.Invoke(this, action);
            }
        }

        private void PrepareModel()
        {
            outputStickIndex =
                outputStickHolder.StickAliasIndex(action.OutputAction.StickCode);
        }
    }
}
