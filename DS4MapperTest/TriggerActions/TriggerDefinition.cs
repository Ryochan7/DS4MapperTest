using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.TriggerActions
{
    public class TriggerDefinition
    {
        public struct TriggerAxisData
        {
            public short max;
            public short min;
            public bool hasClickButton;
            public JoypadActionCodes fullClickBtnCode;
        }

        public TriggerAxisData trigAxis;
        public TriggerActionCodes trigCode;

        public TriggerDefinition(TriggerAxisData axisData, TriggerActionCodes trigCode)
        {
            this.trigAxis = axisData;
            this.trigCode = trigCode;
        }

        public TriggerDefinition(TriggerDefinition sourceDef)
        {
            trigAxis = sourceDef.trigAxis;
            trigCode = sourceDef.trigCode;
        }
    }
}
