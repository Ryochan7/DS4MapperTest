using DS4MapperTest.MapperUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.ActionUtil
{
    public class ReleaseFunc : ActionFunc
    {
        public const int DELAY_DURATION_DEFAULT = 100;

        private bool status;

        private int delayDurationMs = DELAY_DURATION_DEFAULT;
        public int DelayDurationMs { get => delayDurationMs; set => delayDurationMs = value; }

        private int durationMs;
        public int DurationMs { get => durationMs; set => durationMs = value; }

        private bool waited;

        private Stopwatch delayTimer = new Stopwatch();

        public ReleaseFunc()
        {
            onRelease = true;
        }

        public ReleaseFunc(ReleaseFunc srcFunc)
        {
            srcFunc.CopyTo(this);
            onRelease = srcFunc.onRelease;
            durationMs = srcFunc.durationMs;
            delayDurationMs = srcFunc.delayDurationMs;
        }

        public override void Prepare(Mapper mapper, bool state, ActionFuncStateData stateData)
        {
            if (this.status != state)
            {
                this.status = state;
                waited = false;
                activeEvent = true;

                if (status)
                {
                    if (delayEnd)
                    {
                        //ReleaseEvents(mapper);
                    }

                    active = false;
                    delayEnd = false;
                    finished = false;
                }
                else
                {
                    delayTimer.Reset();
                }
            }

            if (!active)
            {
                if (status && !waited)
                {
                    if (stateData.elapsed.ElapsedMilliseconds >= durationMs)
                    {
                        waited = true;
                        active = true;
                        outputActive = true;
                        activeEvent = true;
                        delayEnd = true;
                        delayTimer.Restart();
                        // Execute system event
                        //SendOutputEvent(mapper);
                    }
                }
            }
        }

        public override void Event(Mapper mapper, ActionFuncStateData stateData)
        {
            if (active && delayEnd)
            {
                if (delayTimer.ElapsedMilliseconds >= delayDurationMs)
                {
                    //active = false;
                    //activeEvent = false;
                    //delayEnd = false;
                    //finished = true;
                    //delayTimer.Reset();

                    // Execute Up Events
                    ReleaseEvents(mapper);

                    Release(mapper);
                }
            }
        }

        public override void Release(Mapper mapper)
        {
            status = false;
            active = false;
            outputActive = false;
            activeEvent = false;
            waited = false;
            finished = false;
            delayEnd = false;
            delayTimer.Reset();
        }

        protected override void ReleaseEvents(Mapper mapper)
        {
            OutputActionDataEnumerator activeActionsEnumerator =
                new OutputActionDataEnumerator(outputActions);
            activeActionsEnumerator.MoveToEnd();
            while (activeActionsEnumerator.MovePrevious())
            {
                OutputActionData action = activeActionsEnumerator.Current;
                if (action.activatedEvent)
                {
                    mapper.RunEventFromButton(action, false);
                }
            }

            //mapper.RemoveReleaseFunc(this);
        }

        public override string Describe(Mapper mapper)
        {
            string result = "";
            List<string> tempList = new List<string>();
            foreach (OutputActionData data in outputActions)
            {
                tempList.Add(data.Describe(mapper));
            }

            if (tempList.Count > 0)
            {
                result = string.Join(", ", tempList);
            }

            return result;
        }

        public override string DescribeOutputActions(Mapper mapper)
        {
            string result = "";
            List<string> tempList = new List<string>();
            foreach (OutputActionData data in outputActions)
            {
                tempList.Add(data.Describe(mapper));
            }

            if (tempList.Count > 0)
            {
                result = $"{string.Join(", ", tempList)}";
            }

            return result;
        }
    }
}
