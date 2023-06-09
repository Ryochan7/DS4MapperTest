using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.ViewModels.Common;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.ViewModels.TouchpadActionPropViewModels
{
    public class TouchpadAbsMousePropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private TouchpadAbsAction action;
        public TouchpadAbsAction Action
        {
            get => action;
        }

        public string Name
        {
            get => action.Name;
            set
            {
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
                action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightMaxZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.MAX_ZONE);
        }
        public event EventHandler HighlightMaxZoneChanged;

        public bool HighlightAntiRelease
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.ANTI_RADIUS);
        }
        public event EventHandler HighlightAntiReleaseChanged;

        public bool HighlightUseOuterRing
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.USE_OUTER_RING);
        }
        public event EventHandler HighlightUseOuterRingChanged;

        public bool HighlightOuterRingInvert
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.USE_AS_OUTER_RING);
        }
        public event EventHandler HighlightOuterRingInvertChanged;

        public bool HighlightOuterRingDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.OUTER_RING_DEAD_ZONE);
        }
        public event EventHandler HighlightOuterRingDeadZoneChanged;

        public bool HighlightOuterRingRangeChoice
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.OUTER_RING_FULL_RANGE);
        }
        public event EventHandler HighlightOuterRingRangeChoiceChanged;

        public bool HighlightSnapToCenterRelease
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.SNAP_TO_CENTER_RELEASE);
        }
        public event EventHandler HighlightSnapToCenterReleaseChanged;

        public event EventHandler ActionPropertyChanged;

        private bool usingRealAction = true;

        public TouchpadAbsMousePropViewModel(Mapper mapper,
            TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TouchpadAbsAction;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TouchpadAbsAction baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TouchpadAbsAction;
                TouchpadAbsAction tempAction = new TouchpadAbsAction();
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

            NameChanged += TouchpadAbsMousePropViewModel_NameChanged;
            DeadZoneChanged += TouchpadAbsMousePropViewModel_DeadZoneChanged;
            MaxZoneChanged += TouchpadAbsMousePropViewModel_MaxZoneChanged;
            AntiReleaseChanged += TouchpadAbsMousePropViewModel_AntiReleaseChanged;
            UseOuterRingChanged += TouchpadAbsMousePropViewModel_UseOuterRingChanged;
            OuterRingInvertChanged += TouchpadAbsMousePropViewModel_OuterRingInvertChanged;
            OuterRingDeadZoneChanged += TouchpadAbsMousePropViewModel_OuterRingDeadZoneChanged;
            OuterRingRangeChoiceChanged += TouchpadAbsMousePropViewModel_OuterRingRangeChoiceChanged;
            SnapToCenterReleaseChanged += TouchpadAbsMousePropViewModel_SnapToCenterReleaseChanged;
        }

        private void TouchpadAbsMousePropViewModel_AntiReleaseChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.ANTI_RADIUS))
            {
                action.ChangedProperties.Add(TouchpadAbsAction.PropertyKeyStrings.ANTI_RADIUS);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadAbsAction.PropertyKeyStrings.ANTI_RADIUS);

            HighlightAntiReleaseChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadAbsMousePropViewModel_MaxZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.MAX_ZONE))
            {
                action.ChangedProperties.Add(TouchpadAbsAction.PropertyKeyStrings.MAX_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadAbsAction.PropertyKeyStrings.MAX_ZONE);

            HighlightMaxZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadAbsMousePropViewModel_OuterRingRangeChoiceChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.OUTER_RING_FULL_RANGE))
            {
                action.ChangedProperties.Add(TouchpadAbsAction.PropertyKeyStrings.OUTER_RING_FULL_RANGE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadAbsAction.PropertyKeyStrings.OUTER_RING_FULL_RANGE);

            HighlightOuterRingDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadAbsMousePropViewModel_SnapToCenterReleaseChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.SNAP_TO_CENTER_RELEASE))
            {
                action.ChangedProperties.Add(TouchpadAbsAction.PropertyKeyStrings.SNAP_TO_CENTER_RELEASE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadAbsAction.PropertyKeyStrings.SNAP_TO_CENTER_RELEASE);

            HighlightSnapToCenterReleaseChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadAbsMousePropViewModel_OuterRingDeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.OUTER_RING_DEAD_ZONE))
            {
                action.ChangedProperties.Add(TouchpadAbsAction.PropertyKeyStrings.OUTER_RING_DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadAbsAction.PropertyKeyStrings.OUTER_RING_DEAD_ZONE);

            HighlightOuterRingDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadAbsMousePropViewModel_OuterRingInvertChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.USE_AS_OUTER_RING))
            {
                action.ChangedProperties.Add(TouchpadAbsAction.PropertyKeyStrings.USE_AS_OUTER_RING);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadAbsAction.PropertyKeyStrings.USE_AS_OUTER_RING);

            HighlightOuterRingInvertChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadAbsMousePropViewModel_UseOuterRingChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.USE_OUTER_RING))
            {
                action.ChangedProperties.Add(TouchpadAbsAction.PropertyKeyStrings.USE_OUTER_RING);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadAbsAction.PropertyKeyStrings.USE_OUTER_RING);

            HighlightUseOuterRingChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadAbsMousePropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(TouchpadAbsAction.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadAbsAction.PropertyKeyStrings.DEAD_ZONE);

            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadAbsMousePropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadAbsAction.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(TouchpadAbsAction.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadAbsAction.PropertyKeyStrings.NAME);

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

                    mapper.EditLayer.AddTouchpadAction(this.action);
                    if (mapper.EditActionSet.UsingCompositeLayer)
                    {
                        mapper.EditActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.EditLayer.SyncActions();
                    }

                    resetEvent.Set();
                });

                resetEvent.Wait();

                usingRealAction = true;
            }
        }

        private void PrepareModel()
        {

        }

        public void UpdateRingButton(ButtonAction oldAction, ButtonAction newAction)
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
                    action.RingButton = newAction as AxisDirButton;
                }

                action.ChangedProperties.Add(TouchpadAbsAction.PropertyKeyStrings.OUTER_RING_BUTTON);
                action.UseParentRingButton = false;

                resetEvent.Set();
            });

            resetEvent.Wait();
        }
    }
}
