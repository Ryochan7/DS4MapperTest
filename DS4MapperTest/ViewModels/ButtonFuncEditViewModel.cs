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

        public void UpdateAction(ButtonMapAction action)
        {
            if (this.action.Id == MapAction.DEFAULT_UNBOUND_ID)
            {
                // Need to create new ID for action
                action.Id =
                    mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
            }
            else
            {
                // Can re-use existing ID
                action.Id = this.action.Id;
            }

            action.MappingId = this.action.MappingId;
            this.action = action;
        }

        public void SwitchLayerAction(ButtonMapAction oldAction, ButtonMapAction newAction, bool copyProps = true)
        {
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
                    //newAction.MappingId = oldAction.MappingId;
                    //if (oldAction.Id != MapAction.DEFAULT_UNBOUND_ID)
                    bool exists = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(oldAction);
                    if (exists)
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.ReplaceButtonAction(oldAction, newAction);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddButtonMapAction(newAction);
                    }

                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        if (copyProps)
                        {
                            MapAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[oldAction.MappingId];
                            if (MapAction.IsSameType(baseLayerAction, newAction))
                            {
                                newAction.SoftCopyFromParent(baseLayerAction as ButtonMapAction);
                            }
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

        public ButtonActionViewModel(Mapper mapper, ButtonMapAction action)
        {
            this.mapper = mapper;
            this.action = action as ButtonAction;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                ButtonMapAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as ButtonMapAction;
                ButtonAction tempAction = new ButtonAction();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;

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

        public ButtonNoActionViewModel(Mapper mapper, ButtonMapAction action)
        {
            this.mapper = mapper;
            this.action = action as ButtonNoAction;
        }
    }
}
