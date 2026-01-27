using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest.ViewModels.TouchpadActionPropViewModels
{
    public class TouchpadActionPropVMBase
    {
        public enum SmoothPresetChoices
        {
            None,
            Stiff,
            Normie,
            Loose,
        }

        public class SmoothPresetChoiceItem
        {
            private string displayName;
            public string DisplayName => displayName;

            private SmoothPresetChoices choice;
            public SmoothPresetChoices Choice => choice;

            private double minCutoffValue = 1.0;
            public double MinCutoffValue => minCutoffValue;

            private double betaValue = 1.0;
            public double BetaValue => betaValue;

            public SmoothPresetChoiceItem(string displayName, SmoothPresetChoices choice,
                double minCutoff, double beta)
            {
                this.displayName = displayName;
                this.choice = choice;
                this.minCutoffValue = minCutoff;
                this.betaValue = beta;
            }
        }

        protected Mapper mapper;
        public Mapper Napper
        {
            get => mapper;
        }

        protected TouchpadMapAction baseAction;
        public TouchpadMapAction BaseAction
        {
            get => baseAction;
        }

        public string Name
        {
            get => baseAction.Name;
            set
            {
                if (baseAction.Name == value) return;
                baseAction.Name = value;
                NameChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler NameChanged;

        public virtual event EventHandler ActionPropertyChanged;
        public event EventHandler<TouchpadMapAction> ActionChanged;

        protected bool usingRealAction = true;

        protected void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                mapper.ProcessMappingChangeAction(() =>
                {
                    this.baseAction.ParentAction.Release(mapper, ignoreReleaseActions: true);
                    //this.baseAction.Release(mapper, ignoreReleaseActions: true);

                    mapper.EditLayer.AddTouchpadAction(this.baseAction);
                    if (mapper.EditActionSet.UsingCompositeLayer)
                    {
                        mapper.EditActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.EditLayer.SyncActions();
                        mapper.EditActionSet.ClearCompositeLayerActions();
                        mapper.EditActionSet.PrepareCompositeLayer();
                    }
                });

                usingRealAction = true;

                ActionChanged?.Invoke(this, baseAction);
            }
        }

        protected void ExecuteInMapperThread(Action tempAction)
        {
            mapper.ProcessMappingChangeAction(() =>
            {
                tempAction?.Invoke();
            });
        }
    }
}
