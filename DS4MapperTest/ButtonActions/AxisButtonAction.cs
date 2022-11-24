using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.ButtonActions
{
    public class AxisButtonAction : ButtonAction
    {
        private double value;
        private double axisUnit;

        public override double ButtonDistance
        {
            get
            {
                double result = value;
                return result;
            }
        }

        public AxisButtonAction()
        {
            analog = true;
        }

        public AxisButtonAction(ActionFunc actionFunc) : base(actionFunc)
        {
            analog = true;
        }

        public AxisButtonAction(OutputActionData outputAction) : base(outputAction)
        {
            analog = true;
        }

        public AxisButtonAction(IEnumerable<OutputActionData> outputActions) : base(outputActions)
        {
            analog = true;
        }

        public override void Prepare(Mapper mapper, bool status, bool alterState = true)
        {
            value = status ? 1.0 : 0.0;
            //stateData.wasActive = stateData.state;
            //stateData.state = status;
            if (alterState)
            {
                stateData.axisNormValue = value;
                stateData.state = status;
            }

            base.Prepare(mapper, status, alterState);
        }

        public override void PrepareAnalog(Mapper mapper, double axisValue, double axisUnit,
            bool alterState = true)
        {
            if (value != axisValue)
            {
                status = value != 0.0;
                value = axisValue;
                this.axisUnit = axisUnit;
                Prepare(mapper, status, alterState);

                active = true;
                activeEvent = true;
            }
        }
    }
}
