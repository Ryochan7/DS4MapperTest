using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest
{
    public static class MapActionSerializerFactory
    {
        public static MapActionSerializer CreateSerializer(ActionLayer layer, MapAction action)
        {
            MapActionSerializer serializer = null;
            switch(action.ActionTypeName)
            {
                case "ButtonAction":
                    serializer = new ButtonActionSerializer(layer, action);
                    break;
                case "ButtonNoAction":
                    serializer = new ButtonNoActionSerializer(layer, action);
                    break;
                case "StickPadAction":
                    serializer = new StickPadActionSerializer(layer, action);
                    break;
                case "StickMouseAction":
                    serializer = new StickMouseSerializer(layer, action);
                    break;
                case "StickTranslateAction":
                    serializer = new StickTranslateSerializer(layer, action);
                    break;
                case "StickAbsMouseAction":
                    serializer = new StickAbsMouseActionSerializer(layer, action);
                    break;
                case "StickCircularAction":
                    serializer = new StickCircularSerializer(layer, action);
                    break;
                case "StickFlickStickAction":
                    serializer = new StickFlickStickActionSerializer(layer, action);
                    break;
                case "StickNoAction":
                    serializer = new StickNoActionSerializer(layer, action);
                    break;
                case "TriggerTranslateAction":
                    serializer = new TriggerTranslateActionSerializer(layer, action);
                    break;
                case "TriggerButtonAction":
                    serializer = new TriggerButtonActionSerializer(layer, action);
                    break;
                case "TriggerDualStageAction":
                    serializer = new TriggerDualStageActionSerializer(layer, action);
                    break;
                case "TouchStickTranslateAction":
                    serializer = new TouchpadStickActionSerializer(layer, action);
                    break;
                case "TouchMouseAction":
                    serializer = new TouchpadMouseSerializer(layer, action);
                    break;
                case "TouchMouseJoystickAction":
                    serializer = new TouchpadMouseJoystickSerializer(layer, action);
                    break;
                case "TouchActionPadAction":
                    serializer = new TouchpadActionPadSerializer(layer, action);
                    break;
                case "TouchAbsPadAction":
                    serializer = new TouchpadAbsActionSerializer(layer, action);
                    break;
                case "TouchCircularAction":
                    serializer = new TouchpadCircularSerializer(layer, action);
                    break;
                case "TouchDirSwipeAction":
                    serializer = new TouchpadDirectionalSwipeSerializer(layer, action);
                    break;
                case "TouchAxesAction":
                    serializer = new TouchpadAxesActionSerializer(layer, action);
                    break;
                case "TouchSingleButtonAction":
                    serializer = new TouchpadSingleButtonSerializer(layer, action);
                    break;
                case "TouchNoAction":
                    serializer = new TouchpadNoActionSerializer(layer, action);
                    break;
                case "DPadAction":
                    serializer = new DpadActionSerializer(layer, action);
                    break;
                case "DPadNoAction":
                    serializer = new DpadNoActionSerializer(layer, action);
                    break;
                case "DPadTranslateAction":
                    serializer = new DpadTranslateSerializer(layer, action);
                    break;
                case "GyroMouseAction":
                    serializer = new GyroMouseSerializer(layer, action);
                    break;
                case "GyroMouseJoystickAction":
                    serializer = new GyroMouseJoystickSerializer(layer, action);
                    break;
                case "GyroDirSwipeAction":
                    serializer = new GyroDirectionalSwipeSerializer(layer, action);
                    break;
                case "GyroNoAction":
                    serializer = new GyroNoMapActionSerializer(layer, action);
                    break;
                default:
                    break;
            }
            return serializer;
        }
    }
}
