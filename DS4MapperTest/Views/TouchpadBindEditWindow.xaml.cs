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
using System.Windows.Shapes;
using DS4MapperTest.ViewModels;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.Views.TouchpadActionPropControls;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for TouchpadBindEditWindow.xaml
    /// </summary>
    public partial class TouchpadBindEditWindow : Window
    {
        private TouchpadBindEditViewModel touchBindEditVM;
        public TouchpadBindEditViewModel TouchBindEditVM => touchBindEditVM;

        public event EventHandler<TouchpadMapAction> TouchActionUpdated;

        public TouchpadBindEditWindow()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TouchpadMapAction action)
        {
            touchBindEditVM = new TouchpadBindEditViewModel(mapper, action);

            DataContext = touchBindEditVM;

            touchpadSelectControl.PostInit(mapper, action);
            touchpadSelectControl.TouchOutputSelVM.SelectedIndexChanged += TouchOutputSelVM_SelectedIndexChanged;

            SetupDisplayControl();
        }

        private void TouchOutputSelVM_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedInd = touchpadSelectControl.TouchOutputSelVM.SelectedIndex;
            TouchpadMapAction tempAction = touchBindEditVM.PrepareNewAction(selectedInd);
            if (tempAction != null)
            {
                tempAction.CopyBaseMapProps(touchBindEditVM.Action);
                touchBindEditVM.MigrateActionId(tempAction);
                touchBindEditVM.SwitchAction(tempAction);
                SetupDisplayControl();
            }
        }

        public void SetupDisplayControl()
        {
            switch(touchBindEditVM.Action)
            {
                case TouchpadStickAction:
                    {
                        TouchpadStickActionPropControl propControl = new TouchpadStickActionPropControl();
                        propControl.PostInit(touchBindEditVM.Mapper, touchBindEditVM.Action);
                        touchBindEditVM.DisplayControl = propControl;
                        touchBindEditVM.ActionBaseDisplayControl = propControl;
                    }

                    break;
                case TouchpadActionPad:
                    {
                        TouchpadActionPadPropControl propActionPadControl = new TouchpadActionPadPropControl();
                        propActionPadControl.PostInit(touchBindEditVM.Mapper, touchBindEditVM.Action);
                        propActionPadControl.RequestFuncEditor += PropActionPadControl_RequestFuncEditor;
                        touchBindEditVM.DisplayControl = propActionPadControl;
                        touchBindEditVM.ActionBaseDisplayControl = propActionPadControl;
                    }

                    break;
                case TouchpadMouseJoystick:
                    {
                        TouchpadMouseJoystickPropControl propControl = new TouchpadMouseJoystickPropControl();
                        propControl.PostInit(touchBindEditVM.Mapper, touchBindEditVM.Action);
                        touchBindEditVM.DisplayControl = propControl;
                        touchBindEditVM.ActionBaseDisplayControl = propControl;
                    }

                    break;
                case TouchpadMouse:
                    {
                        TouchpadMousePropControl propControl = new TouchpadMousePropControl();
                        propControl.PostInit(touchBindEditVM.Mapper, touchBindEditVM.Action);
                        touchBindEditVM.DisplayControl = propControl;
                        touchBindEditVM.ActionBaseDisplayControl = propControl;
                    }

                    break;
                case TouchpadAbsAction:
                    {
                        TouchpadAbsMousePropControl propControl = new TouchpadAbsMousePropControl();
                        propControl.PostInit(touchBindEditVM.Mapper, touchBindEditVM.Action);
                        propControl.RequestFuncEditor += PropControl_RequestFuncEditor;
                        touchBindEditVM.DisplayControl = propControl;
                        touchBindEditVM.ActionBaseDisplayControl = propControl;
                    }

                    break;
                case TouchpadCircular:
                    {
                        TouchpadCircularPropControl propControl = new TouchpadCircularPropControl();
                        propControl.PostInit(touchBindEditVM.Mapper, touchBindEditVM.Action);
                        propControl.RequestFuncEditor += TouchCircularPropControl_RequestFuncEditor;
                        touchBindEditVM.DisplayControl = propControl;
                        touchBindEditVM.ActionBaseDisplayControl = propControl;
                    }

                    break;
                case TouchpadSingleButton:
                    {
                        TouchpadSingleButtonPropControl propControl = new TouchpadSingleButtonPropControl();
                        propControl.PostInit(touchBindEditVM.Mapper, touchBindEditVM.Action);
                        propControl.RequestFuncEditor += TouchpadSingleBtnPropControl_RequestFuncEditor;
                        touchBindEditVM.DisplayControl = propControl;
                        touchBindEditVM.ActionBaseDisplayControl = propControl;
                    }

                    break;
                case TouchpadDirectionalSwipe:
                    {
                        TouchpadDirSwipePropControl propControl = new TouchpadDirSwipePropControl();
                        propControl.PostInit(touchBindEditVM.Mapper, touchBindEditVM.Action);
                        propControl.RequestFuncEditor += TouchDirSwipePropControl_RequestFuncEditor;
                        touchBindEditVM.DisplayControl = propControl;
                        touchBindEditVM.ActionBaseDisplayControl = propControl;
                    }

                    break;
                default:
                    touchBindEditVM.DisplayControl = null;
                    touchBindEditVM.ActionBaseDisplayControl = null;
                    break;
            }
        }

        private void TouchDirSwipePropControl_RequestFuncEditor(object sender, TouchpadActionPadPropControl.DirButtonBindingArgs e)
        {
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(touchBindEditVM.Mapper, e.DirBtn);
            tempControl.RequestBindingEditor += TempControl_RequestBindingEditor;
            tempControl.FuncBindVM.IsRealAction = e.RealAction;
            tempControl.PreActionSwitch += (oldAction, newAction) =>
            {
                e.UpdateActHandler?.Invoke(oldAction, newAction);
            };
            tempControl.ActionChanged += (sender, action) =>
            {
                e.UpdateActHandler?.Invoke(null, action);
            };

            UserControl oldControl = touchBindEditVM.DisplayControl;
            touchpadSelectControl.Visibility = Visibility.Collapsed;
            tempControl.RequestClose += (sender, args) =>
            {
                (oldControl as TouchpadDirSwipePropControl).RefreshView();
                touchBindEditVM.DisplayControl = oldControl;
                touchpadSelectControl.Visibility = Visibility.Visible;
            };

            touchBindEditVM.DisplayControl = tempControl;
        }

        private void TouchpadSingleBtnPropControl_RequestFuncEditor(object sender, TouchpadActionPadPropControl.DirButtonBindingArgs e)
        {
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(touchBindEditVM.Mapper, e.DirBtn);
            tempControl.RequestBindingEditor += TempControl_RequestBindingEditor;
            tempControl.FuncBindVM.IsRealAction = e.RealAction;
            tempControl.PreActionSwitch += (oldAction, newAction) =>
            {
                e.UpdateActHandler?.Invoke(oldAction, newAction);
            };
            tempControl.ActionChanged += (sender, action) =>
            {
                e.UpdateActHandler?.Invoke(null, action);
            };

            UserControl oldControl = touchBindEditVM.DisplayControl;
            touchpadSelectControl.Visibility = Visibility.Collapsed;
            tempControl.RequestClose += (sender, args) =>
            {
                (oldControl as TouchpadSingleButtonPropControl).RefreshView();
                touchBindEditVM.DisplayControl = oldControl;
                touchpadSelectControl.Visibility = Visibility.Visible;
            };

            touchBindEditVM.DisplayControl = tempControl;
        }

        private void TouchCircularPropControl_RequestFuncEditor(object sender, TouchpadActionPadPropControl.DirButtonBindingArgs e)
        {
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(touchBindEditVM.Mapper, e.DirBtn);
            tempControl.RequestBindingEditor += TempControl_RequestBindingEditor;
            tempControl.FuncBindVM.IsRealAction = e.RealAction;
            tempControl.PreActionSwitch += (oldAction, newAction) =>
            {
                e.UpdateActHandler?.Invoke(oldAction, newAction);
            };
            tempControl.ActionChanged += (sender, action) =>
            {
                e.UpdateActHandler?.Invoke(null, action);
            };

            UserControl oldControl = touchBindEditVM.DisplayControl;
            touchpadSelectControl.Visibility = Visibility.Collapsed;
            tempControl.RequestClose += (sender, args) =>
            {
                (oldControl as TouchpadCircularPropControl).RefreshView();
                touchBindEditVM.DisplayControl = oldControl;
                touchpadSelectControl.Visibility = Visibility.Visible;
            };

            touchBindEditVM.DisplayControl = tempControl;
        }

        private void PropControl_RequestFuncEditor(object sender, TouchpadAbsMousePropControl.ButtonBindingArgs e)
        {
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(touchBindEditVM.Mapper, e.ActionBtn);
            tempControl.RequestBindingEditor += TempControl_RequestBindingEditor;
            tempControl.FuncBindVM.IsRealAction = e.RealAction;
            tempControl.PreActionSwitch += (oldAction, newAction) =>
            {
                e.UpdateActHandler?.Invoke(oldAction, newAction);
            };
            tempControl.ActionChanged += (sender, action) =>
            {
                e.UpdateActHandler?.Invoke(null, action);
            };

            UserControl oldControl = touchBindEditVM.DisplayControl;
            touchpadSelectControl.Visibility = Visibility.Collapsed;
            tempControl.RequestClose += (sender, args) =>
            {
                touchBindEditVM.DisplayControl = oldControl;
                touchpadSelectControl.Visibility = Visibility.Visible;
            };

            touchBindEditVM.DisplayControl = tempControl;
        }

        private void PropActionPadControl_RequestFuncEditor(object sender, TouchpadActionPadPropControl.DirButtonBindingArgs e)
        {
            TouchpadActionPadPropControl senderControl = sender as TouchpadActionPadPropControl;
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(touchBindEditVM.Mapper, e.DirBtn);
            tempControl.RequestBindingEditor += TempControl_RequestBindingEditor;
            tempControl.FuncBindVM.IsRealAction = e.RealAction;
            tempControl.PreActionSwitch += (oldAction, newAction) =>
            {
                e.UpdateActHandler?.Invoke(oldAction, newAction);
            };
            tempControl.ActionChanged += (sender, action) =>
            {
                e.UpdateActHandler?.Invoke(null, action);
            };

            UserControl oldControl = touchBindEditVM.DisplayControl;
            touchpadSelectControl.Visibility = Visibility.Collapsed;
            tempControl.RequestClose += (sender, args) =>
            {
                (oldControl as TouchpadActionPadPropControl).RefreshView();
                touchBindEditVM.DisplayControl = oldControl;
                touchpadSelectControl.Visibility = Visibility.Visible;
            };

            touchBindEditVM.DisplayControl = tempControl;
        }

        private void TempControl_RequestBindingEditor(object sender, ActionFunc e)
        {
            OutputBindingEditorControl tempControl = new OutputBindingEditorControl();
            FuncBindingControl bindControl = sender as FuncBindingControl;
            tempControl.PostInit(touchBindEditVM.Mapper, bindControl.FuncBindVM.Action, e);
            UserControl oldControl = bindControl;
            tempControl.Finished += (sender, args) =>
            {
                bindControl.RefreshView();
                touchBindEditVM.DisplayControl = oldControl;
            };

            touchBindEditVM.DisplayControl = tempControl;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            switch (touchBindEditVM.Action)
            {
                case TouchpadStickAction:
                    {
                        TouchpadStickActionPropControl tempControl = touchBindEditVM.ActionBaseDisplayControl as TouchpadStickActionPropControl;
                        if (tempControl.TouchStickPropVM.Action != touchBindEditVM.Action)
                        {
                            touchBindEditVM.UpdateAction(tempControl.TouchStickPropVM.Action);
                            TouchActionUpdated?.Invoke(this, tempControl.TouchStickPropVM.Action);
                        }
                    }

                    break;
                case TouchpadActionPad:
                    {
                        TouchpadActionPadPropControl tempControl = touchBindEditVM.ActionBaseDisplayControl as TouchpadActionPadPropControl;
                        if (tempControl.TouchActionPropVM.Action != touchBindEditVM.Action)
                        {
                            touchBindEditVM.UpdateAction(tempControl.TouchActionPropVM.Action);
                            TouchActionUpdated?.Invoke(this, tempControl.TouchActionPropVM.Action);
                        }
                    }

                    break;
                case TouchpadMouseJoystick:
                    {
                        TouchpadMouseJoystickPropControl tempControl = touchBindEditVM.ActionBaseDisplayControl as TouchpadMouseJoystickPropControl;
                        if (tempControl.TouchMouseJoyPropVM.Action != touchBindEditVM.Action)
                        {
                            touchBindEditVM.UpdateAction(tempControl.TouchMouseJoyPropVM.Action);
                            TouchActionUpdated?.Invoke(this, tempControl.TouchMouseJoyPropVM.Action);
                        }
                    }

                    break;
                case TouchpadMouse:
                    {
                        TouchpadMousePropControl tempControl = touchBindEditVM.ActionBaseDisplayControl as TouchpadMousePropControl;
                        if (tempControl.TouchMousePropVM.Action != touchBindEditVM.Action)
                        {
                            touchBindEditVM.UpdateAction(tempControl.TouchMousePropVM.Action);
                            TouchActionUpdated?.Invoke(this, tempControl.TouchMousePropVM.Action);
                        }
                    }

                    break;
                case TouchpadAbsAction:
                    {
                        TouchpadAbsMousePropControl tempControl = touchBindEditVM.ActionBaseDisplayControl as TouchpadAbsMousePropControl;
                        if (tempControl.TouchAbsMousePropVM.Action != touchBindEditVM.Action)
                        {
                            touchBindEditVM.UpdateAction(tempControl.TouchAbsMousePropVM.Action);
                            TouchActionUpdated?.Invoke(this, tempControl.TouchAbsMousePropVM.Action);
                        }
                    }

                    break;
                case TouchpadCircular:
                    {
                        TouchpadCircularPropControl tempControl = touchBindEditVM.ActionBaseDisplayControl as TouchpadCircularPropControl;
                        if (tempControl.TouchCircVM.Action != touchBindEditVM.Action)
                        {
                            touchBindEditVM.UpdateAction(tempControl.TouchCircVM.Action);
                            TouchActionUpdated?.Invoke(this, tempControl.TouchCircVM.Action);
                        }
                    }

                    break;
                case TouchpadSingleButton:
                    {
                        TouchpadSingleButtonPropControl tempControl = touchBindEditVM.ActionBaseDisplayControl as TouchpadSingleButtonPropControl;
                        if (tempControl.TouchSingleBtnVM.Action != touchBindEditVM.Action)
                        {
                            touchBindEditVM.UpdateAction(tempControl.TouchSingleBtnVM.Action);
                            TouchActionUpdated?.Invoke(this, tempControl.TouchSingleBtnVM.Action);
                        }
                    }

                    break;
                case TouchpadDirectionalSwipe:
                    {
                        TouchpadDirSwipePropControl tempControl = touchBindEditVM.ActionBaseDisplayControl as TouchpadDirSwipePropControl;
                        if (tempControl.TouchDirSwipeVM.Action != touchBindEditVM.Action)
                        {
                            touchBindEditVM.UpdateAction(tempControl.TouchDirSwipeVM.Action);
                            TouchActionUpdated?.Invoke(this, tempControl.TouchDirSwipeVM.Action);
                        }
                    }

                    break;
                default:
                    break;
            }
        }
    }
}
