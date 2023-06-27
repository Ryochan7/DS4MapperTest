using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using DS4MapperTest.StickActions;

namespace DS4MapperTest.ViewModels
{
    public class StickBindEditViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private StickMapAction action;
        public StickMapAction Action
        {
            get => action;
        }

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

        public StickBindEditViewModel(Mapper mapper, StickMapAction action)
        {
            this.mapper = mapper;
            this.action = action;
        }

        public StickMapAction PrepareNewAction(int ind)
        {
            StickMapAction result = null;
            switch (ind)
            {
                case 0:
                    result = new StickNoAction();
                    break;
                case 1:
                    {
                        StickTranslate tempAction = new StickTranslate();
                        var joyDefaults = mapper.DeviceActionDefaults.GrabStickTranslateActionDefaults();
                        joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 2:
                    {
                        StickPadAction tempAction = new StickPadAction();
                        var joyDefaults = mapper.DeviceActionDefaults.GrabStickPadActionActionDefaults();
                        joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 3:
                    {
                        StickMouse tempAction = new StickMouse();
                        var joyDefaults = mapper.DeviceActionDefaults.GrabStickMouseActionDefaults();
                        joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 4:
                    {
                        StickCircular tempAction = new StickCircular();
                        //var joyDefaults = mapper.DeviceActionDefaults.GrabTouchActionPadDefaults();
                        //joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                default:
                    break;
            }

            return result;
        }

        public void SwitchAction(StickMapAction action)
        {
            StickMapAction oldAction = this.action;
            StickMapAction newAction = action;

            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            mapper.QueueEvent(() =>
            {
                oldAction.Release(mapper, ignoreReleaseActions: true);
                //int tempInd = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.LayerActions.FindIndex((item) => item == tempAction);
                //if (tempInd >= 0)
                {
                    //mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.LayerActions.RemoveAt(tempInd);
                    //mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.LayerActions.Insert(tempInd, newAction);

                    //oldAction.Release(mapper, ignoreReleaseActions: true);

                    //mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddTouchpadAction(this.action);
                    if (oldAction.Id != MapAction.DEFAULT_UNBOUND_ID)
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.ReplaceStickAction(oldAction, newAction);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddStickAction(newAction);
                    }

                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        MapAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[oldAction.MappingId];
                        if (MapAction.IsSameType(baseLayerAction, newAction))
                        {
                            newAction.SoftCopyFromParent(baseLayerAction as StickMapAction);
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

                resetEvent.Set();
            });

            resetEvent.Wait();

            this.action = action;
        }

        public void MigrateActionId(StickMapAction newAction)
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

        public void UpdateAction(StickMapAction newAction)
        {
            this.action = newAction;
        }
    }
}
