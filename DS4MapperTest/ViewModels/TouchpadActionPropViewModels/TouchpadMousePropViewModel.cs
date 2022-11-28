using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.ViewModels.TouchpadActionPropViewModels
{
    public class TouchpadMousePropViewModel : TouchpadActionPropVMBase
    {
        private TouchpadMouse action;
        public TouchpadMouse Action => action;

        public string DeadZone
        {
            get => action.DeadZone.ToString();
            set
            {
                if (int.TryParse(value, out int temp))
                {
                    action.DeadZone = Math.Clamp(temp, 0, 10000);
                    DeadZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler DeadZoneChanged;

        public bool TrackballEnabled
        {
            get => action.TrackballEnabled;
            set
            {
                action.TrackballEnabled = value;
                TrackballEnabledChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TrackballEnabledChanged;

        public string TrackballFriction
        {
            get => action.TrackballFriction.ToString();
            set
            {
                if (int.TryParse(value, out int temp))
                {
                    action.TrackballFriction = Math.Clamp(temp, 0, 100);
                    TrackballFrictionChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler TrackballFrictionChanged;

        public double Sensitivity
        {
            get => action.Sensitivity;
            set
            {
                action.Sensitivity = Math.Clamp(value, 0.0, 10.0);
                SensitivityChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SensitivityChanged;

        public double VerticalScale
        {
            get => action.VerticalScale;
            set
            {
                action.VerticalScale = Math.Clamp(value, 0.0, 10.0);
                VerticalScaleChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler VerticalScaleChanged;

        public bool SmoothingEnabled
        {
            get => action.SmoothingEnabled;
            set
            {
                if (action.SmoothingEnabled == value) return;
                action.SmoothingEnabled = value;
                SmoothingEnabledChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SmoothingEnabledChanged;

        public double SmoothingMinCutoff
        {
            get => action.ActionSmoothingSettings.minCutOff;
            set
            {
                if (action.ActionSmoothingSettings.minCutOff == value) return;
                action.ActionSmoothingSettings.minCutOff = Math.Clamp(value, 0.0, 10.0);
                SmoothingMinCutoffChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SmoothingMinCutoffChanged;

        public double SmoothingBeta
        {
            get => action.ActionSmoothingSettings.beta;
            set
            {
                if (action.ActionSmoothingSettings.beta == value) return;
                action.ActionSmoothingSettings.beta = Math.Clamp(value, 0.0, 1.0);
                SmoothingBetaChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SmoothingBetaChanged;

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightTrackballEnabled
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.TRACKBALL_MODE);
        }
        public event EventHandler HighlightTrackballEnabledChanged;

        public bool HighlightTrackballFriction
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.TRACKBALL_FRICTION);
        }
        public event EventHandler HighlightTrackballFrictionChanged;

        public bool HighlightSensitivity
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.SENSITIVITY);
        }
        public event EventHandler HighlightSensitivityChanged;

        public bool HighlightVerticalScale
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.VERTICAL_SCALE);
        }
        public event EventHandler HighlightVerticalScaleChanged;

        public bool HighlightSmoothingEnabled
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.SMOOTHING_ENABLED);
        }
        public event EventHandler HighlightSmoothingEnabledChanged;

        public bool HighlightSmoothingFilter
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.SMOOTHING_FILTER);
        }
        public event EventHandler HighlightSmoothingFilterChanged;

        public override event EventHandler ActionPropertyChanged;

        public TouchpadMousePropViewModel(Mapper mapper, TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TouchpadMouse;
            this.baseAction = action;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TouchpadMouse baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TouchpadMouse;
                TouchpadMouse tempAction = new TouchpadMouse();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
                this.baseAction = this.action;
                usingRealAction = false;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            NameChanged += TouchpadMousePropViewModel_NameChanged;
            DeadZoneChanged += TouchpadMousePropViewModel_DeadZoneChanged;
            TrackballEnabledChanged += TouchpadMousePropViewModel_TrackballEnabledChanged;
            TrackballFrictionChanged += TouchpadMousePropViewModel_TrackballFrictionChanged;
            SensitivityChanged += TouchpadMousePropViewModel_SensitivityChanged;
            VerticalScaleChanged += TouchpadMousePropViewModel_VerticalScaleChanged;
            SmoothingEnabledChanged += TouchpadMousePropViewModel_SmoothingEnabledChanged;
            SmoothingMinCutoffChanged += TouchpadMousePropViewModel_SmoothingMinCutoffChanged;
            SmoothingBetaChanged += TouchpadMousePropViewModel_SmoothingBetaChanged;
            ActionPropertyChanged += SetProfileDirty;
        }

        private void TouchpadMousePropViewModel_SmoothingMinCutoffChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.SMOOTHING_FILTER))
            {
                this.action.ChangedProperties.Add(TouchpadMouse.PropertyKeyStrings.SMOOTHING_FILTER);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouse.PropertyKeyStrings.SMOOTHING_FILTER);
                action.ActionSmoothingSettings.UpdateSmoothingFilters();
            });

            HighlightSmoothingFilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMousePropViewModel_SmoothingBetaChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.SMOOTHING_FILTER))
            {
                this.action.ChangedProperties.Add(TouchpadMouse.PropertyKeyStrings.SMOOTHING_FILTER);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouse.PropertyKeyStrings.SMOOTHING_FILTER);
                action.ActionSmoothingSettings.UpdateSmoothingFilters();
            });

            HighlightSmoothingFilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMousePropViewModel_SmoothingEnabledChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.SMOOTHING_ENABLED))
            {
                this.action.ChangedProperties.Add(TouchpadMouse.PropertyKeyStrings.SMOOTHING_ENABLED);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadMouse.PropertyKeyStrings.SMOOTHING_ENABLED);
            HighlightSmoothingEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMousePropViewModel_VerticalScaleChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.VERTICAL_SCALE))
            {
                this.action.ChangedProperties.Add(TouchpadMouse.PropertyKeyStrings.VERTICAL_SCALE);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouse.PropertyKeyStrings.VERTICAL_SCALE);
            });

            HighlightVerticalScaleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMousePropViewModel_SensitivityChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.SENSITIVITY))
            {
                this.action.ChangedProperties.Add(TouchpadMouse.PropertyKeyStrings.SENSITIVITY);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadMouse.PropertyKeyStrings.SENSITIVITY);
            HighlightSensitivityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMousePropViewModel_TrackballFrictionChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.TRACKBALL_FRICTION))
            {
                this.action.ChangedProperties.Add(TouchpadMouse.PropertyKeyStrings.TRACKBALL_FRICTION);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouse.PropertyKeyStrings.TRACKBALL_FRICTION);
            });

            HighlightTrackballFrictionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMousePropViewModel_TrackballEnabledChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.TRACKBALL_MODE))
            {
                this.action.ChangedProperties.Add(TouchpadMouse.PropertyKeyStrings.TRACKBALL_MODE);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouse.PropertyKeyStrings.TRACKBALL_MODE);
            });

            HighlightTrackballEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SetProfileDirty(object sender, EventArgs e)
        {
            mapper.ActionProfile.Dirty = true;
        }

        private void TouchpadMousePropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.DEAD_ZONE))
            {
                this.action.ChangedProperties.Add(TouchpadMouse.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadMouse.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMousePropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouse.PropertyKeyStrings.NAME))
            {
                this.action.ChangedProperties.Add(TouchpadMouse.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadMouse.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
