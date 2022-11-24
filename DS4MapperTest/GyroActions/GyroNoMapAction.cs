using DS4MapperTest.ActionUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.GyroActions
{
    public class GyroNoMapAction : GyroMapAction
    {
        public const string ACTION_TYPE_NAME = "GyroNoAction";
        public GyroNoMapAction()
        {
            actionTypeName = ACTION_TYPE_NAME;
        }

        public GyroNoMapAction(GyroNoMapAction parentAction)
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.parentAction = parentAction;
            parentAction.hasLayeredAction = true;
            mappingId = parentAction.mappingId;
        }

        public override void BlankEvent(Mapper mapper)
        {
        }

        public override void Event(Mapper mapper)
        {
        }

        public override void Prepare(Mapper mapper, ref GyroEventFrame gyroFrame, bool alterState = true)
        {
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
        }

        public override GyroMapAction DuplicateAction()
        {
            return new GyroNoMapAction(this);
        }

        public override void SoftCopyFromParent(GyroMapAction parentAction)
        {
            if (parentAction is GyroNoMapAction tempNoAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                mappingId = tempNoAction.mappingId;
            }
        }
    }
}
