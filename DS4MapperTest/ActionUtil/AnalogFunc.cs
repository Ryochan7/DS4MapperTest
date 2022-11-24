using System;
using System.Collections.Generic;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ActionUtil
{
    public class AnalogFunc : ActionFunc
    {
        public const double DEFAULT_MIN_OUTPUT = 0.2;
        public const double DEFAULT_MAX_OUTPUT = 1.0;

        private bool inputStatus;
        //private JoypadActionCodes outputAxis;
        private double minOutput = DEFAULT_MIN_OUTPUT;
        private double maxOutput = DEFAULT_MAX_OUTPUT;
        private OutputActionData analogActionData =
            new OutputActionData(OutputActionData.ActionType.GamepadControl,
                JoypadActionCodes.Empty);

        public JoypadActionCodes OutputAxis
        {
            get => analogActionData.JoypadCode;
            set
            {
                if (value >= JoypadActionCodes.Axis1 &&
                    value <= JoypadActionCodes.AxisMax)
                {
                    analogActionData.JoypadCode = value;
                }
            }
        }

        public double MinOutput
        {
            get => minOutput;
            set
            {
                minOutput = Math.Clamp(value, 0.0, 1.0);
                CheckOutputBounds();
            }
        }

        public double MaxOutput
        {
            get => maxOutput;
            set
            {
                maxOutput = Math.Clamp(value, 0.0, 1.0);
                CheckOutputBounds();
            }
        }

        private void CheckOutputBounds()
        {
            if (minOutput > maxOutput)
            {
                minOutput = maxOutput;
            }
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
        }

        public override void Event(Mapper mapper, ActionFuncStateData stateData)
        {
            if (inputStatus)
            {
                double axisNorm = stateData.axisNormValue;
                double outputNorm = (maxOutput - minOutput) * axisNorm + minOutput;
                mapper.GamepadFromAxisInput(analogActionData, outputNorm);
            }

            activeEvent = false;
        }

        public override void Release(Mapper mapper)
        {
            if (active)
            {
                mapper.GamepadFromAxisInput(analogActionData, 0.0);
            }

            inputStatus = false;
            active = false;
            outputActive = active;
            activeEvent = false;
            finished = false;
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

                result = $"Analog({string.Join(", ", tempList)})";
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
