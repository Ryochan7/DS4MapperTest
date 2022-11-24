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
using DS4MapperTest.Views.TriggerActionPropControls;
using DS4MapperTest.TriggerActions;

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for TriggerBindEditWindow.xaml
    /// </summary>
    public partial class TriggerBindEditWindow : Window
    {
        private TriggerBindEditViewModel trigBindEditVM;
        public TriggerBindEditViewModel TrigBindEditVM => trigBindEditVM;

        public TriggerBindEditWindow()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, TriggerMapAction action)
        {
            trigBindEditVM = new TriggerBindEditViewModel(mapper, action);

            DataContext = trigBindEditVM;

            SetupDisplayControl();
        }

        public void SetupDisplayControl()
        {
            switch (trigBindEditVM.Action)
            {
                case TriggerTranslate:
                    {
                        TriggerTranslatePropControl propControl = new TriggerTranslatePropControl();
                        propControl.PostInit(trigBindEditVM.Mapper, trigBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        propControl.TrigTransPropVM.ActionChanged += TrigTransPropVM_ActionChanged;
                        trigBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case TriggerDualStageAction:
                    {
                        TriggerDualStagePropControl propControl = new TriggerDualStagePropControl();
                        propControl.PostInit(trigBindEditVM.Mapper, trigBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        propControl.RequestFuncEditor += PropControl_RequestFuncEditor;
                        propControl.TrigDualStagePropVM.ActionChanged += TrigDualStagePropVM_ActionChanged;
                        trigBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case TriggerNoAction:
                    {
                        TriggerNoActPropControl propControl = new TriggerNoActPropControl();
                        propControl.PostInit(trigBindEditVM.Mapper, trigBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        trigBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case TriggerButtonAction:
                    {
                        TriggerButtonActPropControl propControl = new TriggerButtonActPropControl();
                        propControl.PostInit(trigBindEditVM.Mapper, trigBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        propControl.RequestFuncEditor += TrigBtnAct_PropControl_RequestFuncEditor;
                        propControl.TrigBtnActVM.ActionChanged += TrigTransPropVM_ActionChanged;
                        trigBindEditVM.DisplayControl = propControl;
                    }

                    break;

                default:
                    trigBindEditVM.DisplayControl = null;
                    break;
            }
        }

        private void TrigBtnAct_PropControl_RequestFuncEditor(object sender,
            TriggerButtonActPropControl.TriggerButtonBindingArgs e)
        {
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(trigBindEditVM.Mapper, e.ActionBtn);
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

            UserControl oldControl = trigBindEditVM.DisplayControl;
            tempControl.RequestClose += (sender, args) =>
            {
                (oldControl as TriggerButtonActPropControl).RefreshView();
                trigBindEditVM.DisplayControl = oldControl;
            };

            trigBindEditVM.DisplayControl = tempControl;
        }

        private void TrigTransPropVM_ActionChanged(object sender, TriggerMapAction e)
        {
            trigBindEditVM.UpdateAction(e);
        }

        private void TrigDualStagePropVM_ActionChanged(object sender, TriggerMapAction e)
        {
            trigBindEditVM.UpdateAction(e);
        }

        private void PropControl_RequestFuncEditor(object sender,
            TriggerDualStagePropControl.DualStageBindingArgs e)
        {
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(trigBindEditVM.Mapper, e.PullBtn);
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

            UserControl oldControl = trigBindEditVM.DisplayControl;
            tempControl.RequestClose += (sender, args) =>
            {
                (oldControl as TriggerDualStagePropControl).RefreshView();
                trigBindEditVM.DisplayControl = oldControl;
            };

            trigBindEditVM.DisplayControl = tempControl;
        }

        private void TempControl_RequestBindingEditor(object sender, ActionUtil.ActionFunc e)
        {
            OutputBindingEditorControl tempControl = new OutputBindingEditorControl();
            FuncBindingControl bindControl = sender as FuncBindingControl;
            tempControl.PostInit(trigBindEditVM.Mapper, bindControl.FuncBindVM.Action, e);
            UserControl oldControl = bindControl;
            tempControl.Finished += (sender, args) =>
            {
                bindControl.RefreshView();
                trigBindEditVM.DisplayControl = oldControl;
                //FuncBindingControl tempControl = new FuncBindingControl();
                //tempControl.PostInit(btnFuncEditVM.Mapper, btnFuncEditVM.Action);
                //tempControl.RequestBindingEditor += TempControl_RequestBindingEditor;
                //btnFuncEditVM.DisplayControl = tempControl;
            };

            trigBindEditVM.DisplayControl = tempControl;
        }

        private void PropControl_ActionTypeIndexChanged(object sender, int ind)
        {
            TriggerMapAction tempAction = trigBindEditVM.PrepareNewAction(ind);
            if (tempAction != null)
            {
                tempAction.CopyBaseMapProps(trigBindEditVM.Action);
                trigBindEditVM.MigrateActionId(tempAction);
                trigBindEditVM.SwitchAction(tempAction);
                SetupDisplayControl();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            switch (trigBindEditVM.Action)
            {
                case TriggerTranslate:
                    break;
                case TriggerDualStageAction:
                    break;
                case TriggerNoAction:
                    break;
                default:
                    break;
            }
        }
    }
}
