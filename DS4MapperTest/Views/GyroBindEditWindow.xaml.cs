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
using DS4MapperTest.Views.GyroActionPropControls;
using DS4MapperTest.GyroActions;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for GyroBindEditWindow.xaml
    /// </summary>
    public partial class GyroBindEditWindow : Window
    {
        private GyroBindEditViewModel gyroBindEditVM;
        public GyroBindEditViewModel GyroBindEditVM => gyroBindEditVM;

        public GyroBindEditWindow()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, GyroMapAction action)
        {
            gyroBindEditVM = new GyroBindEditViewModel(mapper, action);

            DataContext = gyroBindEditVM;

            SetupDisplayControl();
        }

        public void SetupDisplayControl()
        {
            switch(gyroBindEditVM.Action)
            {
                case GyroNoMapAction:
                    {
                        GyroNoActionControl propControl = new GyroNoActionControl();
                        propControl.PostInit(gyroBindEditVM.Mapper, gyroBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        gyroBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case GyroMouse:
                    {
                        GyroMousePropControl propControl = new GyroMousePropControl();
                        propControl.PostInit(gyroBindEditVM.Mapper, gyroBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        gyroBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case GyroMouseJoystick:
                    {
                        GyroMouseJoystickPropControl propControl = new GyroMouseJoystickPropControl();
                        propControl.PostInit(gyroBindEditVM.Mapper, gyroBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        gyroBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case GyroDirectionalSwipe:
                    {
                        GyroDirSwipePropControl propControl = new GyroDirSwipePropControl();
                        propControl.PostInit(gyroBindEditVM.Mapper, gyroBindEditVM.Action);
                        propControl.RequestFuncEditor += PropControl_RequestFuncEditor;
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        gyroBindEditVM.DisplayControl = propControl;
                    }

                    break;
                default:
                    gyroBindEditVM.DisplayControl = null;
                    break;
            }
        }

        private void PropControl_RequestFuncEditor(object sender, TouchpadActionPropControls.TouchpadActionPadPropControl.DirButtonBindingArgs e)
        {
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(gyroBindEditVM.Mapper, e.DirBtn);
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

            UserControl oldControl = gyroBindEditVM.DisplayControl;
            tempControl.RequestClose += (sender, args) =>
            {
                (oldControl as GyroDirSwipePropControl).RefreshView();
                gyroBindEditVM.DisplayControl = oldControl;
            };

            gyroBindEditVM.DisplayControl = tempControl;
        }

        private void TempControl_RequestBindingEditor(object sender, ActionUtil.ActionFunc e)
        {
            OutputBindingEditorControl tempControl = new OutputBindingEditorControl();
            FuncBindingControl bindControl = sender as FuncBindingControl;
            tempControl.PostInit(gyroBindEditVM.Mapper, bindControl.FuncBindVM.Action, e);
            UserControl oldControl = bindControl;
            tempControl.Finished += (sender, args) =>
            {
                bindControl.RefreshView();
                gyroBindEditVM.DisplayControl = oldControl;
            };

            gyroBindEditVM.DisplayControl = tempControl;
        }

        private void PropControl_ActionTypeIndexChanged(object sender, int ind)
        {
            GyroMapAction tempAction = gyroBindEditVM.PrepareNewAction(ind);
            if (tempAction != null)
            {
                tempAction.CopyBaseMapProps(gyroBindEditVM.Action);
                gyroBindEditVM.MigrateActionId(tempAction);
                gyroBindEditVM.SwitchAction(tempAction);
                SetupDisplayControl();
            }
        }
    }
}
