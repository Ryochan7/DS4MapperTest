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
    public class TouchpadStickActionPropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private TouchpadStickAction action;
        public TouchpadStickAction Action
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

        private List<OutputStickSelectionItem> outputStickItems =
            new List<OutputStickSelectionItem>();
        public List<OutputStickSelectionItem> OutputStickItems => outputStickItems;

        private int outputStickIndex = -1;
        public int OutputStickIndex
        {
            get => outputStickIndex;
            set
            {
                outputStickIndex = value;
                OutputStickIndexChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OutputStickIndexChanged;

        public string DeadZone
        {
            get => action.DeadMod.DeadZone.ToString("N2");
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

        public string AntiDeadZone
        {
            get => action.DeadMod.AntiDeadZone.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.DeadMod.AntiDeadZone = Math.Clamp(temp, 0.0, 1.0);
                    AntiDeadZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler AntiDeadZoneChanged;

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

        public event EventHandler ActionPropertyChanged;

        private bool usingRealAction = false;

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightOutputStickIndex
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK);
        }
        public event EventHandler HighlightOutputStickIndexChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightAntiDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.ANTIDEAD_ZONE);
        }
        public event EventHandler HighlightAntiDeadZoneChanged;

        public bool HighlightMaxZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.MAX_ZONE);
        }
        public event EventHandler HighlightMaxZoneChanged;

        public TouchpadStickActionPropViewModel(Mapper mapper,
            TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TouchpadStickAction;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TouchpadStickAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TouchpadStickAction;
                TouchpadStickAction tempAction = new TouchpadStickAction();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
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


            NameChanged += TouchpadStickActionPropViewModel_NameChanged;
            DeadZoneChanged += TouchpadStickActionPropViewModel_DeadZoneChanged;
            AntiDeadZoneChanged += TouchpadStickActionPropViewModel_AntiDeadZoneChanged;
            MaxZoneChanged += TouchpadStickActionPropViewModel_MaxZoneChanged;
            OutputStickIndexChanged += MarkOutputStickChanged;
            ActionPropertyChanged += SetProfileDirty;

            OutputStickIndexChanged += TouchpadStickActionPropViewModel_OutputStickIndexChanged;
        }

        private void TouchpadStickActionPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.NAME))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MarkOutputStickChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK);
            HighlightOutputStickIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_MaxZoneChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.MAX_ZONE))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.MAX_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.MAX_ZONE);
            HighlightMaxZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_AntiDeadZoneChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.ANTIDEAD_ZONE))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.ANTIDEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.ANTIDEAD_ZONE);
            HighlightAntiDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

                mapper.QueueEvent(() =>
                {
                    this.action.ParentAction.Release(mapper, ignoreReleaseActions: true);

                    mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddTouchpadAction(this.action);
                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        mapper.ActionProfile.CurrentActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.SyncActions();
                    }

                    resetEvent.Set();
                });

                resetEvent.Wait();

                usingRealAction = true;
            }
        }

        private void TouchpadStickActionPropViewModel_OutputStickIndexChanged(object sender, EventArgs e)
        {
            OutputStickSelectionItem item = outputStickItems[outputStickIndex];
            mapper.QueueEvent(() =>
            {
                action.OutputAction.StickCode = item.Code;
                action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK);
                action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK);
            });
        }

        private void SetProfileDirty(object sender, EventArgs e)
        {
            mapper.ActionProfile.Dirty = true;
        }

        private void PrepareModel()
        {
            switch(action.OutputAction.StickCode)
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

    public class OutputStickSelectionItem
    {
        private string displayName;
        public string DisplayName
        {
            get => displayName;
        }

        private StickActionCodes code;
        public StickActionCodes Code => code;

        public OutputStickSelectionItem(string displayName, StickActionCodes code)
        {
            this.displayName = displayName;
            this.code = code;
        }
    }
}
