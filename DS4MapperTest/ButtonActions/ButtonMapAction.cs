using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ButtonActions
{
    public abstract class ButtonMapAction : MapAction
    {
        public bool activeEvent;
        public bool active;
        public bool analog;
        public bool useNotches;
        //public bool preprocess;
        public bool processAction;

        public abstract double ButtonDistance { get; }

        public abstract double AxisUnit { get; }

        public abstract void Prepare(Mapper mapper, bool status, bool alterState = true);
        public abstract void PrepareAnalog(Mapper mapper, double axisNorm, double axisUnit,
            bool alterState = true);

        public abstract ButtonMapAction DuplicateAction();
        public virtual void CopyAction(ButtonMapAction sourceAction, bool addProps = true)
        {
        }

        public virtual void SoftCopyFromParent(ButtonMapAction parentAction)
        {
        }

        public virtual void SoftCopy(ButtonMapAction parentAction)
        {
        }

        public virtual void ProcessAction(Mapper mapper, bool outputActive,
            OutputActionData action)
        {
        }

        protected virtual void CascadePropertyChange(Mapper mapper, string propertyName)
        {
        }

        public virtual void RaiseNotifyPropertyChange(Mapper mapper, string propertyName)
        {
        }

        public virtual string DescribeActions(Mapper mapper)
        {
            string result = "";
            return result;
        }
    }
}
