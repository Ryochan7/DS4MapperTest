using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using DS4MapperTest.DPadActions;
using DS4MapperTest.StickActions;

namespace DS4MapperTest.ViewModels
{
    public class DPadBindEditViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private DPadMapAction action;
        public DPadMapAction Action
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

        public DPadBindEditViewModel(Mapper mapper, DPadMapAction action)
        {
            this.mapper = mapper;
            this.action = action;
        }

        public DPadMapAction PrepareNewAction(int ind)
        {
            DPadMapAction result = null;
            switch (ind)
            {
                case 0:
                    result = new DPadNoAction();
                    break;
                case 1:
                    {
                        DPadTranslate tempAction = new DPadTranslate();
                        //var joyDefaults = mapper.DeviceActionDefaults.GrabTouchJoystickDefaults();
                        //joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 2:
                    {
                        DPadAction tempAction = new DPadAction();
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

        public void SwitchAction(DPadMapAction action)
        {
            DPadMapAction oldAction = this.action;
            DPadMapAction newAction = action;

            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

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
                    if (oldAction.Id != MapAction.DEFAULT_UNBOUND_ID)
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.ReplaceDPadAction(oldAction, newAction);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddDPadAction(newAction);
                    }

                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        MapAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[oldAction.MappingId];
                        if (MapAction.IsSameType(baseLayerAction, newAction))
                        {
                            newAction.SoftCopyFromParent(baseLayerAction as DPadMapAction);
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

        public void MigrateActionId(DPadMapAction newAction)
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

        public void UpdateAction(DPadMapAction newAction)
        {
            this.action = newAction;
        }
    }
}
