using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;

namespace DS4MapperTest
{
    public class ActionSet
    {
        public const int DEFAULT_ACTION_LAYER_INDEX = 0;
        private const int DEFAULT_ACTION_LAYERS_NUMBER = 1;
        public const string ACTION_SET_ACTION_PREFIX = "ActionSet_";

        private List<ActionLayer> actionLayers =
            new List<ActionLayer>(DEFAULT_ACTION_LAYERS_NUMBER);
        public List<ActionLayer> ActionLayers
        {
            get => actionLayers; set => actionLayers = value;
        }

        private ActionLayer currentActionLayer;
        public ActionLayer CurrentActionLayer { get => currentActionLayer; }

        private ActionLayer defaultActionLayer;
        public ActionLayer DefaultActionLayer { get => defaultActionLayer; }

        private List<ActionLayer> appliedLayers = new List<ActionLayer>();
        private ActionLayer recentAppliedLayer;
        public ActionLayer RecentAppliedLayer
        {
            //get => appliedLayers.Last();
            get => recentAppliedLayer;
        }
        // Composite layer used when merging ActionLayer instances
        private ActionLayer activeCompositeLayer;

        public bool UsingCompositeLayer
        {
            get => currentActionLayer == activeCompositeLayer;
        }


        /// <summary>
        /// Will contain all mapped actions associated in the sets
        /// for all ActionLayer instances
        /// </summary>
        //private List<MapAction> setActions = new List<MapAction>();

        private int index;
        public int Index { get => index; }

        private string name;
        public string Name { get => name; set => name = value; }

        private string description;
        public string Description { get => description; set => description = value; }

        public string ActionButtonId { get => $"{ACTION_SET_ACTION_PREFIX}{index}"; }

        public Dictionary<string, ButtonMapAction> ButtonActionDict { get => currentActionLayer.buttonActionDict; }

        
        public Dictionary<string, DPadMapAction> DpadActionDict { get => currentActionLayer.dpadActionDict; }

        
        public Dictionary<string, StickMapAction> StickActionDict { get => currentActionLayer.stickActionDict; }

        
        public Dictionary<string, GyroMapAction> GyroActionDict { get => currentActionLayer.gyroActionDict; }

        public ActionSet(int index, string name)
        {
            this.index = index;
            this.name = name;

            // Create default Action Layer for use in Action Set
            ActionLayer actionLayer = new ActionLayer(0);
            actionLayers.Add(actionLayer);
            // Create default Action Set virtual action button
            actionLayer.actionSetActionDict.Add($"{ACTION_SET_ACTION_PREFIX}{index}", new ButtonNoAction()
            {
                MappingId = $"{ACTION_SET_ACTION_PREFIX}{index}",
            });

            // Set up initial references
            defaultActionLayer = actionLayers[0];
            currentActionLayer = defaultActionLayer;

            appliedLayers.Add(currentActionLayer);
            recentAppliedLayer = currentActionLayer;

            activeCompositeLayer = new ActionLayer(0);
        }

        public void SyncActions()
        {
            //currentActionLayer.SyncActions();
            /*theLawActions.Clear();
            foreach (ButtonMapAction action in ButtonActionDict.Values)
            {
                theLawActions.Add(action);
            }

            foreach(DPadMapAction action in DpadActionDict.Values)
            {
                theLawActions.Add(action);
            }

            foreach(StickMapAction action in StickActionDict.Values)
            {
                theLawActions.Add(action);
            }

            foreach(GyroMapAction action in GyroActionDict.Values)
            {
                theLawActions.Add(action);
            }
            */
        }

        public void CompileActiveLayer()
        {

        }

        public void AddActionLayer(Mapper mapper, int index)
        {
            if (0 <= index && index < actionLayers.Count)
            {
                ActionLayer tempLayer = actionLayers[index];
                AddActionLayer(mapper, tempLayer);
            }
        }

