using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest
{
    public class LightbarSettingsSerializer
    {
        private LightbarSettings settings;

        public LightbarMode Mode
        {
            get => settings.Mode;
            set => settings.Mode = value;
        }

        public DS4Library.DS4Color SolidColor
        {
            get => settings.SolidColor;
            set => settings.SolidColor = value;
        }
        public bool ShouldSerializeSolidColor()
        {
            return settings.Mode == LightbarMode.SolidColor;
        }

        public DS4Library.DS4Color FlashColor
        {
            get => settings.FlashColor;
            set => settings.FlashColor = value;
        }
        public bool ShouldSerializeFlashColor()
        {
            return settings.Mode == LightbarMode.Flashing;
        }

        public LightbarSettingsSerializer(LightbarSettings settings)
        {
            this.settings = settings;
        }
    }

    public class ProfileSerializer
    {
        //public class ProfileSettings
        //{
        //    private Profile tempProfile;

        //    public int LeftStickRotation
        //    {
        //        get => tempProfile.LeftStickRotation;
        //        set => tempProfile.LeftStickRotation = Math.Clamp(-180, value, 180);
        //    }

        //    public int RightStickRotation
        //    {
        //        get => tempProfile.RightStickRotation;
        //        set => tempProfile.RightStickRotation = Math.Clamp(-180, value, 180);
        //    }

        //    public ProfileSettings(Profile tempProfile)
        //    {
        //        this.tempProfile = tempProfile;
        //    }
        //}

        private static FakerInputMapping fakerInputMapper = new FakerInputMapping();
        private static bool mapperPopulated;

        public static FakerInputMapping FakerInputMapper
        {
            get
            {
                if (!mapperPopulated)
                {
                    fakerInputMapper.PopulateConstants();
                    fakerInputMapper.PopulateMappings();
                    mapperPopulated = true;
                }

                return fakerInputMapper;
            }
        }

        private Profile tempProfile;

        [JsonProperty(Required = Required.Always)]
        public string Name { get => tempProfile.Name; set => tempProfile.Name = value; }

        public string Description { get => tempProfile.Description; set => tempProfile.Description = value; }
        public bool ShouldSerializeDescription()
        {
            return !string.IsNullOrEmpty(tempProfile.Description);
        }

        public string Creator { get => tempProfile.Creator; set => tempProfile.Creator = value; }

        //[JsonProperty(Required = Required.Always)]
        public DateTime CreationDate { get => tempProfile.CreationDate; set => tempProfile.CreationDate = value; }

        [JsonIgnore]
        public string ControllerType { get => tempProfile.ControllerType; set => tempProfile.ControllerType = value; }
        public bool ShouldSerializeControllerType()
        {
            return !string.IsNullOrEmpty(tempProfile.ControllerType);
        }

        public EmulatedControllerSettings OutputGamepadSettings
        {
            get => tempProfile.OutputGamepadSettings;
            set => tempProfile.OutputGamepadSettings = value;
        }

        private LightbarSettingsSerializer lightbarSerializer;
        public LightbarSettingsSerializer LightbarSettings
        {
            get => lightbarSerializer;
            set => lightbarSerializer = value;
        }


        private List<CycleButtonBindingSerializer> cycleSerializers = new List<CycleButtonBindingSerializer>();
        public List<CycleButtonBindingSerializer> CycleBindings
        {
            get => cycleSerializers;
        }

        private List<ActionSetSerializer> actionSets = new List<ActionSetSerializer>();
        [JsonProperty(Required = Required.Always, PropertyName = "ActionSets")]
        public List<ActionSetSerializer> ActionSets
        {
            get => actionSets;
            set => actionSets = value;
        }

        private List<ProfileActionsMapping> actionMappings = new List<ProfileActionsMapping>();
        [JsonProperty("Mappings")]
        public List<ProfileActionsMapping> ActionMappings { get => actionMappings; set => actionMappings = value; }

        //private ProfileDeviceSettings settings;
        //private ProfileSettings settings;
        //public ProfileSettings Settings
        //{
        //    get => settings;
        //    set => settings = value;
        //}

        public ProfileSerializer(Profile tempProfile)
        {
            this.tempProfile = tempProfile;
            this.lightbarSerializer = new LightbarSettingsSerializer(tempProfile.LightbarSettings);
            //settings = new DS4ProfileDeviceSettings(tempProfile);
            //settings = new ProfileSettings(tempProfile);

            foreach (CycleButton cycleBtn in tempProfile.CycleBindings.Values)
            {
                CycleButtonBindingSerializer cycleSerializer =
                    new CycleButtonBindingSerializer(tempProfile, cycleBtn);
                cycleSerializers.Add(cycleSerializer);
            }

            foreach (ActionSet actionSet in tempProfile.ActionSets)
            {
                ActionSetSerializer setSerializer =
                    new ActionSetSerializer(tempProfile, actionSet);
                actionSets.Add(setSerializer);
            }

            foreach(ActionSet actionSet in tempProfile.ActionSets)
            {
                foreach(ActionLayer layer in actionSet.ActionLayers)
                {
                    ProfileActionsMapping tempMapping = new ProfileActionsMapping()
                    {
                        ActionSet = actionSet.Index,
                        ActionLayer = layer.Index,
                    };

                    List<LayerMapping> tempLayerMapping = new List<LayerMapping>();
                    // Skip over unbound actions
                    foreach(KeyValuePair<string, MapAction> actionPair in
                        layer.normalActionDict.Where((item) => item.Value.Id != MapAction.DEFAULT_UNBOUND_ID))
                    {
                        LayerMapping layerMapping = new LayerMapping();
                        layerMapping.inputBinding = actionPair.Key;
                        layerMapping.ActionIndex = actionPair.Value.Id;

                        tempLayerMapping.Add(layerMapping);
                    }

                    // Re-order temp LayerMapping list based on asc ActionIndex value
                    List<LayerMapping> orderedList = tempLayerMapping.OrderBy((mapping) => mapping.ActionIndex).ToList();
                    tempMapping.LayerMappings.AddRange(orderedList);

                    actionMappings.Add(tempMapping);
                }
            }
        }

        public void PopulateProfile()
        {
            tempProfile.CycleBindings.Clear();
            foreach(CycleButtonBindingSerializer serializer in cycleSerializers)
            {
                serializer.PopulateProfileSet(tempProfile);
                if (tempProfile.CycleBindings.ContainsKey(serializer.CycleId))
                {
                    throw new JsonException($"Duplicate cycle id [{serializer.CycleId}] found in profile");
                }

                tempProfile.CycleBindings.Add(serializer.CycleId, serializer.TempCycleButton);
                //tempProfile.CycleBindings.Add(serializer.CycleId, serializer.)
                //CycleButton tempBtn = new CycleButton(serializer.CycleId);
            }

            tempProfile.ActionSets.Clear();
            foreach (ActionSetSerializer serializer in actionSets)
            {
                serializer.PopulateProfileSet(tempProfile);
                tempProfile.ActionSets.Add(serializer.TempActionSet);
            }
        }

        public bool ShouldSerializeActionMappings()
        {
            return actionMappings.Count > 0;
        }

        public bool ShouldSerializeCycleBindings()
        {
            return cycleSerializers.Count > 0;
        }


        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            Trace.WriteLine("IN ProfileSerializer.OnDeserializingMethod");
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Trace.WriteLine("IN ProfileSerializer.OnDeserializedMethod");
        }
    }

    public class CycleButtonBindingSerializer
    {
        private string cycleId;
        public string CycleId
        {
            get => cycleId;
            set => cycleId = value;
        }

        private CycleButton tempCycleButton = new CycleButton("temp");
        [JsonIgnore]
        public CycleButton TempCycleButton
        {
            get => tempCycleButton;
        }

        private List<OutputActionDataSerializer> dataBinds =
            new List<OutputActionDataSerializer>();

        public List<OutputActionDataSerializer> CycleActions
        {
            get => dataBinds;
        }

        [JsonConstructor]
        public CycleButtonBindingSerializer()
        {
        }

        public CycleButtonBindingSerializer(Profile tempProfile, CycleButton cycleButton)
        {
            dataBinds.Clear();

            foreach(OutputActionData data in cycleButton.Actions)
            {
                dataBinds.Add(new OutputActionDataSerializer(data));
            }
        }

        public void PopulateProfileSet(Profile tempProfile)
        {
            tempCycleButton = new CycleButton(cycleId);
            foreach (OutputActionDataSerializer serializer in dataBinds)
            {
                if (!CycleButton.ValidDataBinding(serializer.ActionType))
                {
                    throw new JsonException($"Invalid output type [{serializer.ActionType}] for cycle binding");
                }

                tempCycleButton.Actions.Add(serializer.OutputData);
            }
        }
    }

    public class ActionSetSerializer
    {
        private static ActionSet currentSet;
        [JsonIgnore]
        internal static ActionSet CurrentSet => currentSet;

        private static ActionLayer topActionLayer;
        [JsonIgnore]
        internal static ActionLayer TopActionLayer
        {
            get => topActionLayer;
            set => topActionLayer = value;
        }

        //private Profile tempProfile;
        private ActionSet tempActionSet = new ActionSet(0, "");
        [JsonIgnore]
        public ActionSet TempActionSet { get => tempActionSet; }

        [JsonProperty(Required = Required.Always)]
        public int Index { get => tempActionSet.Index; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get => tempActionSet.Name; set => tempActionSet.Name = value; }

        public string Description { get => tempActionSet.Description; set => tempActionSet.Description = value; }
        public bool ShouldSerializeDescription()
        {
            return !string.IsNullOrEmpty(tempActionSet.Description);
        }

        private List<ActionLayerSerializer> actionLayers = new List<ActionLayerSerializer>();
        [JsonProperty(PropertyName = "ActionLayers", Required = Required.Always)]
        public List<ActionLayerSerializer> ActionLayers
        {
            get => actionLayers;
            set => actionLayers = value;
        }

        [JsonConstructor]
        public ActionSetSerializer()
        {
            Console.WriteLine("FUCKERY");
        }

        public ActionSetSerializer(Profile tempProfile, ActionSet tempActionSet)
        {
            //this.tempProfile = tempProfile;
            this.tempActionSet = tempActionSet;

            foreach (ActionLayer tempActionLayer in tempActionSet.ActionLayers)
            {
                ActionLayerSerializer serializer = new ActionLayerSerializer(tempActionSet,
                    tempActionLayer);
                actionLayers.Add(serializer);
            }
        }

        public void PopulateProfileSet(Profile tempProfile)
        {
            tempActionSet.ActionLayers.Clear();
            foreach (ActionLayerSerializer serializer in actionLayers)
            {
                serializer.PopulateLayer();
                tempActionSet.ActionLayers.Add(serializer.ActionLayer);
            }
        }

        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            Trace.WriteLine("IN ActionSetSerializer.OnDeserializingMethod");
            currentSet = tempActionSet;
            topActionLayer = null;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Trace.WriteLine("IN ActionSetSerializer.OnDeserializedMethod");
            currentSet = null;
            topActionLayer = null;
        }
    }

    public class ActionLayerSerializer
    {
        private static int currentActionIndex = 0;
        [JsonIgnore]
        internal static int CurrentActionIndex
        {
            get => currentActionIndex;
            set => currentActionIndex = value;
        }

        private static ActionSet parentActionSet;
        [JsonIgnore]
        internal static ActionSet ParentActionSet => parentActionSet;

        //private static ActionLayerSerializer currentSerializer;
        //[JsonIgnore]
        //internal static ActionLayerSerializer CurrentSerializer => currentSerializer;


        private ActionLayer actionLayer = new ActionLayer(0);
        [JsonIgnore]
        public ActionLayer ActionLayer { get => actionLayer; }

        [JsonProperty(Required = Required.Always)]
        public int Index
        {
            get => actionLayer.Index; set => actionLayer.Index = value;
        }

        [JsonProperty(Required = Required.Always)]
        public string Name
        {
            get => actionLayer.Name;
            set => actionLayer.Name = value;
        }

        public string Description
        {
            get => actionLayer.Description; set => actionLayer.Description = value;
        }
        public bool ShouldSerializeDescription()
        {
            return !string.IsNullOrEmpty(actionLayer.Description);
        }

        private List<MapActionSerializer> mapActionSerializers =
            new List<MapActionSerializer>();
        [JsonProperty(PropertyName = "MappedActions")]
        public List<MapActionSerializer> MapActionSerializers
        {
            get => mapActionSerializers;
            set => mapActionSerializers = value;
        }

        [JsonConstructor]
        public ActionLayerSerializer()
        {
            Trace.WriteLine("LKJDFLKJDLKJ");
        }

        public ActionLayerSerializer(ActionSet tempActionSet, ActionLayer layer)
        {
            actionLayer = layer;
            parentActionSet = tempActionSet;

            foreach (MapAction action in layer.LayerActions)
            {
                //MapActionSerializer serializer = new MapActionSerializer(layer, action);
                MapActionSerializer serializer = MapActionSerializerFactory.CreateSerializer(layer, action);
                if (serializer != null)
                {
                    mapActionSerializers.Add(serializer);
                }
            }
        }

        public void PopulateLayer()
        {
            actionLayer.LayerActions.Clear();
            foreach (MapActionSerializer serializer in mapActionSerializers)
            {
                serializer.PopulateMap();

                if (actionLayer.LayerActions.FindIndex((act) => act.Id == serializer.MapAction.Id) != -1)
                {
                    throw new JsonException($"Duplicate action index [{serializer.MapAction.Id}] on layer [{actionLayer.Index}]");
                }

                serializer.PostPopulateMap(parentActionSet, actionLayer);
                actionLayer.LayerActions.Add(serializer.MapAction);
            }
        }

        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            Trace.WriteLine("IN ActionLayerSerializer.OnDeserializingMethod");
            parentActionSet = ActionSetSerializer.CurrentSet;
            currentActionIndex = 0;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Trace.WriteLine("IN ActionLayerSerializer.OnDeserializedMethod");
            parentActionSet = null;
            if (ActionSetSerializer.TopActionLayer == null)
            {
                ActionSetSerializer.TopActionLayer = actionLayer;
            }
        }
    }

    public class ProfileActionsMapping
    {
        private int actionSet;
        [JsonProperty(Required = Required.Always)]
        public int ActionSet { get => actionSet; set => actionSet = value; }

        private int actionLayer;
        [JsonProperty(Required = Required.Always)]
        public int ActionLayer { get => actionLayer; set => actionLayer = value; }

        private List<LayerMapping> layerMappings = new List<LayerMapping>();
        [JsonProperty("InputMappings", Required = Required.Always)]
        public List<LayerMapping> LayerMappings { get => layerMappings; set => layerMappings = value; }
    }

    public class LayerMapping
    {
        [JsonIgnore]
        public string inputBinding;

        [JsonProperty("Input", Required = Required.Always, Order = 1)]
        public string InputBinding { get => inputBinding; set => inputBinding = value; }

        private int actionIndex;
        [JsonProperty("Action", Required = Required.Always, Order = 2)]
        public int ActionIndex { get => actionIndex; set => actionIndex = value; }
    }
}
