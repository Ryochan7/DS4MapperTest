using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows; // Rect
using DS4MapperTest.DPadActions;
using DS4MapperTest.MapperUtil;
using Nefarius.ViGEm.Client;
using DS4MapperTest.ButtonActions;
using Sensorit.Base;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.StickActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.TriggerActions;
using Newtonsoft.Json;
using System.IO;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using System.Runtime.CompilerServices;

namespace DS4MapperTest
{
    public abstract class Mapper
    {
        protected const int X360_STICK_MAX = 32767;
        protected const int X360_STICK_MIN = -32768;
        protected const int OUTPUT_X360_RESOLUTION = X360_STICK_MAX - X360_STICK_MIN;

        protected const int DS4_STICK_MAX = 255;
        protected const int DS4_STICK_MIN = 0;
        protected const int DS4_STICK_MID = 128;
        protected const int OUTPUT_DS4_RESOLUTION = DS4_STICK_MAX - DS4_STICK_MIN;

        protected double absMouseX = 0.0;
        protected double absMouseY = 0.0;
        protected bool absMouseSync;

        public double AbsMouseX
        {
            get => absMouseX; set => absMouseX = value;
        }

        public double AbsMouseY
        {
            get => absMouseY; set => absMouseY = value;
        }

        public bool AbsMouseSync
        {
            get => absMouseSync;
            set => absMouseSync = value;
        }


        /// <summary>
        /// Used to tell the Mapper that a MapAction fired a mouse event. Mainly so
        /// Mapper skips resetting mouse remainders
        /// </summary>
        protected bool mouseEventFired;
        public bool MouseEventFired { get => mouseEventFired; set => mouseEventFired = value; }

        protected double mouseX = 0.0;
        protected double mouseY = 0.0;
        protected bool mouseSync;
        public bool MouseSync { get => mouseSync; set => mouseSync = value; }

        public double MouseX { get => mouseX; set => mouseX = value; }
        public double MouseY { get => mouseY; set => mouseY = value; }
        protected double mouseXRemainder = 0.0;
        protected double mouseYRemainder = 0.0;
        public double MouseXRemainder { get => mouseXRemainder; set => mouseXRemainder = value; }
        public double MouseYRemainder { get => mouseYRemainder; set => mouseYRemainder = value; }

        protected int mouseWheelX;
        protected int mouseWheelY;
        protected bool mouseWheelSync;
        public int MouseWheelX
        {
            get => mouseWheelX; set => mouseWheelX = value;
        }

        public int MouseWheelY
        {
            get => mouseWheelY; set => mouseWheelY = value;
        }

        public bool MouseWheelSync
        {
            get => mouseWheelSync; set => mouseWheelSync = value;
        }

        // Used to ensure output gamepad events are sent only as
        // needed when dealing with multiple input controllers (JoyCon)
        protected bool gamepadSync = true;

        protected double currentRate = 1.0; // Expressed in Hz
        protected double currentLatency = 1.0; // Expressed in sec

        public double CurrentRate { get => currentRate; set => currentRate = value; }
        public double CurrentLatency { get => currentLatency; }

        protected const int EMPTY_QUEUED_ACTION_SET = -1;
        protected int queuedActionSet = EMPTY_QUEUED_ACTION_SET;
        public int QueuedActionSet { get => queuedActionSet; set => queuedActionSet = value; }

        protected const int EMPTY_QUEUED_ACTION_LAYER = -1;
        protected int queuedActionLayer = EMPTY_QUEUED_ACTION_LAYER;
        protected bool applyQueuedActionLayer;
        protected bool switchQueuedActionLayer;
        public int QueuedActionLayer { get => queuedActionLayer; set => queuedActionLayer = value; }

        protected List<InputBindingMeta> bindingList = new List<InputBindingMeta>();
        public List<InputBindingMeta> BindingList
        {
            get => bindingList;
        }

        protected Dictionary<string, InputBindingMeta> bindingDict = new Dictionary<string, InputBindingMeta>();
        public Dictionary<string, InputBindingMeta> BindingDict
        {
            get => bindingDict;
        }

        protected Dictionary<string, StickDefinition> knownStickDefinitions =
            new Dictionary<string, StickDefinition>();
        protected Dictionary<string, TriggerDefinition> knownTriggerDefinitions =
            new Dictionary<string, TriggerDefinition>();
        protected Dictionary<string, TouchpadDefinition> knownTouchpadDefinitions =
            new Dictionary<string, TouchpadDefinition>();
        protected Dictionary<string, GyroSensDefinition> knownGyroSensDefinitions =
            new Dictionary<string, GyroSensDefinition>();

        protected Profile actionProfile = new Profile();
        public Profile ActionProfile => actionProfile;
        protected IntermediateState intermediateState = new IntermediateState();
        public ref IntermediateState IntermediateStateRef => ref intermediateState;

        protected List<OutputActionData> pendingReleaseActions =
            new List<OutputActionData>();
        public List<OutputActionData> PendingReleaseActions { get => pendingReleaseActions; set => pendingReleaseActions = value; }
        protected List<ActionFunc> pendingReleaseFuns = new List<ActionFunc>();
        public List<ActionFunc> PendingReleaseFuns { get => pendingReleaseFuns; }


        protected string profileFile = string.Empty;
        public string ProfileFile
        {
            get => profileFile;
            set
            {
                profileFile = value;
            }
        }

        protected bool processCycle = false;
        protected List<CycleButton> processCycleList = new List<CycleButton>();
        protected List<int> removePendingCandidates = new List<int>();

        protected AppGlobalData appGlobal;
        public AppGlobalData AppGlobal
        {
            get => appGlobal;
        }

        public virtual InputDeviceType DeviceType => InputDeviceType.None;
        public abstract DeviceReaderBase BaseReader
        {
            get;
        }

        protected bool quit = false;
        public bool Quit { get => quit; set => quit = value; }

        public event EventHandler<string> ProfileChanged;
        public event EventHandler PostProfileChange;

        protected DeviceActionDefaultsCreator deviceActionDefaults =
            new DummyActionDefaultsCreator();
        public DeviceActionDefaultsCreator DeviceActionDefaults => deviceActionDefaults;

        // Establish default items. Using old Steam Controller defaults
        protected List<ActionTriggerItem> actionTriggerItems = new List<ActionTriggerItem>()
        {
            new ActionTriggerItem("Always On", JoypadActionCodes.AlwaysOn),
            new ActionTriggerItem("A", JoypadActionCodes.BtnSouth),
            new ActionTriggerItem("B", JoypadActionCodes.BtnEast),
            new ActionTriggerItem("X", JoypadActionCodes.BtnWest),
            new ActionTriggerItem("Y", JoypadActionCodes.BtnNorth),
            new ActionTriggerItem("Left Bumper", JoypadActionCodes.BtnLShoulder),
            new ActionTriggerItem("Right Bumper", JoypadActionCodes.BtnRShoulder),
            new ActionTriggerItem("Left Trigger", JoypadActionCodes.AxisLTrigger),
            new ActionTriggerItem("Right Trigger", JoypadActionCodes.AxisRTrigger),
            new ActionTriggerItem("Left Grip", JoypadActionCodes.BtnLGrip),
            new ActionTriggerItem("Right Grip", JoypadActionCodes.BtnRGrip),
            new ActionTriggerItem("Stick Click", JoypadActionCodes.BtnThumbL),
            new ActionTriggerItem("Left Touchpad Touch", JoypadActionCodes.LPadTouch),
            new ActionTriggerItem("Right Touchpad Touch", JoypadActionCodes.RPadTouch),
            new ActionTriggerItem("Left Touchpad Click", JoypadActionCodes.LPadClick),
            new ActionTriggerItem("Right Touchpad Click", JoypadActionCodes.RPadClick),
            new ActionTriggerItem("Back", JoypadActionCodes.BtnSelect),
            new ActionTriggerItem("Start", JoypadActionCodes.BtnStart),
            new ActionTriggerItem("Steam", JoypadActionCodes.BtnMode),
        };
        public List<ActionTriggerItem> ActionTriggerItems => actionTriggerItems;

        protected FakerInputHandler fakerInputHandler;

        protected ViGEmClient vigemTestClient = null;
        //protected IXbox360Controller outputX360 = null;
        protected IVirtualGamepad outputController = null;
        protected OutputContType outputControlType = OutputContType.None;
        protected Xbox360FeedbackReceivedEventHandler outputForceFeedbackDel;
        protected Xbox360FeedbackReceivedEventHandler outputForceFeedbackSecondDel;

        // TODO: Move elsewhere
        public enum OutputContType : ushort
        {
            None,
            Xbox360,
            DualShock4,
        }

        // Keep reference to current editing action set from GUI
        // Allows different profile usage from mapper
        private ActionSet editActionSet;
        public ActionSet EditActionSet
        {
            get => editActionSet; set => editActionSet = value;
        }

        // Keep reference to current editing action layer from GUI
        // Allows different profile usage from mapper
        private ActionLayer editLayer;
        public ActionLayer EditLayer
        {
            get => editLayer; set => editLayer = value;
        }

        // VK, Count
        protected static Dictionary<uint, int> keyReferenceCountDict = new Dictionary<uint, int>();
        // VK
        protected static HashSet<uint> activeKeys = new HashSet<uint>();
        // VK
        protected static HashSet<uint> releasedKeys = new HashSet<uint>();

        protected static HashSet<int> currentMouseButtons = new HashSet<int>();
        protected static HashSet<int> activeMouseButtons = new HashSet<int>();
        protected static HashSet<int> releasedMouseButtons = new HashSet<int>();

        protected bool hasInputEvts;
        //protected object eventQueueLock = new object();
        protected ReaderWriterLockSlim eventQueueLocker = new ReaderWriterLockSlim();
        protected Queue<Action> eventQueue = new Queue<Action>();

        protected ReaderWriterLockSlim mapperActiveEditLock = new ReaderWriterLockSlim();
        protected bool mapperActionActive;
        protected bool pauseMapper;
        protected bool skipMapping;

        private void ReadFromProfile()
        {
            editActionSet = null;
            editLayer = null;

            actionProfile = new Profile();
            Profile tempProfile = actionProfile;

            tempProfile.ActionSets.Clear();
            List<ProfileActionsMapping> tempMappings = null;

            using (StreamReader sreader = new StreamReader(profileFile))
            {
                ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);

                string json = sreader.ReadToEnd();

                try
                {
                    JsonConvert.PopulateObject(json, profileSerializer);
                }
                catch (JsonSerializationException)
                {
                    UseBlankProfile();
                    return;
                }

                profileSerializer.PopulateProfile();
                tempProfile.ResetAliases();
                tempMappings = profileSerializer.ActionMappings;
            }

            //tempProfile.LeftTouchpadRotation = device.DeviceOptions.LeftTouchpadRotation;
            //tempProfile.RightTouchpadRotation = device.DeviceOptions.RightTouchpadRotation;

            // Populate ActionLayer dicts with default no action elements
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                //ActionLayer layer = set.ActionLayers.First();
                //if (layer != null)

                int layerIndex = 0;
                foreach (ActionLayer layer in set.ActionLayers)
                {
                    if (layerIndex == 0)
                    {
                        foreach (KeyValuePair<string, InputBindingMeta> tempMeta in bindingDict)
                        {
                            switch (tempMeta.Value.controlType)
                            {
                                case InputBindingMeta.InputControlType.Button:
                                    ButtonNoAction btnNoAction = new ButtonNoAction();
                                    btnNoAction.MappingId = tempMeta.Key;
                                    layer.buttonActionDict.Add(tempMeta.Key, btnNoAction);
                                    break;
                                case InputBindingMeta.InputControlType.DPad:
                                    DPadNoAction dpadNoAction = new DPadNoAction();
                                    dpadNoAction.MappingId = tempMeta.Key;
                                    layer.dpadActionDict.Add(tempMeta.Key, dpadNoAction);
                                    break;
                                case InputBindingMeta.InputControlType.Stick:
                                    {
                                        StickNoAction stickNoAct = new StickNoAction();
                                        stickNoAct.MappingId = tempMeta.Key;
                                        if (knownStickDefinitions.TryGetValue(tempMeta.Key,
                                            out StickDefinition tempDef))
                                        {
                                            stickNoAct.StickDefinition = tempDef;
                                        }
                                        layer.stickActionDict.Add(tempMeta.Key, stickNoAct);
                                    }

                                    break;
                                case InputBindingMeta.InputControlType.Trigger:
                                    {
                                        TriggerNoAction trigNoAct = new TriggerNoAction();
                                        trigNoAct.MappingId = tempMeta.Key;
                                        if (knownTriggerDefinitions.TryGetValue(tempMeta.Key,
                                            out TriggerDefinition tempDef))
                                        {
                                            trigNoAct.TriggerDef = tempDef;
                                        }
                                        layer.triggerActionDict.Add(tempMeta.Key, trigNoAct);
                                    }

                                    break;
                                case InputBindingMeta.InputControlType.Touchpad:
                                    {
                                        TouchpadNoAction touchNoAct = new TouchpadNoAction();
                                        touchNoAct.MappingId = tempMeta.Key;
                                        if (knownTouchpadDefinitions.TryGetValue(tempMeta.Key,
                                            out TouchpadDefinition tempDef))
                                        {
                                            touchNoAct.TouchDefinition = tempDef;
                                        }
                                        layer.touchpadActionDict.Add(tempMeta.Key, touchNoAct);
                                    }

                                    break;
                                case InputBindingMeta.InputControlType.Gyro:
                                    {
                                        GyroNoMapAction gyroNoMapAct = new GyroNoMapAction();
                                        gyroNoMapAct.MappingId = tempMeta.Key;
                                        if (knownGyroSensDefinitions.TryGetValue(tempMeta.Key,
                                            out GyroSensDefinition tempDef))
                                        {
                                            gyroNoMapAct.GyroSensDefinition = tempDef;
                                        }
                                        
                                        layer.gyroActionDict.Add(tempMeta.Key, gyroNoMapAct);
                                    }

                                    break;
                                default:
                                    break;
                            }
                        }

                        ButtonNoAction btnNoActionSet = new ButtonNoAction();
                        btnNoActionSet.MappingId = $"{ActionSet.ACTION_SET_ACTION_PREFIX}{set.Index}";
                        layer.actionSetActionDict.Add(btnNoActionSet.MappingId, btnNoActionSet);
                    }

                    layerIndex++;
                }
            }

