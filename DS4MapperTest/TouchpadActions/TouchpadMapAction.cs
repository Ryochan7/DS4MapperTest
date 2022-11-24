using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.TouchpadActions
{
    public enum TouchesMode : ushort
    {
        OneTouch,
        TwoTouch,
    }

    public enum OuterRingUseRange
    {
        OnlyActive,
        FullRange,
    }

    public struct TouchEventFrame
    {
        public short X;
        public short X2;
        public short Y;
        public short Y2;
        public bool Touch;
        public bool Click;
        public uint numTouches;
        //public TouchesMode supportedTouches;
        public double timeElapsed;
        public bool passDelta;
    }

    public abstract class TouchpadMapAction : MapAction
    {
        public bool activeEvent;
        public bool active;

        protected TouchpadDefinition touchpadDefinition;
        public TouchpadDefinition TouchDefinition
        {
            get => touchpadDefinition; set => touchpadDefinition = value;
        }

        protected event EventHandler<NotifyPropertyChangeArgs> NotifyPropertyChanged;

        public abstract void Prepare(Mapper mapper, ref TouchEventFrame touchFrame, bool alterState = true);
        public virtual void SoftCopyFromParent(TouchpadMapAction parentAction)
        {
            name = parentAction.name;
            mappingId = parentAction.mappingId;
            this.parentAction = parentAction;
            this.touchpadDefinition = new TouchpadDefinition(parentAction.touchpadDefinition);
        }

        public void CopyBaseMapProps(TouchpadMapAction sourceAction)
        {
            mappingId = sourceAction.mappingId;
            touchpadDefinition = new TouchpadDefinition(sourceAction.touchpadDefinition);
        }

        public virtual void PrepareActions()
        {
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
