using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.TriggerActions;

namespace DS4MapperTest.ViewModels.TriggerActionPropViewModels
{
    public class TriggerButtonActPropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private TriggerButtonAction action;
        public TriggerButtonAction Action
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
            get => $"{action.DeadZone.DeadZone:N2}";
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.DeadZone.DeadZone = Math.Clamp(temp, 0.0, 1.0);
                    DeadZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler DeadZoneChanged;

        public string ActionBindName
        {
            get => action.EventButton.DescribeActions(mapper);
        }

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TriggerButtonAction.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TriggerButtonAction.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public event EventHandler ActionPropertyChanged;
        public event EventHandler<TriggerMapAction> ActionChanged;

        private bool usingRealAction = true;

        public TriggerButtonActPropViewModel(Mapper mapper, TriggerMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TriggerButtonAction;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TriggerButtonAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TriggerButtonAction;
                TriggerButtonAction tempAction = new TriggerButtonAction();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
                usingRealAction = false;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            PrepareModel();

            NameChanged += TriggerButtonActPropViewModel_NameChanged;
            DeadZoneChanged += TriggerButtonActPropViewModel_DeadZoneChanged;
        }

        private void TriggerButtonActPropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TriggerButtonAction.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(TriggerButtonAction.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TriggerButtonAction.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TriggerButtonActPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TriggerButtonAction.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(TriggerButtonAction.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TriggerButtonAction.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

                mapper.QueueEvent(() =>
                {
                    this.action.ParentAction?.Release(mapper, ignoreReleaseActions: true);

                    mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddTriggerAction(this.action);
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

        }

        public void UpdateEventButton(ButtonAction oldAction, ButtonAction newAction)
        {
            if (!usingRealAction)
            {
                ReplaceExistingLayerAction(this, EventArgs.Empty);
            }

            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            //ExecuteInMapperThread(() =>
            mapper.QueueEvent(() =>
            {
                if (oldAction != null)
                {
                    oldAction.Release(mapper, ignoreReleaseActions: true);
                    action.EventButton = newAction as AxisDirButton;
                }

                action.ChangedProperties.Add(TriggerButtonAction.PropertyKeyStrings.OUTPUT_BINDING);
                action.UseParentEventButton = false;
                action.RaiseNotifyPropertyChange(mapper, TriggerButtonAction.PropertyKeyStrings.OUTPUT_BINDING);

                resetEvent.Set();
            });

            resetEvent.Wait();
        }
    }
}
