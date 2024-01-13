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
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

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

            mapper.ProcessMappingChangeAction(() =>
            {
                tempAction?.Invoke();

                resetEvent.Set();
            });

            resetEvent.Wait();
        }
    }
}
