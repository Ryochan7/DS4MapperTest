using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.ViewModels.TouchpadActionPropViewModels
{
    public class TouchpadCircularPropViewModel : TouchpadActionPropVMBase
    {
        private TouchpadCircular action;
        public TouchpadCircular Action
        {
            get => action;
        }

        public string ForwardDisplayBind
        {
            get => action.ClockWiseBtn.DescribeActions(mapper);
        }

        public string BackwardDisplayBind
        {
            get => action.CounterClockwiseBtn.DescribeActions(mapper);
        }

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadCircular.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public TouchpadCircularPropViewModel(Mapper mapper, TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TouchpadCircular;
            this.baseAction = action;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TouchpadCircular baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TouchpadCircular;
                TouchpadCircular tempAction = new TouchpadCircular();
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

            NameChanged += TouchpadCircularPropViewModel_NameChanged;
        }

        private void TouchpadCircularPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadCircular.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(TouchpadCircular.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadCircular.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PrepareModel()
        {

        }

        public void UpdateClockWiseBtn(ButtonAction oldAction, ButtonAction newAction)
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
                    action.ClockWiseBtn = newAction as TouchpadCircularButton;
                }

                action.ChangedProperties.Add(TouchpadCircular.PropertyKeyStrings.SCROLL_BUTTON_1);
                action.RaiseNotifyPropertyChange(mapper, TouchpadCircular.PropertyKeyStrings.SCROLL_BUTTON_1);
            });
        }

        public void UpdateCounterClockWiseBtn(ButtonAction oldAction, ButtonAction newAction)
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
                    action.CounterClockwiseBtn = newAction as TouchpadCircularButton;
                }

                action.ChangedProperties.Add(TouchpadCircular.PropertyKeyStrings.SCROLL_BUTTON_2);
                action.RaiseNotifyPropertyChange(mapper, TouchpadCircular.PropertyKeyStrings.SCROLL_BUTTON_2);
            });
        }
    }
}
