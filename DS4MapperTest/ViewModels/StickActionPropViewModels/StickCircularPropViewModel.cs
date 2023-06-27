using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.StickActions;
using DS4MapperTest.ButtonActions;

namespace DS4MapperTest.ViewModels.StickActionPropViewModels
{
    public class StickCircularPropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private StickCircular action;
        public StickCircular Action
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
                action.ChangedProperties.Contains(StickCircular.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public event EventHandler ActionPropertyChanged;
        public event EventHandler<StickMapAction> ActionChanged;

        private bool usingRealAction = false;

        public StickCircularPropViewModel(Mapper mapper, StickMapAction action)
        {
            this.mapper = mapper;
            this.action = action as StickCircular;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                StickCircular baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as StickCircular;
                StickCircular tempAction = new StickCircular();
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

            NameChanged += StickCircularPropViewModel_NameChanged;
        }

        private void StickCircularPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickCircular.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(StickCircular.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, StickCircular.PropertyKeyStrings.NAME);
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

                    mapper.EditActionSet.RecentAppliedLayer.AddStickAction(this.action);
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

                action.ChangedProperties.Add(StickCircular.PropertyKeyStrings.SCROLL_BUTTON_1);
                action.RaiseNotifyPropertyChange(mapper, StickCircular.PropertyKeyStrings.SCROLL_BUTTON_1);
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

                action.ChangedProperties.Add(StickCircular.PropertyKeyStrings.SCROLL_BUTTON_2);
                action.RaiseNotifyPropertyChange(mapper, StickCircular.PropertyKeyStrings.SCROLL_BUTTON_2);
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
}
