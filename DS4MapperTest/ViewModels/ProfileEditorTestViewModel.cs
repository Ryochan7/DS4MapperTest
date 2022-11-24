using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.TriggerActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.StickActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.DPadActions;

namespace DS4MapperTest.ViewModels
{
    public class ProfileEditorTestViewModel
    {
        private ManualResetEventSlim actionResetEvent = new ManualResetEventSlim(false);
        public ManualResetEventSlim ActionResetEvent => actionResetEvent;

        private Mapper mapper;
        public Mapper DeviceMapper
        {
            get => mapper;
        }

        private ProfileEntity profileEnt;
        public ProfileEntity ProfileEnt
        {
            get => profileEnt;
        }

        private Profile tempProfile;
        public Profile CurrentProfile
        {
            get => tempProfile;
        }

        public string ProfileName
        {
            get => tempProfile.Name;
            set
            {
                tempProfile.Name = value;
            }
        }

        private List<BindingItemsTest> buttonBindings = new List<BindingItemsTest>();
        public List<BindingItemsTest> ButtonBindings
        {
            get => buttonBindings;
        }

        private Dictionary<string, int> buttonBindingsIndexDict =
            new Dictionary<string, int>();
        public Dictionary<string, int> ButtonBindingsIndexDict
        {
            get => buttonBindingsIndexDict;
        }

        private List<TouchBindingItemsTest> touchpadBindings = new List<TouchBindingItemsTest>();
        public List<TouchBindingItemsTest> TouchpadBindings
        {
            get => touchpadBindings;
        }

        private List<TriggerBindingItemsTest> triggerBindings = new List<TriggerBindingItemsTest>();
        public List<TriggerBindingItemsTest> TriggerBindings => triggerBindings;

