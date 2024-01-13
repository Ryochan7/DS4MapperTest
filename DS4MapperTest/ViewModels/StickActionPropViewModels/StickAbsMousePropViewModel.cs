using DS4MapperTest.ButtonActions;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS4MapperTest.ViewModels.StickActionPropViewModels
{
    public class StickAbsMousePropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private StickAbsMouse action;
        public StickAbsMouse Action
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
            get => action.DeadMod.DeadZone.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.DeadMod.DeadZone = Math.Clamp(temp, 0, 10000);
                    DeadZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler DeadZoneChanged;

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

        public string AntiRelease
        {
            get => action.AntiRadius.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.AntiRadius = Math.Clamp(temp, 0.0, 1.0);
                    AntiReleaseChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler AntiReleaseChanged;

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

        public bool SnapToCenterRelease
        {
            get => action.SnapToCenterRelease;
            set
            {
                if (action.SnapToCenterRelease == value) return;
                action.SnapToCenterRelease = value;
                SnapToCenterReleaseChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SnapToCenterReleaseChanged;

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
                action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightMaxZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.MAX_ZONE);
        }
        public event EventHandler HighlightMaxZoneChanged;

        public bool HighlightAntiRelease
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.ANTI_RADIUS);
        }
        public event EventHandler HighlightAntiReleaseChanged;

        public bool HighlightUseOuterRing
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.USE_OUTER_RING);
        }
        public event EventHandler HighlightUseOuterRingChanged;

        public bool HighlightOuterRingInvert
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.USE_AS_OUTER_RING);
        }
        public event EventHandler HighlightOuterRingInvertChanged;

        public bool HighlightOuterRingDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.OUTER_RING_DEAD_ZONE);
        }
        public event EventHandler HighlightOuterRingDeadZoneChanged;

        public bool HighlightSnapToCenterRelease
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.SNAP_TO_CENTER_RELEASE);
        }
        public event EventHandler HighlightSnapToCenterReleaseChanged;


        public event EventHandler ActionPropertyChanged;
        public event EventHandler<StickMapAction> ActionChanged;

        private bool usingRealAction = false;

        public StickAbsMousePropViewModel(Mapper mapper, StickMapAction action)
        {
            this.mapper = mapper;
            this.action = action as StickAbsMouse;
            usingRealAction = true;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                StickAbsMouse baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as StickAbsMouse;
                StickAbsMouse tempAction = new StickAbsMouse();
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

            NameChanged += StickAbsMousePropViewModel_NameChanged;
            DeadZoneChanged += StickAbsMousePropViewModel_DeadZoneChanged;
            MaxZoneChanged += StickAbsMousePropViewModel_MaxZoneChanged;
            AntiReleaseChanged += StickAbsMousePropViewModel_AntiReleaseChanged;
            UseOuterRingChanged += StickAbsMousePropViewModel_UseOuterRingChanged;
            OuterRingInvertChanged += StickAbsMousePropViewModel_OuterRingInvertChanged;
            OuterRingDeadZoneChanged += StickAbsMousePropViewModel_OuterRingDeadZoneChanged;
            SnapToCenterReleaseChanged += StickAbsMousePropViewModel_SnapToCenterReleaseChanged;
        }

        private void StickAbsMousePropViewModel_MaxZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.MAX_ZONE))
            {
                action.ChangedProperties.Add(StickAbsMouse.PropertyKeyStrings.MAX_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickAbsMouse.PropertyKeyStrings.MAX_ZONE);

            HighlightMaxZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickAbsMousePropViewModel_AntiReleaseChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.ANTI_RADIUS))
            {
                action.ChangedProperties.Add(StickAbsMouse.PropertyKeyStrings.ANTI_RADIUS);
            }

            action.RaiseNotifyPropertyChange(mapper, StickAbsMouse.PropertyKeyStrings.ANTI_RADIUS);

            HighlightAntiReleaseChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickAbsMousePropViewModel_OuterRingDeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.OUTER_RING_DEAD_ZONE))
            {
                action.ChangedProperties.Add(StickAbsMouse.PropertyKeyStrings.OUTER_RING_DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickAbsMouse.PropertyKeyStrings.OUTER_RING_DEAD_ZONE);

            HighlightOuterRingDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickAbsMousePropViewModel_OuterRingInvertChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.USE_AS_OUTER_RING))
            {
                action.ChangedProperties.Add(StickAbsMouse.PropertyKeyStrings.USE_AS_OUTER_RING);
            }

            action.RaiseNotifyPropertyChange(mapper, StickAbsMouse.PropertyKeyStrings.USE_AS_OUTER_RING);

            HighlightOuterRingInvertChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickAbsMousePropViewModel_UseOuterRingChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.USE_OUTER_RING))
            {
                action.ChangedProperties.Add(StickAbsMouse.PropertyKeyStrings.USE_OUTER_RING);
            }

            action.RaiseNotifyPropertyChange(mapper, StickAbsMouse.PropertyKeyStrings.USE_OUTER_RING);

            HighlightUseOuterRingChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickAbsMousePropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(StickAbsMouse.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickAbsMouse.PropertyKeyStrings.DEAD_ZONE);

            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickAbsMousePropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(StickAbsMouse.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, StickAbsMouse.PropertyKeyStrings.NAME);

            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickAbsMousePropViewModel_SnapToCenterReleaseChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickAbsMouse.PropertyKeyStrings.SNAP_TO_CENTER_RELEASE))
            {
                action.ChangedProperties.Add(StickAbsMouse.PropertyKeyStrings.SNAP_TO_CENTER_RELEASE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickAbsMouse.PropertyKeyStrings.SNAP_TO_CENTER_RELEASE);

            HighlightSnapToCenterReleaseChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PrepareModel()
        {

        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

                mapper.ProcessMappingChangeAction(() =>
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

                action.ChangedProperties.Add(StickAbsMouse.PropertyKeyStrings.OUTER_RING_BUTTON);
                //action.UseParentRingButton = false;

                resetEvent.Set();
            });

            resetEvent.Wait();
        }
    }
}
