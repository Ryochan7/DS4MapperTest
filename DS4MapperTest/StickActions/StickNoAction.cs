using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;

namespace DS4MapperTest.StickActions
{
    public class StickNoAction : StickMapAction
    {
        public const string ACTION_TYPE_NAME = "StickNoAction";
        public StickNoAction()
        {
            actionTypeName = ACTION_TYPE_NAME;
        }

        public StickNoAction(StickNoAction parentAction)
        {
            actionTypeName = ACTION_TYPE_NAME;
            if (parentAction != null)
            {
                this.parentAction = parentAction;
                mappingId = parentAction.mappingId;
            }
        }

        public override void Event(Mapper mapper)
        {
        }

        public override void Prepare(Mapper mapper, int axisXVal, int axisYVal, bool alterState = true)
        {
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
        }

        public override StickMapAction DuplicateAction()
        {
            return new StickNoAction(this);
        }

        public override void SoftCopyFromParent(StickMapAction parentAction)
        {
            if (parentAction is StickNoAction tempNoAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                mappingId = tempNoAction.mappingId;
            }
        }
    }
}