            if (tempMappings != null)
            {
                foreach (ProfileActionsMapping mapping in tempMappings)
                {
                    ActionSet tempSet = null;
                    ActionLayer tempLayer = null;
                    if (mapping.ActionSet >= 0 && mapping.ActionSet < tempProfile.ActionSets.Count)
                    {
                        tempSet = tempProfile.ActionSets[mapping.ActionSet];
                        if (mapping.ActionLayer >= 0 && mapping.ActionLayer < tempSet.ActionLayers.Count)
                        {
                            tempLayer = tempSet.ActionLayers[mapping.ActionLayer];
                        }
                    }

                    if (tempLayer != null)
                    {
                        //ActionLayer parentLayer = (mapping.ActionLayer > 0 && mapping.ActionLayer < tempLayer.LayerActions.Count) ? tempLayer : null;
                        ActionLayer parentLayer = tempLayer != tempSet.DefaultActionLayer ? tempSet.DefaultActionLayer : null;
                        foreach (LayerMapping layerMapping in mapping.LayerMappings)
                        {
                            MapAction tempAction = layerMapping.ActionIndex >= 0 ?
                                tempLayer.LayerActions.Find((act) => act.Id == layerMapping.ActionIndex) : null;
                            if (tempAction != null)// layerMapping.ActionIndex < tempLayer.LayerActions.Count)
                            {
                                //MapAction tempAction = tempLayer.LayerActions[layerMapping.ActionIndex];
                                if (bindingDict.TryGetValue(layerMapping.InputBinding, out InputBindingMeta tempBind))
                                {
                                    switch (tempBind.controlType)
                                    {
                                        case InputBindingMeta.InputControlType.Button:
                                            if (tempAction is ButtonMapAction)
                                            {
                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.buttonActionDict[tempBind.id] = tempAction as ButtonMapAction;
                                                if (parentLayer != null && parentLayer.buttonActionDict.TryGetValue(tempBind.id, out ButtonMapAction tempParentBtnAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentBtnAction))
                                                {
                                                    //(tempAction as ButtonMapAction).SoftCopyFromParent(tempParentBtnAction);
                                                    //(tempAction as ButtonMapAction).CopyAction(tempParentBtnAction);
                                                }

                                                //if (parentLayer != null && parentLayer.LayerActions[layerMapping.ActionIndex] is ButtonMapAction)
                                                //{
                                                //    tempLayer.buttonActionDict[tempBind.id] = (tempAction as ButtonMapAction).DuplicateAction();
                                                //}
                                                //else
                                                //{
                                                //    tempLayer.buttonActionDict[tempBind.id] = tempAction as ButtonMapAction;
                                                //}
                                            }

                                            break;
                                        case InputBindingMeta.InputControlType.DPad:
                                            if (tempAction is DPadMapAction)
                                            {
                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.dpadActionDict[tempBind.id] = tempAction as DPadMapAction;
                                                if (parentLayer != null && parentLayer.dpadActionDict.TryGetValue(tempBind.id, out DPadMapAction tempParentDpadAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentDpadAction))
                                                {
                                                    (tempAction as DPadMapAction).SoftCopyFromParent(tempParentDpadAction);
                                                }
                                            }

                                            break;
                                        case InputBindingMeta.InputControlType.Stick:
                                            if (tempAction is StickMapAction)
                                            {
                                                StickMapAction tempStickAction = tempAction as StickMapAction;
                                                if (knownStickDefinitions.TryGetValue(tempBind.id,
                                                    out StickDefinition tempDef))
                                                {
                                                    tempStickAction.StickDefinition = tempDef;
                                                }

                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.stickActionDict[tempBind.id] = tempStickAction;

                                                if (parentLayer != null && parentLayer.stickActionDict.TryGetValue(tempBind.id, out StickMapAction tempParentStickAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentStickAction))
                                                {
                                                    (tempAction as StickMapAction).SoftCopyFromParent(tempParentStickAction);
                                                }
                                            }

                                            break;
                                        case InputBindingMeta.InputControlType.Trigger:
                                            if (tempAction is TriggerMapAction)
                                            {
                                                TriggerMapAction triggerAct = tempAction as TriggerMapAction;
                                                if (knownTriggerDefinitions.TryGetValue(tempBind.id, out TriggerDefinition tempDef))
                                                {
                                                    triggerAct.TriggerDef = tempDef;
                                                }

                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.triggerActionDict[tempBind.id] = tempAction as TriggerMapAction;
                                                if (parentLayer != null && parentLayer.triggerActionDict.TryGetValue(tempBind.id, out TriggerMapAction tempParentTrigAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentTrigAction))
                                                {
                                                    (tempAction as TriggerMapAction).SoftCopyFromParent(tempParentTrigAction);
                                                }
                                            }

                                            break;
                                        case InputBindingMeta.InputControlType.Touchpad:
                                            if (tempAction is TouchpadMapAction)
                                            {
                                                TouchpadMapAction touchAct = tempAction as TouchpadMapAction;
                                                if (knownTouchpadDefinitions.TryGetValue(tempBind.id, out TouchpadDefinition tempDef))
                                                {
                                                    touchAct.TouchDefinition = tempDef;
                                                }

                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.touchpadActionDict[tempBind.id] = tempAction as TouchpadMapAction;
                                                if (parentLayer != null && parentLayer.touchpadActionDict.TryGetValue(tempBind.id, out TouchpadMapAction tempParentTouchAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentTouchAction))
                                                {
                                                    (tempAction as TouchpadMapAction).SoftCopyFromParent(tempParentTouchAction);
                                                }

                                                touchAct.PrepareActions();
                                            }

                                            break;
                                        case InputBindingMeta.InputControlType.Gyro:
                                            if (tempAction is GyroMapAction)
                                            {
                                                GyroMapAction gyroAction = tempAction as GyroMapAction;
                                                //if (tempBind.id == "Gyro")
                                                if (knownGyroSensDefinitions.TryGetValue(tempBind.id, out GyroSensDefinition tempDef))
                                                {
                                                    gyroAction.GyroSensDefinition = tempDef;
                                                }

                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.gyroActionDict[tempBind.id] = tempAction as GyroMapAction;
                                                if (parentLayer != null && parentLayer.gyroActionDict.TryGetValue(tempBind.id, out GyroMapAction tempParentGyroAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentGyroAction))
                                                {
                                                    (tempAction as GyroMapAction).SoftCopyFromParent(tempParentGyroAction);
                                                }
                                            }

                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else if (layerMapping.InputBinding == $"{ActionSet.ACTION_SET_ACTION_PREFIX}{mapping.ActionSet}" && tempAction is ButtonMapAction)
                                {
                                    //if (tempAction is ButtonMapAction)
                                    {
                                        //tempAction.DefaultUnbound = false;
                                        tempAction.MappingId = $"{ActionSet.ACTION_SET_ACTION_PREFIX}{mapping.ActionSet}";
                                        tempLayer.actionSetActionDict[tempAction.MappingId] = tempAction as ButtonMapAction;
                                        if (parentLayer != null && parentLayer.actionSetActionDict.TryGetValue(tempAction.MappingId, out ButtonMapAction tempParentBtnAction) &&
                                            MapAction.IsSameType(tempAction, tempParentBtnAction))
                                        {
                                            //(tempAction as ButtonMapAction).SoftCopyFromParent(tempParentBtnAction);
                                            //(tempAction as ButtonMapAction).CopyAction(tempParentBtnAction);
                                        }

                                        //if (parentLayer != null && parentLayer.LayerActions[layerMapping.ActionIndex] is ButtonMapAction)
                                        //{
                                        //    tempLayer.buttonActionDict[tempBind.id] = (tempAction as ButtonMapAction).DuplicateAction();
                                        //}
                                        //else
                                        //{
                                        //    tempLayer.buttonActionDict[tempBind.id] = tempAction as ButtonMapAction;
                                        //}
                                    }
                                }
                            }
                        }

                    }
                }
            }

            //tempProfile.CurrentActionSet.CreateDupActionLayer();
            //tempLayer.buttonActionDict[tempBind.id] = tempAction as ButtonMapAction;
            //(tempProfile.CurrentActionSet.ActionLayers[1].buttonActionDict["A"] as ButtonAction).ActionFuncs.Clear();
            //(tempProfile.CurrentActionSet.ActionLayers[1].buttonActionDict["A"] as ButtonAction).ActionFuncs.Add(new NormalPressFunc(new OutputActionData(OutputActionData.ActionType.Keyboard, KeyInterop.VirtualKeyFromKey(Key.L))));
            //new ButtonAction(new OutputActionData(OutputActionData.ActionType.Keyboard, KeyInterop.VirtualKeyFromKey(Key.L)));

            // SyncActions for currently active ActionLayer instance
            //foreach (ActionSet set in tempProfile.ActionSets)
            //{
            //    set.CurrentActionLayer.SyncActions();
            //}

            // Compile convenience List for MapActions instances in layers
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                //int layerIdx = -1;
                ActionLayer parentLayer = set.DefaultActionLayer;
                foreach (ActionLayer layer in set.ActionLayers)
                {
                    //layerIdx++;
                    //if (layerIdx > 0)
                    //{
                    //    parentLayer.MergeLayerActions(layer);
                    //}

                    layer.SyncActions();
                }
            }

            // Prepare initial composite ActionLayer instance using
            // base ActionLayer references
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                //ActionLayer parentLayer = set.DefaultActionLayer;
                set.ClearCompositeLayerActions();
                set.PrepareCompositeLayer();
            }

            //tempProfile.CurrentActionSet.SwitchActionLayer(this, 1);

