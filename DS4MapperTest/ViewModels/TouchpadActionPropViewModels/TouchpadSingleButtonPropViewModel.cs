using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.ViewModels.TouchpadActionPropViewModels
{
    public class TouchpadSingleButtonPropViewModel : TouchpadActionPropVMBase
    {
        private TouchpadSingleButton action;
        public TouchpadSingleButton Action
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

        public string ActionDisplayBind
        {
            get => action.EventButton.DescribeActions(mapper);
        }

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadSingleButton.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadSingleButton.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public override event EventHandler ActionPropertyChanged;

        public TouchpadSingleButtonPropViewModel(Mapper mapper, TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TouchpadSingleButton;
            this.baseAction = action;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TouchpadSingleButton baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TouchpadSingleButton;
                TouchpadSingleButton tempAction = new TouchpadSingleButton();
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

            NameChanged += TouchpadSingleButtonPropViewModel_NameChanged;
            DeadZoneChanged += TouchpadSingleButtonPropViewModel_DeadZoneChanged;
        }

        private void TouchpadSingleButtonPropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadSingleButton.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(TouchpadSingleButton.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadSingleButton.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadSingleButtonPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadSingleButton.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(TouchpadSingleButton.PropertyKeyStrings.NAME);
            }

            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
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

            ExecuteInMapperThread(() =>
            {
                if (oldAction != null)
                {
                    oldAction.Release(mapper, ignoreReleaseActions: true);
                    action.EventButton = newAction;
                }

                action.ChangedProperties.Add(TouchpadSingleButton.PropertyKeyStrings.FUNCTIONS);
                action.UseParentActions = false;
            });
        }
    }
}