        public void AddActionLayer(Mapper mapper, ActionLayer layer)
        {
            if (layer != defaultActionLayer &&
                !appliedLayers.Contains(layer))
            {
                appliedLayers.Add(layer);
                recentAppliedLayer = layer;

                currentActionLayer.CompareReleaseActions(mapper, layer, false);

                HashSet<string> tempOverrideIds =
                        layer.normalActionDict.Keys.ToHashSet();

                //IEnumerable<ActionLayer> revLayers =
                //        appliedLayers.Reverse<ActionLayer>();

                foreach (string mapId in tempOverrideIds)
                {
                    {
                        MapAction currentMapAction = layer.normalActionDict[mapId];
                        // Some logic needed to check for transition (Release vs SoftRelease)
                        //currentMapAction.Release(mapper);

                        if (mapId.StartsWith(ACTION_SET_ACTION_PREFIX))
                        {
                            activeCompositeLayer.actionSetActionDict[mapId] = currentMapAction as ButtonMapAction;
                        }
                        else
                        {
                            switch (mapper.BindingDict[mapId].controlType)
                            {
                                case InputBindingMeta.InputControlType.Button:
                                    activeCompositeLayer.buttonActionDict[mapId] = currentMapAction as ButtonMapAction;
                                    break;
                                case InputBindingMeta.InputControlType.DPad:
                                    activeCompositeLayer.dpadActionDict[mapId] = currentMapAction as DPadMapAction;
                                    break;
                                case InputBindingMeta.InputControlType.Stick:
                                    activeCompositeLayer.stickActionDict[mapId] = currentMapAction as StickMapAction;
                                    break;
                                case InputBindingMeta.InputControlType.Gyro:
                                    activeCompositeLayer.gyroActionDict[mapId] = currentMapAction as GyroMapAction;
                                    break;
                                case InputBindingMeta.InputControlType.Touchpad:
                                    activeCompositeLayer.touchpadActionDict[mapId] = currentMapAction as TouchpadMapAction;
                                    break;
                                case InputBindingMeta.InputControlType.Trigger:
                                    activeCompositeLayer.triggerActionDict[mapId] = currentMapAction as TriggerMapAction;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                activeCompositeLayer.Name = layer.Name;
                activeCompositeLayer.Index = layer.Index;

                // Make composite ActionLayer the current layer
                currentActionLayer = activeCompositeLayer;
                currentActionLayer.SyncActions();
            }
        }

        
        public void RemovePartialActionLayer(Mapper mapper, int index)
        {
            if (0 <= index && index < actionLayers.Count)
            {
                ActionLayer tempLayer = actionLayers[index];
                RemovePartialActionLayer(mapper, tempLayer);
            }
        }
        
        public void RemovePartialActionLayer(Mapper mapper, ActionLayer layer)
        {
            if (appliedLayers.Count > 1 && appliedLayers.Remove(layer))
            {
                // Check if only using base ActionLayer
                if (appliedLayers.Count == 1)
                {
                    currentActionLayer.CompareReleaseActions(mapper, defaultActionLayer);
                    //currentActionLayer.ReleaseActions(mapper);

                    // Clear composite ActionLayer references and restore using default layer
                    activeCompositeLayer.ClearActions();
                    PrepareCompositeLayer();

                    // Applying base ActionLayer
                    currentActionLayer = defaultActionLayer;
                    recentAppliedLayer = defaultActionLayer;
                }
                else
                {
                    // Child ActionLayer instances still active. Make new composite mapping
                    HashSet<string> tempOverrideIds =
                        layer.normalActionDict.Keys.ToHashSet();

                    //int priorityId = 0;
                    IEnumerable<ActionLayer> revLayers =
                        appliedLayers.Reverse<ActionLayer>();

                    foreach(string mapId in tempOverrideIds)
                    {
                        ActionLayer usedIdLayer = null;
                        MapAction tempBaseAction = null;
                        foreach(ActionLayer tempLayer in revLayers)
                        {
                            if (tempLayer.normalActionDict.ContainsKey(mapId))
                            {
                                usedIdLayer = tempLayer;
                                tempBaseAction = usedIdLayer.normalActionDict[mapId];
                                break;
                            }
                        }

                        if (usedIdLayer != null && tempBaseAction != null)
                        {
                            MapAction currentMapAction = layer.normalActionDict[mapId];
                            // Some logic needed to check for transition (Release vs SoftRelease)
                            //currentMapAction.Release(mapper);
                            layer.CompareReleaseActions(mapper, currentMapAction, tempBaseAction);

                            if (mapId.StartsWith(ACTION_SET_ACTION_PREFIX))
                            {
                                activeCompositeLayer.actionSetActionDict[tempBaseAction.MappingId] = tempBaseAction as ButtonAction;
                            }
                            else
                            {
                                switch (mapper.BindingDict[tempBaseAction.MappingId].controlType)
                                {
                                    case InputBindingMeta.InputControlType.Button:
                                        activeCompositeLayer.buttonActionDict[tempBaseAction.MappingId] = tempBaseAction as ButtonMapAction;
                                        break;
                                    case InputBindingMeta.InputControlType.DPad:
                                        activeCompositeLayer.dpadActionDict[tempBaseAction.MappingId] = tempBaseAction as DPadMapAction;
                                        break;
                                    case InputBindingMeta.InputControlType.Stick:
                                        activeCompositeLayer.stickActionDict[tempBaseAction.MappingId] = tempBaseAction as StickMapAction;
                                        break;
                                    case InputBindingMeta.InputControlType.Gyro:
                                        activeCompositeLayer.gyroActionDict[tempBaseAction.MappingId] = tempBaseAction as GyroMapAction;
                                        break;
                                    case InputBindingMeta.InputControlType.Touchpad:
                                        activeCompositeLayer.touchpadActionDict[tempBaseAction.MappingId] = tempBaseAction as TouchpadMapAction;
                                        break;
                                    case InputBindingMeta.InputControlType.Trigger:
                                        activeCompositeLayer.triggerActionDict[tempBaseAction.MappingId] = tempBaseAction as TriggerMapAction;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    //foreach (ActionLayer tempLayer in appliedLayers)
                    
                    {
                        activeCompositeLayer.Name = revLayers.First().Name;
                        activeCompositeLayer.Index = revLayers.First().Index;

                        // Make composite ActionLayer the current layer
                        activeCompositeLayer.SyncActions();
                        currentActionLayer = activeCompositeLayer;
                        //priorityId++;
                    }
                }
            }
        }

        public void ReleaseActions(Mapper mapper, bool ignoreReleaseActions = false)
        {
            currentActionLayer.ReleaseActions(mapper, true,
                ignoreReleaseActions: ignoreReleaseActions);
            /*foreach(MapAction action in theLawActions)
            {
                action.Release(mapper);
            }
            */
        }

        public void SwitchActionLayer(Mapper mapper, int index)
        {
            if (0 <= index && index < actionLayers.Count)
            {
                ActionLayer changingLayer = actionLayers[index];
                //tempLayer.CopyParentStates();

                currentActionLayer.CompareReleaseActions(mapper, changingLayer, false);

                if (changingLayer == defaultActionLayer)
                {
                    // Applying base ActionLayer instance
                    appliedLayers.RemoveRange(1, appliedLayers.Count - 1);

                    // Clear composite ActionLayer references and restore using default layer
                    activeCompositeLayer.ClearActions();
                    PrepareCompositeLayer();

                    currentActionLayer = defaultActionLayer;
                    recentAppliedLayer = defaultActionLayer;
                }
                else
                {
                    // Skip resetting composite layer if switching from base
                    if (appliedLayers.Count > 1)
                    {
                        //appliedLayers.Remove(currentActionLayer);
                        appliedLayers.RemoveRange(1, appliedLayers.Count - 1);

                        // Clear composite ActionLayer references and restore using default layer
                        activeCompositeLayer.ClearActions();
                        PrepareCompositeLayer();
                    }

                    appliedLayers.Add(changingLayer);
                    recentAppliedLayer = changingLayer;

                    HashSet<string> tempOverrideIds =
                        changingLayer.normalActionDict.Keys.ToHashSet();

                    IEnumerable<ActionLayer> revLayers =
                        appliedLayers.Reverse<ActionLayer>();

                    foreach (string mapId in tempOverrideIds)
                    {
                        ActionLayer usedIdLayer = null;
                        MapAction tempBaseAction = null;
                        foreach (ActionLayer tempLayer in revLayers)
                        {
                            if (tempLayer.normalActionDict.ContainsKey(mapId))
                            {
                                usedIdLayer = tempLayer;
                                tempBaseAction = usedIdLayer.normalActionDict[mapId];
                                break;
                            }
                        }

                        if (usedIdLayer != null && tempBaseAction != null)
                        {
                            MapAction currentMapAction = changingLayer.normalActionDict[mapId];

                            if (mapId.StartsWith(ACTION_SET_ACTION_PREFIX))
                            {
                                activeCompositeLayer.actionSetActionDict[mapId] = currentMapAction as ButtonMapAction;
                            }
                            else
                            {
                                switch (mapper.BindingDict[mapId].controlType)
                                {
                                    case InputBindingMeta.InputControlType.Button:
                                        activeCompositeLayer.buttonActionDict[mapId] = currentMapAction as ButtonMapAction;
                                        break;
                                    case InputBindingMeta.InputControlType.DPad:
                                        activeCompositeLayer.dpadActionDict[mapId] = currentMapAction as DPadMapAction;
                                        break;
                                    case InputBindingMeta.InputControlType.Stick:
                                        activeCompositeLayer.stickActionDict[mapId] = currentMapAction as StickMapAction;
                                        break;
                                    case InputBindingMeta.InputControlType.Gyro:
                                        activeCompositeLayer.gyroActionDict[mapId] = currentMapAction as GyroMapAction;
                                        break;
                                    case InputBindingMeta.InputControlType.Touchpad:
                                        activeCompositeLayer.touchpadActionDict[mapId] = currentMapAction as TouchpadMapAction;
                                        break;
                                    case InputBindingMeta.InputControlType.Trigger:
                                        activeCompositeLayer.triggerActionDict[mapId] = currentMapAction as TriggerMapAction;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    activeCompositeLayer.Name = changingLayer.Name;
                    activeCompositeLayer.Index = changingLayer.Index;

                    // Make composite ActionLayer the current layer
                    activeCompositeLayer.SyncActions();
                    currentActionLayer = activeCompositeLayer;
                }

                //currentActionLayer.SoftReleaseActions(mapper, false);
                //currentActionLayer.ReleaseActions(mapper, false);
                //if (index == 0)
                //{
                //    currentActionLayer.ReleaseActions(mapper, false);
                //}
                //else
                //{
                //    currentActionLayer.SoftReleaseActions(mapper, false);
                //}

                //currentActionLayer = changingLayer;
            }
        }

        /*public void CreateEmptyActionLayer()
        {
            ActionLayer tempLayer = new ActionLayer(actionLayers.Count);
            actionLayers.Add(tempLayer);
        }

        public void CreateDupActionLayer()
        {
            ActionLayer tempLayer = new ActionLayer(actionLayers.Count);
            actionLayers.Add(tempLayer);

            foreach (KeyValuePair<string, ButtonMapAction> pair in defaultActionLayer.actionSetActionDict)
            {
                //Trace.WriteLine(pair.Key);
                tempLayer.actionSetActionDict.Add(pair.Key,
                    pair.Value.DuplicateAction());
                //tempLayer.buttonActionDict.Add(pair.Key, pair.Value);
                //theLawActions.Add(action);
            }

            foreach (KeyValuePair<string, ButtonMapAction> pair in defaultActionLayer.buttonActionDict)
            {
                Trace.WriteLine(pair.Key);
                tempLayer.buttonActionDict.Add(pair.Key,
                    pair.Value.DuplicateAction());
                //tempLayer.buttonActionDict.Add(pair.Key, pair.Value);
                //theLawActions.Add(action);
            }

            //Trace.WriteLine("TEST IT NOW PUNK");
            //AxisDirButton testActionnow = new AxisDirButton();
            //ButtonMapAction testGeneric = testActionnow as ButtonMapAction;
            //testGeneric.DuplicateAction();

            foreach (KeyValuePair<string, DPadMapAction> pair in defaultActionLayer.dpadActionDict)
            {
                Trace.WriteLine(pair.Key);
                tempLayer.dpadActionDict.Add(pair.Key,
                    pair.Value.DuplicateAction());
                //tempLayer.dpadActionDict.Add(pair.Key, pair.Value);
                //theLawActions.Add(action);
            }

            foreach (KeyValuePair<string, StickMapAction> pair in defaultActionLayer.stickActionDict)
            {
                tempLayer.stickActionDict.Add(pair.Key,
                    pair.Value.DuplicateAction());
                //tempLayer.stickActionDict.Add(pair.Key, pair.Value);
                //theLawActions.Add(action);
            }

            foreach (KeyValuePair<string, GyroMapAction> pair in defaultActionLayer.gyroActionDict)
            {
                tempLayer.gyroActionDict.Add(pair.Key,
                    pair.Value.DuplicateAction());
                //tempLayer.gyroActionDict.Add(pair.Key, pair.Value);
                //theLawActions.Add(action);
            }
        }
        

        public void RemoveActionLayer(Mapper mapper, int index)
        {
            ActionLayer tempLayer = actionLayers[index];
            if (tempLayer != null && tempLayer != defaultActionLayer)
            {
                tempLayer.ReleaseActions(mapper);

                if (tempLayer == currentActionLayer)
                {
                    currentActionLayer = defaultActionLayer;
                }

                actionLayers.RemoveAt(index);
            }
        }
        */

        public void ResetAliases()
        {
            currentActionLayer = null;
            defaultActionLayer = null;
            recentAppliedLayer = null;
            appliedLayers.Clear();
            if (actionLayers.Count > 0)
            {
                currentActionLayer = actionLayers[0];
                defaultActionLayer = currentActionLayer;
                appliedLayers.Add(defaultActionLayer);
                recentAppliedLayer = defaultActionLayer;
            }
        }

        public void PrepareCompositeLayer()
        {
            defaultActionLayer.CopyLayerReferences(activeCompositeLayer);
            activeCompositeLayer.SyncActions();
        }

        public void ClearCompositeLayerActions()
        {
            // Clear composite ActionLayer references and restore using default layer
            activeCompositeLayer.ClearActions();
        }

        public void RecompileCompositeLayer(Mapper mapper)
        {
            if (appliedLayers.Count == 1)
            {
                // Clear composite ActionLayer references and restore using default layer
                //activeCompositeLayer.ClearActions();
                //PrepareCompositeLayer();

                return;
            }

            // Skip resetting composite layer if switching from base
            //if (appliedLayers.Count > 1)
            //{
            //    //appliedLayers.Remove(currentActionLayer);
            //    appliedLayers.RemoveRange(1, appliedLayers.Count - 1);

            //    // Clear composite ActionLayer references and restore using default layer
            //    activeCompositeLayer.ClearActions();
            //    PrepareCompositeLayer();
            //}

            //ActionLayer lastAppliedLayer = appliedLayers.Last();
            ActionLayer lastAppliedLayer = recentAppliedLayer;

            HashSet<string> tempOverrideIds =
                        lastAppliedLayer.normalActionDict.Keys.ToHashSet();

            IEnumerable<ActionLayer> revLayers =
                appliedLayers.Reverse<ActionLayer>();

            foreach (string mapId in tempOverrideIds)
            {
                ActionLayer usedIdLayer = null;
                MapAction tempBaseAction = null;
                foreach (ActionLayer tempLayer in revLayers)
                {
                    if (tempLayer.normalActionDict.ContainsKey(mapId))
                    {
                        usedIdLayer = tempLayer;
                        tempBaseAction = usedIdLayer.normalActionDict[mapId];
                        break;
                    }
                }

                if (usedIdLayer != null && tempBaseAction != null)
                {
                    MapAction currentMapAction = lastAppliedLayer.normalActionDict[mapId];

                    if (mapId.StartsWith(ACTION_SET_ACTION_PREFIX))
                    {
                        activeCompositeLayer.actionSetActionDict[mapId] = currentMapAction as ButtonMapAction;
                    }
                    else
                    {
                        switch (mapper.BindingDict[mapId].controlType)
                        {
                            case InputBindingMeta.InputControlType.Button:
                                activeCompositeLayer.buttonActionDict[mapId] = currentMapAction as ButtonMapAction;
                                break;
                            case InputBindingMeta.InputControlType.DPad:
                                activeCompositeLayer.dpadActionDict[mapId] = currentMapAction as DPadMapAction;
                                break;
                            case InputBindingMeta.InputControlType.Stick:
                                activeCompositeLayer.stickActionDict[mapId] = currentMapAction as StickMapAction;
                                break;
                            case InputBindingMeta.InputControlType.Gyro:
                                activeCompositeLayer.gyroActionDict[mapId] = currentMapAction as GyroMapAction;
                                break;
                            case InputBindingMeta.InputControlType.Touchpad:
                                activeCompositeLayer.touchpadActionDict[mapId] = currentMapAction as TouchpadMapAction;
                                break;
                            case InputBindingMeta.InputControlType.Trigger:
                                activeCompositeLayer.triggerActionDict[mapId] = currentMapAction as TriggerMapAction;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            // Sync base MapAction Lists and Dicts
            activeCompositeLayer.SyncActions();
        }
    }
}
