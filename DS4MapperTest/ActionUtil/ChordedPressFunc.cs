using System;
using System.Collections.Generic;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ActionUtil
{
    public class ChordedPressFunc : ActionFunc
    {
        private bool inputStatus;
        private JoypadActionCodes triggerButton;
        private bool chordActive;

        public JoypadActionCodes TriggerButton
        {
            get => triggerButton;
            set => triggerButton = value;
        }

        public ChordedPressFunc()
        {
            onChorded = true;
        }

        public override void Prepare(Mapper mapper, bool state,
            ActionFuncStateData stateData)
        {
            if (inputStatus != state)
            {
                inputStatus = state;
                activeEvent = true;

                if (inputStatus)
                {
                    bool triggerActive = mapper.IsButtonActive(triggerButton);
                    chordActive = triggerActive;
                    active = chordActive;
                    finished = false;
                    outputActive = active;
                }
                else
                {
                    chordActive = false;
                    active = false;
                    finished = true;
                    outputActive = active;
                }
            }
        }

        public override void Event(Mapper mapper, ActionFuncStateData stateData)
        {
            if (inputStatus)
            {
                bool chordActive = mapper.IsButtonActive(triggerButton);
                if (chordActive)
                {
                    active = true;
                    outputActive = active;
                    finished = false;
                }
                else
                {
                    active = false;
                    outputActive = active;
                    finished = true;
                }
            }
            else
            {
                active = false;
                outputActive = active;
                finished = true;
                chordActive = false;
            }

            activeEvent = false;
        }

        public override void Release(Mapper mapper)
        {
            inputStatus = false;
            active = false;
            outputActive = active;
            activeEvent = false;
            finished = true;
            chordActive = false;
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

                result = $"Chord({string.Join(", ", tempList)})";
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
            else
            {
                result = "Unbound";
            }

            return result;
        }
    }
}
