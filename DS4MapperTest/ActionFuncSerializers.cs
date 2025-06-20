using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest
{
    [JsonConverter(typeof(ActionFuncTypeConverter))]
    public class ActionFuncSerializer
    {
        protected string type = "Invalid";

        protected ActionFunc actionFunc;
        [JsonProperty(Required = Required.Always, Order = -3)]
        public string Type { get => type; }

        [JsonIgnore]
        public ActionFunc ActionFunc
        {
            get => actionFunc; set => actionFunc = value;
        }

        protected List<OutputActionDataSerializer> actionDataSerializers =
            new List<OutputActionDataSerializer>();
        [JsonProperty("OutputActions", Required = Required.Always, Order = -2)]
        public List<OutputActionDataSerializer> ActionDataSerializers
        {
            get => actionDataSerializers;
            set => actionDataSerializers = value;
        }

        public ActionFuncSerializer()
        {
            this.actionFunc = new NormalPressFunc(new MapperUtil.OutputActionData(MapperUtil.OutputActionData.ActionType.Empty, 0));
        }

        public ActionFuncSerializer(ActionFunc tempFunc)
        {
            this.actionFunc = tempFunc;
        }

        // Deserialize
        public void PopulateFunc()
        {
            actionFunc.OutputActions.Clear();
            foreach(OutputActionDataSerializer serializer in actionDataSerializers)
            {
                actionFunc.OutputActions.Add(serializer.OutputData);
            }
        }

        // Serialize
        public void PopulateOutputActionData()
        {
            foreach(OutputActionData data in actionFunc.OutputActions)
            {
                actionDataSerializers.Add(new OutputActionDataSerializer(data));
            }
        }
    }

    public class NormalPressFuncSerializer : ActionFuncSerializer
    {
        public class NormalPressSettings
        {
            private NormalPressFunc pressFunc;

            public bool Toggle
            {
                get => pressFunc.toggleEnabled;
                set => pressFunc.toggleEnabled = value;
            }
            public bool ShouldSerializeToggle()
            {
                return pressFunc.toggleEnabled == true;
            }

            public bool TurboEnabled
            {
                get => pressFunc.TurboEnabled;
                set => pressFunc.TurboEnabled = value;
            }
            public bool ShouldSerializeTurboEnabled()
            {
                return pressFunc.TurboEnabled == true;
            }

            public int TurboDurationMs
            {
                get => pressFunc.TurboDurationMs;
                set => pressFunc.TurboDurationMs = value;
            }
            public bool ShouldSerializeTurboDurationMs()
            {
                return pressFunc.TurboDurationMs != NormalPressFunc.DEFAULT_TURBO_DURATION_MS;
            }

            public int FireDelayMs
            {
                get => pressFunc.FireDelayMs;
                set => pressFunc.FireDelayMs = value;
            }

            public bool IsDefault()
            {
                return pressFunc.toggleEnabled == false &&
                    pressFunc.TurboEnabled == false &&
                    pressFunc.TurboDurationMs == NormalPressFunc.DEFAULT_TURBO_DURATION_MS &&
                    pressFunc.FireDelayMs == NormalPressFunc.FIRE_DELAY_MS_DEFAULT;
            }

            public NormalPressSettings(NormalPressFunc actionFunc)
            {
                this.pressFunc = actionFunc;
            }
        }

        private const string typeString = "NormalPress";

        private NormalPressFunc pressFunc = new NormalPressFunc();
        [JsonIgnore]
        public NormalPressFunc PressFunc { get => pressFunc; set => pressFunc = value; }

        private NormalPressSettings settings;
        public NormalPressSettings Settings
        {
            get => settings;
            set => settings = value;
        }
        public bool ShouldSerializeSettings()
        {
            return !settings.IsDefault();
        }

        public NormalPressFuncSerializer(): base()
        {
            this.type = typeString;
            actionFunc = pressFunc;
            settings = new NormalPressSettings(pressFunc);
        }

        public NormalPressFuncSerializer(ActionFunc tempFunc) : base(tempFunc)
        {
            if (tempFunc is NormalPressFunc temp)
            {
                pressFunc = temp;
                this.type = typeString;
                actionFunc = pressFunc;
                settings = new NormalPressSettings(pressFunc);

                PopulateOutputActionData();
            }
        }
    }

    public class HoldPressFuncSerializer : ActionFuncSerializer
    {
        public class HoldPressSettings
        {
            private HoldPressFunc holdPressFunc;

            [JsonProperty("HoldTime")]
            public int Duration
            {
                get => holdPressFunc.DurationMs;
                set => holdPressFunc.DurationMs = value;
            }
            public bool ShouldSerializeDuration()
            {
                return holdPressFunc.DurationMs != 30;
            }

            public bool Toggle
            {
                get => holdPressFunc.toggleEnabled;
                set => holdPressFunc.toggleEnabled = value;
            }
            public bool ShouldSerializeToggle()
            {
                return holdPressFunc.toggleEnabled == true;
            }

            public bool TurboEnabled
            {
                get => holdPressFunc.TurboEnabled;
                set => holdPressFunc.TurboEnabled = value;
            }
            public bool ShouldSerializeTurboEnabled()
            {
                return holdPressFunc.TurboEnabled == true;
            }

            public int TurboDurationMs
            {
                get => holdPressFunc.TurboDurationMs;
                set => holdPressFunc.TurboDurationMs = value;
            }
            public bool ShouldSerializeTurboDurationMs()
            {
                return holdPressFunc.TurboDurationMs != NormalPressFunc.DEFAULT_TURBO_DURATION_MS;
            }

            public bool IsDefault()
            {
                return holdPressFunc.DurationMs == 30 &&
                    holdPressFunc.toggleEnabled == false &&
                    holdPressFunc.TurboEnabled == false &&
                    holdPressFunc.TurboDurationMs == NormalPressFunc.DEFAULT_TURBO_DURATION_MS;
            }

            public HoldPressSettings(HoldPressFunc actionFunc)
            {
                this.holdPressFunc = actionFunc;
            }
        }

        private const string typeString = "HoldPress";
        private HoldPressFunc holdPressFunc = new HoldPressFunc();
        private HoldPressSettings settings;

        [JsonIgnore]
        public HoldPressFunc HoldPressFunc
        {
            get => holdPressFunc; set => holdPressFunc = value;
        }

        public HoldPressSettings Settings
        {
            get => settings;
            set => settings = value;
        }
        public bool ShouldSerializeSettings()
        {
            return !settings.IsDefault();
        }

        public HoldPressFuncSerializer() : base()
        {
            this.type = typeString;
            actionFunc = holdPressFunc;
            settings = new HoldPressSettings(holdPressFunc);
        }

        public HoldPressFuncSerializer(ActionFunc tempFunc) : base(tempFunc)
        {
            if (tempFunc is HoldPressFunc temp)
            {
                holdPressFunc = temp;
                this.type = typeString;
                actionFunc = holdPressFunc;
                settings = new HoldPressSettings(holdPressFunc);

                PopulateOutputActionData();
            }
        }
    }

    public class ReleaseFuncSerializer: ActionFuncSerializer
    {
        public class ReleaseFuncSettings
        {
            private ReleaseFunc releaseFuncInstance;
            public int Duration
            {
                get => releaseFuncInstance.DurationMs;
                set => releaseFuncInstance.DurationMs = value;
            }
            public bool ShouldSerializeDuration()
            {
                return releaseFuncInstance.DurationMs != 0;
            }

            public bool Interruptable
            {
                get => releaseFuncInstance.interruptable;
                set => releaseFuncInstance.interruptable = value;
            }
            public bool ShouldSerializeInterruptable()
            {
                return releaseFuncInstance.interruptable == true;
            }

            public int DelayDuration
            {
                get => releaseFuncInstance.DelayDurationMs;
                set => releaseFuncInstance.DelayDurationMs = value;
            }
            public bool ShouldSerializeDelayDuration()
            {
                return releaseFuncInstance.DelayDurationMs != ReleaseFunc.DELAY_DURATION_DEFAULT;
            }

            public bool IsDefault()
            {
                return releaseFuncInstance.DurationMs == 0 &&
                    releaseFuncInstance.DelayDurationMs == ReleaseFunc.DELAY_DURATION_DEFAULT &&
                    releaseFuncInstance.interruptable == false;
            }

            public ReleaseFuncSettings(ReleaseFunc funcInstance)
            {
                releaseFuncInstance = funcInstance;
            }
        }

        private const string typeString = "Release";
        private ReleaseFunc releaseFuncInstance = new ReleaseFunc();
        private ReleaseFuncSettings settings;

        [JsonIgnore]
        public ReleaseFunc RelaseFuncInstance
        {
            get => releaseFuncInstance; set => releaseFuncInstance = value;
        }

        [JsonProperty(PropertyName = "Settings")]
        public ReleaseFuncSettings Settings
        {
            get => settings;
            set => settings = value;
        }
        public bool ShouldSerializeSettings()
        {
            return !settings.IsDefault();
        }

        public ReleaseFuncSerializer() : base()
        {
            this.type = typeString;
            actionFunc = releaseFuncInstance;
            settings = new ReleaseFuncSettings(releaseFuncInstance);
        }

        public ReleaseFuncSerializer(ActionFunc tempFunc) : base(tempFunc)
        {
            if (tempFunc is ReleaseFunc temp)
            {
                this.releaseFuncInstance = temp;
                this.type = typeString;
                actionFunc = releaseFuncInstance;
                settings = new ReleaseFuncSettings(releaseFuncInstance);

                PopulateOutputActionData();
            }
        }
    }

    public class StartPressFuncSerializer : ActionFuncSerializer
    {
        public class StartPressFuncSettings
        {
            private StartPressFunc startPressFuncInstance;

            public int DurationMs
            {
                get => startPressFuncInstance.DurationMs;
                set => startPressFuncInstance.DurationMs = value;
            }
            public bool ShouldSerializeDurationMs()
            {
                return startPressFuncInstance.DurationMs != 0;
            }

            public bool Toggle
            {
                get => startPressFuncInstance.toggleEnabled;
                set => startPressFuncInstance.toggleEnabled = value;
            }
            public bool ShouldSerializeToggle()
            {
                return startPressFuncInstance.toggleEnabled == true;
            }

            public bool IsDefault()
            {
                return startPressFuncInstance.DurationMs == 0 &&
                    startPressFuncInstance.toggleEnabled == false;
            }

            public StartPressFuncSettings(StartPressFunc funcInstance)
            {
                startPressFuncInstance = funcInstance;
            }
        }

        private const string typeString = "StartPress";
        private StartPressFunc startPressFuncInstance = new StartPressFunc();
        private StartPressFuncSettings settings;

        [JsonIgnore]
        public StartPressFunc StartPressFuncInstance
        {
            get => startPressFuncInstance; set => startPressFuncInstance = value;
        }

        [JsonProperty(PropertyName = "Settings")]
        public StartPressFuncSettings Settings
        {
            get => settings;
            set => settings = value;
        }
        public bool ShouldSerializeSettings()
        {
            return !settings.IsDefault();
        }

        // Deserialize
        public StartPressFuncSerializer() : base()
        {
            this.type = typeString;
            actionFunc = startPressFuncInstance;
            settings = new StartPressFuncSettings(startPressFuncInstance);
        }

        // Pre-serialize
        public StartPressFuncSerializer(ActionFunc tempFunc) : base(tempFunc)
        {
            if (tempFunc is StartPressFunc temp)
            {
                this.startPressFuncInstance = temp;
                this.type = typeString;
                actionFunc = startPressFuncInstance;
                settings = new StartPressFuncSettings(startPressFuncInstance);

                PopulateOutputActionData();
            }
        }
    }

    public class ChordedPressFuncSerializer : ActionFuncSerializer
    {
        public class ChordedPressSettings
        {
            private ChordedPressFunc chordedPressFunc;

            [JsonIgnore]
            public JoypadActionCodes Trigger
            {
                get => chordedPressFunc.TriggerButton;
                set
                {
                    chordedPressFunc.TriggerButton = value;
                }
            }
            public bool ShouldSerializeTrigger()
            {
                return chordedPressFunc.TriggerButton != JoypadActionCodes.Empty;
            }

            public bool IsDefault()
            {
                return chordedPressFunc.TriggerButton == JoypadActionCodes.Empty;
            }

            public ChordedPressSettings(ChordedPressFunc actionFunc)
            {
                this.chordedPressFunc = actionFunc;
            }
        }

        private const string typeString = "ChordedPress";

        private ChordedPressFunc chordedPressFunc = new ChordedPressFunc();
        [JsonIgnore]
        public ChordedPressFunc ChorededPressFunc { get => chordedPressFunc; set => chordedPressFunc = value; }

        // Can't decide if I want in Settings or inside main Func object
        //public JoypadActionCodes Trigger
        //{
        //    get => chordedPressFunc.TriggerButton;
        //    set
        //    {
        //        chordedPressFunc.TriggerButton = value;
        //    }
        //}
        //public bool ShouldSerializeTrigger()
        //{
        //    return chordedPressFunc.TriggerButton != JoypadActionCodes.Empty;
        //}

        private ChordedPressSettings settings;
        public ChordedPressSettings Settings
        {
            get => settings;
            set => settings = value;
        }
        public bool ShouldSerializeSettings()
        {
            return !settings.IsDefault();
        }

        public ChordedPressFuncSerializer() : base()
        {
            this.type = typeString;
            actionFunc = chordedPressFunc;
            settings = new ChordedPressSettings(chordedPressFunc);
        }

        public ChordedPressFuncSerializer(ActionFunc tempFunc) : base(tempFunc)
        {
            if (tempFunc is ChordedPressFunc temp)
            {
                chordedPressFunc = temp;
                this.type = typeString;
                actionFunc = chordedPressFunc;
                settings = new ChordedPressSettings(chordedPressFunc);

                PopulateOutputActionData();
            }
        }
    }

    public class AnalogFuncSerializer : ActionFuncSerializer
    {
        public class AnalogFuncSettings
        {
            private AnalogFunc analogFunc;

            public double MinOutput
            {
                get => analogFunc.MinOutput;
                set => analogFunc.MinOutput = Math.Clamp(value, 0.0, 1.0);
            }
            public bool ShouldSerializeMinOutput()
            {
                return analogFunc.MinOutput == AnalogFunc.DEFAULT_MIN_OUTPUT;
            }

            public double MaxOutput
            {
                get => analogFunc.MinOutput;
                set => analogFunc.MaxOutput = Math.Clamp(value, 0.0, 1.0);
            }
            public bool ShouldSerializeMaxOutput()
            {
                return analogFunc.MaxOutput == AnalogFunc.DEFAULT_MAX_OUTPUT;
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public JoypadActionCodes OutputAxis
            {
                get => analogFunc.OutputAxis;
                set
                {
                    if (value >= JoypadActionCodes.Axis1 &&
                        value <= JoypadActionCodes.AxisMax)
                    {
                        analogFunc.OutputAxis = value;
                    }
                    else
                    {
                        analogFunc.OutputAxis = JoypadActionCodes.Empty;
                    }
                }
            }
            public bool ShouldSerializeOutputAxis()
            {
                return analogFunc.OutputAxis == JoypadActionCodes.Empty;
            }

            public bool IsDefault()
            {
                return analogFunc.MinOutput == AnalogFunc.DEFAULT_MIN_OUTPUT &&
                    analogFunc.MaxOutput == AnalogFunc.DEFAULT_MAX_OUTPUT &&
                    analogFunc.OutputAxis == JoypadActionCodes.Empty;
            }

            public AnalogFuncSettings(AnalogFunc actionFunc)
            {
                this.analogFunc = actionFunc;
            }
        }

        private const string typeString = "Analog";

        private AnalogFunc analogFunc = new AnalogFunc();
        [JsonIgnore]
        public AnalogFunc AnalogActionFunc { get => analogFunc; set => analogFunc = value; }

        private AnalogFuncSettings settings;
        public AnalogFuncSettings Settings
        {
            get => settings;
            set => settings = value;
        }
        public bool ShouldSerializeSettings()
        {
            return !settings.IsDefault();
        }

        public AnalogFuncSerializer() : base()
        {
            this.type = typeString;
            actionFunc = analogFunc;
            settings = new AnalogFuncSettings(analogFunc);
        }

        public AnalogFuncSerializer(ActionFunc tempFunc) : base(tempFunc)
        {
            if (tempFunc is AnalogFunc temp)
            {
                analogFunc = temp;
                this.type = typeString;
                actionFunc = analogFunc;
                settings = new AnalogFuncSettings(analogFunc);

                PopulateOutputActionData();
            }
        }
    }

    public class DistanceFuncSerializer : ActionFuncSerializer
    {
        public class DistanceSettings
        {
            private DistanceFunc distanceFunc;

            public bool IsDefault()
            {
                return true;
            }

            public DistanceSettings(DistanceFunc actionFunc)
            {
                this.distanceFunc = actionFunc;
            }
        }

        private const string typeString = "Distance";

        private DistanceFunc distanceFunc = new DistanceFunc();
        [JsonIgnore]
        public DistanceFunc DistanceFuncAct
        {
            get => distanceFunc; set => distanceFunc = value;
        }

        private DistanceSettings settings;
        public DistanceSettings Settings
        {
            get => settings;
            set => settings = value;
        }
        public bool ShouldSerializeSettings()
        {
            return !settings.IsDefault();
        }

        public DistanceFuncSerializer() : base()
        {
            this.type = typeString;
            actionFunc = distanceFunc;
            settings = new DistanceSettings(distanceFunc);
        }

        public DistanceFuncSerializer(ActionFunc tempFunc) : base(tempFunc)
        {
            if (tempFunc is DistanceFunc temp)
            {
                distanceFunc = temp;
                this.type = typeString;
                actionFunc = distanceFunc;
                settings = new DistanceSettings(distanceFunc);

                PopulateOutputActionData();
            }
        }
    }

    public class ActionFuncTypeConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ActionFuncSerializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject j = JObject.Load(reader);

            string actionOutput = j["Type"]?.ToString();
            ActionFuncSerializer resultInstance = null;
            switch (actionOutput)
            {
                case "NormalPress":
                    NormalPressFuncSerializer instance = new NormalPressFuncSerializer();
                    JsonConvert.PopulateObject(j.ToString(), instance);
                    instance.ActionDataSerializers.RemoveAll((item) => item == null);
                    resultInstance = instance;
                    //throw new JsonSerializationException();
                    //return null;
                    break;
                case "HoldPress":
                    HoldPressFuncSerializer holdInstance = new HoldPressFuncSerializer();
                    JsonConvert.PopulateObject(j.ToString(), holdInstance);
                    holdInstance.ActionDataSerializers.RemoveAll((item) => item == null);
                    resultInstance = holdInstance;
                    break;
                case "Release":
                    ReleaseFuncSerializer releaseInstance = new ReleaseFuncSerializer();
                    JsonConvert.PopulateObject(j.ToString(), releaseInstance);
                    releaseInstance.ActionDataSerializers.RemoveAll((item) => item == null);
                    resultInstance = releaseInstance;
                    break;
                case "StartPress":
                    StartPressFuncSerializer startPressInstance = new StartPressFuncSerializer();
                    JsonConvert.PopulateObject(j.ToString(), startPressInstance);
                    startPressInstance.ActionDataSerializers.RemoveAll((item) => item == null);
                    resultInstance = startPressInstance;
                    break;
                case "ChordedPress":
                    ChordedPressFuncSerializer chordedInstance = new ChordedPressFuncSerializer();
                    JsonConvert.PopulateObject(j.ToString(), chordedInstance);
                    chordedInstance.ActionDataSerializers.RemoveAll((item) => item == null);
                    resultInstance = chordedInstance;
                    break;
                case "Analog":
                    AnalogFuncSerializer analogInstance = new AnalogFuncSerializer();
                    JsonConvert.PopulateObject(j.ToString(), analogInstance);
                    analogInstance.ActionDataSerializers.RemoveAll((item) => item == null);
                    resultInstance = analogInstance;
                    //throw new JsonSerializationException();
                    //return null;
                    break;
                case "Distance":
                    DistanceFuncSerializer distanceInstance = new DistanceFuncSerializer();
                    JsonConvert.PopulateObject(j.ToString(), distanceInstance);
                    distanceInstance.ActionDataSerializers.RemoveAll((item) => item == null);
                    resultInstance = distanceInstance;
                    //throw new JsonSerializationException();
                    //return null;
                    break;
                default:
                    break;
            }

            resultInstance?.ActionDataSerializers.RemoveAll((item) => item == null);
            return resultInstance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ActionFuncSerializer current = value as ActionFuncSerializer;
            switch (current.Type)
            {
                case "NormalPress":
                    if (current is NormalPressFuncSerializer funcSerializer)
                    {
                        serializer.Serialize(writer, funcSerializer);
                    }
                    
                    break;
                case "HoldPress":
                    if (current is HoldPressFuncSerializer holdFuncSerializer)
                    {
                        serializer.Serialize(writer, holdFuncSerializer);
                    }

                    break;
                case "Release":
                    if (current is ReleaseFuncSerializer releaseFuncSerializer)
                    {
                        serializer.Serialize(writer, releaseFuncSerializer);
                    }

                    break;
                case "StartPress":
                    if (current is StartPressFuncSerializer startPressFuncSerializer)
                    {
                        serializer.Serialize(writer, startPressFuncSerializer);
                    }

                    break;
                case "ChordedPress":
                    if (current is ChordedPressFuncSerializer chordedFuncSerializer)
                    {
                        serializer.Serialize(writer, chordedFuncSerializer);
                    }

                    break;
                case "Analog":
                    if (current is AnalogFuncSerializer analogFuncSerializer)
                    {
                        serializer.Serialize(writer, analogFuncSerializer);
                    }

                    break;
                case "Distance":
                    if (current is DistanceFuncSerializer distanceFuncSerializer)
                    {
                        serializer.Serialize(writer, distanceFuncSerializer);
                    }

                    break;
                default:
                    break;
            }
        }
    }
}
