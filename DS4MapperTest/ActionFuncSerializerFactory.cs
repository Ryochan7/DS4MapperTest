using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;

namespace DS4MapperTest
{
    public static class ActionFuncSerializerFactory
    {
        public static ActionFuncSerializer CreateSerializer(ActionFunc tempFunc)
        {
            ActionFuncSerializer serializer = null;
            switch(tempFunc)
            {
                case NormalPressFunc:
                    serializer = new NormalPressFuncSerializer(tempFunc);
                    break;
                case HoldPressFunc:
                    serializer = new HoldPressFuncSerializer(tempFunc);
                    break;
                case StartPressFunc:
                    serializer = new StartPressFuncSerializer(tempFunc);
                    break;
                case ReleaseFunc:
                    serializer = new ReleaseFuncSerializer(tempFunc);
                    break;
                case DistanceFunc:
                    serializer = new DistanceFuncSerializer(tempFunc);
                    break;
                case ChordedPressFunc:
                    serializer = new ChordedPressFuncSerializer(tempFunc);
                    break;
                case AnalogFunc:
                    serializer = new AnalogFuncSerializer(tempFunc);
                    break;
                case DoublePressFunc:
                    break;
                default:
                    break;
            }

            return serializer;
        }
    }
}
