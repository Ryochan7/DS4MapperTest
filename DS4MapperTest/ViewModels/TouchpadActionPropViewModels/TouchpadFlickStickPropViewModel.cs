using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.ViewModels.TouchpadActionPropViewModels
{
    public class TouchpadFlickStickPropViewModel : TouchpadActionPropVMBase
    {
        private TouchpadFlickStick action;
        public TouchpadFlickStick Action
        {
            get => action;
        }

        public double RealWorldCalibration
        {
            get => action.RealWorldCalibration;
            set
            {
                if (action.RealWorldCalibration == value) return;
                action.RealWorldCalibration = value;
                RealWorldCalibrationChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RealWorldCalibrationChanged;

        public double FlickThreshold
        {
            get => action.FlickThreshold;
            set
            {
                if (action.FlickThreshold == value) return;
                action.FlickThreshold = value;
                FlickThresholdChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler FlickThresholdChanged;

        public double FlickTime
        {
            get => action.FlickTime;
            set
            {
                if (action.FlickTime == value) return;
                action.FlickTime = value;
                FlickTimeChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler FlickTimeChanged;

        public double MinAngleThreshold
        {
            get => action.MinAngleThreshold;
            set
            {
                if (action.MinAngleThreshold == value) return;
                action.MinAngleThreshold = value;
                MinAngleThresholdChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MinAngleThresholdChanged;

        public double InGameSens
        {
            get => action.InGameSens;
            set
            {
                if (action.InGameSens == value) return;
                action.InGameSens = value;
                InGameSensChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler InGameSensChanged;

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightRealWorldCalibration
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.REAL_WORLD_CALIBRATION);
        }
        public event EventHandler HighlightRealWorldCalibrationChanged;

        public bool HighlightFlickThreshold
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.FLICK_THRESHOLD);
        }
        public event EventHandler HighlightFlickThresholdChanged;

        public bool HighlightFlickTime
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.FLICK_TIME);
        }
        public event EventHandler HighlightFlickTimeChanged;

        public bool HighlightMinAngleThreshold
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.MIN_ANGLE_THRESHOLD);
        }
        public event EventHandler HighlightMinAngleThresholdChanged;

        public bool HighlightInGameSens
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.IN_GAME_SENS);
        }
        public event EventHandler HighlightInGameSensChanged;

        public override event EventHandler ActionPropertyChanged;

        public TouchpadFlickStickPropViewModel(Mapper mapper, TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TouchpadFlickStick;
            usingRealAction = true;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TouchpadFlickStick baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TouchpadFlickStick;
                TouchpadFlickStick tempAction = new TouchpadFlickStick();
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

            NameChanged += TouchpadFlickStickPropViewModel_NameChanged;
            RealWorldCalibrationChanged += TouchpadFlickStickPropViewModel_RealWorldCalibrationChanged;
            FlickThresholdChanged += TouchpadFlickStickPropViewModel_FlickThresholdChanged;
            FlickTimeChanged += TouchpadFlickStickPropViewModel_FlickTimeChanged;
            MinAngleThresholdChanged += TouchpadFlickStickPropViewModel_MinAngleThresholdChanged;
            InGameSensChanged += TouchpadFlickStickPropViewModel_InGameSensChanged;
        }

        private void TouchpadFlickStickPropViewModel_InGameSensChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadFlickStick.PropertyKeyStrings.IN_GAME_SENS))
            {
                action.ChangedProperties.Add(TouchpadFlickStick.PropertyKeyStrings.IN_GAME_SENS);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadFlickStick.PropertyKeyStrings.IN_GAME_SENS);
            HighlightInGameSensChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadFlickStickPropViewModel_MinAngleThresholdChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadFlickStick.PropertyKeyStrings.MIN_ANGLE_THRESHOLD))
            {
                action.ChangedProperties.Add(TouchpadFlickStick.PropertyKeyStrings.MIN_ANGLE_THRESHOLD);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadFlickStick.PropertyKeyStrings.MIN_ANGLE_THRESHOLD);
            HighlightMinAngleThresholdChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadFlickStickPropViewModel_FlickTimeChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadFlickStick.PropertyKeyStrings.FLICK_TIME))
            {
                action.ChangedProperties.Add(TouchpadFlickStick.PropertyKeyStrings.FLICK_TIME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadFlickStick.PropertyKeyStrings.FLICK_TIME);
            HighlightFlickTimeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadFlickStickPropViewModel_FlickThresholdChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadFlickStick.PropertyKeyStrings.FLICK_THRESHOLD))
            {
                action.ChangedProperties.Add(TouchpadFlickStick.PropertyKeyStrings.FLICK_THRESHOLD);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadFlickStick.PropertyKeyStrings.FLICK_THRESHOLD);
            HighlightFlickThresholdChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadFlickStickPropViewModel_RealWorldCalibrationChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadFlickStick.PropertyKeyStrings.REAL_WORLD_CALIBRATION))
            {
                action.ChangedProperties.Add(TouchpadFlickStick.PropertyKeyStrings.REAL_WORLD_CALIBRATION);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadFlickStick.PropertyKeyStrings.REAL_WORLD_CALIBRATION);
            HighlightRealWorldCalibrationChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadFlickStickPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(TouchpadFlickStick.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(TouchpadFlickStick.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadFlickStick.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PrepareModel()
        {
            
        }
    }
}