        private int selectedTouchBindIndex = -1;
        public int SelectTouchBindIndex
        {
            get => selectedTouchBindIndex;
            set
            {
                if (selectedTouchBindIndex == value) return;
                selectedTouchBindIndex = value;
                SelectTouchBindIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectTouchBindIndexChanged;

        private int selectTriggerBindIndex = -1;
        public int SelectTriggerBindIndex
        {
            get => selectTriggerBindIndex;
            set
            {
                if (selectTriggerBindIndex == value) return;
                selectTriggerBindIndex = value;
                SelectTriggerBindIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectTriggerBindIndexChanged;

        private List<StickBindingItemsTest> stickBindings = new List<StickBindingItemsTest>();
        public List<StickBindingItemsTest> StickBindings => stickBindings;

        private int selectStickBindIndex = -1;
        public int SelectStickBindIndex
        {
            get => selectStickBindIndex;
            set
            {
                if (selectStickBindIndex == value) return;
                selectStickBindIndex = value;
                SelectStickBindIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectStickBindIndexChanged;


        private List<GyroBindingItemsTest> gyroBindings = new List<GyroBindingItemsTest>();
        public List<GyroBindingItemsTest> GyroBindings => gyroBindings;

        private int selectGyroBindIndex = -1;
        public int SelectGyroBindIndex
        {
            get => selectGyroBindIndex;
            set
            {
                if (selectGyroBindIndex == value) return;
                selectGyroBindIndex = value;
                SelectGyroBindIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectGyroBindIndexChanged;

        private List<DPadBindingItemsTest> dpadBindings = new List<DPadBindingItemsTest>();
        public List<DPadBindingItemsTest> DPadBindings => dpadBindings;

        private int selectDPadBindIndex = -1;
        public int SelectDPadBindIndex
        {
            get => selectDPadBindIndex;
            set
            {
                if (selectDPadBindIndex == value) return;
                selectDPadBindIndex = value;
                SelectDPadBindIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectDPadBindIndexChanged;


        private ObservableCollection<ActionSetItemsTest> actionSetItems = new ObservableCollection<ActionSetItemsTest>();
        public ObservableCollection<ActionSetItemsTest> ActionSetItems => actionSetItems;

        private int selectedActionSetIndex = 0;
        public int SelectedActionSetIndex
        {
            get => selectedActionSetIndex;
            set => selectedActionSetIndex = value;
        }

        private ObservableCollection<ActionLayerItemsTest> layerItems = new ObservableCollection<ActionLayerItemsTest>();
        public ObservableCollection<ActionLayerItemsTest> LayerItems => layerItems;

        private int selectedActionLayerIndex = 0;
        public int SelectedActionLayerIndex
        {
            get => selectedActionLayerIndex;
            set => selectedActionLayerIndex = value;
        }

        public string CurrentLayerName
        {
            get => layerItems[selectedActionLayerIndex].Layer.Name;
            set
            {
                string currentName = layerItems[selectedActionLayerIndex].Layer.Name;
                if (currentName == value) return;
                layerItems[selectedActionLayerIndex].Layer.Name = value;
                layerItems[selectedActionLayerIndex].RaiseDisplayNameChanged();
            }
        }

        public string CurrentSetName
        {
            get => actionSetItems[selectedActionSetIndex].Set.Name;
            set
            {
                string currentName = actionSetItems[selectedActionSetIndex].Set.Name;
                if (currentName == value) return;
                actionSetItems[selectedActionSetIndex].Set.Name = value;
                actionSetItems[selectedActionSetIndex].RaiseDisplayNameChanged();
            }
        }

        private bool overwriteFile;
        public bool OverwriteFile
        {
            get => overwriteFile;
            set => overwriteFile = value;
        }

        public bool OutControllerEnabled
        {
            get => tempProfile.OutputGamepadSettings.enabled;
            set
            {
                tempProfile.OutputGamepadSettings.enabled = value;
            }
        }

        public int OutputControllerTypeIdx
        {
            get
            {
                int result = -1;
                switch (tempProfile.OutputGamepadSettings.OutputGamepad)
                {
                    case Mapper.OutputContType.Xbox360:
                        result = 0;
                        break;
                    case Mapper.OutputContType.DualShock4:
                        result = 1;
                        break;
                    default:
                        break;
                }
                return result;
            }
            set
            {
                switch(value)
                {
                    case 0:
                        tempProfile.OutputGamepadSettings.OutputGamepad = Mapper.OutputContType.Xbox360;
                        break;
                    case 1:
                        tempProfile.OutputGamepadSettings.OutputGamepad = Mapper.OutputContType.DualShock4;
                        break;
                    default:
                        break;
                }
            }
        }

        public bool ForceFeedbackEnabled
        {
            get => tempProfile.OutputGamepadSettings.ForceFeedbackEnabled;
            set
            {
                tempProfile.OutputGamepadSettings.ForceFeedbackEnabled = value;
            }
        }

        public ProfileEditorTestViewModel(Mapper mapper, ProfileEntity profileEnt, Profile currentProfile)
        {
            this.mapper = mapper;
            this.profileEnt = profileEnt;
            this.tempProfile = currentProfile;

            tempProfile.DirtyChanged += TempProfile_DirtyChanged;
        }

        private void TempProfile_DirtyChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void Test()
        {
            foreach(ActionSet set in tempProfile.ActionSets)
            {
                ActionSetItemsTest tempItem = new ActionSetItemsTest(set);
                actionSetItems.Add(tempItem);
            }

            //selectedActionLayerIndex = 0;
            //selectedActionSetIndex = 0;
            selectedActionLayerIndex = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
            selectedActionSetIndex = mapper.ActionProfile.CurrentActionSetIndex;
            PopulateLayerItems();
            PopulateCurrentLayerBindings();

            layerItems[selectedActionLayerIndex].ItemActive = true;
            actionSetItems[selectedActionSetIndex].ItemActive = true;
        }

        public void RefreshSetBindings()
        {
            buttonBindings.Clear();
            buttonBindingsIndexDict.Clear();
            touchpadBindings.Clear();
            triggerBindings.Clear();
            stickBindings.Clear();
            gyroBindings.Clear();

            PopulateLayerItems();
            PopulateCurrentLayerBindings();

            SelectedActionLayerIndex = 0;
            layerItems[selectedActionLayerIndex].ItemActive = true;
        }

        public void RefreshLayerBindings()
        {
            buttonBindings.Clear();
            buttonBindingsIndexDict.Clear();
            touchpadBindings.Clear();
            triggerBindings.Clear();
            stickBindings.Clear();
            gyroBindings.Clear();

            PopulateCurrentLayerBindings();
        }

        private void PopulateLayerItems()
        {
            ActionSetItemsTest setItem = actionSetItems[selectedActionSetIndex];
            ActionSet set = setItem.Set;

            layerItems.Clear();
            int tempInd = 0;
            foreach (ActionLayer layer in set.ActionLayers)
            {
                ActionLayerItemsTest tempLayerItem = new ActionLayerItemsTest(set, layer, tempInd++);
                layerItems.Add(tempLayerItem);
            }
        }

        private void PopulateCurrentLayerBindings()
        {
            int tempBtnInd = 0;

            foreach(InputBindingMeta meta in
                mapper.BindingList.Where((item) => item.controlType == InputBindingMeta.InputControlType.Button))
            {
                if (tempProfile.CurrentActionSet.CurrentActionLayer.buttonActionDict.
                    TryGetValue(meta.id, out ButtonMapAction tempBtnAct))
                {
                    BindingItemsTest tempItem = new BindingItemsTest(meta.id, meta.displayName, tempBtnAct, mapper);
                    buttonBindings.Add(tempItem);
                    buttonBindingsIndexDict.Add(meta.id, tempBtnInd++);
                }
            }

            foreach (InputBindingMeta meta in
                mapper.BindingList.Where((item) => item.controlType == InputBindingMeta.InputControlType.Touchpad))
            {
                if (tempProfile.CurrentActionSet.CurrentActionLayer.touchpadActionDict.
                        TryGetValue(meta.id, out TouchpadMapAction tempTouchAct))
                {
                    TouchBindingItemsTest tempItem = new TouchBindingItemsTest(meta.id, meta.displayName, tempTouchAct, mapper);
                    touchpadBindings.Add(tempItem);
                }
            }

            foreach (InputBindingMeta meta in
                mapper.BindingList.Where((item) => item.controlType == InputBindingMeta.InputControlType.Trigger))
            {
                if (tempProfile.CurrentActionSet.CurrentActionLayer.triggerActionDict.
                        TryGetValue(meta.id, out TriggerMapAction tempTrigAct))
                {
                    TriggerBindingItemsTest tempItem = new TriggerBindingItemsTest(meta.id, meta.displayName, tempTrigAct, mapper);
                    triggerBindings.Add(tempItem);
                }
            }

            foreach (InputBindingMeta meta in
                mapper.BindingList.Where((item) => item.controlType == InputBindingMeta.InputControlType.Stick))
            {
                if (tempProfile.CurrentActionSet.CurrentActionLayer.stickActionDict.
                        TryGetValue(meta.id, out StickMapAction tempTrigAct))
                {
                    StickBindingItemsTest tempItem = new StickBindingItemsTest(meta.id, meta.displayName, tempTrigAct, mapper);
                    stickBindings.Add(tempItem);
                }
            }

            foreach (InputBindingMeta meta in
                mapper.BindingList.Where((item) => item.controlType == InputBindingMeta.InputControlType.DPad))
            {
                if (tempProfile.CurrentActionSet.CurrentActionLayer.dpadActionDict.
                        TryGetValue(meta.id, out DPadMapAction tempDPadAct))
                {
                    DPadBindingItemsTest tempItem = new DPadBindingItemsTest(meta.id, meta.displayName, tempDPadAct, mapper);
                    dpadBindings.Add(tempItem);
                }
            }

            foreach (InputBindingMeta meta in
                mapper.BindingList.Where((item) => item.controlType == InputBindingMeta.InputControlType.Gyro))
            {
                if (tempProfile.CurrentActionSet.CurrentActionLayer.gyroActionDict.
                        TryGetValue(meta.id, out GyroMapAction tempTrigAct))
                {
                    GyroBindingItemsTest tempItem = new GyroBindingItemsTest(meta.id, meta.displayName, tempTrigAct, mapper);
                    gyroBindings.Add(tempItem);
                }
            }
        }

        public void SwitchActionSets(int ind)
        {
            actionSetItems[selectedActionSetIndex].ItemActive = false;

            selectedActionSetIndex = ind;
            actionSetItems[ind].ItemActive = true;

            actionResetEvent.Reset();
            mapper.QueueEvent(() =>
            {
                mapper.ActionProfile.SwitchSets(ind, mapper);
                mapper.ActionProfile.CurrentActionSet.RecompileCompositeLayer(mapper);

                actionResetEvent.Set();
            });

            SelectedActionLayerIndex = 0;
        }

        public void SwitchActionLayer(int layerInd)
        {
            layerItems[selectedActionLayerIndex].ItemActive = false;

            selectedActionLayerIndex = layerInd;
            layerItems[layerInd].ItemActive = true;

            actionResetEvent.Reset();
            mapper.QueueEvent(() =>
            {
                mapper.ActionProfile.CurrentActionSet.SwitchActionLayer(mapper, layerInd);
                actionResetEvent.Set();
            });
        }

        public void TestFakeSave(ProfileEntity entity, Profile profile)
        {
            ProfileEntity tempEntity = entity;
            Profile tempProfile = profile;
            string tempOutJson = string.Empty;
            actionResetEvent.Reset();

            mapper.QueueEvent(() =>
            {
                ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);
                tempOutJson = JsonConvert.SerializeObject(profileSerializer, Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        //Converters = new List<JsonConverter>()
                        //{
                        //    new MapActionSubTypeConverter(),
                        //}
                        //TypeNameHandling = TypeNameHandling.Objects
                        //ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                Trace.WriteLine(tempOutJson);

                actionResetEvent.Set();
            });

            actionResetEvent.Wait();

            if (!string.IsNullOrEmpty(tempOutJson) && overwriteFile)
            {
                using (StreamWriter writer = new StreamWriter(tempEntity.ProfilePath))
                using (JsonTextWriter jwriter = new JsonTextWriter(writer))
                {
                    jwriter.Formatting = Formatting.Indented;
                    jwriter.Indentation = 2;
                    JObject tempJObj = JObject.Parse(tempOutJson);
                    tempJObj.WriteTo(jwriter);
                    //writer.Write(tempOutJson);
                }
            }
        }

        public void TestSave(ProfileEntity entity, Profile profile)
        {
            ProfileEntity tempEntity = entity;
            Profile tempProfile = profile;
            string tempOutJson = string.Empty;
            actionResetEvent.Reset();

            mapper.QueueEvent(() =>
            {
                ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);
                tempOutJson = JsonConvert.SerializeObject(profileSerializer, Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        //Converters = new List<JsonConverter>()
                        //{
                        //    new MapActionSubTypeConverter(),
                        //}
                        //TypeNameHandling = TypeNameHandling.Objects
                        //ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                //Trace.WriteLine(tempOutJson);

                actionResetEvent.Set();
            });

            actionResetEvent.Wait();

            if (!string.IsNullOrEmpty(tempOutJson))
            {
                using (StreamWriter writer = new StreamWriter(tempEntity.ProfilePath))
                using (JsonTextWriter jwriter = new JsonTextWriter(writer))
                {
                    jwriter.Formatting = Formatting.Indented;
                    jwriter.Indentation = 2;
                    JObject tempJObj = JObject.Parse(tempOutJson);
                    tempJObj.WriteTo(jwriter);
                    //writer.Write(tempOutJson);
                }
            }
        }

        public void AddLayer()
        {
            ActionLayer tempLayer = null;
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            mapper.QueueEvent(() =>
            {
                int ind = mapper.ActionProfile.CurrentActionSet.ActionLayers.Count;
                tempLayer = new ActionLayer(ind);
                mapper.ActionProfile.CurrentActionSet.ActionLayers.Add(tempLayer);

                resetEvent.Set();
            });

            resetEvent.Wait();

            ActionLayerItemsTest tempItem = new ActionLayerItemsTest(mapper.ActionProfile.CurrentActionSet, tempLayer, layerItems.Count);
            layerItems.Add(tempItem);
        }

        public void RemoveLayer()
        {
            if (selectedActionLayerIndex <= 0) return;

            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            mapper.QueueEvent(() =>
            {
                ActionLayer tempLayer = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer;
                tempLayer.ReleaseActions(mapper, ignoreReleaseActions: true);
                mapper.ActionProfile.CurrentActionSet.ActionLayers.Remove(tempLayer);
                mapper.ActionProfile.CurrentActionSet.RecompileCompositeLayer(mapper);

                resetEvent.Set();
            });

            layerItems.RemoveAt(selectedActionLayerIndex);
            SelectedActionLayerIndex = 0;
            resetEvent.Wait();
        }

        public void AddSet()
        {
            ActionSet tempSet = null;
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            mapper.QueueEvent(() =>
            {
                int ind = mapper.ActionProfile.ActionSets.Count;
                tempSet = new ActionSet(ind, $"Set {ind+1}");
                mapper.ActionProfile.ActionSets.Add(tempSet);
                mapper.PrepopulateBlankActionLayer(tempSet.DefaultActionLayer);

                tempSet.ClearCompositeLayerActions();
                tempSet.PrepareCompositeLayer();

                resetEvent.Set();
            });

            resetEvent.Wait();

            ActionSetItemsTest tempItem = new ActionSetItemsTest(tempSet);
            actionSetItems.Add(tempItem);
        }

        public void RemoveSet()
        {
            if (selectedActionSetIndex <= 0) return;

            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            mapper.QueueEvent(() =>
            {
                ActionSet tempSet = mapper.ActionProfile.CurrentActionSet;
                tempSet.ReleaseActions(mapper, ignoreReleaseActions: true);

                // Switch to default set before removing current ActionSet
                mapper.ActionProfile.SwitchSets(0, mapper);
                mapper.ActionProfile.ActionSets.Remove(tempSet);

                mapper.ActionProfile.CurrentActionSet.RecompileCompositeLayer(mapper);

                resetEvent.Set();
            });

            actionSetItems.RemoveAt(SelectedActionSetIndex);
            SelectedActionSetIndex = 0;
            resetEvent.Wait();
        }
    }

    public class ActionLayerItemsTest
    {
        private ActionSet set;
        public ActionSet Set => set;

        private ActionLayer layer;
        public ActionLayer Layer => layer;

        public string DisplayName
        {
            get
            {
                string result = $"Layer {layer.Index+1}";
                if (!string.IsNullOrEmpty(layer.Name))
                {
                    result = layer.Name;
                }

                return result;
            }
        }
        public event EventHandler DisplayNameChanged;

        private int index;
        public int LayerIndex
        {
            get => index;
        }

        private bool itemActive;
        public bool ItemActive
        {
            get => itemActive;
            set
            {
                if (itemActive == value) return;
                itemActive = value;
                ItemActiveChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ItemActiveChanged;

        public ActionLayerItemsTest(ActionSet set, ActionLayer layer, int index)
        {
            this.set = set;
            this.layer = layer;
            this.index = index;
        }

        public void RaiseDisplayNameChanged()
        {
            DisplayNameChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ActionSetItemsTest
    {
        private ActionSet set;
        public ActionSet Set => set;

        public string DisplayName
        {
            get
            {
                string result = $"Set {set.Index+1}";
                if (!string.IsNullOrEmpty(set.Name))
                {
                    result = set.Name;
                }

                return result;
            }
        }
        public event EventHandler DisplayNameChanged;

        public int SetIndex
        {
            get => set.Index;
        }

        private bool itemActive;
        public bool ItemActive
        {
            get => itemActive;
            set
            {
                if (itemActive == value) return;
                itemActive = value;
                ItemActiveChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ItemActiveChanged;

        public ActionSetItemsTest(ActionSet set)
        {
            this.set = set;
        }

        public void RaiseDisplayNameChanged()
        {
            DisplayNameChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class TouchBindingItemsTest
    {
        private string displayInputMapString;
        public string DisplayInputMapString
        {
            get => displayInputMapString;
        }

        public string bindingName;
        public string BindingName
        {
            get => bindingName;
            //set => bindingName = value;
        }
        //public event EventHandler BindingNameChanged;

        private TouchpadMapAction mappedAction;
        public TouchpadMapAction MappedAction
        {
            get => mappedAction;
        }

        public string MappedActionType
        {
            get => mappedAction.ActionTypeName;
        }
        public event EventHandler MappedActionTypeChanged;

        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        public TouchBindingItemsTest(string bindingName, string displayInputMap,
            MapAction mappedAction, Mapper mapper)
        {
            this.bindingName = bindingName;
            this.displayInputMapString = displayInputMap;
            this.mappedAction = mappedAction as TouchpadMapAction;
            this.mapper = mapper;
        }

        public void UpdateAction(TouchpadMapAction action)
        {
            this.mappedAction = action;
            RaiseUIUpdate();
        }

        private void RaiseUIUpdate()
        {
            MappedActionTypeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class BindingItemsTest
    {
        private string displayInputMapString;
        public string DisplayInputMapString
        {
            get => displayInputMapString;
        }

        public string bindingName;
        public string BindingName
        {
            get => bindingName;
            //set => bindingName = value;
        }
        //public event EventHandler BindingNameChanged;

        private ButtonMapAction mappedAction;
        public ButtonMapAction MappedAction
        {
            get => mappedAction;
        }

        public string MappedActionType
        {
            get => mappedAction.ActionTypeName;
        }
        public event EventHandler MappedActionTypeChanged;

        public string DisplayBind
        {
            get
            {
                string result = mappedAction.DescribeActions(mapper);
                if (string.IsNullOrEmpty(result))
                {
                    result = "Unknown";
                }

                return result;
            }
        }
        public event EventHandler DisplayBindChanged;

        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        public BindingItemsTest(string bindingName, string displayInputMap, MapAction mappedAction, Mapper mapper)
        {
            this.bindingName = bindingName;
            this.displayInputMapString = displayInputMap;
            this.mappedAction = mappedAction as ButtonMapAction;
            this.mapper = mapper;
        }

        public void UpdateAction(MapAction action)
        {
            this.mappedAction = action as ButtonMapAction;
            RaiseUIUpdate();
        }

        private void RaiseUIUpdate()
        {
            MappedActionTypeChanged?.Invoke(this, EventArgs.Empty);
            DisplayBindChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class TriggerBindingItemsTest
    {
        private string displayInputMapString;
        public string DisplayInputMapString
        {
            get => displayInputMapString;
        }

        public string bindingName;
        public string BindingName
        {
            get => bindingName;
            //set => bindingName = value;
        }
        //public event EventHandler BindingNameChanged;

        private TriggerMapAction mappedAction;
        public TriggerMapAction MappedAction
        {
            get => mappedAction;
        }

        public string MappedActionType
        {
            get => mappedAction.ActionTypeName;
        }
        public event EventHandler MappedActionTypeChanged;

        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        public TriggerBindingItemsTest(string bindingName, string displayInputMap,
            MapAction mappedAction, Mapper mapper)
        {
            this.bindingName = bindingName;
            this.displayInputMapString = displayInputMap;
            this.mappedAction = mappedAction as TriggerMapAction;
            this.mapper = mapper;
        }

        public void UpdateAction(TriggerMapAction action)
        {
            this.mappedAction = action;
            RaiseUIUpdate();
        }

        private void RaiseUIUpdate()
        {
            MappedActionTypeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class StickBindingItemsTest
    {
        private string displayInputMapString;
        public string DisplayInputMapString
        {
            get => displayInputMapString;
        }

        public string bindingName;
        public string BindingName
        {
            get => bindingName;
            //set => bindingName = value;
        }
        //public event EventHandler BindingNameChanged;

        private StickMapAction mappedAction;
        public StickMapAction MappedAction
        {
            get => mappedAction;
        }

        public string MappedActionType
        {
            get => mappedAction.ActionTypeName;
        }
        public event EventHandler MappedActionTypeChanged;

        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        public StickBindingItemsTest(string bindingName, string displayInputMap,
            MapAction mappedAction, Mapper mapper)
        {
            this.bindingName = bindingName;
            this.displayInputMapString = displayInputMap;
            this.mappedAction = mappedAction as StickMapAction;
            this.mapper = mapper;
        }

        public void UpdateAction(StickMapAction action)
        {
            this.mappedAction = action;
            RaiseUIUpdate();
        }

        private void RaiseUIUpdate()
        {
            MappedActionTypeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class DPadBindingItemsTest
    {
        private string displayInputMapString;
        public string DisplayInputMapString
        {
            get => displayInputMapString;
        }

        public string bindingName;
        public string BindingName
        {
            get => bindingName;
            //set => bindingName = value;
        }
        //public event EventHandler BindingNameChanged;

        private DPadMapAction mappedAction;
        public DPadMapAction MappedAction
        {
            get => mappedAction;
        }

        public string MappedActionType
        {
            get => mappedAction.ActionTypeName;
        }
        public event EventHandler MappedActionTypeChanged;

        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        public DPadBindingItemsTest(string bindingName, string displayInputMap,
            MapAction mappedAction, Mapper mapper)
        {
            this.bindingName = bindingName;
            this.displayInputMapString = displayInputMap;
            this.mappedAction = mappedAction as DPadMapAction;
            this.mapper = mapper;
        }

        public void UpdateAction(DPadMapAction action)
        {
            this.mappedAction = action;
            RaiseUIUpdate();
        }

        private void RaiseUIUpdate()
        {
            MappedActionTypeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class GyroBindingItemsTest
    {
        private string displayInputMapString;
        public string DisplayInputMapString
        {
            get => displayInputMapString;
        }

        public string bindingName;
        public string BindingName
        {
            get => bindingName;
            //set => bindingName = value;
        }
        //public event EventHandler BindingNameChanged;

        private GyroMapAction mappedAction;
        public GyroMapAction MappedAction
        {
            get => mappedAction;
        }

        public string MappedActionType
        {
            get => mappedAction.ActionTypeName;
        }
        public event EventHandler MappedActionTypeChanged;

        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        public GyroBindingItemsTest(string bindingName, string displayInputMap,
            MapAction mappedAction, Mapper mapper)
        {
            this.bindingName = bindingName;
            this.displayInputMapString = displayInputMap;
            this.mappedAction = mappedAction as GyroMapAction;
            this.mapper = mapper;
        }

        public void UpdateAction(GyroMapAction action)
        {
            this.mappedAction = action;
            RaiseUIUpdate();
        }

        private void RaiseUIUpdate()
        {
            MappedActionTypeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
