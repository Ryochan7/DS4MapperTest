using DS4MapperTest.ActionUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.DPadActions
{
    public class DPadNoAction : DPadMapAction
    {
        public const string ACTION_TYPE_NAME = "DPadNoAction";

        public DPadNoAction()
        {
            actionTypeName = ACTION_TYPE_NAME;
        }

        public DPadNoAction(DPadNoAction parentAction)
        {
            actionTypeName = ACTION_TYPE_NAME;

            if (parentAction != null)
            {
                this.parentAction = parentAction;
                parentAction.hasLayeredAction = true;
                mappingId = parentAction.mappingId;
            }
        }

        public override void Prepare(Mapper mapper, DpadDirections value, bool alterState = true)
        {
        }

        public override void Event(Mapper mapper)
        {
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
        }

        public override DPadMapAction DuplicateAction()
        {
            return new DPadNoAction(this);
        }

        public override void SoftCopyFromParent(DPadMapAction parentAction)
        {
            if (parentAction is DPadNoAction tempNoAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                tempNoAction.hasLayeredAction = true;
                mappingId = tempNoAction.mappingId;
            }
        }
    }
}
