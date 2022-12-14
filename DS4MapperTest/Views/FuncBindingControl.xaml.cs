using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.ViewModels;
using DS4MapperTest.ButtonActions;
using System.Threading;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for FuncBindingControl.xaml
    /// </summary>
    public partial class FuncBindingControl : UserControl
    {
        private FuncBindingControlViewModel funcBindVM;
        public FuncBindingControlViewModel FuncBindVM => funcBindVM;
        private DefaultFuncPropControl defaultPropControl = new DefaultFuncPropControl();
        public event EventHandler<ActionFunc> RequestBindingEditor;
        public event EventHandler<ButtonAction> ActionChanged;
        public event EventHandler RequestClose;

        // Repurpose to use for only action switch
        public delegate void PreActionSwitchHandler(ButtonAction oldAction, ButtonAction newAction);
        public event PreActionSwitchHandler PreActionSwitch;

        public FuncBindingControl()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, ButtonMapAction action)
        {
            funcBindVM = new FuncBindingControlViewModel(mapper, action as ButtonAction, defaultPropControl);

            if (funcBindVM.FuncList.Count > 0)
            {
                int ind = funcBindVM.FuncList.Count - 1;
                FuncBindItem item = funcBindVM.FuncList[ind];
                funcBindVM.CurrentItem = item;
                funcBindVM.CurrentBindItemIndex = ind;
                item.ItemActive = true;

                //CheckSelectionActionType(item);
                SwitchPropView(item);
            }

            ConnectPartialEvents();
            DataContext = funcBindVM;
        }

        //private void FuncBindVM_ActionTypeIndexChanged(object sender, EventArgs e)
        //{
        //    DataContext = null;

        //    if (funcBindVM.ActionTypeIndex == 0)
        //    {
        //        int currentInd = funcBindVM.CurrentBindItemIndex;
        //        funcBindVM.RemoveBindItem(currentInd);

        //        funcBindVM.DisplayPropControl = null;
        //        Task.Delay(1000).Wait();
        //        Dispatcher.BeginInvoke((Action)(() => {
        //            FuncBindItem item = funcBindVM.CurrentItem;
        //            //CheckSelectionActionType(item);
        //            //SwitchPropView(item);
        //        }));
        //    }
        //    else
        //    {
        //        funcBindVM.ChangeFunc(funcBindVM.CurrentItem.Index);

        //        FuncBindItem item = funcBindVM.CurrentItem;
        //        //CheckSelectionActionType(item);
        //        SwitchPropView(item);
        //    }

        //    DataContext = funcBindVM;
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int ind = Convert.ToInt32((sender as Button).Tag);
            FuncBindItem item = funcBindVM.FuncList[ind];
            if (item != funcBindVM.CurrentItem)
            {
                funcBindVM.CurrentItem.ItemActive = false;

                funcBindVM.CurrentItem = item;
                funcBindVM.CurrentBindItemIndex = ind;
                item.ItemActive = true;

                DataContext = null;

                //CheckSelectionActionType(item);
                SwitchPropView(item);

                DataContext = funcBindVM;
            }
        }

        public void RefreshView()
        {
            DataContext = null;

            //CheckSelectionActionType(funcBindVM.CurrentItem);
            SwitchPropView(funcBindVM.CurrentItem);

            DataContext = funcBindVM;
        }

        private void SwitchPropView(FuncBindItem item)
        {
            // Switch based on class type
            switch(item.Func)
            {
                case NormalPressFunc normPress:
                    {
                        NormalPressFuncPropControl propControl = new NormalPressFuncPropControl();
                        propControl.PostInit(funcBindVM.Mapper, funcBindVM.Action, normPress);
                        propControl.RequestBindingEditor += PropControl_RequestBindingEditor;
                        propControl.RequestChangeFuncType += PropControl_RequestChangeFuncType;
                        funcBindVM.DisplayPropControl = propControl;
                    }

                    break;
                case HoldPressFunc holdPress:
                    {
                        HoldPressFuncPropControl propControl = new HoldPressFuncPropControl();
                        propControl.PostInit(funcBindVM.Mapper, funcBindVM.Action, holdPress);
                        propControl.RequestBindingEditor += PropControl_RequestBindingEditor;
                        propControl.RequestChangeFuncType += PropControl_RequestChangeFuncType;
                        funcBindVM.DisplayPropControl = propControl;
                    }

                    break;
                case ReleaseFunc releaseFunc:
                    {
                        ReleaseFuncPropControl propControl = new ReleaseFuncPropControl();
                        propControl.PostInit(funcBindVM.Mapper, funcBindVM.Action, releaseFunc);
                        propControl.RequestBindingEditor += PropControl_RequestBindingEditor;
                        propControl.RequestChangeFuncType += PropControl_RequestChangeFuncType;
                        funcBindVM.DisplayPropControl = propControl;
                    }

                    break;
                case StartPressFunc startFunc:
                    {
                        StartPressFuncPropControl propControl = new StartPressFuncPropControl();
                        propControl.PostInit(funcBindVM.Mapper, funcBindVM.Action, startFunc);
                        propControl.RequestBindingEditor += PropControl_RequestBindingEditor;
                        propControl.RequestChangeFuncType += PropControl_RequestChangeFuncType;
                        funcBindVM.DisplayPropControl = propControl;
                    }

                    break;
                case DistanceFunc distFunc:
                    {
                        DistanceFuncPropControl propControl = new DistanceFuncPropControl();
                        propControl.PostInit(funcBindVM.Mapper, funcBindVM.Action, distFunc);
                        propControl.RequestBindingEditor += PropControl_RequestBindingEditor;
                        propControl.RequestChangeFuncType += PropControl_RequestChangeFuncType;
                        funcBindVM.DisplayPropControl = propControl;
                    }

                    break;
                default:
                    break;
            }
        }

        private void PropControl_RequestChangeFuncType(object sender, int e)
        {
            int ind = e;
            if (ind == 0)
            {
                funcBindVM.DisplayPropControl = null;

                int currentInd = funcBindVM.CurrentBindItemIndex;
                funcBindVM.RemoveBindItem(currentInd);
                ActionChanged?.Invoke(this, funcBindVM.Action);

                SwitchPropView(funcBindVM.CurrentItem);
            }
            else
            {
                funcBindVM.ChangeFunc(funcBindVM.CurrentItem.Index, ind);

                FuncBindItem item = funcBindVM.CurrentItem;
                ////CheckSelectionActionType(item);
                SwitchPropView(item);
            }
        }

        private void PropControl_RequestBindingEditor(object sender, EventArgs e)
        {
            funcBindVM.DisplayPropControl = null;
            FuncBindItem item = funcBindVM.CurrentItem;
            RequestBindingEditor?.Invoke(this, item.Func);
        }

        private void AddFuncButton_Click(object sender, RoutedEventArgs e)
        {
            FuncBindItem item = funcBindVM.CurrentItem;
            if (item != null)
            {
                item.ItemActive = false;
            }

            item = funcBindVM.AddTempBindItem();
            funcBindVM.CurrentItem = item;
            funcBindVM.CurrentBindItemIndex = funcBindVM.FuncList.Count-1;
            item.ItemActive = true;
            ActionChanged?.Invoke(this, funcBindVM.Action);

            //DataContext = null;

            //CheckSelectionActionType(item);
            SwitchPropView(item);

            //DataContext = funcBindVM;
        }

        private void ConnectPartialEvents()
        {
            //funcBindVM.ActionTypeIndexChanged += FuncBindVM_ActionTypeIndexChanged;
        }

        private void DisconnectPartialEvents()
        {
            //funcBindVM.ActionTypeIndexChanged -= FuncBindVM_ActionTypeIndexChanged;
        }

        private void CopyFuncButton_Click(object sender, RoutedEventArgs e)
        {
            DisconnectPartialEvents();

            DataContext = null;

            Mapper mapper = funcBindVM.Mapper;
            //ButtonAction oldAction = funcBindVM.Action.ParentAction as ButtonAction;
            ButtonAction oldAction = funcBindVM.Action as ButtonAction;
            ButtonAction newAction = FuncBindingControlViewModel.CopyAction(oldAction);

            PreActionSwitch?.Invoke(oldAction, newAction);
            funcBindVM.SwitchAction(oldAction, newAction);
            //ActionChanged?.Invoke(this, newAction);

            funcBindVM = new FuncBindingControlViewModel(mapper, newAction, defaultPropControl);
            funcBindVM.IsRealAction = true;

            if (funcBindVM.FuncList.Count > 0)
            {
                int ind = funcBindVM.FuncList.Count - 1;
                FuncBindItem item = funcBindVM.FuncList[ind];
                funcBindVM.CurrentItem = item;
                funcBindVM.CurrentBindItemIndex = ind;
                item.ItemActive = true;

                //CheckSelectionActionType(item);
                SwitchPropView(item);
            }

            ConnectPartialEvents();
            DataContext = funcBindVM;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
