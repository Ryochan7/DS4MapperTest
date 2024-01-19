using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.ButtonActions;

namespace DS4MapperTest.ViewModels
{
    public class ButtonFuncEditViewModel
    {
        protected Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        protected ButtonMapAction action;
        public ButtonMapAction Action
        {
            get => action;
        }

        protected ButtonMapAction tempAction;
        public ButtonMapAction TempAction
        {
            get => tempAction;
            set => tempAction = value;
        }

        protected UserControl displayControl;
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

        private bool isTransformOutputVisible;
        public bool IsTransformOutputVisible
        {
            get => isTransformOutputVisible;
            set
            {
                if (isTransformOutputVisible == value) return;
                isTransformOutputVisible = value;
                IsTransformOutputVisibleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler IsTransformOutputVisibleChanged;

        private int selectedTransformIndex = 0;
        public int SelectedTransformIndex
        {
            get => selectedTransformIndex;
            set
            {
                selectedTransformIndex = value;
                SelectedTransformIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectedTransformIndexChanged;

        private bool topTransformPanelVisible = true;
        public bool TopTransformPanelVisible
        {
            get => topTransformPanelVisible;
            set
            {
                if (topTransformPanelVisible == value) return;
                topTransformPanelVisible = value;
                TopTransformPanelVisibleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TopTransformPanelVisibleChanged;

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

        private bool usingRealAction = false;
        public bool UsingRealAction
        {
            get => usingRealAction;
            set => usingRealAction = value;
        }

        public ButtonFuncEditViewModel(Mapper mapper, ButtonMapAction action)
        {
            this.mapper = mapper;
            this.action = action;
        }

        public ButtonMapAction PrepareNewAction(int ind)
        {
            ButtonMapAction result = null;
            switch(ind)
            {
                case 0:
                    result = new ButtonNoAction();
                    break;
                case 1:
                    result = new ButtonAction(
                        new NormalPressFunc(
                            new MapperUtil.OutputActionData(MapperUtil.OutputActionData.ActionType.Empty, 0)));
                    break;
                default:
                    break;
            }

            return result;
        }

        public void MigrationActionId(ButtonMapAction action)
        {
            if (this.action.Id == MapAction.DEFAULT_UNBOUND_ID)
            {
                // Need to create new ID for action
                action.Id =
                    mapper.EditLayer.FindNextAvailableId();
            }
            else
            {
                // Can re-use existing ID
                action.Id = this.action.Id;
            }
        }

        public void UpdateAction(ButtonMapAction action)
        {
            action.MappingId = this.action.MappingId;
            this.action = action;
        }

        public void SwitchLayerAction(ButtonMapAction oldAction, ButtonMapAction newAction, bool copyProps = true)
        {
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
                    //newAction.MappingId = oldAction.MappingId;
                    //if (oldAction.Id != MapAction.DEFAULT_UNBOUND_ID)
                    bool exists = mapper.EditLayer.LayerActions.Contains(oldAction);
                    if (exists)
                    {
                        mapper.EditLayer.ReplaceButtonAction(oldAction, newAction);
                    }
                    else
                    {
                        mapper.EditLayer.AddButtonMapAction(newAction);
                    }

                    if (mapper.EditActionSet.UsingCompositeLayer)
                    {
                        if (copyProps)
                        {
                            MapAction baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[oldAction.MappingId];
                            if (MapAction.IsSameType(baseLayerAction, newAction))
                            {
                                newAction.SoftCopy(baseLayerAction as ButtonMapAction);
                            }
                        }

                        mapper.EditActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.EditLayer.SyncActions();
                        mapper.EditActionSet.ClearCompositeLayerActions();
                        mapper.EditActionSet.PrepareCompositeLayer();
                    }
                }

                usingRealAction = true;
            });
        }
    }

    public class ButtonActionViewModel
    {
        private Mapper mapper;
        public Mapper Mapper => mapper;
        private ButtonAction action;
        public ButtonAction Action => action;

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

        //public event EventHandler ActionPropertyChanged;

        private bool usingRealAction = false;
        public bool UsingRealAction
        {
            get => usingRealAction;
            set => usingRealAction = value;
        }

        public ButtonActionViewModel(Mapper mapper, ButtonMapAction action)
        {
            this.mapper = mapper;
            this.action = action as ButtonAction;
            usingRealAction = true;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                ButtonMapAction baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as ButtonMapAction;
                ButtonAction tempAction = new ButtonAction();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.EditLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
                usingRealAction = false;

                //ActionPropertyChanged += ReplaceExistingLayerAction;
            }
        }
    }

    public class ButtonNoActionViewModel
    {
        private Mapper mapper;
        public Mapper Mapper => mapper;
        private ButtonNoAction action;
        public ButtonNoAction Action => action;

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

        private bool usingRealAction = false;
        public bool UsingRealAction
        {
            get => usingRealAction;
            set => usingRealAction = value;
        }

        public ButtonNoActionViewModel(Mapper mapper, ButtonMapAction action)
        {
            this.mapper = mapper;
            this.action = action as ButtonNoAction;
            usingRealAction = true;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                ButtonMapAction baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as ButtonMapAction;
                ButtonNoAction tempAction = new ButtonNoAction();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.EditLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
                usingRealAction = false;

                //ActionPropertyChanged += ReplaceExistingLayerAction;
            }
        }
    }
}
