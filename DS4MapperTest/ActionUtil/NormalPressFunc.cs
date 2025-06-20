using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ActionUtil
{
    public class NormalPressFunc : ActionFunc
    {
        public const int DEFAULT_TURBO_DURATION_MS = 0;
        public const int FIRE_DELAY_MS_DEFAULT = 0;

        private bool inputStatus;
        private bool inToggleState;

        private bool turboEnabled;
        public bool TurboEnabled { get => turboEnabled; set => turboEnabled = value; }

        private int turboDurationMs;
        public int TurboDurationMs { get => turboDurationMs; set => turboDurationMs = value; }

        private Stopwatch turboStopwatch = new Stopwatch();

        private int fireDelayMs;
        public int FireDelayMs { get => fireDelayMs; set => fireDelayMs = value; }

        private Stopwatch fireDelaySw = new Stopwatch();
        private bool fireDelayPassed;

        /*private bool cycleEnabled;
        public bool CycleEnabled
        {
            get => cycleEnabled;
            set => cycleEnabled = value;
        }
        // Keep list of active cycle slots for enumeration purposes in ButtonAction.
        // Edit list contents in Prepare method when activating the ActionFunc
        private List<OutputActionData> cycleActiveActionList =
            new List<OutputActionData>();
        public List<OutputActionData> CycleActionList
        {
            get => cycleActiveActionList;
        }
        // Not sure if this will be useful
        //private OutputActionDataEnumerator cycleActionEnumerator;
        */

        public NormalPressFunc()
        {
        }

        public NormalPressFunc(OutputActionData outputAction)
        {
            outputActions.Add(outputAction);
            outputActionEnumerator = new OutputActionDataEnumerator(outputActions);
        }

        public NormalPressFunc(IEnumerable<OutputActionData> outputActions)
        {
            this.outputActions.AddRange(outputActions);
            outputActionEnumerator =
                new OutputActionDataEnumerator(this.outputActions);
        }

        public NormalPressFunc(NormalPressFunc secondFunc)
        {
            foreach(OutputActionData actionData in secondFunc.outputActions)
            {
                OutputActionData tempData = new OutputActionData(actionData);
                outputActions.Add(tempData);
            }

            outputActionEnumerator =
                new OutputActionDataEnumerator(this.outputActions);
        }

        public override void Prepare(Mapper mapper, bool state,
            ActionFuncStateData stateData)
        {
            if (inputStatus != state)
            {
                bool oldActive = active;
                inputStatus = state;
                activeEvent = true;
                if (inputStatus)
                {
                    bool fireDelayEnabled = fireDelayMs > FIRE_DELAY_MS_DEFAULT;
                    if (!toggleEnabled)
                    {
                        active = true;
                        outputActive = active;
                        finished = false;
                        if (turboEnabled)
                        {
                            turboStopwatch.Restart();
                        }

                        if (fireDelayEnabled)
                        {
                            outputActive = false;
                            fireDelaySw.Restart();
                        }
                    }
                    else
                    {
                        active = true;
                        outputActive = active;
                        finished = false;

                        if (turboEnabled && !oldActive)
                        {
                            turboStopwatch.Restart();
                        }

                        if (fireDelayEnabled)
                        {
                            fireDelaySw.Restart();
                        }
                    }
                }
                else
                {
                    bool fireDelayEnabled = fireDelayMs > FIRE_DELAY_MS_DEFAULT;
                    if (!toggleEnabled)
                    {
                        active = false;
                        outputActive = active;
                        finished = true;
                        if (turboEnabled && turboStopwatch.IsRunning)
                        {
                            turboStopwatch.Reset();
                        }

                        if (fireDelayEnabled && fireDelaySw.IsRunning)
                        {
                            fireDelaySw.Reset();
                        }
                    }
                    else if (inToggleState)
                    {
                        active = false;
                        outputActive = active;
                        finished = true;
                        inToggleState = false;
                        if (turboEnabled && turboStopwatch.IsRunning)
                        {
                            turboStopwatch.Reset();
                        }

                        if (fireDelayEnabled && fireDelaySw.IsRunning)
                        {
                            fireDelaySw.Reset();
                        }
                    }
                    else
                    {
                        // TODO. What does this clause do?
                        active = true;
                        outputActive = active;
                        finished = false;
                        inToggleState = true;
                        fireDelaySw.Restart();
                    }
                }
            }
        }

        public override void Event(Mapper mapper, ActionFuncStateData stateData)
        {
            if (!turboEnabled)
            {
                if (fireDelayMs == FIRE_DELAY_MS_DEFAULT)
                {
                    outputActive = active;
                }
                else if (fireDelaySw.IsRunning && fireDelaySw.ElapsedMilliseconds >= fireDelayMs)
                {
                    fireDelaySw.Reset();
                    fireDelayPassed = true;
                    outputActive = active;
                }
            }
            else
            {
                if (active)
                {
                    bool fireDelayEnabled = fireDelayMs > 0;
                    if (!fireDelayEnabled || (fireDelayEnabled && fireDelayPassed))
                    {
                        if (turboStopwatch.ElapsedMilliseconds >= turboDurationMs)
                        {
                            // Make state change occur
                            turboStopwatch.Restart();
                            outputActive = !outputActive;
                        }
                    }
                    else if (fireDelayEnabled && fireDelaySw.IsRunning &&
                        fireDelaySw.ElapsedMilliseconds >= fireDelayMs)
                    {
                        fireDelaySw.Reset();
                        fireDelayPassed = true;
                    }
                }
                else if (!active)
                {
                    turboStopwatch.Reset();
                    fireDelaySw.Reset();
                    fireDelayPassed = false;
                    outputActive = false;
                }
            }

            activeEvent = false;
        }

        public override void Release(Mapper mapper)
        {
            inputStatus = false;
            active = false;
            activeEvent = false;
            outputActive = false;
            finished = true;
            inToggleState = false;
            fireDelayPassed = false;

            if (turboEnabled && turboStopwatch.IsRunning)
            {
                turboStopwatch.Reset();
            }

            if (fireDelaySw.IsRunning)
            {
                fireDelaySw.Reset();
            }
        }

        public override string Describe(Mapper mapper)
        {
            string result = "";
            List<string> tempList = new List<string>();
            foreach(OutputActionData data in outputActions)
            {
                tempList.Add(data.Describe(mapper));
            }

            if (tempList.Count > 0)
            {
                result = string.Join(", ", tempList);
            }
            else
            {
                result = "Unbound";
            }

            return result;
        }

        public override string DescribeOutputActions(Mapper mapper)
        {
            return Describe(mapper);
        }
    }
}
