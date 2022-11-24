using DS4MapperTest.ActionUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.ButtonActions
{
    public class ButtonNoAction : ButtonMapAction
    {
        private const string ACTION_TYPE = "ButtonNoAction";

        public override double ButtonDistance => throw new NotImplementedException();
        public override double AxisUnit => throw new NotImplementedException();

        public ButtonNoAction()
        {
            actionTypeName = ACTION_TYPE;
        }

        protected ButtonNoAction(ButtonNoAction parentAction)
        {
            if (parentAction != null)
            {
                this.parentAction = parentAction;
                parentAction.hasLayeredAction = true;
                mappingId = parentAction.mappingId;
            }

            actionTypeName = ACTION_TYPE;
        }

        public override void Prepare(Mapper mapper, bool status, bool alterState = true)
        {
        }

        public override void Event(Mapper mapper)
        {
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
        }

        public override void PrepareAnalog(Mapper mapper, double axisValue, double axisUnit,
            bool alterState = true)
        {
            throw new NotImplementedException();
        }

        public override ButtonMapAction DuplicateAction()
        {
            return new ButtonNoAction(this);
        }

        public override void SoftCopyFromParent(ButtonMapAction parentAction)
        {
            if (parentAction is ButtonNoAction tempNoAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                tempNoAction.hasLayeredAction = true;
                mappingId = tempNoAction.mappingId;
            }
        }

        public override string DescribeActions(Mapper mapper)
        {
            return "None";
        }
    }
}
