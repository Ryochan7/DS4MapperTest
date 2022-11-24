using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ButtonActions
{
    public class TouchpadCircularButton : ButtonAction
    {
        protected double notches;

        public TouchpadCircularButton()
        {
            useNotches = true;
            processAction = true;
        }

        public void PrepareCircular(Mapper mapper, double notches, bool alterState = true)
        {
            this.notches = notches;
            bool tempStatus = notches != 0.0;
            //if (this.status != tempStatus)
            {
                status = tempStatus;

                if (useParentActions)
                {
                    actionFuncCandidates.AddRange(parentButtonAct.ActionFuncs);
                }
                else
                {
                    actionFuncCandidates.AddRange(actionFuncs);
                }

                if (alterState)
                {
                    stateData.wasActive = stateData.state;
                    stateData.state = status;
                }
            }

            active = true;
            activeEvent = true;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            notches = 0.0;

            base.Release(mapper, resetState, ignoreReleaseActions);
        }

        public override void WrapNotchesProcess(OutputActionData action)
        {
            //base.WrapTickProcess(action);

            switch (action.OutputType)
            {
                case OutputActionData.ActionType.Keyboard:
                case OutputActionData.ActionType.MouseButton:
                case OutputActionData.ActionType.MouseWheel:
                case OutputActionData.ActionType.GamepadControl:
                    {
                        action.currentNotches += this.notches;
                    }

                    break;
                case OutputActionData.ActionType.ApplyActionLayer:
                case OutputActionData.ActionType.HoldActionLayer:
                case OutputActionData.ActionType.RemoveActionLayer:
                case OutputActionData.ActionType.SwitchActionLayer:
                    action.currentNotches = 1.0;
                    break;
                default:
                    action.currentNotches += this.notches;
                    break;
            }
        }

        public override void ProcessAction(Mapper mapper, bool outputActive, OutputActionData action)
        {
            WrapNotchesProcess(action);

            switch(action.OutputType)
            {
                case OutputActionData.ActionType.Keyboard:
                case OutputActionData.ActionType.MouseButton:
                case OutputActionData.ActionType.GamepadControl:
                    {
                        //action.currentNotches += this.notches;
                        //double currentNotches = (int)action.currentNotches;
                        if (outputActive && !action.activatedEvent && action.currentNotches >= 1.0)
                        {
                            mapper.RunEventFromButton(action, true);
                            action.currentNotches -= 1.0;
                        }
                        else if (!outputActive && action.activatedEvent)
                        {
                            mapper.RunEventFromButton(action, false);
                        }

                        action.firstRun = false;
                    }
                    break;
                case OutputActionData.ActionType.MouseWheel:
                    {
                        double currentNotches = (int)action.currentNotches;
                        if (outputActive && !action.activatedEvent && action.currentNotches >= 1.0)
                        {
                            mapper.RunEventFromRelative(action, true, currentNotches);
                            action.currentNotches = action.currentNotches - currentNotches;
                        }
                        else if (!outputActive && action.activatedEvent)
                        {
                            mapper.RunEventFromButton(action, false);
                        }

                        action.firstRun = false;
                    }

                    break;
                default:
                    break;
            }
        }

        public void PrepareActions()
        {
            foreach(ActionFunc func in actionFuncs)
            {
                foreach(OutputActionData actionData in func.OutputActions)
                {
                    actionData.useNotches = true;
                    actionData.currentNotches = 0.0;
                }
            }
        }
    }
}
