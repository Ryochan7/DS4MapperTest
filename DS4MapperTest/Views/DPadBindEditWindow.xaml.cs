using DS4MapperTest.DPadActions;
using DS4MapperTest.StickActions;
using DS4MapperTest.ViewModels;
using DS4MapperTest.Views.DPadActionPropControls;
using DS4MapperTest.Views.StickActionPropControls;
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

namespace DS4MapperTest.Views
{
    /// <summary>
    /// Interaction logic for DPadBindEditWindow.xaml
    /// </summary>
    public partial class DPadBindEditWindow : Window
    {
        private DPadBindEditViewModel dpadBindEditVM;
        public DPadBindEditViewModel DPadBindEditVM => dpadBindEditVM;

        public DPadBindEditWindow()
        {
            InitializeComponent();
        }

        public void PostInit(Mapper mapper, DPadMapAction action)
        {
            dpadBindEditVM = new DPadBindEditViewModel(mapper, action);

            DataContext = dpadBindEditVM;

            SetupDisplayControl();
        }

        public void SetupDisplayControl()
        {
            switch(dpadBindEditVM.Action)
            {
                case DPadNoAction:
                    {
                        DPadNoActPropControl propControl = new DPadNoActPropControl();
                        propControl.PostInit(dpadBindEditVM.Mapper, dpadBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        dpadBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case DPadTranslate:
                    {
                        DPadTranslatePropControl propControl = new DPadTranslatePropControl();
                        propControl.PostInit(dpadBindEditVM.Mapper, dpadBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        dpadBindEditVM.DisplayControl = propControl;
                    }

                    break;
                case DPadAction:
                    {
                        DPadActionPadPropControl propControl = new DPadActionPadPropControl();
                        propControl.PostInit(dpadBindEditVM.Mapper, dpadBindEditVM.Action);
                        propControl.ActionTypeIndexChanged += PropControl_ActionTypeIndexChanged;
                        propControl.RequestFuncEditor += DPadAct_PropControl_RequestFuncEditor;
                        dpadBindEditVM.DisplayControl = propControl;
                    }

                    break;
                default: break;
            }
        }

        private void DPadAct_PropControl_RequestFuncEditor(object sender, DPadActionPadPropControl.DPadDirButtonBindingArgs e)
        {
            FuncBindingControl tempControl = new FuncBindingControl();
            tempControl.PostInit(dpadBindEditVM.Mapper, e.DirBtn);
            tempControl.RequestBindingEditor += TempControl_RequestBindingEditor;
            tempControl.FuncBindVM.IsRealAction = e.RealAction;
            tempControl.PreActionSwitch += (oldAction, newAction) =>
            {
                e.UpdateActHandler?.Invoke(oldAction, newAction);
            };
            //tempControl.PreActionSwitch += (sender, action) =>
            //{
            //    e.UpdateActHandler?.Invoke(null, action);
            //};

            UserControl oldControl = dpadBindEditVM.DisplayControl;
            tempControl.RequestClose += (sender, args) =>
            {
                (oldControl as DPadActionPadPropControl).RefreshView();
                dpadBindEditVM.DisplayControl = oldControl;
            };

            dpadBindEditVM.DisplayControl = tempControl;
        }

        private void TempControl_RequestBindingEditor(object sender,
            ActionUtil.ActionFunc e)
        {
            OutputBindingEditorControl tempControl = new OutputBindingEditorControl();
            FuncBindingControl bindControl = sender as FuncBindingControl;
            tempControl.PostInit(dpadBindEditVM.Mapper, bindControl.FuncBindVM.Action, e);
            UserControl oldControl = bindControl;
            tempControl.Finished += (sender, args) =>
            {
                bindControl.RefreshView();
                dpadBindEditVM.DisplayControl = oldControl;
            };

            dpadBindEditVM.DisplayControl = tempControl;
        }

        private void PropControl_ActionTypeIndexChanged(object sender, int ind)
        {
            DPadMapAction tempAction = dpadBindEditVM.PrepareNewAction(ind);
            if (tempAction != null)
            {
                tempAction.CopyBaseMapProps(dpadBindEditVM.Action);
                dpadBindEditVM.MigrateActionId(tempAction);
                dpadBindEditVM.SwitchAction(tempAction);
                SetupDisplayControl();
            }
        }
    }
}
