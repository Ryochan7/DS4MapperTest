using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.AxisModifiers;
using DS4MapperTest.ButtonActions;

namespace DS4MapperTest.TriggerActions
{
    public class TriggerButtonAction : TriggerMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE = "DeadZone";
            public const string OUTPUT_BINDING = "OutputBinding";
            //public const string MAX_ZONE = "MaxZone";
            //public const string ANTIDEAD_ZONE = "AntiDeadZone";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.DEAD_ZONE,
            PropertyKeyStrings.OUTPUT_BINDING,
            //PropertyKeyStrings.MAX_ZONE,
            //PropertyKeyStrings.ANTIDEAD_ZONE,
        };

        public const string ACTION_TYPE_NAME = "TriggerButtonAction";

        private bool inputStatus;
        private AxisDirButton eventButton = new AxisDirButton();
        public AxisDirButton EventButton
        {
            get => eventButton;
            set => eventButton = value;
        }

        private bool useParentEventButton;
        public bool UseParentEventButton
        {
            get => useParentEventButton;
            set => useParentEventButton = value;
        }

        private double axisNorm = 0.0;
        private AxisDeadZone deadZone;

        public AxisDeadZone DeadZone
        {
            get => deadZone;
        }

        public TriggerButtonAction()
        {
            actionTypeName = ACTION_TYPE_NAME;

            double tempDeadZone = 30 / 255.0;
            deadZone = new AxisDeadZone(tempDeadZone, 1.0, 0.0);
        }

        public override void Prepare(Mapper mapper, ref TriggerEventFrame eventFrame, bool alterState = true)
        {
            //bool inSafeZone = axisValue > 30;
            int maxDir = triggerDefinition.trigAxis.max;
            deadZone.CalcOutValues((int)eventFrame.axisValue, maxDir, out axisNorm);
            //bool inSafeZone = axisNorm != 0.0;
            //if (inSafeZone)
            //{
            //    axisNorm = (axisValue - 30.0) / (255.0 - 30.0);
            //}
            //else
            //{
            //    axisNorm = 0.0;
            //}

            eventButton.PrepareAnalog(mapper, axisNorm, 1.0);

            inputStatus = axisNorm > 0.0;
            active = eventButton.active;
            activeEvent = true;
        }

        public override void Event(Mapper mapper)
        {
            if (eventButton.active) eventButton.Event(mapper);

            active = axisNorm > 0.0;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            eventButton.Release(mapper, ignoreReleaseActions);

            axisNorm = 0.0;
            inputStatus = false;
            active = activeEvent = false;
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            if (!useParentEventButton)
            {
                eventButton.PrepareAnalog(mapper, 0.0, 0.0);
                eventButton.Event(mapper);
            }

            axisNorm = 0.0;
            inputStatus = false;
            active = activeEvent = false;
        }

        public override void SoftCopyFromParent(TriggerMapAction parentAction)
        {
            if (parentAction is TriggerButtonAction tempBtnAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                mappingId = tempBtnAction.mappingId;

                tempBtnAction.NotifyPropertyChanged += TempBtnAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch (parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempBtnAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE:
                            deadZone.DeadZone = tempBtnAction.deadZone.DeadZone;
                            break;
                        case PropertyKeyStrings.OUTPUT_BINDING:
                            useParentEventButton = true;
                            eventButton = tempBtnAction.EventButton;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempBtnAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
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

            TriggerButtonAction tempBtnAction = parentAction as TriggerButtonAction;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempBtnAction.name;
                    break;
                case PropertyKeyStrings.DEAD_ZONE:
                    deadZone.DeadZone = tempBtnAction.deadZone.DeadZone;
                    break;
                case PropertyKeyStrings.OUTPUT_BINDING:
                    useParentEventButton = true;
                    eventButton = tempBtnAction.EventButton;
                    break;
                default:
                    break;
            }
        }
    }
}
