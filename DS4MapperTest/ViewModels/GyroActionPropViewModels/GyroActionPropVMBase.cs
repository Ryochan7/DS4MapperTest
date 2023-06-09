using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.ViewModels.Common;
using DS4MapperTest.GyroActions;

namespace DS4MapperTest.ViewModels.GyroActionPropViewModels
{
    public enum InvertChocies
    {
        None,
        InvertX,
        InvertY,
        InvertXY,
    }

    public class InvertChoiceItem
    {
        private string displayName;
        public string DisplayName => displayName;

        private InvertChocies choice;
        public InvertChocies Choice => choice;

        public InvertChoiceItem(string displayName, InvertChocies choice)
        {
            this.displayName = displayName;
            this.choice = choice;
        }
    }

    public class GyroActionPropVMBase
    {
        protected const string DEFAULT_EMPTY_TRIGGER_STR = "None";

        protected Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        protected GyroMapAction baseAction;
        public GyroMapAction BaseAction
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

        protected List<EnumChoiceSelection<bool>> gyroTriggerCondItems =
            new List<EnumChoiceSelection<bool>>()
        {
            new EnumChoiceSelection<bool>("And", true),
            new EnumChoiceSelection<bool>("Or", false),
        };

        public List<EnumChoiceSelection<bool>> GyroTriggerCondItems => gyroTriggerCondItems;

        public virtual event EventHandler ActionPropertyChanged;
        public event EventHandler<GyroMapAction> ActionChanged;

        protected bool usingRealAction = true;

        protected void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

                mapper.QueueEvent(() =>
                {
                    this.baseAction.ParentAction.Release(mapper, ignoreReleaseActions: true);

                    mapper.EditLayer.AddGyroAction(this.baseAction);
                    if (mapper.EditActionSet.UsingCompositeLayer)
                    {
                        mapper.EditActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.EditLayer.SyncActions();
                    }

                    resetEvent.Set();
                });

                resetEvent.Wait();

                usingRealAction = true;

                ActionChanged?.Invoke(this, baseAction);
            }
        }

        protected void ExecuteInMapperThread(Action tempAction)
        {
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            mapper.QueueEvent(() =>
            {
                tempAction?.Invoke();

                resetEvent.Set();
            });

            resetEvent.Wait();
        }
    }
}
