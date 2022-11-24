using DS4MapperTest.ActionUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.GyroActions
{
    public struct GyroEventFrame
    {
        public short GyroYaw;
        public short GyroPitch;
        public short GyroRoll;
        public double AngGyroYaw;
        public double AngGyroPitch;
        public double AngGyroRoll;
        public short AccelX;
        public short AccelY;
        public short AccelZ;
        public double AccelXG, AccelYG, AccelZG;
        public double timeElapsed;

        public double elapsedReference;
    }

    public abstract class GyroMapAction : MapAction
    {
        public bool active;
        public bool activeEvent;
        protected GyroSensDefinition gyroSensDefinition = new GyroSensDefinition();
        public GyroSensDefinition GyroSensDefinition
        {
            get => gyroSensDefinition;
            set => gyroSensDefinition = value;
        }

        protected event EventHandler<NotifyPropertyChangeArgs> NotifyPropertyChanged;

        public abstract void Prepare(Mapper mapper, ref GyroEventFrame gyroFrame, bool alterState = true);

        public abstract void BlankEvent(Mapper mapper);

        public abstract GyroMapAction DuplicateAction();

        public void CopyBaseMapProps(GyroMapAction sourceAction)
        {
            mappingId = sourceAction.mappingId;
            gyroSensDefinition = new GyroSensDefinition(sourceAction.GyroSensDefinition);
        }

        public virtual void SoftCopyFromParent(GyroMapAction parentAction)
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
