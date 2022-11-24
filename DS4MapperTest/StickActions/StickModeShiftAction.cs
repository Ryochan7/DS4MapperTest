using DS4MapperTest.ActionUtil;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.MapperUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.StickActions
{
    public class StickModeShiftAction : StickMapAction
    {
        public StickMapAction primaryAction;
        public StickMapAction modeShiftAction;
        public StickMapAction currentAction;
        private StickMapAction previousAction;
        private JoypadActionCodes modeShiftTrigger;
        public JoypadActionCodes ModeShiftTrigger
        {
            get => modeShiftTrigger; set => modeShiftTrigger = value;
        }

        private StickDefinition stickDefinition;

        public StickModeShiftAction(StickDefinition stickDefinition)
        {
            this.stickDefinition = stickDefinition;
        }

        private StickModeShiftAction(StickModeShiftAction parentAction)
        {
            if (parentAction != null)
            {
                this.parentAction = parentAction;
            }
        }

        public override void Prepare(Mapper mapper, int axisXVal, int axisYVal, bool alterState = true)
        {
            bool shifted = modeShiftTrigger != JoypadActionCodes.Empty &&
                    mapper.IsButtonActive(modeShiftTrigger);

            if (!shifted)
            {
                currentAction = primaryAction;
            }
            else
            {
                currentAction = modeShiftAction;
            }

            currentAction.Prepare(mapper, axisXVal, axisYVal, alterState);
            active = true;
        }

        public override void Event(Mapper mapper)
        {
            if (previousAction != currentAction && previousAction != null)
            {
                previousAction.Prepare(mapper, stickDefinition.xAxis.mid,
                    stickDefinition.yAxis.mid);
                previousAction.Event(mapper);
            }

            currentAction.Event(mapper);

            previousAction = currentAction;
            active = false;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            if (previousAction != currentAction && previousAction != null)
            {
                previousAction.Release(mapper, resetState, ignoreReleaseActions);
            }

            currentAction?.Release(mapper, resetState, ignoreReleaseActions);
        }

        public override StickMapAction DuplicateAction()
        {
            return new StickModeShiftAction(this);
        }
    }
}
