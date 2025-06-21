using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.StickActions;

namespace DS4MapperTest.ViewModels.StickActionPropViewModels
{
    public class StickFlickStickPropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private StickFlickStick action;
        public StickFlickStick Action
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

        public event EventHandler ActionPropertyChanged;
        public event EventHandler<StickMapAction> ActionChanged;

        private bool usingRealAction = false;

        public StickFlickStickPropViewModel(Mapper mapper, StickMapAction action)
        {
            this.mapper = mapper;
            this.action = action as StickFlickStick;
            usingRealAction = true;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                StickFlickStick baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as StickFlickStick;
                StickFlickStick tempAction = new StickFlickStick();
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

            NameChanged += StickFlickStickPropViewModel_NameChanged;
            RealWorldCalibrationChanged += StickFlickStickPropViewModel_RealWorldCalibrationChanged;
            FlickThresholdChanged += StickFlickStickPropViewModel_FlickThresholdChanged;
            FlickTimeChanged += StickFlickStickPropViewModel_FlickTimeChanged;
            MinAngleThresholdChanged += StickFlickStickPropViewModel_MinAngleThresholdChanged;
            InGameSensChanged += StickFlickStickPropViewModel_InGameSensChanged;
        }

        private void StickFlickStickPropViewModel_InGameSensChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.IN_GAME_SENS))
            {
                action.ChangedProperties.Add(StickFlickStick.PropertyKeyStrings.IN_GAME_SENS);
            }

            action.RaiseNotifyPropertyChange(mapper, StickFlickStick.PropertyKeyStrings.IN_GAME_SENS);
            HighlightInGameSensChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickFlickStickPropViewModel_MinAngleThresholdChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.MIN_ANGLE_THRESHOLD))
            {
                action.ChangedProperties.Add(StickFlickStick.PropertyKeyStrings.MIN_ANGLE_THRESHOLD);
            }

            action.RaiseNotifyPropertyChange(mapper, StickFlickStick.PropertyKeyStrings.MIN_ANGLE_THRESHOLD);
            HighlightMinAngleThresholdChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickFlickStickPropViewModel_FlickTimeChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.FLICK_TIME))
            {
                action.ChangedProperties.Add(StickFlickStick.PropertyKeyStrings.FLICK_TIME);
            }

            action.RaiseNotifyPropertyChange(mapper, StickFlickStick.PropertyKeyStrings.FLICK_TIME);
            HighlightFlickTimeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickFlickStickPropViewModel_FlickThresholdChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.FLICK_THRESHOLD))
            {
                action.ChangedProperties.Add(StickFlickStick.PropertyKeyStrings.FLICK_THRESHOLD);
            }

            action.RaiseNotifyPropertyChange(mapper, StickFlickStick.PropertyKeyStrings.FLICK_THRESHOLD);
            HighlightFlickThresholdChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickFlickStickPropViewModel_RealWorldCalibrationChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.REAL_WORLD_CALIBRATION))
            {
                action.ChangedProperties.Add(StickFlickStick.PropertyKeyStrings.REAL_WORLD_CALIBRATION);
            }

            action.RaiseNotifyPropertyChange(mapper, StickFlickStick.PropertyKeyStrings.REAL_WORLD_CALIBRATION);
            HighlightRealWorldCalibrationChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickFlickStickPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickFlickStick.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(StickFlickStick.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, StickFlickStick.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                mapper.ProcessMappingChangeAction(() =>
                {
                    this.action.ParentAction.Release(mapper, ignoreReleaseActions: true);

                    mapper.EditLayer.AddStickAction(this.action);
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
                });

                usingRealAction = true;

                ActionChanged?.Invoke(this, action);
            }
        }

        private void PrepareModel()
        {
        }
    }
}
