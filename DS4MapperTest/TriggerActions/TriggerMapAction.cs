using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.TriggerActions
{
    public struct TriggerEventFrame
    {
        public short axisValue;
        public bool fullClick;
    }

    public abstract class TriggerMapAction : MapAction
    {
        public bool activeEvent;
        public bool active;

        protected TriggerDefinition triggerDefinition;
        public TriggerDefinition TriggerDef
        {
            get => triggerDefinition;
            set => triggerDefinition = value;
        }

        protected event EventHandler<NotifyPropertyChangeArgs> NotifyPropertyChanged;

        public abstract void Prepare(Mapper mapper, ref TriggerEventFrame eventFrame, bool alterState = true);

        public void CopyBaseMapProps(TriggerMapAction sourceAction)
        {
            mappingId = sourceAction.mappingId;
            triggerDefinition = new TriggerDefinition(sourceAction.triggerDefinition);
        }

        public virtual void SoftCopyFromParent(TriggerMapAction parentAction)
        {
            name = parentAction.name;
            mappingId = parentAction.mappingId;

            this.parentAction = parentAction;
            this.triggerDefinition = new TriggerDefinition(parentAction.triggerDefinition);
        }

        protected virtual void CascadePropertyChange(Mapper mapper, string propertyName)
        {
        }

        public virtual void RaiseNotifyPropertyChange(Mapper mapper, string propertyName)
        {
            NotifyPropertyChanged?.Invoke(this,
                new NotifyPropertyChangeArgs(mapper, propertyName));
        }
    }
}
