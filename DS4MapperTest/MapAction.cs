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
