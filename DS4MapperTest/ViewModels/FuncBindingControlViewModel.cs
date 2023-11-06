using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.MapperUtil;
using System.Threading;

namespace DS4MapperTest.ViewModels
{
    public class FuncBindingControlViewModel
    {
        private ObservableCollection<FuncBindItem> thing;
        public ObservableCollection<FuncBindItem> FuncList => thing;

        private UserControl displayPropControl;
        public UserControl DisplayPropControl
        {
            get => displayPropControl;
            set
            {
                displayPropControl = value;
                DisplayPropControlChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DisplayPropControlChanged;

        private Mapper mapper;
        public Mapper Mapper => mapper;
        private ButtonAction action;
        public ButtonAction Action => action;

        private int currentBindItemIndex = -1;
        public int CurrentBindItemIndex
        {
            get => currentBindItemIndex;
            set
            {
                currentBindItemIndex = value;
            }
        }

        private FuncBindItem currentItem;
        public FuncBindItem CurrentItem
        {
            get => currentItem;
            set => currentItem = value;
        }

        private bool realAction;
        public bool IsRealAction
        {
            get => realAction;
            set
            {
                realAction = value;
                IsRealActionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler IsRealActionChanged;

        public bool IsNotRealAction
        {
            get => !realAction;
        }
        public event EventHandler IsNotRealActionChanged;

        public FuncBindingControlViewModel(Mapper mapper, ButtonAction action, UserControl currentPropControl)
        {
            this.mapper = mapper;
            this.action = action;
            thing = new ObservableCollection<FuncBindItem>();

            int tempInd = 0;
            foreach(ActionFunc func in action.ActionFuncs)
            {
                thing.Add(new FuncBindItem(action, func, tempInd++));
            }

            currentItem = thing.FirstOrDefault();
            if (currentItem != null)
            {
                currentBindItemIndex = 0;
            }

            //realAction = action.ParentAction == null;

            //thing.Add(new FuncBindItem(action, null, tempInd++));
        }

        public FuncBindItem AddTempBindItem()
        {
            // Use NormalPressFunc instance by default
            NormalPressFunc tempFunc =
                new NormalPressFunc(new OutputActionData(OutputActionData.ActionType.Empty, 0));

            mapper.QueueEvent(() =>
            {
                action.Release(mapper, ignoreReleaseActions: true);
                action.ActionFuncs.Add(tempFunc);
            });

            int tempInd = thing.Count;
            FuncBindItem item = new FuncBindItem(Action, tempFunc, tempInd);
            thing.Add(item);

            currentItem = item;
            currentBindItemIndex = tempInd;

            return item;
        }

        public void RemoveBindItem(int ind)
        {
            if (thing.Count == 1)
            {
                return;
            }
            else if (ind >= thing.Count)
            {
                return;
            }

            thing.RemoveAt(ind);
            int removeInd = ind;
            mapper.QueueEvent(() =>
            {
                action.Release(mapper, ignoreReleaseActions: true);
                action.ActionFuncs.RemoveAt(removeInd);
            });

            int tempInd = ind;
            foreach(FuncBindItem item in thing.Where((item) => item.Index > ind))
            {
                item.Index = tempInd++;
            }

            int selectInd = ind < thing.Count ? ind : thing.Count - 1;
            currentItem = thing[selectInd];
            currentBindItemIndex = selectInd;
            if (currentItem != null)
            {
                currentItem.ItemActive = true;
            }
        }

        public void ChangeFunc(int ind, int selectFunc)
        {
            ActionFunc func = CreateFuncForSelection(selectFunc);
            FuncBindItem item = thing[ind];
            item.Func = func;
            item.RaiseDisplayNameChanged();

            mapper.QueueEvent(() =>
            {
                action.Release(mapper, ignoreReleaseActions: true);
                action.ActionFuncs.RemoveAt(ind);
                action.ActionFuncs.Insert(ind, func);
            });
        }

        private ActionFunc CreateFuncForSelection(int selectFunc)
        {
            ActionFunc result = null;
            OutputActionData tempData = new OutputActionData(OutputActionData.ActionType.Empty, 0);
            switch(selectFunc)
            {
                case 1:
                    result = new NormalPressFunc(tempData);
                    break;
                case 2:
                    result = new HoldPressFunc();
                    result.OutputActions.Add(tempData);
                    break;
                case 3:
                    result = new StartPressFunc();
                    result.OutputActions.Add(tempData);
                    break;
                case 4:
                    result = new ReleaseFunc();
                    result.OutputActions.Add(tempData);
                    break;
                case 5:
                    result = new DistanceFunc();
                    result.OutputActions.Add(tempData);
                    break;
                default:
                    break;
            }

            return result;
        }

        public static ButtonAction CreateNewAction(ButtonAction sourceAction)
        {
            ButtonAction result = null;
            switch (sourceAction)
            {
                case AxisDirButton:
                    result = new AxisDirButton();
                    break;
                case TouchpadCircularButton:
                    result = new TouchpadCircularButton();
                    break;
                case ButtonAction:
                    result = new ButtonAction();
                    break;
                default:
                    break;
            }

            return result;
        }

        public static ButtonAction CopyAction(ButtonAction sourceAction)
        {
            ButtonAction result = CreateNewAction(sourceAction);

            if (result != null)
            {
                result.CopyBaseProps(sourceAction);
                result.CopyAction(sourceAction);
            }

            return result;
        }

        public static ButtonAction CreateUnboundAction(ButtonAction sourceAction)
        {
            ButtonAction result = CreateNewAction(sourceAction);

            if (result != null)
            {
                result.CopyBaseProps(sourceAction);
                result.ActionFuncs.Add(new NormalPressFunc(
                            new OutputActionData(OutputActionData.ActionType.Empty, 0)));
                result.ChangedProperties.Add(ButtonAction.PropertyKeyStrings.NAME);
                result.ChangedProperties.Add(ButtonAction.PropertyKeyStrings.FUNCTIONS);
            }

            return result;
        }

        public void SwitchAction(ButtonAction oldAction, ButtonAction newAction)
        {
            /*ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            mapper.QueueEvent(() =>
            {
                oldAction.Release(mapper, ignoreReleaseActions: true);

                newAction.CopyBaseProps(oldAction);

                if (newAction.Id != MapAction.DEFAULT_UNBOUND_ID)
                {
                    mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.ReplaceButtonAction(oldAction, newAction);
                }
                else
                {
                    // Need to create new ID for action
                    newAction.Id =
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();

                    mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddButtonMapAction(newAction);
                }

                if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                {
                    mapper.ActionProfile.CurrentActionSet.RecompileCompositeLayer(mapper);
                }

                resetEvent.Set();
            });

            resetEvent.Wait();
            */

            this.action = newAction;
        }

        public void ResetFunctions(ButtonAction action)
        {
            thing.Clear();

            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            mapper.QueueEvent(() =>
            {
                action.Release(mapper, ignoreReleaseActions: true);

                if (action is ButtonAction)
                {
                    action.ActionFuncs.Clear();
                    action.ActionFuncs.Add(new NormalPressFunc(
                                new OutputActionData(OutputActionData.ActionType.Empty, 0)));
                    action.ChangedProperties.Add(ButtonAction.PropertyKeyStrings.FUNCTIONS);
                }

                resetEvent.Set();
            });

            resetEvent.Wait(1000);

            int tempInd = 0;
            foreach (ActionFunc func in action.ActionFuncs)
            {
                thing.Add(new FuncBindItem(action, func, tempInd++));
            }

            currentItem = thing.FirstOrDefault();
            if (currentItem != null)
            {
                currentBindItemIndex = 0;
                currentItem.ItemActive = true;
            }
        }
    }

    public class FuncBindItem
    {
        private const string DEFAULT_DISPLAY_NAME = "NormalPress";

        private ButtonAction action;
        private ActionFunc func;
        public ActionFunc Func
        {
            get => func;
            set => func = value;
        }

        private int index;
        public int Index
        {
            get => index;
            set
            {
                index = value;
                IndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler IndexChanged;

        public string DisplayName
        {
            get
            {
                string result = DEFAULT_DISPLAY_NAME;
                switch(func)
                {
                    case NormalPressFunc:
                        break;
                    case HoldPressFunc:
                        result = "HoldPress";
                        break;
                    case StartPressFunc:
                        result = "StartPress";
                        break;
                    case ReleaseFunc:
                        result = "Release";
                        break;
                    case DistanceFunc:
                        result = "Distance";
                        break;
                    default:
                        break;
                }

                return result;
            }
        }
        public event EventHandler DisplayNameChanged;

        private bool itemActive;
        public bool ItemActive
        {
            get => itemActive;
            set
            {
                itemActive = value;
                ItemActiveChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ItemActiveChanged;

        public FuncBindItem(ButtonAction action, ActionFunc func, int index)
        {
            this.action = action;
            this.func = func;
            this.index = index;
        }

        public void RaiseDisplayNameChanged()
        {
            DisplayNameChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