            Trace.WriteLine("IT IS FINISHED");
        }

        public void UseBlankProfile()
        {
            actionProfile = new Profile();
            actionProfile.Name = "Blank";
            Profile tempProfile = actionProfile;
            profileFile = string.Empty;

            //tempProfile.ActionSets.Clear();
            PrepareProfileActions(null);
        }

        private void PrepareProfileActions(List<ProfileActionsMapping> tempMappings)
        {
            Profile tempProfile = actionProfile;

            // Populate ActionLayer dicts with default no action elements
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                //ActionLayer layer = set.ActionLayers.First();
                //if (layer != null)

                int layerIndex = 0;
                foreach (ActionLayer layer in set.ActionLayers)
                {
                    if (layerIndex == 0)
                    {
                        foreach (KeyValuePair<string, InputBindingMeta> tempMeta in bindingDict)
                        {
                            switch (tempMeta.Value.controlType)
                            {
                                case InputBindingMeta.InputControlType.Button:
                                    ButtonNoAction btnNoAction = new ButtonNoAction();
                                    btnNoAction.MappingId = tempMeta.Key;
                                    layer.buttonActionDict.Add(tempMeta.Key, btnNoAction);
                                    break;
                                case InputBindingMeta.InputControlType.DPad:
                                    DPadNoAction dpadNoAction = new DPadNoAction();
                                    dpadNoAction.MappingId = tempMeta.Key;
                                    layer.dpadActionDict.Add(tempMeta.Key, dpadNoAction);
                                    break;
                                case InputBindingMeta.InputControlType.Stick:
                                    {
                                        StickNoAction stickNoAct = new StickNoAction();
                                        stickNoAct.MappingId = tempMeta.Key;
                                        if (knownStickDefinitions.TryGetValue(tempMeta.Key, out StickDefinition tempDef))
                                        {
                                            stickNoAct.StickDefinition = tempDef;
                                        }
                                        layer.stickActionDict.Add(tempMeta.Key, stickNoAct);
                                    }

                                    break;
                                case InputBindingMeta.InputControlType.Trigger:
                                    {
                                        TriggerNoAction trigNoAct = new TriggerNoAction();
                                        trigNoAct.MappingId = tempMeta.Key;
                                        if (knownTriggerDefinitions.TryGetValue(tempMeta.Key, out TriggerDefinition tempDef))
                                        {
                                            trigNoAct.TriggerDef = tempDef;
                                        }
                                        layer.triggerActionDict.Add(tempMeta.Key, trigNoAct);
                                    }

                                    break;
                                case InputBindingMeta.InputControlType.Touchpad:
                                    {
                                        TouchpadNoAction touchNoAct = new TouchpadNoAction();
                                        touchNoAct.MappingId = tempMeta.Key;
                                        if (knownTouchpadDefinitions.TryGetValue(tempMeta.Key, out TouchpadDefinition tempDef))
                                        {
                                            touchNoAct.TouchDefinition = tempDef;
                                        }
                                        layer.touchpadActionDict.Add(tempMeta.Key, touchNoAct);
                                    }

                                    break;
                                case InputBindingMeta.InputControlType.Gyro:
                                    {
                                        GyroNoMapAction gyroNoMapAct = new GyroNoMapAction();
                                        gyroNoMapAct.MappingId = tempMeta.Key;
                                        if (knownGyroSensDefinitions.TryGetValue(tempMeta.Key, out GyroSensDefinition tempDef))
                                        {
                                            gyroNoMapAct.GyroSensDefinition = tempDef;
                                        }

                                        layer.gyroActionDict.Add(tempMeta.Key, gyroNoMapAct);
                                    }

                                    break;
                                default:
                                    break;
                            }
                        }

                        if (!layer.actionSetActionDict.ContainsKey($"{ActionSet.ACTION_SET_ACTION_PREFIX}{set.Index}"))
                        {
                            ButtonNoAction btnNoActionSet = new ButtonNoAction();
                            btnNoActionSet.MappingId = $"{ActionSet.ACTION_SET_ACTION_PREFIX}{set.Index}";
                            layer.actionSetActionDict.Add(btnNoActionSet.MappingId, btnNoActionSet);
                        }
                    }

                    layerIndex++;
                }
            }

            if (tempMappings != null)
            {
                foreach (ProfileActionsMapping mapping in tempMappings)
                {
                    ActionSet tempSet = null;
                    ActionLayer tempLayer = null;
                    if (mapping.ActionSet >= 0 && mapping.ActionSet < tempProfile.ActionSets.Count)
                    {
                        tempSet = tempProfile.ActionSets[mapping.ActionSet];
                        if (mapping.ActionLayer >= 0 && mapping.ActionLayer < tempSet.ActionLayers.Count)
                        {
                            tempLayer = tempSet.ActionLayers[mapping.ActionLayer];
                        }
                    }

                    if (tempLayer != null)
                    {
                        //ActionLayer parentLayer = (mapping.ActionLayer > 0 && mapping.ActionLayer < tempLayer.LayerActions.Count) ? tempLayer : null;
                        ActionLayer parentLayer = tempLayer != tempSet.DefaultActionLayer ? tempSet.DefaultActionLayer : null;
                        foreach (LayerMapping layerMapping in mapping.LayerMappings)
                        {
                            MapAction tempAction = layerMapping.ActionIndex >= 0 ?
                                tempLayer.LayerActions.Find((act) => act.Id == layerMapping.ActionIndex) : null;
                            if (tempAction != null)// layerMapping.ActionIndex < tempLayer.LayerActions.Count)
                            {
                                //MapAction tempAction = tempLayer.LayerActions[layerMapping.ActionIndex];
                                if (bindingDict.TryGetValue(layerMapping.InputBinding, out InputBindingMeta tempBind))
                                {
                                    switch (tempBind.controlType)
                                    {
                                        case InputBindingMeta.InputControlType.Button:
                                            if (tempAction is ButtonMapAction)
                                            {
                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.buttonActionDict[tempBind.id] = tempAction as ButtonMapAction;
                                                if (parentLayer != null && parentLayer.buttonActionDict.TryGetValue(tempBind.id, out ButtonMapAction tempParentBtnAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentBtnAction))
                                                {
                                                    //(tempAction as ButtonMapAction).SoftCopyFromParent(tempParentBtnAction);
                                                    //(tempAction as ButtonMapAction).CopyAction(tempParentBtnAction);
                                                }

                                                //if (parentLayer != null && parentLayer.LayerActions[layerMapping.ActionIndex] is ButtonMapAction)
                                                //{
                                                //    tempLayer.buttonActionDict[tempBind.id] = (tempAction as ButtonMapAction).DuplicateAction();
                                                //}
                                                //else
                                                //{
                                                //    tempLayer.buttonActionDict[tempBind.id] = tempAction as ButtonMapAction;
                                                //}
                                            }

                                            break;
                                        case InputBindingMeta.InputControlType.DPad:
                                            if (tempAction is DPadMapAction)
                                            {
                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.dpadActionDict[tempBind.id] = tempAction as DPadMapAction;
                                                if (parentLayer != null && parentLayer.dpadActionDict.TryGetValue(tempBind.id, out DPadMapAction tempParentDpadAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentDpadAction))
                                                {
                                                    (tempAction as DPadMapAction).SoftCopyFromParent(tempParentDpadAction);
                                                }
                                            }

                                            break;
                                        case InputBindingMeta.InputControlType.Stick:
                                            if (tempAction is StickMapAction)
                                            {
                                                StickMapAction tempStickAction = tempAction as StickMapAction;
                                                if (knownStickDefinitions.TryGetValue(tempBind.id, out StickDefinition tempDef))
                                                {
                                                    tempStickAction.StickDefinition = tempDef;
                                                }

                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.stickActionDict[tempBind.id] = tempStickAction;

                                                if (parentLayer != null && parentLayer.stickActionDict.TryGetValue(tempBind.id, out StickMapAction tempParentStickAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentStickAction))
                                                {
                                                    (tempAction as StickMapAction).SoftCopyFromParent(tempParentStickAction);
                                                }
                                            }

                                            break;
                                        case InputBindingMeta.InputControlType.Trigger:
                                            if (tempAction is TriggerMapAction)
                                            {
                                                TriggerMapAction triggerAct = tempAction as TriggerMapAction;
                                                if (knownTriggerDefinitions.TryGetValue(tempBind.id, out TriggerDefinition tempDef))
                                                {
                                                    triggerAct.TriggerDef = tempDef;
                                                }

                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.triggerActionDict[tempBind.id] = tempAction as TriggerMapAction;
                                                if (parentLayer != null && parentLayer.triggerActionDict.TryGetValue(tempBind.id, out TriggerMapAction tempParentTrigAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentTrigAction))
                                                {
                                                    (tempAction as TriggerMapAction).SoftCopyFromParent(tempParentTrigAction);
                                                }
                                            }

                                            break;
                                        case InputBindingMeta.InputControlType.Touchpad:
                                            if (tempAction is TouchpadMapAction)
                                            {
                                                TouchpadMapAction touchAct = tempAction as TouchpadMapAction;
                                                if (knownTouchpadDefinitions.TryGetValue(tempBind.id, out TouchpadDefinition tempDef))
                                                {
                                                    touchAct.TouchDefinition = tempDef;
                                                }

                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.touchpadActionDict[tempBind.id] = tempAction as TouchpadMapAction;
                                                if (parentLayer != null && parentLayer.touchpadActionDict.TryGetValue(tempBind.id, out TouchpadMapAction tempParentTouchAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentTouchAction))
                                                {
                                                    (tempAction as TouchpadMapAction).SoftCopyFromParent(tempParentTouchAction);
                                                }

                                                touchAct.PrepareActions();
                                            }

                                            break;
                                        case InputBindingMeta.InputControlType.Gyro:
                                            if (tempAction is GyroMapAction)
                                            {
                                                GyroMapAction gyroAction = tempAction as GyroMapAction;
                                                //if (tempBind.id == "Gyro")
                                                if (knownGyroSensDefinitions.TryGetValue(tempBind.id, out GyroSensDefinition tempDef))
                                                {
                                                    gyroAction.GyroSensDefinition = tempDef;
                                                }

                                                //tempAction.DefaultUnbound = false;
                                                tempAction.MappingId = tempBind.id;
                                                tempLayer.gyroActionDict[tempBind.id] = tempAction as GyroMapAction;
                                                if (parentLayer != null && parentLayer.gyroActionDict.TryGetValue(tempBind.id, out GyroMapAction tempParentGyroAction) &&
                                                    MapAction.IsSameType(tempAction, tempParentGyroAction))
                                                {
                                                    (tempAction as GyroMapAction).SoftCopyFromParent(tempParentGyroAction);
                                                }
                                            }

                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else if (layerMapping.InputBinding == $"{ActionSet.ACTION_SET_ACTION_PREFIX}{mapping.ActionSet}" && tempAction is ButtonMapAction)
                                {
                                    //if (tempAction is ButtonMapAction)
                                    {
                                        //tempAction.DefaultUnbound = false;
                                        tempAction.MappingId = $"{ActionSet.ACTION_SET_ACTION_PREFIX}{mapping.ActionSet}";
                                        tempLayer.actionSetActionDict[tempAction.MappingId] = tempAction as ButtonMapAction;
                                        if (parentLayer != null && parentLayer.actionSetActionDict.TryGetValue(tempAction.MappingId, out ButtonMapAction tempParentBtnAction) &&
                                            MapAction.IsSameType(tempAction, tempParentBtnAction))
                                        {
                                            //(tempAction as ButtonMapAction).SoftCopyFromParent(tempParentBtnAction);
                                            //(tempAction as ButtonMapAction).CopyAction(tempParentBtnAction);
                                        }

                                        //if (parentLayer != null && parentLayer.LayerActions[layerMapping.ActionIndex] is ButtonMapAction)
                                        //{
                                        //    tempLayer.buttonActionDict[tempBind.id] = (tempAction as ButtonMapAction).DuplicateAction();
                                        //}
                                        //else
                                        //{
                                        //    tempLayer.buttonActionDict[tempBind.id] = tempAction as ButtonMapAction;
                                        //}
                                    }
                                }
                            }
                        }

                    }
                }
            }

            //tempProfile.CurrentActionSet.CreateDupActionLayer();
            //tempLayer.buttonActionDict[tempBind.id] = tempAction as ButtonMapAction;
            //(tempProfile.CurrentActionSet.ActionLayers[1].buttonActionDict["A"] as ButtonAction).ActionFuncs.Clear();
            //(tempProfile.CurrentActionSet.ActionLayers[1].buttonActionDict["A"] as ButtonAction).ActionFuncs.Add(new NormalPressFunc(new OutputActionData(OutputActionData.ActionType.Keyboard, KeyInterop.VirtualKeyFromKey(Key.L))));
            //new ButtonAction(new OutputActionData(OutputActionData.ActionType.Keyboard, KeyInterop.VirtualKeyFromKey(Key.L)));

            // SyncActions for currently active ActionLayer instance
            //foreach (ActionSet set in tempProfile.ActionSets)
            //{
            //    set.CurrentActionLayer.SyncActions();
            //}

            // Compile convenience List for MapActions instances in layers
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                //int layerIdx = -1;
                ActionLayer parentLayer = set.DefaultActionLayer;
                foreach (ActionLayer layer in set.ActionLayers)
                {
                    //layerIdx++;
                    //if (layerIdx > 0)
                    //{
                    //    parentLayer.MergeLayerActions(layer);
                    //}

                    layer.SyncActions();
                }
            }

            // Prepare initial composite ActionLayer instance using
            // base ActionLayer references
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                //ActionLayer parentLayer = set.DefaultActionLayer;
                set.ClearCompositeLayerActions();
                set.PrepareCompositeLayer();
            }

            //tempProfile.CurrentActionSet.SwitchActionLayer(this, 1);

            Trace.WriteLine("IT IS FINISHED");
        }

        public void PrepopulateBlankActionLayer(ActionLayer layer)
        {
            foreach (KeyValuePair<string, InputBindingMeta> tempMeta in bindingDict)
            {
                switch (tempMeta.Value.controlType)
                {
                    case InputBindingMeta.InputControlType.Button:
                        ButtonNoAction btnNoAction = new ButtonNoAction();
                        btnNoAction.MappingId = tempMeta.Key;
                        layer.buttonActionDict.Add(tempMeta.Key, btnNoAction);
                        break;
                    case InputBindingMeta.InputControlType.DPad:
                        DPadNoAction dpadNoAction = new DPadNoAction();
                        dpadNoAction.MappingId = tempMeta.Key;
                        layer.dpadActionDict.Add(tempMeta.Key, dpadNoAction);
                        break;
                    case InputBindingMeta.InputControlType.Stick:
                        {
                            StickNoAction stickNoAct = new StickNoAction();
                            stickNoAct.MappingId = tempMeta.Key;
                            if (knownStickDefinitions.TryGetValue(tempMeta.Key, out StickDefinition tempDef))
                            {
                                stickNoAct.StickDefinition = tempDef;
                            }
                            layer.stickActionDict.Add(tempMeta.Key, stickNoAct);
                        }

                        break;
                    case InputBindingMeta.InputControlType.Trigger:
                        {
                            TriggerNoAction trigNoAct = new TriggerNoAction();
                            trigNoAct.MappingId = tempMeta.Key;
                            if (knownTriggerDefinitions.TryGetValue(tempMeta.Key, out TriggerDefinition tempDef))
                            {
                                trigNoAct.TriggerDef = tempDef;
                            }
                            layer.triggerActionDict.Add(tempMeta.Key, trigNoAct);
                        }

                        break;
                    case InputBindingMeta.InputControlType.Touchpad:
                        {
                            TouchpadNoAction touchNoAct = new TouchpadNoAction();
                            touchNoAct.MappingId = tempMeta.Key;
                            if (knownTouchpadDefinitions.TryGetValue(tempMeta.Key, out TouchpadDefinition tempDef))
                            {
                                touchNoAct.TouchDefinition = tempDef;
                            }
                            layer.touchpadActionDict.Add(tempMeta.Key, touchNoAct);
                        }

                        break;
                    case InputBindingMeta.InputControlType.Gyro:
                        {
                            GyroNoMapAction gyroNoMapAct = new GyroNoMapAction();
                            gyroNoMapAct.MappingId = tempMeta.Key;
                            if (knownGyroSensDefinitions.TryGetValue(tempMeta.Key, out GyroSensDefinition tempDef))
                            {
                                gyroNoMapAct.GyroSensDefinition = tempDef;
                            }

                            layer.gyroActionDict.Add(tempMeta.Key, gyroNoMapAct);
                        }

                        break;
                    default:
                        break;
                }
            }

            layer.SyncActions();
        }

        public void ChangeProfile(string profilePath)
        {
            //if (!inMapperEvent)
            {
                //if (calibrationFinished)
                //{
                //    // Disconnect event
                //    reader.Report -= ControllerReader_Report;
                //}

                // Reset actions from current profile
                actionProfile.CurrentActionSet.ReleaseActions(this, true);

                // Relay changes to event systems
                SyncKeyboard();
                SyncMouseButtons();
                fakerInputHandler.Sync();

                // Might use this info later. Output controller device switch?
                EmulatedControllerSettings oldEmuControlSettings =
                    new EmulatedControllerSettings()
                    {
                        enabled = actionProfile.OutputGamepadSettings.enabled,
                        outputGamepad = actionProfile.OutputGamepadSettings.outputGamepad,
                    };

                // Reset virtual controller if currently connected
                if (outputController != null)
                {
                    outputController.ResetReport();
                    outputController.SubmitReport();
                }

                OutputContType oldContType = outputControlType;

                // Change profile path
                profileFile = profilePath;

                // Read file
                try
                {
                    ReadFromProfile();
                    ProfileChanged?.Invoke(this, profileFile);
                }
                catch (JsonException e)
                {
                    UseBlankProfile();
                    profileFile = string.Empty;
                    ProfileChanged?.Invoke(this, profileFile);
                    throw e;
                }

                // Check if requested output controller is different than the currently
                // connected type
                if (actionProfile.OutputGamepadSettings.Enabled && outputController != null &&
                    actionProfile.OutputGamepadSettings.OutputGamepad != outputControlType)
                {
                    outputController.Disconnect();
                    outputController = null;
                    outputControlType = OutputContType.None;
                    Thread.Sleep(100); // More of a pre-caution
                }

                // Create virtual controller if desired
                if (actionProfile.OutputGamepadSettings.Enabled && outputController == null &&
                    actionProfile.OutputGamepadSettings.OutputGamepad != OutputContType.None)
                {
                    Thread contThr = new Thread(() =>
                    {
                        if (actionProfile.OutputGamepadSettings.OutputGamepad == OutputContType.Xbox360)
                        {
                            IXbox360Controller tempOutputX360 = vigemTestClient.CreateXbox360Controller();
                            tempOutputX360.AutoSubmitReport = false;
                            tempOutputX360.Connect();
                            outputController = tempOutputX360;
                            outputControlType = OutputContType.Xbox360;
                        }
                        else if (actionProfile.OutputGamepadSettings.OutputGamepad == OutputContType.DualShock4)
                        {
                            IDualShock4Controller tempOutputDS4 = vigemTestClient.CreateDualShock4Controller();
                            tempOutputDS4.AutoSubmitReport = false;
                            tempOutputDS4.Connect();
                            outputController = tempOutputDS4;
                            outputControlType = OutputContType.DualShock4;
                        }
                    });
                    contThr.Priority = ThreadPriority.Normal;
                    contThr.IsBackground = true;
                    contThr.Start();
                    contThr.Join(); // Wait for bus object start
                    contThr = null;
                }
                else if (!actionProfile.OutputGamepadSettings.enabled && outputController != null)
                {
                    RemoveFeedback();
                    outputController.Disconnect();
                    outputController = null;
                    outputControlType = OutputContType.None;
                }

                // Check for current output controller and check for desired vibration
                // status
                if (outputController != null)
                {
                    if (actionProfile.OutputGamepadSettings.ForceFeedbackEnabled &&
                        outputControlType == OutputContType.Xbox360 &&
                        outputForceFeedbackDel == null)
                    {
                        Thread.Sleep(100);
                        EstablishForceFeedback();
                        HookFeedback();
                    }
                    else if (!actionProfile.OutputGamepadSettings.ForceFeedbackEnabled &&
                        outputControlType == OutputContType.Xbox360 &&
                        outputForceFeedbackDel != null)
                    {
                        RemoveFeedback();
                    }
                }

                PostProfileChange?.Invoke(this, EventArgs.Empty);

                //if (outputController != null)
                //{
                //    if (actionProfile.OutputGamepadSettings.ForceFeedbackEnabled &&
                //        outputForceFeedbackDel == null)
                //    {
                //        outputForceFeedbackDel = (sender, e) =>
                //        {
                //            device.currentLeftAmpRatio = e.LargeMotor / 255.0;
                //            device.currentRightAmpRatio = e.SmallMotor / 255.0;
                //            reader.WriteRumbleReport();
                //        };

                //        outputX360.FeedbackReceived += outputForceFeedbackDel;
                //    }
                //    else if (!actionProfile.OutputGamepadSettings.ForceFeedbackEnabled &&
                //        outputForceFeedbackDel != null)
                //    {
                //        outputX360.FeedbackReceived -= outputForceFeedbackDel;
                //        outputForceFeedbackDel = null;
                //    }
                //}

                //if (calibrationFinished)
                //{
                //    // Re-connect event
                //    reader.Report += ControllerReader_Report;
                //}

                //ProfileChanged?.Invoke(this, profilePath);

                //ProfileSerializer profileSerializer = new ProfileSerializer(actionProfile);
                //string tempOutJson = JsonConvert.SerializeObject(profileSerializer, Formatting.Indented,
                //    new JsonSerializerSettings()
                //    {
                //        //Converters = new List<JsonConverter>()
                //        //{
                //        //    new MapActionSubTypeConverter(),
                //        //}
                //        //TypeNameHandling = TypeNameHandling.Objects
                //        //ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //    });
                //Trace.WriteLine(tempOutJson);
            }
        }

        public virtual void HookFeedback()
        {
            if (outputController != null &&
                outputForceFeedbackDel != null)
            {
                (outputController as IXbox360Controller).FeedbackReceived += outputForceFeedbackDel;
            }
        }

        public virtual void RemoveFeedback()
        {
            if (outputController != null &&
                outputForceFeedbackDel != null)
            {
                (outputController as IXbox360Controller).FeedbackReceived -= outputForceFeedbackDel;
                outputForceFeedbackDel = null;
            }
        }

        public void SyncKeyboard()
        {
            var removed = releasedKeys.Except(activeKeys);
            var added = activeKeys.Except(releasedKeys);
            foreach (uint vk in removed)
            {
                if (keyReferenceCountDict.TryGetValue(vk, out int refCount))
                {
                    refCount--;
                    if (refCount <= 0)
                    {
#if !MAKE_TESTS
                        fakerInputHandler.PerformKeyRelease(vk);
#endif
                        //keyboardReport.KeyUp((KeyboardKey)vk);
                        //InputMethods.performKeyRelease((ushort)vk);
                        keyReferenceCountDict.Remove(vk);
                    }
                    else
                    {
                        keyReferenceCountDict[vk] = refCount;
                    }
                }
            }

            foreach (uint vk in added)
            {
                if (!keyReferenceCountDict.TryGetValue(vk, out int refCount))
                {
#if !MAKE_TESTS
                    fakerInputHandler.PerformKeyPress(vk);
#endif
                    //keyboardReport.KeyDown((KeyboardKey)vk);
                    //InputMethods.performKeyPress((ushort)vk);
                    keyReferenceCountDict.Add(vk, 1);
                }
                else
                {
                    keyReferenceCountDict[vk] = refCount++;
                }
            }

            releasedKeys.Clear();
            activeKeys.Clear();
        }

        public void SyncMouseButtons()
        {
            var removed = releasedMouseButtons.Except(activeMouseButtons);
            var added = activeMouseButtons.Except(releasedMouseButtons);

            foreach (int mouseCode in removed)
            {
                if (currentMouseButtons.Contains(mouseCode))
                {
                    uint mouseButton = 0;
                    switch (mouseCode)
                    {
                        case MouseButtonCodes.MOUSE_LEFT_BUTTON:
                            mouseButton = (uint)FakerInputWrapper.MouseButton.LeftButton;
                            //mouseButton = InputMethods.MOUSEEVENTF_LEFTUP;
                            break;
                        case MouseButtonCodes.MOUSE_MIDDLE_BUTTON:
                            mouseButton = (uint)FakerInputWrapper.MouseButton.MiddleButton;
                            //mouseButton = InputMethods.MOUSEEVENTF_MIDDLEUP;
                            break;
                        case MouseButtonCodes.MOUSE_RIGHT_BUTTON:
                            mouseButton = (uint)FakerInputWrapper.MouseButton.RightButton;
                            //mouseButton = InputMethods.MOUSEEVENTF_RIGHTUP;
                            break;
                        default:
                            break;
                    }

                    if (mouseButton != 0)
                    {
                        fakerInputHandler.PerformMouseButtonEvent(mouseButton);
                        //mouseReport.ButtonUp((FakerInputWrapper.MouseButton)mouseButton);
                        //InputMethods.MouseEvent(mouseButton);
                        currentMouseButtons.Remove(mouseCode);
                    }
                }
            }

            foreach (int mouseCode in added)
            {
                if (!currentMouseButtons.Contains(mouseCode))
                {
                    uint mouseButton = 0;
                    switch (mouseCode)
                    {
                        case MouseButtonCodes.MOUSE_LEFT_BUTTON:
                            mouseButton = (uint)FakerInputWrapper.MouseButton.LeftButton;
                            //mouseButton = InputMethods.MOUSEEVENTF_LEFTDOWN;
                            break;
                        case MouseButtonCodes.MOUSE_MIDDLE_BUTTON:
                            mouseButton = (uint)FakerInputWrapper.MouseButton.MiddleButton;
                            //mouseButton = InputMethods.MOUSEEVENTF_MIDDLEDOWN;
                            break;
                        case MouseButtonCodes.MOUSE_RIGHT_BUTTON:
                            mouseButton = (uint)FakerInputWrapper.MouseButton.RightButton;
                            //mouseButton = InputMethods.MOUSEEVENTF_RIGHTDOWN;
                            break;
                        default:
                            break;
                    }

                    if (mouseButton != 0)
                    {
                        fakerInputHandler.PerformMouseButtonPress(mouseButton);
                        //mouseReport.ButtonDown((FakerInputWrapper.MouseButton)mouseCode);
                        //InputMethods.MouseEvent(mouseButton);
                        currentMouseButtons.Add(mouseCode);
                    }
                }
            }

            releasedMouseButtons.Clear();
            activeMouseButtons.Clear();
        }

        public void TranslateCoorToAbsDisplay(double inX, double inY,
            ref Rect absDisplayBounds, ref Rect fullDesktopBounds,
            out double outX, out double outY)
        {
            //outX = outY = 0.0;
            //int topLeftX = (int)absDisplayBounds.Left;
            //double testLeft = 0.0;
            //double testRight = 0.0;
            //double testTop = 0.0;
            //double testBottom = 0.0;

            double widthRatio = (absDisplayBounds.Left + absDisplayBounds.Right) / fullDesktopBounds.Width;
            double heightRatio = (absDisplayBounds.Top + absDisplayBounds.Bottom) / fullDesktopBounds.Height;
            double bX = absDisplayBounds.Left / fullDesktopBounds.Width;
            double bY = absDisplayBounds.Top / fullDesktopBounds.Height;

            outX = widthRatio * inX + bX;
            outY = heightRatio * inY + bY;
            //outX = (absDisplayBounds.TopRight.X - absDisplayBounds.TopLeft.X) * inX + absDisplayBounds.TopLeft.X;
            //outY = (absDisplayBounds.BottomRight.Y - absDisplayBounds.TopLeft.Y) * inY + absDisplayBounds.TopLeft.Y;
        }


        protected void GenerateMouseMoveEvent()
        {
            if (mouseX != 0.0 || mouseY != 0.0)
            {
                if ((mouseX > 0.0 && mouseXRemainder > 0.0) || (mouseX < 0.0 && mouseXRemainder < 0.0))
                {
                    mouseX += mouseXRemainder;
                }
                else
                {
                    mouseXRemainder = 0.0;
                }

                if ((mouseY > 0.0 && mouseYRemainder > 0.0) || (mouseY < 0.0 && mouseYRemainder < 0.0))
                {
                    mouseY += mouseYRemainder;
                }
                else
                {
                    mouseYRemainder = 0.0;
                }

                //mouseX = filterX.Filter(mouseX, 1.0 / 0.016);
                //mouseY = filterY.Filter(mouseY, 1.0 / 0.016);
                //mouseX = filterX.Filter(mouseX, currentRate);
                //mouseY = filterY.Filter(mouseY, currentRate);

                //// Filter does not go back to absolute zero for reasons.Check
                //// for low number and reset to zero
                //if (Math.Abs(mouseX) < 0.0001) mouseX = 0.0;
                //if (Math.Abs(mouseY) < 0.0001) mouseY = 0.0;

                double mouseXTemp = mouseX - (remainderCutoff(mouseX * 100.0, 1.0) / 100.0);
                int mouseXInt = (int)(mouseXTemp);
                mouseXRemainder = mouseXTemp - mouseXInt;

                double mouseYTemp = mouseY - (remainderCutoff(mouseY * 100.0, 1.0) / 100.0);
                int mouseYInt = (int)(mouseYTemp);
                mouseYRemainder = mouseYTemp - mouseYInt;
                fakerInputHandler.MoveRelativeMouse(mouseXInt, mouseYInt);
                //mouseReport.MouseX = (short)mouseXInt;
                //mouseReport.MouseY = (short)mouseYInt;
                //InputMethods.MoveCursorBy(mouseXInt, mouseYInt);
            }
            else
            {
                mouseXRemainder = mouseYRemainder = 0.0;
                //mouseX = filterX.Filter(0.0, 1.0 / 0.016);
                //mouseY = filterY.Filter(0.0, 1.0 / 0.016);
                //filterX.Filter(mouseX, currentRate);
                //filterY.Filter(mouseY, currentRate);
            }

            mouseX = mouseY = 0.0;
        }

        public void GenerateMouseEventFiltered(OneEuroFilter filterX, OneEuroFilter filterY)
        {
            if (mouseX != 0.0 || mouseY != 0.0)
            {
                if ((mouseX > 0.0 && mouseXRemainder > 0.0) || (mouseX < 0.0 && mouseXRemainder < 0.0))
                {
                    mouseX += mouseXRemainder;
                }
                else
                {
                    mouseXRemainder = 0.0;
                }

                if ((mouseY > 0.0 && mouseYRemainder > 0.0) || (mouseY < 0.0 && mouseYRemainder < 0.0))
                {
                    mouseY += mouseYRemainder;
                }
                else
                {
                    mouseYRemainder = 0.0;
                }

                //mouseX = filterX.Filter(mouseX, 1.0 / 0.016);
                //mouseY = filterY.Filter(mouseY, 1.0 / 0.016);
                mouseX = filterX.Filter(mouseX, currentRate);
                mouseY = filterY.Filter(mouseY, currentRate);

                // Filter does not go back to absolute zero for reasons.Check
                // for low number and reset to zero
                if (Math.Abs(mouseX) < 0.0001) mouseX = 0.0;
                if (Math.Abs(mouseY) < 0.0001) mouseY = 0.0;

                double mouseXTemp = mouseX - (remainderCutoff(mouseX * 100.0, 1.0) / 100.0);
                int mouseXInt = (int)(mouseXTemp);
                mouseXRemainder = mouseXTemp - mouseXInt;

                double mouseYTemp = mouseY - (remainderCutoff(mouseY * 100.0, 1.0) / 100.0);
                int mouseYInt = (int)(mouseYTemp);
                mouseYRemainder = mouseYTemp - mouseYInt;
                fakerInputHandler.MoveRelativeMouse(mouseXInt, mouseYInt);
                //mouseReport.MouseX = (short)mouseXInt;
                //mouseReport.MouseY = (short)mouseYInt;
                //InputMethods.MoveCursorBy(mouseXInt, mouseYInt);
            }
            else
            {
                mouseXRemainder = mouseYRemainder = 0.0;
                //mouseX = filterX.Filter(0.0, 1.0 / 0.016);
                //mouseY = filterY.Filter(0.0, 1.0 / 0.016);
                filterX.Filter(mouseX, currentRate);
                filterY.Filter(mouseY, currentRate);
            }

            mouseX = mouseY = 0.0;
        }

        protected double remainderCutoff(double dividend, double divisor)
        {
            return dividend - (divisor * (int)(dividend / divisor));
        }

        //protected short AxisScale(int value, bool flip)
        //{
        //    unchecked
        //    {
        //        float temp = (value - STICK_MIN) * reciprocalInputResolution;
        //        if (flip) temp = (temp - 0.5f) * -1.0f + 0.5f;
        //        return (short)(temp * OUTPUT_X360_RESOLUTION + X360_STICK_MIN);
        //    }
        //}

        public virtual ref TouchEventFrame GetPreviousTouchEventFrame(TouchpadActionCodes padID)
        {
            throw new NotImplementedException();
        }

        public void RunEventFromRelative(OutputActionData actionData, bool pressed, double outputValue,
            bool fullRelease = true)
        {
            switch (actionData.OutputType)
            {
                case OutputActionData.ActionType.MouseWheel:
                    if (pressed && !actionData.activatedEvent)
                    {
                        int vWheel = 0; int hWheel = 0;
                        double absValue = Math.Abs(outputValue);
                        switch (actionData.OutputCode)
                        {
                            case 1: // Wheel Up
                                    //vWheel = 120;
                                vWheel = (int)(1 * absValue);
                                mouseWheelY = vWheel;
                                mouseWheelSync = true;
                                break;
                            case 2: // Wheel Down
                                    //vWheel = -120;
                                vWheel = (int)(-1 * absValue);
                                mouseWheelY = vWheel;
                                mouseWheelSync = true;
                                break;
                            case 3: // Wheel Left
                                    //hWheel = 120;
                                hWheel = (int)(1 * absValue);
                                mouseWheelX = hWheel;
                                mouseWheelSync = true;
                                break;
                            case 4: // Wheel Right
                                    //hWheel = -120;
                                hWheel = (int)(-1 * absValue);
                                mouseWheelX = hWheel;
                                mouseWheelSync = true;
                                break;
                            default:
                                break;
                        }

                        //fakerInputHandler.PerformMouseWheelEvent(vWheel, hWheel);
                        //InputMethods.MouseWheel(vWheel, hWheel);
                        actionData.activatedEvent = true;
                    }
                    else if (!pressed)
                    {
                        actionData.activatedEvent = false;
                    }

                    break;
                case OutputActionData.ActionType.RelativeMouse:
                    {
                        if (pressed)
                        {
                            double distance = 0.0;
                            double absValue = Math.Abs(outputValue);
                            bool xDir = false;
                            bool yDir = false;
                            switch (actionData.mouseDir)
                            {
                                case OutputActionData.RelativeMouseDir.MouseUp:
                                    distance = -1.0 * absValue;
                                    xDir = false;
                                    yDir = true;
                                    break;
                                case OutputActionData.RelativeMouseDir.MouseDown:
                                    distance = 1.0 * absValue;
                                    xDir = false;
                                    yDir = true;
                                    break;
                                case OutputActionData.RelativeMouseDir.MouseLeft:
                                    distance = -1.0 * absValue;
                                    xDir = true;
                                    yDir = false;
                                    break;
                                case OutputActionData.RelativeMouseDir.MouseRight:
                                    distance = 1.0 * absValue;
                                    xDir = true;
                                    yDir = false;
                                    break;
                                default:
                                    break;
                            }

                            int xSpeed = actionData.extraSettings.mouseXSpeed;
                            int ySpeed = actionData.extraSettings.mouseYSpeed;

                            const int MOUSESPEEDFACTOR = 20;
                            const double MOUSE_VELOCITY_OFFSET = 0.013;
                            double timeDelta = CurrentLatency;
                            int mouseVelocity = xDir ? xSpeed * MOUSESPEEDFACTOR : ySpeed * MOUSESPEEDFACTOR;
                            double mouseOffset = MOUSE_VELOCITY_OFFSET * mouseVelocity;
                            double tempMouseOffset = mouseOffset;

                            if (xDir)
                            {
                                double xMotion = ((mouseVelocity - tempMouseOffset) * timeDelta * distance + (mouseOffset * timeDelta));
                                MouseX = xMotion;
                                MouseSync = true;
                            }
                            else if (yDir)
                            {
                                double yMotion = ((mouseVelocity - tempMouseOffset) * timeDelta * distance + (mouseOffset * timeDelta));
                                MouseY = yMotion;
                                MouseSync = true;
                            }
                            //xMotion = ((mouseVelocity - tempMouseOffsetX) * timeDelta * absXNorm + (tempMouseOffsetX * timeDelta)) * xSign;
                            //yMotion = ((mouseVelocity - tempMouseOffsetY) * timeDelta * absYNorm + (tempMouseOffsetY * timeDelta)) * -ySign;
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        public void RunEventFromAnalog(OutputActionData actionData, bool pressed, double outputNorm,
            double axisUnit, bool fullRelease = true)
        {
            switch (actionData.OutputType)
            {
                case OutputActionData.ActionType.MouseWheel:
                    {
                        if (pressed && !actionData.activatedEvent)
                        {
                            int vWheel = 0; int hWheel = 0;
                            switch (actionData.OutputCode)
                            {
                                case 1: // Wheel Up
                                        //vWheel = 120;
                                    vWheel = 1;
                                    mouseWheelY = vWheel;
                                    mouseWheelSync = true;
                                    break;
                                case 2: // Wheel Down
                                        //vWheel = -120;
                                    vWheel = -1;
                                    mouseWheelY = vWheel;
                                    mouseWheelSync = true;
                                    break;
                                case 3: // Wheel Left
                                        //hWheel = 120;
                                    hWheel = 1;
                                    mouseWheelX = hWheel;
                                    mouseWheelSync = true;
                                    break;
                                case 4: // Wheel Right
                                        //hWheel = -120;
                                    hWheel = -1;
                                    mouseWheelX = hWheel;
                                    mouseWheelSync = true;
                                    break;
                                default:
                                    break;
                            }

                            //fakerInputHandler.PerformMouseWheelEvent(vWheel, hWheel);
                            //InputMethods.MouseWheel(vWheel, hWheel);
                            actionData.activatedEvent = true;
                        }
                        else if (!pressed)
                        {
                            actionData.activatedEvent = false;
                        }
                    }

                    break;
                case OutputActionData.ActionType.Keyboard:
                    {
                        if (pressed)
                        {
                            if (!actionData.activatedEvent)
                            {
                                activeKeys.Add(actionData.OutputCodeAlias);
                                actionData.activatedEvent = true;
                            }
                        }
                        else
                        {
                            if (actionData.activatedEvent)
                            {
                                releasedKeys.Add(actionData.OutputCodeAlias);
                                actionData.activatedEvent = false;
                            }
                        }
                    }

                    break;
                case OutputActionData.ActionType.MouseButton:
                    {
                        switch (actionData.OutputCode)
                        {
                            case MouseButtonCodes.MOUSE_LEFT_BUTTON:
                            case MouseButtonCodes.MOUSE_MIDDLE_BUTTON:
                            case MouseButtonCodes.MOUSE_RIGHT_BUTTON:
                                if (pressed)
                                {
                                    if (!currentMouseButtons.Contains(actionData.OutputCode))
                                    {
                                        activeMouseButtons.Add(actionData.OutputCode);
                                        actionData.activatedEvent = true;
                                    }
                                }
                                else
                                {
                                    if (currentMouseButtons.Contains(actionData.OutputCode))
                                    {
                                        releasedMouseButtons.Add(actionData.OutputCode);
                                        actionData.activatedEvent = false;
                                    }
                                }

                                break;
                            default:
                                break;
                        }
                    }

                    break;
                case OutputActionData.ActionType.RelativeMouse:
                    {
                        if (pressed)
                        {
                            double distance = 0.0;
                            double absNorm = Math.Abs(outputNorm);
                            bool xDir = false;
                            bool yDir = false;
                            switch (actionData.mouseDir)
                            {
                                case OutputActionData.RelativeMouseDir.MouseUp:
                                    distance = -1.0 * absNorm;
                                    xDir = false;
                                    yDir = true;
                                    break;
                                case OutputActionData.RelativeMouseDir.MouseDown:
                                    distance = 1.0 * absNorm;
                                    xDir = false;
                                    yDir = true;
                                    break;
                                case OutputActionData.RelativeMouseDir.MouseLeft:
                                    distance = -1.0 * absNorm;
                                    xDir = true;
                                    yDir = false;
                                    break;
                                case OutputActionData.RelativeMouseDir.MouseRight:
                                    distance = 1.0 * absNorm;
                                    xDir = true;
                                    yDir = false;
                                    break;
                                default:
                                    break;
                            }

                            int xSpeed = actionData.extraSettings.mouseXSpeed;
                            int ySpeed = actionData.extraSettings.mouseYSpeed;

                            const int MOUSESPEEDFACTOR = 20;
                            const double MOUSE_VELOCITY_OFFSET = 0.013;
                            double timeDelta = CurrentLatency;
                            int mouseVelocity = xDir ? xSpeed * MOUSESPEEDFACTOR : ySpeed * MOUSESPEEDFACTOR;
                            double mouseOffset = MOUSE_VELOCITY_OFFSET * mouseVelocity;
                            double tempMouseOffset = axisUnit * mouseOffset;

                            if (xDir)
                            {
                                double xMotion = ((mouseVelocity - tempMouseOffset) * timeDelta * distance + (mouseOffset * timeDelta));
                                MouseX = xMotion;
                                MouseSync = true;
                            }
                            else if (yDir)
                            {
                                double yMotion = ((mouseVelocity - tempMouseOffset) * timeDelta * distance + (mouseOffset * timeDelta));
                                MouseY = yMotion;
                                MouseSync = true;
                            }
                            //xMotion = ((mouseVelocity - tempMouseOffsetX) * timeDelta * absXNorm + (tempMouseOffsetX * timeDelta)) * xSign;
                            //yMotion = ((mouseVelocity - tempMouseOffsetY) * timeDelta * absYNorm + (tempMouseOffsetY * timeDelta)) * -ySign;
                        }
                    }

                    break;
                case OutputActionData.ActionType.GamepadControl:
                    {
                        //actionData.activatedEvent = pressed;
                        GamepadFromAxisInput(actionData, outputNorm);
                    }

                    break;
                case OutputActionData.ActionType.SwitchSet:
                case OutputActionData.ActionType.SwitchActionLayer:
                case OutputActionData.ActionType.ApplyActionLayer:
                case OutputActionData.ActionType.RemoveActionLayer:
                case OutputActionData.ActionType.HoldActionLayer:
                case OutputActionData.ActionType.CycleStep:
                    RunEventFromButton(actionData, pressed);
                    break;
                case OutputActionData.ActionType.Empty:
                    break;
                default:
                    break;
            }
        }

        public void RunEventFromButton(OutputActionData actionData, bool pressed, bool fullRelease = true)
        {
            switch (actionData.OutputType)
            {
                case OutputActionData.ActionType.Keyboard:
                    {
                        if (pressed)
                        {
                            if (!actionData.activatedEvent)
                            {
                                activeKeys.Add(actionData.OutputCodeAlias);
                                actionData.activatedEvent = true;
                            }
                        }
                        else
                        {
                            if (actionData.activatedEvent)
                            {
                                releasedKeys.Add(actionData.OutputCodeAlias);
                                actionData.activatedEvent = false;
                            }
                        }
                    }

                    break;
                case OutputActionData.ActionType.MouseButton:
                    {
                        switch (actionData.OutputCode)
                        {
                            case MouseButtonCodes.MOUSE_LEFT_BUTTON:
                            case MouseButtonCodes.MOUSE_MIDDLE_BUTTON:
                            case MouseButtonCodes.MOUSE_RIGHT_BUTTON:
                                if (pressed)
                                {
                                    if (!currentMouseButtons.Contains(actionData.OutputCode))
                                    {
                                        activeMouseButtons.Add(actionData.OutputCode);
                                        actionData.activatedEvent = true;
                                    }
                                }
                                else
                                {
                                    if (currentMouseButtons.Contains(actionData.OutputCode))
                                    {
                                        releasedMouseButtons.Add(actionData.OutputCode);
                                        actionData.activatedEvent = false;
                                    }
                                }

                                break;
                            default:
                                break;
                        }

                        break;
                    }
                case OutputActionData.ActionType.MouseWheel:
                    {
                        if (pressed && !actionData.activatedEvent)
                        {
                            int vWheel = 0; int hWheel = 0;
                            switch (actionData.OutputCode)
                            {
                                case 1: // Wheel Up
                                        //vWheel = 120;
                                    vWheel = 1;
                                    mouseWheelY = vWheel;
                                    mouseWheelSync = true;
                                    break;
                                case 2: // Wheel Down
                                        //vWheel = -120;
                                    vWheel = -1;
                                    mouseWheelY = vWheel;
                                    mouseWheelSync = true;
                                    break;
                                case 3: // Wheel Left
                                        //hWheel = 120;
                                    hWheel = 1;
                                    mouseWheelX = hWheel;
                                    mouseWheelSync = true;
                                    break;
                                case 4: // Wheel Right
                                        //hWheel = -120;
                                    hWheel = -1;
                                    mouseWheelX = hWheel;
                                    mouseWheelSync = true;
                                    break;
                                default:
                                    break;
                            }

                            //fakerInputHandler.PerformMouseWheelEvent(vWheel, hWheel);
                            //InputMethods.MouseWheel(vWheel, hWheel);
                            actionData.activatedEvent = true;
                        }
                        else if (!pressed)
                        {
                            actionData.activatedEvent = false;
                        }

                        break;
                    }
                case OutputActionData.ActionType.RelativeMouse:
                    {
                        if (pressed)
                        {
                            double distance = 0.0;
                            bool xDir = false;
                            bool yDir = false;
                            switch (actionData.mouseDir)
                            {
                                case OutputActionData.RelativeMouseDir.MouseUp:
                                    distance = -1.0;
                                    xDir = false;
                                    yDir = true;
                                    break;
                                case OutputActionData.RelativeMouseDir.MouseDown:
                                    distance = 1.0;
                                    xDir = false;
                                    yDir = true;
                                    break;
                                case OutputActionData.RelativeMouseDir.MouseLeft:
                                    distance = -1.0;
                                    xDir = true;
                                    yDir = false;
                                    break;
                                case OutputActionData.RelativeMouseDir.MouseRight:
                                    distance = 1.0;
                                    xDir = true;
                                    yDir = false;
                                    break;
                                default:
                                    break;
                            }

                            int xSpeed = actionData.extraSettings.mouseXSpeed;
                            int ySpeed = actionData.extraSettings.mouseYSpeed;

                            const int MOUSESPEEDFACTOR = 20;
                            const double MOUSE_VELOCITY_OFFSET = 0.013;
                            double timeDelta = CurrentLatency;
                            int mouseXVelocity = xSpeed * MOUSESPEEDFACTOR;
                            int mouseYVelocity = ySpeed * MOUSESPEEDFACTOR;
                            double mouseXOffset = MOUSE_VELOCITY_OFFSET * mouseXVelocity;
                            double mouseYOffset = MOUSE_VELOCITY_OFFSET * mouseYVelocity;

                            if (xDir)
                            {
                                double xMotion = ((mouseXVelocity - mouseXOffset) * timeDelta * distance + (mouseXOffset * timeDelta));
                                MouseX = xMotion;
                                MouseSync = true;
                            }

                            if (yDir)
                            {
                                double yMotion = ((mouseYVelocity - mouseYOffset) * timeDelta * distance + (mouseYOffset * timeDelta));
                                MouseY = yMotion;
                                MouseSync = true;
                            }
                            //xMotion = ((mouseVelocity - tempMouseOffsetX) * timeDelta * absXNorm + (tempMouseOffsetX * timeDelta)) * xSign;
                            //yMotion = ((mouseVelocity - tempMouseOffsetY) * timeDelta * absYNorm + (tempMouseOffsetY * timeDelta)) * -ySign;
                        }
                    }

                    break;
                case OutputActionData.ActionType.GamepadControl:
                    actionData.activatedEvent = pressed;
                    GamepadFromButtonInput(actionData, pressed);
                    break;

                case OutputActionData.ActionType.SwitchSet:
                    OutputActionData.SetChangeCondition cond = actionData.ChangeCondition;
                    actionData.activatedEvent = pressed;
                    if (pressed)
                    {
                        if (cond == OutputActionData.SetChangeCondition.Pressed)
                        {
                            queuedActionSet = actionData.ChangeToSet;
                        }
                    }
                    else
                    {
                        if (cond == OutputActionData.SetChangeCondition.Released)
                        {
                            queuedActionSet = actionData.ChangeToSet;
                        }
                    }

                    break;
                //case OutputActionData.ActionType.SwitchActionLayer:
                //    actionData.activatedEvent = pressed;
                //    if (pressed)
                //    {
                //        queuedActionLayer = actionData.ChangeToLayer;
                //    }
                //    else
                //    {
                //        // Revert to default layer
                //        queuedActionLayer = 0;
                //    }

                //    break;

                case OutputActionData.ActionType.SwitchActionLayer:
                    actionData.activatedEvent = pressed;
                    if (pressed)
                    {
                        OutputActionData.ActionLayerChangeCondition layerSwitchCond = actionData.LayerChangeCondition;
                        if (layerSwitchCond == OutputActionData.ActionLayerChangeCondition.Pressed)
                        {
                            queuedActionLayer = actionData.ChangeToLayer;
                            switchQueuedActionLayer = true;
                        }
                    }
                    else
                    {
                        OutputActionData.ActionLayerChangeCondition layerSwitchCond = actionData.LayerChangeCondition;
                        if (layerSwitchCond == OutputActionData.ActionLayerChangeCondition.Released)
                        {
                            queuedActionLayer = actionData.ChangeToLayer;
                            switchQueuedActionLayer = true;
                        }
                    }

                    break;
                case OutputActionData.ActionType.ApplyActionLayer:
                    OutputActionData.ActionLayerChangeCondition layerApplyCond = actionData.LayerChangeCondition;
                    actionData.activatedEvent = pressed;
                    //Trace.WriteLine("Change Action Layer {0}", actionData.ChangeToLayer.ToString());
                    if (pressed)
                    {
                        if (layerApplyCond == OutputActionData.ActionLayerChangeCondition.Pressed)
                        {
                            Trace.WriteLine($"Add Action Layer {actionData.ChangeToLayer}");
                            queuedActionLayer = actionData.ChangeToLayer;
                            applyQueuedActionLayer = true;
                        }
                    }
                    else
                    {
                        if (layerApplyCond == OutputActionData.ActionLayerChangeCondition.Released)
                        {
                            queuedActionLayer = actionData.ChangeToLayer;
                            applyQueuedActionLayer = true;
                        }
                    }

                    break;
                case OutputActionData.ActionType.RemoveActionLayer:
                    OutputActionData.ActionLayerChangeCondition layerRemoveCond = actionData.LayerChangeCondition;
                    actionData.activatedEvent = pressed;
                    //Trace.WriteLine("Remove Action Layer {0}", "0");
                    if (pressed)
                    {
                        if (layerRemoveCond == OutputActionData.ActionLayerChangeCondition.Pressed)
                        {
                            Trace.WriteLine("Removing Action Layer");
                            //queuedActionLayer = ActionSet.DEFAULT_ACTION_LAYER_INDEX;
                            queuedActionLayer = actionData.ChangeToLayer;
                            applyQueuedActionLayer = false;
                        }
                    }
                    else
                    {
                        if (layerRemoveCond == OutputActionData.ActionLayerChangeCondition.Released)
                        {
                            Trace.WriteLine("Removing Action Layer");
                            //queuedActionLayer = ActionSet.DEFAULT_ACTION_LAYER_INDEX;
                            //queuedActionLayer = actionProfile.CurrentActionSet.CurrentActionLayer.Index;
                            queuedActionLayer = actionData.ChangeToLayer;
                            applyQueuedActionLayer = false;
                        }
                    }

                    break;
                case OutputActionData.ActionType.HoldActionLayer:
                    //actionData.activatedEvent = pressed;
                    //Trace.WriteLine("Remove Action Layer {0}", "0");
                    if (pressed)
                    {
                        if (!actionData.activatedEvent)
                        //if (!actionData.waitForRelease)
                        {
                            Trace.WriteLine($"Hold Action Layer {actionData.ChangeToLayer}");
                            actionData.activatedEvent = true;
                            queuedActionLayer = actionData.ChangeToLayer;
                            applyQueuedActionLayer = true;
                            // Temporarily skip release step. Need to reset the flag after
                            actionData.skipRelease = true;
                            actionData.waitForRelease = true;
                        }
                    }
                    else if (!pressed)
                    {
                        //if (actionData.activatedEvent && fullRelease)
                        if (actionData.activatedEvent)
                        //if (!actionData.skipRelease && actionData.waitForRelease)
                        {
                            Trace.WriteLine($"Release Action Layer");
                            actionData.activatedEvent = false;
                            //queuedActionLayer = ActionSet.DEFAULT_ACTION_LAYER_INDEX;
                            queuedActionLayer = actionData.ChangeToLayer;
                            applyQueuedActionLayer = false;
                            actionData.waitForRelease = false;
                        }

                        // Happens on initial Release call from default ActionLayer
                        actionData.skipRelease = false;
                    }
                    break;
                case OutputActionData.ActionType.CycleStep:
                    {
                        if (pressed)
                        {
                            if (!actionData.activatedEvent)
                            {
                                ActivateCycle(actionData.cycleStepAct.cycleId, actionData.cycleStepAct);
                                actionData.activatedEvent = true;
                            }
                        }
                        else
                        {
                            actionData.activatedEvent = false;
                        }
                    }

                    break;
                default:
                    break;
            }

        }

        public void ActivateCycle(string cycleId,
            OutputActionData.CycleStepAction stepAction)
        {
            if (actionProfile.CycleBindings.TryGetValue(cycleId, out CycleButton testCycle))
            {
                switch (stepAction.stepActionType)
                {
                    case OutputActionData.CycleStepActionType.Forward:
                        {
                            testCycle.MoveNext();
                            testCycle.Prepare(this, true);
                            testCycle.Event(this);
                        }

                        break;
                    case OutputActionData.CycleStepActionType.Backward:
                        {
                            testCycle.MovePrevious();
                            testCycle.Prepare(this, true);
                            testCycle.Event(this);
                        }

                        break;
                    case OutputActionData.CycleStepActionType.MoveToFront:
                        {
                            testCycle.ResetCycle();
                            testCycle.Prepare(this, true);
                            testCycle.Event(this);
                        }

                        break;
                    case OutputActionData.CycleStepActionType.MoveToStep:
                        {
                            testCycle.MoveToStep(stepAction.stepNum);
                            testCycle.Prepare(this, true);
                            testCycle.Event(this);
                        }

                        break;
                    case OutputActionData.CycleStepActionType.MoveToEnd:
                        {
                            testCycle.MoveToEnd();
                            testCycle.Prepare(this, true);
                            testCycle.Event(this);
                        }

                        break;
                    default:
                        break;
                }

                processCycleList.Add(testCycle);
                processCycle = true;
            }
        }

        public virtual void Start(ViGEmClient vigemTestClient,
            FakerInputHandler fakerInputHandler)
        {
            this.vigemTestClient = vigemTestClient;
            this.fakerInputHandler = fakerInputHandler;

            if (!string.IsNullOrEmpty(profileFile))
            {
                try
                {
                    ChangeProfile(profileFile);
                }
                catch (JsonException)
                {
                    // Place some log message here eventually
                    // Blank profile will be loaded before exception is rethrown
                }
            }
            else
            {
                UseBlankProfile();
            }
        }

        protected void PopulateXbox()
        {
            IXbox360Controller outputX360 = outputController as IXbox360Controller;

            unchecked
            {
                ushort tempButtons = 0;
                if (intermediateState.BtnSouth) tempButtons |= Xbox360Button.A.Value;
                if (intermediateState.BtnEast) tempButtons |= Xbox360Button.B.Value;
                if (intermediateState.BtnWest) tempButtons |= Xbox360Button.X.Value;
                if (intermediateState.BtnNorth) tempButtons |= Xbox360Button.Y.Value;
                if (intermediateState.BtnStart) tempButtons |= Xbox360Button.Start.Value;
                if (intermediateState.BtnSelect) tempButtons |= Xbox360Button.Back.Value;

                if (intermediateState.BtnLShoulder) tempButtons |= Xbox360Button.LeftShoulder.Value;
                if (intermediateState.BtnRShoulder) tempButtons |= Xbox360Button.RightShoulder.Value;
                if (intermediateState.BtnMode) tempButtons |= Xbox360Button.Guide.Value;

                if (intermediateState.BtnThumbL) tempButtons |= Xbox360Button.LeftThumb.Value;
                if (intermediateState.BtnThumbR) tempButtons |= Xbox360Button.RightThumb.Value;

                if (intermediateState.DpadUp) tempButtons |= Xbox360Button.Up.Value;
                if (intermediateState.DpadDown) tempButtons |= Xbox360Button.Down.Value;
                if (intermediateState.DpadLeft) tempButtons |= Xbox360Button.Left.Value;
                if (intermediateState.DpadRight) tempButtons |= Xbox360Button.Right.Value;

                outputX360.SetButtonsFull(tempButtons);
            }

            outputX360.LeftThumbX = (short)(intermediateState.LX * (intermediateState.LX >= 0 ? X360_STICK_MAX : -X360_STICK_MIN));
            outputX360.LeftThumbY = (short)(intermediateState.LY * (intermediateState.LY >= 0 ? X360_STICK_MAX : -X360_STICK_MIN));

            outputX360.RightThumbX = (short)(intermediateState.RX * (intermediateState.RX >= 0 ? X360_STICK_MAX : -X360_STICK_MIN));
            outputX360.RightThumbY = (short)(intermediateState.RY * (intermediateState.RY >= 0 ? X360_STICK_MAX : -X360_STICK_MIN));

            outputX360.LeftTrigger = (byte)(intermediateState.LTrigger * 255);
            outputX360.RightTrigger = (byte)(intermediateState.RTrigger * 255);
        }

        protected void PopulateDualShock4()
        {
            IDualShock4Controller tempDS4 = outputController as IDualShock4Controller;

            unchecked
            {
                ushort tempButtons = 0;
                DualShock4DPadDirection tempDPad = DualShock4DPadDirection.None;
                ushort tempSpecial = 0;
                if (intermediateState.BtnSouth) tempButtons |= DualShock4Button.Cross.Value;
                if (intermediateState.BtnEast) tempButtons |= DualShock4Button.Circle.Value;
                if (intermediateState.BtnWest) tempButtons |= DualShock4Button.Square.Value;
                if (intermediateState.BtnNorth) tempButtons |= DualShock4Button.Triangle.Value;
                if (intermediateState.BtnStart) tempButtons |= DualShock4Button.Options.Value;
                if (intermediateState.BtnSelect) tempButtons |= DualShock4Button.Share.Value;

                if (intermediateState.BtnLShoulder) tempButtons |= DualShock4Button.ShoulderLeft.Value;
                if (intermediateState.BtnRShoulder) tempButtons |= DualShock4Button.ShoulderRight.Value;
                if (intermediateState.BtnMode) tempSpecial |= DualShock4SpecialButton.Ps.Value;

                if (intermediateState.BtnThumbL) tempButtons |= DualShock4Button.ThumbLeft.Value;
                if (intermediateState.BtnThumbR) tempButtons |= DualShock4Button.ThumbRight.Value;

                if (intermediateState.DpadUp && intermediateState.DpadRight) tempDPad = DualShock4DPadDirection.Northeast;
                else if (intermediateState.DpadUp && intermediateState.DpadLeft) tempDPad = DualShock4DPadDirection.Northwest;
                else if (intermediateState.DpadUp) tempDPad = DualShock4DPadDirection.North;
                else if (intermediateState.DpadRight && intermediateState.DpadDown) tempDPad = DualShock4DPadDirection.Southeast;
                else if (intermediateState.DpadRight) tempDPad = DualShock4DPadDirection.East;
                else if (intermediateState.DpadDown && intermediateState.DpadLeft) tempDPad = DualShock4DPadDirection.Southwest;
                else if (intermediateState.DpadDown) tempDPad = DualShock4DPadDirection.South;
                else if (intermediateState.DpadLeft) tempDPad = DualShock4DPadDirection.West;

                tempDS4.SetButtonsFull(tempButtons);
                tempDS4.SetSpecialButtonsFull((byte)tempSpecial);
                tempDS4.SetDPadDirection(tempDPad);
            }

            tempDS4.LeftThumbX = (byte)((intermediateState.LX >= 0 ? (DS4_STICK_MAX - DS4_STICK_MID) : -(DS4_STICK_MIN - DS4_STICK_MID)) * intermediateState.LX + DS4_STICK_MID);
            tempDS4.LeftThumbY = (byte)((intermediateState.LY >= 0 ? -(DS4_STICK_MIN - DS4_STICK_MID) : (DS4_STICK_MAX - DS4_STICK_MID)) * -intermediateState.LY + DS4_STICK_MID);

            tempDS4.RightThumbX = (byte)((intermediateState.RX >= 0 ? (DS4_STICK_MAX - DS4_STICK_MID) : -(DS4_STICK_MIN - DS4_STICK_MID)) * intermediateState.RX + DS4_STICK_MID);
            tempDS4.RightThumbY = (byte)((intermediateState.RY >= 0 ? -(DS4_STICK_MIN - DS4_STICK_MID) : (DS4_STICK_MAX - DS4_STICK_MID)) * -intermediateState.RY + DS4_STICK_MID);

            tempDS4.LeftTrigger = (byte)(intermediateState.LTrigger * 255);
            tempDS4.RightTrigger = (byte)(intermediateState.RTrigger * 255);
        }

        public void ProcessActionSetLayerChecks()
        {
            if (queuedActionSet != -1)
            {
                //Console.WriteLine("CHANGING SET: {0}", queuedActionSet);
                //actionProfile.CurrentActionSet.ReleaseActions(this);
                //actionProfile.SwitchSets(queuedActionSet, this);
                actionProfile.SwitchSets(queuedActionSet, this);

                // Switch to possible new ActionLayer before engaging new actions
                if (queuedActionLayer != -1 &&
                    actionProfile.CurrentActionSet.CurrentActionLayer.Index != queuedActionLayer)
                {
                    if (switchQueuedActionLayer)
                    {
                        actionProfile.CurrentActionSet.SwitchActionLayer(this, queuedActionLayer);
                    }
                    else if (applyQueuedActionLayer)
                    {
                        actionProfile.CurrentActionSet.AddActionLayer(this, queuedActionLayer);
                    }
                    else if (!applyQueuedActionLayer)
                    {
                        //int tempIndex = actionProfile.CurrentActionSet.CurrentActionLayer.Index;
                        int tempIndex = queuedActionLayer;
                        actionProfile.CurrentActionSet.RemovePartialActionLayer(this, tempIndex);
                        //actionProfile.CurrentActionSet.RemovePartialActionLayer(this, queuedActionLayer);
                    }

                    //actionProfile.CurrentActionSet.SwitchActionLayer(this, queuedActionLayer);
                    queuedActionLayer = -1;
                    applyQueuedActionLayer = false;
                    switchQueuedActionLayer = false;
                }

                // Put new actions into an active state
                //PrepareActionData(ref currentMapperState);
                queuedActionSet = -1;
            }
            // Check if only an ActionLayer change is happening
            else if (queuedActionLayer != -1)
            {
                Trace.WriteLine($"Going to Action Layer {queuedActionLayer}");
                if (switchQueuedActionLayer)
                {
                    actionProfile.CurrentActionSet.SwitchActionLayer(this, queuedActionLayer);
                }
                else if (applyQueuedActionLayer)
                {
                    actionProfile.CurrentActionSet.AddActionLayer(this, queuedActionLayer);
                }
                else if (!applyQueuedActionLayer)
                {
                    //int tempIndex = actionProfile.CurrentActionSet.CurrentActionLayer.Index;
                    int tempIndex = queuedActionLayer;
                    //actionProfile.CurrentActionSet.RemovePartialActionLayer(this, queuedActionLayer);
                    actionProfile.CurrentActionSet.RemovePartialActionLayer(this, tempIndex);
                }

                //actionProfile.CurrentActionSet.SwitchActionLayer(this, queuedActionLayer);

                // Put new actions into an active state
                //PrepareActionData(ref currentMapperState);
                queuedActionLayer = EMPTY_QUEUED_ACTION_LAYER;
                applyQueuedActionLayer = false;
                switchQueuedActionLayer = false;

                /*RequestOSD?.Invoke(this,
                    new RequestOSDArgs($"#{actionProfile.CurrentActionSet.CurrentActionLayer.Index}: {actionProfile.CurrentActionSet.CurrentActionLayer.Name}"));
                */
            }
        }

        public virtual void ProcessSyncEvents()
        {
            if (mouseSync)
            {
                //mouseReport.ResetMousePos();

                if (mouseX != 0.0 || mouseY != 0.0)
                {
                    //Console.WriteLine("MOVE: {0}, {1}", (int)mouseX, (int)mouseY);
                    GenerateMouseMoveEvent();
                }
                else
                {
                    // Probably not needed here. Leave as a temporary precaution
                    mouseXRemainder = mouseYRemainder = 0.0;

                    //filterX.Filter(0.0, currentRate); // Smooth on output
                    //filterY.Filter(0.0, currentRate); // Smooth on output
                }

                mouseSync = false;
            }
            else if (!mouseEventFired)
            {
                // Probably not needed here. Leave as a temporary precaution
                mouseXRemainder = mouseYRemainder = 0.0;
            }

            mouseEventFired = false;

            if (absMouseSync)
            {
                double outX = absMouseX, outY = absMouseY;
                //if (!appGlobal.absUseAllMonitors)
                //{
                //    double tempX = outX, tempY = outY;
                //    TranslateCoorToAbsDisplay(tempX, tempY, ref appGlobal.absDisplayBounds,
                //        ref appGlobal.fullDesktopBounds, out outX, out outY);
                //}

                fakerInputHandler.MoveAbsoluteMouse(outX, outY);
                absMouseSync = false;
            }

            if (mouseWheelSync)
            {
                fakerInputHandler.PerformMouseWheelEvent(vertical: mouseWheelY,
                    horizontal: mouseWheelX);
                mouseWheelSync = false;
            }

            SyncMouseButtons();
            //fakerInputDev.UpdateRelativeMouse(mouseReport);

            SyncKeyboard();
            //fakerInputDev.UpdateKeyboard(keyboardReport);
            fakerInputHandler.Sync();

            if (gamepadSync && intermediateState.Dirty)
            {
                if (outputController != null)
                {
                    if (outputControlType == OutputContType.Xbox360)
                    {
                        PopulateXbox();
                        outputController?.SubmitReport();
                    }
                    else if (outputControlType == OutputContType.DualShock4)
                    {
                        PopulateDualShock4();
                        outputController?.SubmitReport();
                    }
                }

                intermediateState.Dirty = false;
                intermediateState.LSDirty = intermediateState.RSDirty = false;
                gamepadSync = false;
            }
        }

        public void ProcessQueuedActions()
        {
            // Check for any waiting events and call them in this thread
            if (hasInputEvts)
            {
                using (WriteLocker locker = new WriteLocker(eventQueueLocker))
                //lock (eventQueueLock)
                {
                    Action tempAct = null;
                    for (int actInd = 0, actLen = eventQueue.Count;
                        actInd < actLen; actInd++)
                    {
                        tempAct = eventQueue.Dequeue();
                        tempAct.Invoke();
                    }

                    hasInputEvts = false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessCycleChecks()
        {
            if (processCycle)
            {
                foreach (CycleButton btn in processCycleList)
                {
                    btn.Prepare(this, false);
                    btn.Event(this);
                }

                processCycle = false;
                processCycleList.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessReleaseEvents()
        {
            if (pendingReleaseFuns.Count > 0)
            {
                ActionFuncEnumerator activeFuncEnumerator =
                    new ActionFuncEnumerator(pendingReleaseFuns);
                int ind = 0;
                while (activeFuncEnumerator.MoveNext())
                {
                    ActionFunc actionFunc = activeFuncEnumerator.Current;
                    actionFunc.Event(this, MapAction.trueStateData);
                    if (!actionFunc.active)
                    {
                        removePendingCandidates.Add(ind);
                    }

                    ind++;
                }

                if (removePendingCandidates.Count > 0)
                {
                    removePendingCandidates.Reverse();
                    foreach (int index in removePendingCandidates)
                    {
                        pendingReleaseFuns.RemoveAt(index);
                    }

                    removePendingCandidates.Clear();
                }
            }
        }

        /// <summary>
        /// Add Action to a list of Actions to call at the end of mapping routine.
        /// Action will be called in input thread
        /// </summary>
        /// <param name="tempAct">Action to enqueue to Queue</param>
        public void QueueEvent(Action tempAct)
        {
            //lock(eventQueueLock)
            using (WriteLocker locker = new WriteLocker(eventQueueLocker))
            {
                eventQueue.Enqueue(tempAct);
                hasInputEvts = true;
            }
        }

        /// <summary>
        /// Wait for mapping routine to be finished and then call passed Action.
        /// Action will run in called thread
        /// </summary>
        /// <param name="tempAct">Action to call when mapping routine is not running.</param>
        public void ProcessMappingChangeAction(Action tempAct)
        {
            using (WriteLocker locker = new WriteLocker(mapperActiveEditLock))
            {
                while (mapperActionActive)
                {
                    Thread.SpinWait(500);
                }

                // Mapping is not active. Set flag to halt mapper when entered
                pauseMapper = true;

                // Run call
                tempAct.Invoke();

                // Let mapper continue
                pauseMapper = false;
            }
        }

        public abstract void EstablishForceFeedback();

        public abstract bool IsButtonActive(JoypadActionCodes code);
        public abstract bool IsButtonsActiveDraft(IEnumerable<JoypadActionCodes> codes,
            bool andEval = true);

        public virtual void GamepadFromButtonInput(OutputActionData data, bool pressed)
        {
            data.activatedEvent = true;

            switch (data.JoypadCode)
            {
                case JoypadActionCodes.AxisLX:
                    intermediateState.LX = pressed ? (data.Negative ? -1.0 : 1.0) : 0.0;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisLY:
                    intermediateState.LY = pressed ? (data.Negative ? -1.0 : 1.0) : 0.0;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisRX:
                    intermediateState.RX = pressed ? (data.Negative ? -1.0 : 1.0) : 0.0;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisRY:
                    intermediateState.RY = pressed ? (data.Negative ? -1.0 : 1.0) : 0.0;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisLTrigger:
                    intermediateState.LTrigger = pressed ? 1.0 : 0.0;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisRTrigger:
                    intermediateState.RTrigger = pressed ? 1.0 : 0.0;
                    intermediateState.Dirty = true;
                    break;

                case JoypadActionCodes.AxisLXNeg:
                    intermediateState.LX = pressed ? -1.0 : 0.0;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisLXPos:
                    intermediateState.LX = pressed ? 1.0 : 0.0;
                    intermediateState.Dirty = true;
                    break;

                case JoypadActionCodes.AxisLYNeg:
                    intermediateState.LY = pressed ? -1.0 : 0.0;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisLYPos:
                    intermediateState.LY = pressed ? 1.0 : 0.0;
                    intermediateState.Dirty = true;
                    break;

                case JoypadActionCodes.AxisRXNeg:
                    intermediateState.RX = pressed ? -1.0 : 0.0;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisRXPos:
                    intermediateState.RX = pressed ? 1.0 : 0.0;
                    intermediateState.Dirty = true;
                    break;

                case JoypadActionCodes.AxisRYNeg:
                    intermediateState.RY = pressed ? -1.0 : 0.0;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisRYPos:
                    intermediateState.RY = pressed ? 1.0 : 0.0;
                    intermediateState.Dirty = true;
                    break;

                case JoypadActionCodes.BtnDPadUp:
                    intermediateState.DpadUp = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnDPadDown:
                    intermediateState.DpadDown = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnDPadLeft:
                    intermediateState.DpadLeft = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnDPadRight:
                    intermediateState.DpadRight = pressed;
                    intermediateState.Dirty = true;
                    break;

                case JoypadActionCodes.BtnNorth:
                    intermediateState.BtnNorth = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnEast:
                    intermediateState.BtnEast = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnSouth:
                    intermediateState.BtnSouth = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnWest:
                    intermediateState.BtnWest = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnMode:
                    intermediateState.BtnMode = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnStart:
                    intermediateState.BtnStart = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnSelect:
                    intermediateState.BtnSelect = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnLShoulder:
                    intermediateState.BtnLShoulder = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnRShoulder:
                    intermediateState.BtnRShoulder = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnThumbL:
                    intermediateState.BtnThumbL = pressed;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnThumbR:
                    intermediateState.BtnThumbR = pressed;
                    intermediateState.Dirty = true;
                    break;

                default:
                    break;
            }
        }

        public virtual void GamepadFromAxisInput(OutputActionData data, double norm)
        {
            bool active = norm != 0.0 ? true : false;
            data.activatedEvent = active;

            switch (data.JoypadCode)
            {
                case JoypadActionCodes.AxisLX:
                    intermediateState.LX = norm;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisLY:
                    intermediateState.LY = norm;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisRX:
                    intermediateState.RX = norm;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisRY:
                    intermediateState.RY = norm;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisLTrigger:
                    intermediateState.LTrigger = norm;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.AxisRTrigger:
                    intermediateState.RTrigger = norm;
                    intermediateState.Dirty = true;
                    break;

                case JoypadActionCodes.BtnDPadUp:
                    intermediateState.DpadUp = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnDPadDown:
                    intermediateState.DpadDown = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnDPadLeft:
                    intermediateState.DpadLeft = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnDPadRight:
                    intermediateState.DpadRight = active;
                    intermediateState.Dirty = true;
                    break;

                case JoypadActionCodes.BtnNorth:
                    intermediateState.BtnNorth = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnEast:
                    intermediateState.BtnEast = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnSouth:
                    intermediateState.BtnSouth = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnWest:
                    intermediateState.BtnWest = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnMode:
                    intermediateState.BtnMode = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnStart:
                    intermediateState.BtnStart = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnSelect:
                    intermediateState.BtnSelect = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnLShoulder:
                    intermediateState.BtnLShoulder = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnRShoulder:
                    intermediateState.BtnRShoulder = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnThumbL:
                    intermediateState.BtnThumbL = active;
                    intermediateState.Dirty = true;
                    break;
                case JoypadActionCodes.BtnThumbR:
                    intermediateState.BtnThumbR = active;
                    intermediateState.Dirty = true;
                    break;
                default:
                    break;
            }
        }

        public virtual void GamepadFromStickInput(OutputActionData data,
            double xNorm, double yNorm, bool force=true)
        {
            data.activatedEvent = true;

            switch (data.StickCode)
            {
                case StickActionCodes.LS:
                    if (force)
                    {
                        intermediateState.LX = xNorm;
                        intermediateState.LY = yNorm;
                        intermediateState.LSDirty = true;
                    }
                    else
                    {
                        if (!intermediateState.LSDirty)
                        {
                            intermediateState.LX = xNorm;
                            intermediateState.LY = yNorm;
                            intermediateState.LSDirty = true;
                        }
                    }

                    intermediateState.Dirty = true;
                    break;
                case StickActionCodes.RS:
                    if (force)
                    {
                        intermediateState.RX = xNorm;
                        intermediateState.RY = yNorm;
                        intermediateState.RSDirty = true;
                    }
                    else
                    {
                        if (!intermediateState.RSDirty)
                        {
                            intermediateState.RX = xNorm;
                            intermediateState.RY = yNorm;
                            intermediateState.RSDirty = true;
                        }
                    }

                    intermediateState.Dirty = true;
                    break;

                default:
                    break;
            }
        }

        public virtual void GamepadFromDpadInput(OutputActionData data, DpadDirections direction)
        {
            data.activatedEvent = true;

            bool dpadUp = false, dpadLeft = false, dpadDown = false, dpadRight = false;
            unchecked
            {
                if ((direction & DpadDirections.Up) != 0)
                    dpadUp = true;
                if ((direction & DpadDirections.Left) != 0)
                    dpadLeft = true;
                if ((direction & DpadDirections.Down) != 0)
                    dpadDown = true;
                if ((direction & DpadDirections.Right) != 0)
                    dpadRight = true;
            }

            switch (data.DpadCode)
            {
                case DPadActionCodes.DPad1:
                    intermediateState.DpadUp = dpadUp;
                    intermediateState.DpadLeft = dpadLeft;
                    intermediateState.DpadDown = dpadDown;
                    intermediateState.DpadRight = dpadRight;
                    intermediateState.Dirty = true;
                    break;

                default:
                    break;
            }
        }

        public virtual void Stop(bool finalSync = false)
        {
            quit = true;

            actionProfile.CurrentActionSet.ReleaseActions(this, true);

            editActionSet = null;
            editLayer = null;

            // Relay changes to event systems
            SyncKeyboard();
            SyncMouseButtons();
            if (finalSync)
            {
                fakerInputHandler.Sync();
            }

            outputController?.Disconnect();
            if (outputController != null)
            {
                RemoveFeedback();
            }

            outputController = null;
            outputControlType = OutputContType.None;
        }
    }
}
