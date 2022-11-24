using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.TriggerActions
{
    public class TriggerNoAction : TriggerMapAction
    {
        public const string ACTION_TYPE_NAME = "TriggerNoAction";

        public TriggerNoAction()
        {
            actionTypeName = ACTION_TYPE_NAME;
        }

        public override void Prepare(Mapper mapper, ref TriggerEventFrame eventFrame, bool alterState = true)
        {
        }

        public override void Event(Mapper mapper)
        {
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
        }

        public override void SoftCopyFromParent(TriggerMapAction parentAction)
        {
            if (parentAction is TriggerNoAction tempNoAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                mappingId = tempNoAction.mappingId;
            }
        }
    }
}
