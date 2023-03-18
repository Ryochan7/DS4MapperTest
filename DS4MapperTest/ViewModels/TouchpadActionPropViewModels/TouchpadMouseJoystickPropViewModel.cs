using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickModifiers;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.ViewModels.Common;

namespace DS4MapperTest.ViewModels.TouchpadActionPropViewModels
{
    public class TouchpadMouseJoystickPropViewModel : TouchpadActionPropVMBase
    {
        private TouchpadMouseJoystick action;
        public TouchpadMouseJoystick Action
        {
            get => action;
        }

        private List<OutputStickSelectionItem> outputStickItems =
            new List<OutputStickSelectionItem>();
        public List<OutputStickSelectionItem> OutputStickItems => outputStickItems;

        private int outputStickIndex = -1;
        public int OutputStickIndex
        {
            get => outputStickIndex;
            set
            {
                if (outputStickIndex == value) return;
                outputStickIndex = value;
                OutputStickIndexChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OutputStickIndexChanged;

        public int DeadZone
        {
            get => action.MStickParams.deadZone;
            set
            {
                if (action.MStickParams.deadZone == value) return;
                action.MStickParams.deadZone = Math.Clamp(value, 0, 10000);
                DeadZoneChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeadZoneChanged;

        public int MaxZone
        {
            get => action.MStickParams.maxZone;
            set
            {
                if (action.MStickParams.maxZone == value) return;
                action.MStickParams.maxZone = value;
                MaxZoneChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MaxZoneChanged;

        public double AntiDeadZoneX
        {
            get => action.MStickParams.antiDeadzoneX;
            set
            {
                if (action.MStickParams.antiDeadzoneX == value) return;
                action.MStickParams.antiDeadzoneX = Math.Clamp(value, 0.0, 1.0);
                AntiDeadZoneXChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler AntiDeadZoneXChanged;

        public double AntiDeadZoneY
        {
            get => action.MStickParams.antiDeadzoneY;
            set
            {
                if (action.MStickParams.antiDeadzoneY == value) return;
                action.MStickParams.antiDeadzoneY = Math.Clamp(value, 0.0, 1.0);
                AntiDeadZoneYChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler AntiDeadZoneYChanged;

        public bool TrackballEnabled
        {
            get => action.MStickParams.trackballEnabled;
            set
            {
                action.MStickParams.trackballEnabled = value;
                TrackballEnabledChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TrackballEnabledChanged;

        public double TrackballFriction
        {
            get => action.MStickParams.TrackballFriction;
            set
            {
                action.MStickParams.TrackballFriction = (int)Math.Clamp(value, 0, 100);
                TrackballFrictionChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TrackballFrictionChanged;

        public bool JitterCompensation
        {
            get => action.MStickParams.jitterCompensation;
            set
            {
                action.MStickParams.jitterCompensation = value;
                JitterCompensationChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler JitterCompensationChanged;

        private List<EnumChoiceSelection<StickOutCurve.Curve>> outputCurveItems =
            new List<EnumChoiceSelection<StickOutCurve.Curve>>()
            {
                new EnumChoiceSelection<StickOutCurve.Curve>("Linear", StickOutCurve.Curve.Linear),
                new EnumChoiceSelection<StickOutCurve.Curve>("EaseOut Quad", StickOutCurve.Curve.EaseoutQuad),
                new EnumChoiceSelection<StickOutCurve.Curve>("EaseOut Cubic", StickOutCurve.Curve.EaseoutCubic),
                new EnumChoiceSelection<StickOutCurve.Curve>("Enhanced Precision", StickOutCurve.Curve.EnhancedPrecision),
                new EnumChoiceSelection<StickOutCurve.Curve>("Quadratic", StickOutCurve.Curve.Quadratic),
                new EnumChoiceSelection<StickOutCurve.Curve>("Cubic", StickOutCurve.Curve.Cubic),
            };

        public List<EnumChoiceSelection<StickOutCurve.Curve>> OutputCurveItems => outputCurveItems;

        public StickOutCurve.Curve OutputCurve
        {
            get => action.MStickParams.outputCurve;
            set
            {
                if (action.MStickParams.outputCurve == value) return;
                action.MStickParams.outputCurve = value;
                OutputCurveChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OutputCurveChanged;

        public bool SmoothingEnabled
        {
            get => action.MStickParams.smoothing;
            set
            {
                if (action.MStickParams.smoothing == value) return;
                action.MStickParams.smoothing = value;
                SmoothingEnabledChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SmoothingEnabledChanged;

        public double SmoothingMinCutoff
        {
            get => action.MStickParams.smoothingFilterSettings.minCutOff;
            set
            {
                if (action.MStickParams.smoothingFilterSettings.minCutOff == value) return;
                action.MStickParams.smoothingFilterSettings.minCutOff = Math.Clamp(value, 0.0, 10.0);
                SmoothingMinCutoffChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SmoothingMinCutoffChanged;

        public double SmoothingBeta
        {
            get => action.MStickParams.smoothingFilterSettings.beta;
            set
            {
                if (action.MStickParams.smoothingFilterSettings.beta == value) return;
                action.MStickParams.smoothingFilterSettings.beta = Math.Clamp(value, 0.0, 1.0);
                SmoothingBetaChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SmoothingBetaChanged;

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightOutputStick
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.OUTPUT_STICK);
        }
        public event EventHandler HighlightOutputStickChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightMaxZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.MAX_ZONE);
        }
        public event EventHandler HighlightMaxZoneChanged;

        public bool HighlightAntiDeadZoneX
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_X);
        }
        public event EventHandler HighlightAntiDeadZoneXChanged;

        public bool HighlightTrackballEnabled
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.TRACKBALL_MODE);
        }
        public event EventHandler HighlightTrackballEnabledChanged;

        public bool HighlightTrackballFriction
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.TRACKBALL_FRICTION);
        }
        public event EventHandler HighlightTrackballFrictionChanged;

        public bool HighlightAntiDeadZoneY
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_Y);
        }
        public event EventHandler HighlightAntiDeadZoneYChanged;

        public bool HighlightJitterCompensation
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.JITTER_COMPENSATION);
        }
        public event EventHandler HighlightJitterCompensationChanged;

        public bool HighlightOutputCurve
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.OUTPUT_CURVE);
        }
        public event EventHandler HighlightOutputCurveChanged;

        public bool HighlightSmoothingEnabled
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_ENABLED);
        }
        public event EventHandler HighlightSmoothingEnabledChanged;

        public bool HighlightSmoothingFilter
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER);
        }
        public event EventHandler HighlightSmoothingFilterChanged;

        public override event EventHandler ActionPropertyChanged;

        public TouchpadMouseJoystickPropViewModel(Mapper mapper,
            TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TouchpadMouseJoystick;
            this.baseAction = action;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TouchpadMouseJoystick baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TouchpadMouseJoystick;
                TouchpadMouseJoystick tempAction = new TouchpadMouseJoystick();
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

            outputStickItems.AddRange(new OutputStickSelectionItem[]
            {
                new OutputStickSelectionItem("Unbound", StickActionCodes.Empty),
                new OutputStickSelectionItem("Left Stick", StickActionCodes.X360_LS),
                new OutputStickSelectionItem("Right Stick", StickActionCodes.X360_RS),
            });

            PrepareModel();

            NameChanged += TouchpadMouseJoystickPropViewModel_NameChanged;
            OutputStickIndexChanged += TouchpadMouseJoystickPropViewModel_OutputStickIndexChanged;
            DeadZoneChanged += TouchpadMouseJoystickPropViewModel_DeadZoneChanged;
            MaxZoneChanged += TouchpadMouseJoystickPropViewModel_MaxZoneChanged;
            AntiDeadZoneXChanged += TouchpadMouseJoystickPropViewModel_AntiDeadZoneXChanged;
            AntiDeadZoneYChanged += TouchpadMouseJoystickPropViewModel_AntiDeadZoneYChanged;
            TrackballEnabledChanged += TouchpadMouseJoystickPropViewModel_TrackballEnabledChanged;
            TrackballFrictionChanged += TouchpadMouseJoystickPropViewModel_TrackballFrictionChanged;
            JitterCompensationChanged += TouchpadMouseJoystickPropViewModel_JitterCompensationChanged;
            OutputCurveChanged += TouchpadMouseJoystickPropViewModel_OutputCurveChanged;
            SmoothingEnabledChanged += TouchpadMouseJoystickPropViewModel_SmoothingEnabledChanged;
            SmoothingMinCutoffChanged += TouchpadMouseJoystickPropViewModel_SmoothingMinCutoffChanged;
            SmoothingBetaChanged += TouchpadMouseJoystickPropViewModel_SmoothingBetaChanged;
        }

        private void TouchpadMouseJoystickPropViewModel_JitterCompensationChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.JITTER_COMPENSATION))
            {
                action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.JITTER_COMPENSATION);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.JITTER_COMPENSATION);
            HighlightJitterCompensationChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_OutputCurveChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.OUTPUT_CURVE))
            {
                this.action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.OUTPUT_CURVE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.OUTPUT_CURVE);
            HighlightOutputCurveChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_TrackballFrictionChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.TRACKBALL_FRICTION))
            {
                this.action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.TRACKBALL_FRICTION);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.TRACKBALL_FRICTION);
            });

            HighlightTrackballFrictionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_TrackballEnabledChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.TRACKBALL_MODE))
            {
                this.action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.TRACKBALL_MODE);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.TRACKBALL_MODE);
            });

            HighlightTrackballEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_MaxZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.MAX_ZONE))
            {
                action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.MAX_ZONE);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.MAX_ZONE);
            });

            HighlightMaxZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_AntiDeadZoneYChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_Y))
            {
                action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_Y);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_Y);
            });

            HighlightAntiDeadZoneYChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_AntiDeadZoneXChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_X))
            {
                action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_X);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_X);
            });

            HighlightAntiDeadZoneXChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_SmoothingBetaChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER))
            {
                this.action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER);
                action.MStickParams.smoothingFilterSettings.UpdateSmoothingFilters();
            });

            HighlightSmoothingFilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_SmoothingMinCutoffChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER))
            {
                this.action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER);
                action.MStickParams.smoothingFilterSettings.UpdateSmoothingFilters();
            });

            HighlightSmoothingFilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_SmoothingEnabledChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_ENABLED))
            {
                this.action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_ENABLED);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.SMOOTHING_ENABLED);
            HighlightSmoothingEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.NAME))
            {
                this.action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadMouseJoystickPropViewModel_OutputStickIndexChanged(object sender, EventArgs e)
        {
            OutputStickSelectionItem item = outputStickItems[outputStickIndex];

            if (!this.action.ChangedProperties.Contains(TouchpadMouseJoystick.PropertyKeyStrings.OUTPUT_STICK))
            {
                this.action.ChangedProperties.Add(TouchpadMouseJoystick.PropertyKeyStrings.OUTPUT_STICK);
            }

            ExecuteInMapperThread(() =>
            {
                action.MStickParams.OutputStick = item.Code;
                action.RaiseNotifyPropertyChange(mapper, TouchpadMouseJoystick.PropertyKeyStrings.OUTPUT_STICK);
            });

            HighlightOutputStickChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PrepareModel()
        {
            switch (action.MStickParams.OutputStick)
            {
                case StickActionCodes.Empty:
                    outputStickIndex = 0;
                    break;
                case StickActionCodes.X360_LS:
                    outputStickIndex = 1;
                    break;
                case StickActionCodes.X360_RS:
                    outputStickIndex = 2;
                    break;
                default:
                    break;
            }
        }
    }
}
