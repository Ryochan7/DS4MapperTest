using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.AxisModifiers;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.TriggerActions
{
    public class TriggerTranslate : TriggerMapAction
    {
        private TriggerTranslate parentTrigAction;

        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE = "DeadZone";
            public const string MAX_ZONE = "MaxZone";
            public const string ANTIDEAD_ZONE = "AntiDeadZone";
            public const string OUTPUT_TRIGGER = "OutputTrigger";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.DEAD_ZONE,
            PropertyKeyStrings.MAX_ZONE,
            PropertyKeyStrings.ANTIDEAD_ZONE,
            PropertyKeyStrings.OUTPUT_TRIGGER,
        };

        public const string ACTION_TYPE_NAME = "TriggerTranslateAction";

        private double axisNorm;
        private OutputActionData outputData;
        private AxisDeadZone deadMod;

        public OutputActionData OutputData
        {
            get => outputData;
        }

        public AxisDeadZone DeadMod
        {
            get => deadMod;
        }

        public TriggerTranslate()
        {
            actionTypeName = ACTION_TYPE_NAME;
            outputData = new OutputActionData(OutputActionData.ActionType.GamepadControl,
                JoypadActionCodes.AxisLTrigger);

            //deadMod = new AxisDeadZone(30 / 255.0, 1.0, 0.0);
            deadMod = new AxisDeadZone(0.0, 1.0, 0.0);
        }

        public override void Prepare(Mapper mapper, ref TriggerEventFrame eventFrame, bool alterState = true)
        {
            //axisNorm = axisValue / 255.0;
            int maxDir = triggerDefinition.trigAxis.max;
            deadMod.CalcOutValues((int)eventFrame.axisValue, maxDir, out axisNorm);
            stateData.state = axisNorm != 0.0;
            stateData.axisNormValue = axisNorm;

            active = true;
            activeEvent = true;
        }

        public override void Event(Mapper mapper)
        {
            mapper.GamepadFromAxisInput(outputData, axisNorm);

            active = false;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            axisNorm = 0.0;
            mapper.GamepadFromAxisInput(outputData, axisNorm);

            active = false;
            activeEvent = false;

            if (resetState)
            {
                stateData.Reset();
            }
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            axisNorm = 0.0;
            mapper.GamepadFromAxisInput(outputData, axisNorm);

            active = false;
            activeEvent = false;

            if (resetState)
            {
                stateData.Reset();
            }
        }

        public override void SoftCopyFromParent(TriggerMapAction parentAction)
        {
            if (parentAction is TriggerTranslate tempTrigTranslateAction)
            {
                base.SoftCopyFromParent(parentAction);

                parentTrigAction = tempTrigTranslateAction;

                tempTrigTranslateAction.NotifyPropertyChanged += TempTrigTranslateAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch(parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempTrigTranslateAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE:
                            deadMod.DeadZone = tempTrigTranslateAction.deadMod.DeadZone;
                            break;
                        case PropertyKeyStrings.MAX_ZONE:
                            deadMod.MaxZone = tempTrigTranslateAction.deadMod.MaxZone;
                            break;
                        case PropertyKeyStrings.ANTIDEAD_ZONE:
                            deadMod.AntiDeadZone = tempTrigTranslateAction.deadMod.AntiDeadZone;
                            break;
                        case PropertyKeyStrings.OUTPUT_TRIGGER:
                            outputData.JoypadCode = tempTrigTranslateAction.OutputData.JoypadCode;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempTrigTranslateAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
        {
            CascadePropertyChange(e.Mapper, e.PropertyName);
        }

        protected override void CascadePropertyChange(Mapper mapper, string propertyName)
        {
            if (changedProperties.Contains(propertyName))
            {
                // Property already overrridden in action. Leave
                return;
            }
            else if (parentAction == null)
            {
                // No parent action. Leave
                return;
            }

            TriggerTranslate tempTrigTranslateAction = parentAction as TriggerTranslate;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempTrigTranslateAction.name;
                    break;
                case PropertyKeyStrings.DEAD_ZONE:
                    deadMod.DeadZone = tempTrigTranslateAction.deadMod.DeadZone;
                    break;
                case PropertyKeyStrings.MAX_ZONE:
                    deadMod.MaxZone = tempTrigTranslateAction.deadMod.MaxZone;
                    break;
                case PropertyKeyStrings.ANTIDEAD_ZONE:
                    deadMod.AntiDeadZone = tempTrigTranslateAction.deadMod.AntiDeadZone;
                    break;
                case PropertyKeyStrings.OUTPUT_TRIGGER:
                    outputData.JoypadCode = tempTrigTranslateAction.OutputData.JoypadCode;
                    break;
                default:
                    break;
            }
        }
    }
}
