using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ActionUtil
{
    public abstract class ActionFunc
    {
        protected string name;
        public string Name
        {
            get => name;
            set => name = value;
        }

        protected List<OutputActionData> outputActions =
            new List<OutputActionData>();
        protected OutputActionDataEnumerator outputActionEnumerator;

        public List<OutputActionData> OutputActions
        {
            get => outputActions; set => outputActions = value;
        }

        // TODO: Can't remember what I wanted to do with this
        public bool activeEvent;
        // State if an ActionFunc is considered active. Ex. For HoldPressFunc,
        // hold duration has passed and output actions should be checked
        public bool active;
        // State if output actions for an ActionFunc should be sent to the
        // Mapper for processing
        public bool outputActive;

        public bool finished;
        public bool toggleEnabled;
        public bool delayEnd;
        public bool interruptable;
        public bool canPressInterrupt;
        public double distance;

        // Type flags used for ButtonAction to add processing checks without
        // checking the specific ActionFunc class type. Flag should be set in
        // subclass ctor
        public bool analog;
        public bool onRelease;
        public bool onDistance;
        public bool onChorded;

        public abstract void Prepare(Mapper mapper, bool state, ActionFuncStateData stateData);
        /*public virtual void PrepareAnalog(Mapper mapper, double state)
        {
            throw new NotImplementedException();
        }
        */

        public abstract void Event(Mapper mapper, ActionFuncStateData stateData);

        public virtual void PrepareState(Mapper mapper, ActionFunc secondFunc)
        {
        }

        public abstract void Release(Mapper mapper);
        protected virtual void ReleaseEvents(Mapper mapper)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ActionFunc secondFunc)
        {
            foreach (OutputActionData actionData in outputActions)
            {
                OutputActionData tempData = new OutputActionData(actionData);
                secondFunc.outputActions.Add(tempData);
            }

            secondFunc.outputActionEnumerator =
                new OutputActionDataEnumerator(secondFunc.outputActions);
        }

        public virtual string Describe(Mapper mapper)
        {
            string result = "";
            return result;
        }

        public virtual string DescribeOutputActions(Mapper mapper)
        {
            string result = "";
            return result;
        }
    }
}
