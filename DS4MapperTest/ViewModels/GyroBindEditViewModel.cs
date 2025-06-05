using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using DS4MapperTest.GyroActions;

namespace DS4MapperTest.ViewModels
{
    public class GyroBindEditViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private GyroMapAction action;
        public GyroMapAction Action
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

        public GyroBindEditViewModel(Mapper mapper, GyroMapAction action)
        {
            this.mapper = mapper;
            this.action = action;
        }

        public GyroMapAction PrepareNewAction(int ind)
        {
            GyroMapAction result = null;
            switch (ind)
            {
                case 0:
                    result = new GyroNoMapAction();
                    break;
                case 1:
                    result = new GyroMouse();
                    break;
                case 2:
                    result = new GyroMouseJoystick();
                    break;
                case 3:
                    result = new GyroDirectionalSwipe();
                    break;
                default:
                    break;
            }

            return result;
        }

        public void SwitchAction(GyroMapAction action)
        {
            GyroMapAction oldAction = this.action;
            GyroMapAction newAction = action;

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
                    bool exists = mapper.EditLayer.LayerActions.Contains(oldAction);
                    //if (oldAction.Id != MapAction.DEFAULT_UNBOUND_ID)
                    if (exists)
                    {
                        mapper.EditLayer.ReplaceGyroAction(oldAction, newAction);
                    }
                    else
                    {
                        mapper.EditLayer.AddGyroAction(newAction);
                    }

                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        MapAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[oldAction.MappingId];
                        if (MapAction.IsSameType(baseLayerAction, newAction))
                        {
                            newAction.SoftCopyFromParent(baseLayerAction as GyroMapAction);
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

        public void MigrateActionId(GyroMapAction newAction)
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

        public void UpdateAction(GyroMapAction newAction)
        {
            this.action = newAction;
        }
    }
}
