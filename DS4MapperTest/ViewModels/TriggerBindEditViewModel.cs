using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using DS4MapperTest.TriggerActions;

namespace DS4MapperTest.ViewModels
{
    public class TriggerBindEditViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private TriggerMapAction action;
        public TriggerMapAction Action
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

        public TriggerBindEditViewModel(Mapper mapper, TriggerMapAction action)
        {
            this.mapper = mapper;
            this.action = action;
        }

        public TriggerMapAction PrepareNewAction(int ind)
        {
            TriggerMapAction result = null;
            switch (ind)
            {
                case 0:
                    result = new TriggerNoAction();
                    break;
                case 1:
                    {
                        TriggerTranslate tempAction = new TriggerTranslate();
                        //var joyDefaults = mapper.DeviceActionDefaults.GrabTouchJoystickDefaults();
                        //joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 2:
                    {
                        TriggerDualStageAction tempAction = new TriggerDualStageAction();
                        //var joyDefaults = mapper.DeviceActionDefaults.GrabTouchActionPadDefaults();
                        //joyDefaults.Process(tempAction);
                        result = tempAction;
                    }

                    break;
                case 3:
                    {
                        TriggerButtonAction tempAction = new TriggerButtonAction();
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

        public void SwitchAction(TriggerMapAction action)
        {
            TriggerMapAction oldAction = this.action;
            TriggerMapAction newAction = action;

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
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.ReplaceTriggerAction(oldAction, newAction);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddTriggerAction(newAction);
                    }

                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        MapAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[oldAction.MappingId];
                        if (MapAction.IsSameType(baseLayerAction, newAction))
                        {
                            newAction.SoftCopyFromParent(baseLayerAction as TriggerMapAction);
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

        public void MigrateActionId(TriggerMapAction newAction)
        {
            if (action.Id == MapAction.DEFAULT_UNBOUND_ID)
            {
                // Need to create new ID for action
                newAction.Id = mapper.EditLayer.FindNextAvailableId();
            }
            else
            {
                // Can re-use existing ID
                newAction.Id = action.Id;
            }
        }

        public void UpdateAction(TriggerMapAction newAction)
        {
            this.action = newAction;
        }
    }
}
