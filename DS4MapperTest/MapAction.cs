using DS4MapperTest.ActionUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest
{
    public abstract class MapAction
    {
        public enum HapticsIntensity : ushort
        {
            Off,
            Light,
            Medium,
            Heavy,
            Full,
            //Custom,
        }

        public double GetHapticsIntensityRatio(HapticsIntensity intensity)
        {
            double result = 0.0;

            switch(intensity)
            {
                case HapticsIntensity.Off:
                    result = 0.0;
                    break;
                case HapticsIntensity.Light:
                    result = 0.3;
                    break;
                case HapticsIntensity.Medium:
                    result = 0.5;
                    break;
                case HapticsIntensity.Heavy:
                    result = 0.8;
                    break;
                case HapticsIntensity.Full:
                    result = 1.0;
                    break;
                default:
                    result = 0.0;
                    break;
            }

            return result;
        }

        public enum HapticsSide : ushort
        {
            Default, // Make Default mean let the mapper decide to choose a side or use all motors
            All, // Use all available motors
            Rumble1,
            Left = Rumble1,
            Rumble2,
            Right = Rumble2,
        }

        public const int DEFAULT_UNBOUND_ID = -1;
        protected int id = DEFAULT_UNBOUND_ID;
        public int Id
        {
            get => id;
            set => id = value;
        }

        protected string name;
        public string Name { get => name; set => name = value; }

        protected string actionTypeName;
        public string ActionTypeName { get => actionTypeName; set => actionTypeName = value; }

        protected MapAction parentAction;
        public MapAction ParentAction { get => parentAction; set => parentAction = value; }

        protected bool hasLayeredAction;
        public bool HasLayeredAction
        {
            get => hasLayeredAction;
        }

        protected string mappingId;
        public string MappingId
        {
            get => mappingId;
            set => mappingId = value;
        }

        // Keep track of properties that have been explicitly edited in child action.
        // Allows keeping proper track when property in parentAction is changed to match child action
        // or to default
        protected HashSet<string> changedProperties = new HashSet<string>();
        public HashSet<string> ChangedProperties { get => changedProperties; }

        // Need a way to destinguish default created unbound binding from
        // explicitly created version. MIGHT REMOVE AND USE -1 FOR Id INSTEAD
        /*protected bool defaultUnbound = true;
        public bool DefaultUnbound
        {
            get => defaultUnbound;
            set => defaultUnbound = value;
        }
        */


        protected ActionFuncStateData stateData = new ActionFuncStateData();
        public ActionFuncStateData StateData
        {
            get => stateData;
            set => stateData = value;
        }

        public static ActionFuncStateData falseStateData =
            new ActionFuncStateData()
            {
                state = false,
                axisNormValue = 0.0,
            };

        public static ActionFuncStateData trueStateData =
            new ActionFuncStateData()
            {
                state = true,
                axisNormValue = 1.0,
            };

        //protected ActionFuncStateData actionStateData = new ActionFuncStateData();

        protected bool onlyOnPrimary = false;
        public bool OnlyOnPrimary
        {
            get => onlyOnPrimary;
        }

        protected bool outputOnSecondary = true;
        public bool OutputOnSecondary
        {
            get => outputOnSecondary;
        }

        protected class NotifyPropertyChangeArgs
        {
            private string propertyName;
            public string PropertyName => propertyName;

            private Mapper mapper;
            public Mapper Mapper => mapper;

            public NotifyPropertyChangeArgs(Mapper mapper, string propertyName)
            {
                this.mapper = mapper;
                this.propertyName = propertyName;
            }
        }

        protected const double OFF_HAPTICS_INTENSITY_RATIO = 0.0;
        protected double hapticsIntensityRatio = OFF_HAPTICS_INTENSITY_RATIO;
        public double HapticsIntensityRatio => hapticsIntensityRatio;

        public abstract void Event(Mapper mapper);

        public abstract void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false);

        public virtual void SoftRelease(Mapper mapper, MapAction checkAction,
            bool resetState = true)
        {
        }

        public static bool IsSameType(MapAction action1, MapAction action2)
        {
            return action1.GetType() == action2.GetType();
        }

        public void CopyBaseProps(MapAction sourceAction)
        {
            name = sourceAction.name;
            mappingId = sourceAction.mappingId;
        }

        public virtual string Describe()
        {
            string result = "";
            return result;
        }
    }
}
