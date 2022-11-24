using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ButtonActions
{
    public class CycleButton : ButtonMapAction
    {
        public const string ACTION_TYPE_NAME = "CycleButtonAction";

        public override double ButtonDistance
        {
            get
            {
                double result = 0.0;
                if (inputStatus) result = 1.0;
                return result;
            }
        }

        public override double AxisUnit
        {
            get
            {
                double result = 0.0;
                if (inputStatus) result = 1.0;
                return result;
            }
        }

        // Specify the input state of the button
        protected bool inputStatus;

        private List<OutputActionData> actions = new List<OutputActionData>();
        public List<OutputActionData> Actions => actions;
        private OutputActionDataEnumerator actionsEnumerator;

        private string cycleIdentifier;
        public string CycleIdentifier => cycleIdentifier;

        public CycleButton(string cycleIdentifier)
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.cycleIdentifier = cycleIdentifier;
            actionsEnumerator = new OutputActionDataEnumerator(actions);
        }

        public override void Prepare(Mapper mapper, bool status, bool alterState = true)
        {
            if (this.inputStatus != status)
            {
                inputStatus = status;
                active = status;
            }

            activeEvent = status;
        }

        public override void Event(Mapper mapper)
        {
            OutputActionData current = actionsEnumerator.Current;
            mapper.RunEventFromButton(current, inputStatus);
        }

        public void MoveNext()
        {
            if (actionsEnumerator.AtEnd())
            {
                actionsEnumerator.Reset();
            }

            actionsEnumerator.MoveNext();
        }

        public void MovePrevious()
        {
            if (actionsEnumerator.AtStart())
            {
                actionsEnumerator.MoveToEnd();
            }

            actionsEnumerator.MovePrevious();
        }

        public void ResetCycle()
        {
            actionsEnumerator.Reset();
        }

        public void MoveToEnd()
        {
            actionsEnumerator.MoveToEnd();
        }

        public void MoveToStep(int index)
        {
            actionsEnumerator.MoveToStep(index);
        }

        public static bool ValidDataBinding(OutputActionData.ActionType testType)
        {
            bool result = false;
            switch(testType)
            {
                case OutputActionData.ActionType.Keyboard:
                case OutputActionData.ActionType.MouseButton:
                case OutputActionData.ActionType.MouseWheel:
                case OutputActionData.ActionType.GamepadControl:
                case OutputActionData.ActionType.SwitchSet:
                case OutputActionData.ActionType.SwitchActionLayer:
                case OutputActionData.ActionType.ApplyActionLayer:
                case OutputActionData.ActionType.RemoveActionLayer:
                    result = true;
                    break;
                default:
                    break;
            }

            return result;
        }

        public override ButtonMapAction DuplicateAction()
        {
            throw new NotImplementedException();
        }

        public override void PrepareAnalog(Mapper mapper, double axisValue, double axisUnit, bool alterState = true)
        {
            Prepare(mapper, axisValue != 0.0, alterState);
        }

        public void CreateNewEnumerator()
        {
            actionsEnumerator = new OutputActionDataEnumerator(actions);
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            if (inputStatus && !ignoreReleaseActions)
            {
                OutputActionData current = actionsEnumerator.Current;
                mapper.RunEventFromButton(current, false);
            }

            actionsEnumerator.Reset();

            if (resetState)
            {
                stateData.Reset();
            }

            inputStatus = false;
        }
    }
}
