using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.ViewModels
{
    public class TouchpadBindEditViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private TouchpadMapAction action;
        public TouchpadMapAction Action
        {
            get => action;
        }

        // Keep reference to current screen
        private UserControl displayControl;
        public UserControl DisplayControl
        {
            get => displayControl;
            set
            {
                displayControl = value;
                DisplayControlChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DisplayControlChanged;

        // Keep reference to main screen for action editor
        private UserControl actionBaseDisplayControl;
        public UserControl ActionBaseDisplayControl
        {
            get => actionBaseDisplayControl;
            set
            {
                actionBaseDisplayControl = value;
            }
        }

        public string InputControlName
        {
            get
            {
                string result = "";
                if (mapper.BindingDict.TryGetValue(action.MappingId,
                    out InputBindingMeta tempMeta))
                {
                    result = tempMeta.displayName;
                }

                return result;
            }
        }

        public TouchpadBindEditViewModel(Mapper mapper, TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action;
        }

        public TouchpadMapAction PrepareNewAction(int ind)
        {
            TouchpadMapAction result = null;
            switch (ind)
            {
                case 0:
                    result = new TouchpadNoAction();
                    break;
                case 1:
                    {
                        TouchpadStickAction tempAction = new TouchpadStickAction();
                        // TODO: Fix
                        var joyDefaults = mapper.DeviceActionDefaults.GrabTouchJoystickDefaults();
                        joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 2:
                    {
                        TouchpadActionPad tempAction = new TouchpadActionPad();
                        // TODO: Fix
                        var joyDefaults = mapper.DeviceActionDefaults.GrabTouchActionPadDefaults();
                        joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 3:
                    {
                        TouchpadMouseJoystick tempAction = new TouchpadMouseJoystick();
                        // TODO: Fix
                        var joyDefaults = mapper.DeviceActionDefaults.GrabTouchMouseJoystickDefaults();
                        joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 4:
                    {
                        TouchpadMouse tempAction = new TouchpadMouse();
                        // TODO: Fix
                        var joyDefaults = mapper.DeviceActionDefaults.GrabTouchMouseDefaults();
                        joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 5:
                    {
                        TouchpadCircular tempAction = new TouchpadCircular();
                        // TODO: Fix
                        var joyDefaults = mapper.DeviceActionDefaults.GrabTouchCircularActionDefaults();
                        joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 6:
                    {
                        TouchpadAbsAction tempAction = new TouchpadAbsAction();
                        //var joyDefaults = mapper.DeviceActionDefaults.GrabTouchMouseDefaults();
                        //joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 7:
                    {
                        TouchpadDirectionalSwipe tempAction = new TouchpadDirectionalSwipe();
                        var joyDefaults = mapper.DeviceActionDefaults.GetTouchDirectionSwipeActionDefaults();
                        joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 8:
                    {
                        TouchpadSingleButton tempAction = new TouchpadSingleButton();
                        //var joyDefaults = mapper.DeviceActionDefaults.GrabTouchCircularActionDefaults();
                        //joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                default:
                    break;
            }
            return result;
        }

        public void SwitchAction(TouchpadMapAction action)
        {
            TouchpadMapAction oldAction = this.action;
            TouchpadMapAction newAction = action;

            mapper.ProcessMappingChangeAction(() =>
            {
                oldAction.Release(mapper, ignoreReleaseActions: true);
                //int tempInd = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.LayerActions.FindIndex((item) => item == tempAction);
                //if (tempInd >= 0)
                {
                    //mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.LayerActions.RemoveAt(tempInd);
                    //mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.LayerActions.Insert(tempInd, newAction);

                    //oldAction.Release(mapper, ignoreReleaseActions: true);

                    //mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddTouchpadAction(this.action);
                    bool exists = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(oldAction);
                    //if (oldAction.Id != MapAction.DEFAULT_UNBOUND_ID)
                    if (exists)
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.ReplaceTouchpadAction(oldAction, newAction);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddTouchpadAction(newAction);
                    }

                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        MapAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[oldAction.MappingId];
                        if (MapAction.IsSameType(baseLayerAction, newAction))
                        {
                            newAction.SoftCopyFromParent(baseLayerAction as TouchpadMapAction);
                        }

                        mapper.ActionProfile.CurrentActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.SyncActions();
                        mapper.ActionProfile.CurrentActionSet.ClearCompositeLayerActions();
                        mapper.ActionProfile.CurrentActionSet.PrepareCompositeLayer();
                    }
                }
            });

            this.action = action;
        }

        public void MigrateActionId(TouchpadMapAction newAction)
        {
            if (action.Id == MapAction.DEFAULT_UNBOUND_ID)
            {
                // Need to create new ID for action
                newAction.Id = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
            }
            else
            {
                // Can re-use existing ID
                newAction.Id = action.Id;
            }
        }

        public void UpdateAction(TouchpadMapAction currentAction)
        {
            this.action = currentAction;
        }
    }
}
