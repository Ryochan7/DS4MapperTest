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
using DS4MapperTest.Views.StickActionPropControls;
using DS4MapperTest.ViewModels;
using DS4MapperTest.StickActions;
using DS4MapperTest.Views.TouchpadActionPropControls;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for StickBindEditWindow.xaml
    /// </summary>
    public partial class StickBindEditWindow : Window
    {
        private StickBindEditViewModel stickBindEditVM;
        public StickBindEditViewModel StickBindEditVM => stickBindEditVM;

        public StickBindEditWindow()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, StickMapAction action)
        {
            stickBindEditVM = new StickBindEditViewModel(mapper, action);

            DataContext = stickBindEditVM;

            SetupDisplayControl();
        }

        public void SetupDisplayControl()
        {
            switch(stickBindEditVM.Action)
            {
                case StickNoAction:
                    {
                        StickNoActPropControl propControl = new StickNoActPropControl();
                        propControl.PostInit(stickBindEditVM.Mapper, stickBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        stickBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case StickTranslate:
                    {
                        StickTranslatePropControl propControl = new StickTranslatePropControl();
                        propControl.PostInit(stickBindEditVM.Mapper, stickBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        stickBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case StickPadAction:
                    {
                        StickPadActionControl propControl = new StickPadActionControl();
                        propControl.PostInit(stickBindEditVM.Mapper, stickBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        propControl.RequestFuncEditor += StickPadAct_PropControl_RequestFuncEditor;
                        stickBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case StickMouse:
                    {
                        StickMousePropControl propControl = new StickMousePropControl();
                        propControl.PostInit(stickBindEditVM.Mapper, stickBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        stickBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case StickCircular:
                    {
                        StickCircularPropControl propControl = new StickCircularPropControl();
                        propControl.PostInit(stickBindEditVM.Mapper, stickBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        propControl.RequestFuncEditor += StickCircularPropControl_RequestFuncEditor;
                        stickBindEditVM.DisplayControl = propControl;
                        //stickBindEditVM.ActionBaseDisplayControl = propControl;
                    }

                    break;
                default:
                    break;
            }
        }

        private void StickPadAct_PropControl_RequestFuncEditor(object sender, StickPadActionControl.DirButtonBindingArgs e)
        {
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(stickBindEditVM.Mapper, e.DirBtn);
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

            UserControl oldControl = stickBindEditVM.DisplayControl;
            tempControl.RequestClose += (sender, args) =>
            {
                (oldControl as StickPadActionControl).RefreshView();
                stickBindEditVM.DisplayControl = oldControl;
            };

            stickBindEditVM.DisplayControl = tempControl;
        }

        private void TempControl_RequestBindingEditor(object sender,
            ActionUtil.ActionFunc e)
        {
            OutputBindingEditorControl tempControl = new OutputBindingEditorControl();
            FuncBindingControl bindControl = sender as FuncBindingControl;
            tempControl.PostInit(stickBindEditVM.Mapper, bindControl.FuncBindVM.Action, e);
            UserControl oldControl = bindControl;
            tempControl.Finished += (sender, args) =>
            {
                bindControl.RefreshView();
                stickBindEditVM.DisplayControl = oldControl;
            };

            stickBindEditVM.DisplayControl = tempControl;
        }

        private void PropControl_ActionTypeIndexChanged(object sender, int ind)
        {
            StickMapAction tempAction = stickBindEditVM.PrepareNewAction(ind);
            if (tempAction != null)
            {
                tempAction.CopyBaseMapProps(stickBindEditVM.Action);
                stickBindEditVM.MigrateActionId(tempAction);
                stickBindEditVM.SwitchAction(tempAction);
                SetupDisplayControl();
            }
        }

        private void StickCircularPropControl_RequestFuncEditor(object sender, TouchpadActionPadPropControl.DirButtonBindingArgs e)
        {
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(stickBindEditVM.Mapper, e.DirBtn);
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

            UserControl oldControl = stickBindEditVM.DisplayControl;
            tempControl.RequestClose += (sender, args) =>
            {
                (oldControl as StickCircularPropControl).RefreshView();
                stickBindEditVM.DisplayControl = oldControl;
            };

            stickBindEditVM.DisplayControl = tempControl;
        }
    }
}
