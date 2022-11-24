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
using DS4MapperTest.TriggerActions;
using DS4MapperTest.TouchpadActions;

namespace DS4MapperTest
{
    public class ActionLayer
    {
        /// <summary>
        /// Contains MapAction instances only associated with this ActionLayer
        /// </summary>
        private List<MapAction> layerActions = new List<MapAction>(20);
        public List<MapAction> LayerActions { get => layerActions; set => layerActions = value; }

        private List<MapAction> mappedActions = new List<MapAction>(20);
        public List<MapAction> MappedActions { get => mappedActions; set => mappedActions = value; }

        private int index;
        public int Index { get => index; set => index = value; }

        private string name;

        public string Name { get => name; set => name = value; }

        private string description;
        public string Description { get => description; set => description = value; }

        public Dictionary<string, ButtonMapAction> buttonActionDict = new Dictionary<string, ButtonMapAction>();
        public Dictionary<string, DPadMapAction> dpadActionDict = new Dictionary<string, DPadMapAction>();
        public Dictionary<string, StickMapAction> stickActionDict = new Dictionary<string, StickMapAction>();
        public Dictionary<string, TriggerMapAction> triggerActionDict = new Dictionary<string, TriggerMapAction>();
        public Dictionary<string, TouchpadMapAction> touchpadActionDict = new Dictionary<string, TouchpadMapAction>();
        public Dictionary<string, GyroMapAction> gyroActionDict = new Dictionary<string, GyroMapAction>();
        public Dictionary<string, ButtonMapAction> actionSetActionDict = new Dictionary<string, ButtonMapAction>();

        public Dictionary<string, MapAction> normalActionDict = new Dictionary<string, MapAction>();
        public Dictionary<MapAction, string> reverseActionDict = new Dictionary<MapAction, string>();

        public ActionLayer(int index)
        {
            this.index = index;
        }

        public void SyncActions()
        {
            mappedActions.Clear();
            reverseActionDict.Clear();
            normalActionDict.Clear();

            foreach (KeyValuePair<string, ButtonMapAction> pair in actionSetActionDict)
            {
                mappedActions.Add(pair.Value);
                reverseActionDict.Add(pair.Value, pair.Key);
            }

            foreach (KeyValuePair<string, ButtonMapAction> pair in buttonActionDict)
            {
                mappedActions.Add(pair.Value);
                reverseActionDict.Add(pair.Value, pair.Key);
            }

            foreach (KeyValuePair<string, DPadMapAction> pair in dpadActionDict)
            {
                mappedActions.Add(pair.Value);
                reverseActionDict.Add(pair.Value, pair.Key);
            }

            foreach (KeyValuePair<string, StickMapAction> pair in stickActionDict)
            {
                mappedActions.Add(pair.Value);
                reverseActionDict.Add(pair.Value, pair.Key);
            }

            foreach (KeyValuePair<string, TriggerMapAction> pair in triggerActionDict)
            {
                mappedActions.Add(pair.Value);
                reverseActionDict.Add(pair.Value, pair.Key);
            }

            foreach (KeyValuePair<string, TouchpadMapAction> pair in touchpadActionDict)
            {
                mappedActions.Add(pair.Value);
                reverseActionDict.Add(pair.Value, pair.Key);
            }

            foreach (KeyValuePair<string, GyroMapAction> pair in gyroActionDict)
            {
                mappedActions.Add(pair.Value);
                reverseActionDict.Add(pair.Value, pair.Key);
            }

            foreach(KeyValuePair<MapAction, string> pair in reverseActionDict)
            {
                normalActionDict.Add(pair.Value, pair.Key);
            }
        }

        // Full Release primarily meant to be used when switching
        // ActionSet instances
        public void ReleaseActions(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            foreach (MapAction action in mappedActions)
            {
                action.Release(mapper, resetState, ignoreReleaseActions);
            }
        }

        // Soft Release meant to be used for switching actions in an
        // ActionLayer
        public void SoftReleaseActions(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            foreach (MapAction action in mappedActions)
            {
                if (action.HasLayeredAction || action.ParentAction != null)
                {
                    action.SoftRelease(mapper, action.ParentAction, resetState);
                }
                else
                {
                    action.Release(mapper, resetState, ignoreReleaseActions);
                }
            }
        }

        /// <summary>
        /// Testing function for switching ActionLayer instances.
        /// Need to check mapped actions across ActionLayer instances
        /// to see if the Default layer action is being switched.
        /// Also check for a jump across secondary ActionLayer instances
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="changeLayer"></param>
        /// <param name="resetState"></param>
        public void CompareReleaseActions(Mapper mapper,
            ActionLayer changeLayer,
            bool resetState = true,
            bool ignoreReleaseActions = false)
        {
            foreach (MapAction action in mappedActions)
            {
                //bool foundInputId = reverseActionDict.TryGetValue(action, out string mappedId);
                string mappedId = action.MappingId;
                MapAction changingAction;
                //if (foundInputId &&
                if (changeLayer.normalActionDict.TryGetValue(mappedId, out changingAction))
                {
                    bool hasParentAction = action.ParentAction != null;
                    if (hasParentAction && changingAction == action.ParentAction)
                    {
                        // Reverting back to non-duplicated Default action. Perform SoftRelease
                        action.SoftRelease(mapper, changingAction, resetState);
                    }
                    else if (!hasParentAction && changingAction.ParentAction == action)
                    {
                        // Changing to non-Default ActionLayer with duplicated action. Perform SoftRelease
                        action.SoftRelease(mapper, changingAction, resetState);
                    }
                    else if (changingAction != action)
                    {
                        // Jump across layers or used unique action. Perform Release
                        action.Release(mapper, resetState, ignoreReleaseActions);
                    }
                }
                else
                {
                    // No applicable action in changing ActionLayer found. Perform Release
                    //action.Release(mapper, resetState);
                }
            }
        }

        public void CompareReleaseActions(Mapper mapper, MapAction action,
            MapAction changingAction, bool resetState = true, bool ignoreReleaseActions = false)
        {
            //bool foundInputId = reverseActionDict.TryGetValue(action, out string mappedId);
            //string mappedId = action.MappingId;
            //if (foundInputId &&
            //if (changeLayer.normalActionDict.TryGetValue(mappedId, out changingAction))
            {
                bool hasParentAction = action.ParentAction != null;
                if (hasParentAction && changingAction == action.ParentAction)
                {
                    // Reverting back to non-duplicated Default action. Perform SoftRelease
                    action.SoftRelease(mapper, changingAction, resetState);
                }
                else if (!hasParentAction && changingAction.ParentAction == action)
                {
                    // Changing to non-Default ActionLayer with duplicated action. Perform SoftRelease
                    action.SoftRelease(mapper, changingAction, resetState);
                }
                else if (action != changingAction)
                {
                    if (MapAction.IsSameType(action, changingAction))
                    {
                        // Changing between ActionLayer instances with identical actions. Perform SoftRelease
                        //Trace.WriteLine($"SAME SAME: PERFORMING SOFTRELEASE {changingAction.MappingId} {changingAction.Name}");
                        action.SoftRelease(mapper, changingAction, resetState);
                    }
                    else
                    {
                        // Jump across layers or used unique action. Perform Release
                        action.Release(mapper, resetState, ignoreReleaseActions);
                    }
                }
            }
            //else
            //{
            //    // No applicable action in changing ActionLayer found. Perform Release
            //    //action.Release(mapper, resetState);
            //}
        }

        public void CopyParentStates()
        {
            /*foreach(MapAction action in mappedActions)
            {
                if (action.ParentAction != null)
                {
                    action.StateData = action.ParentAction.StateData;
                }
            }
            */
        }

        public void MergeLayerActions(ActionLayer secondLayer)
        {
            foreach (KeyValuePair<string, ButtonMapAction> pair in actionSetActionDict)
            {
                if (!secondLayer.actionSetActionDict.TryGetValue(pair.Key, out ButtonMapAction _))
                {
                    secondLayer.actionSetActionDict.Add(pair.Key, pair.Value.DuplicateAction());
                }
            }

            foreach (KeyValuePair<string, ButtonMapAction> pair in buttonActionDict)
            {
                if (!secondLayer.buttonActionDict.TryGetValue(pair.Key, out ButtonMapAction _))
                {
                    secondLayer.buttonActionDict.Add(pair.Key, pair.Value.DuplicateAction());
                }
            }

            foreach (KeyValuePair<string, DPadMapAction> pair in dpadActionDict)
            {
                if (!secondLayer.dpadActionDict.TryGetValue(pair.Key, out DPadMapAction _))
                {
                    secondLayer.dpadActionDict.Add(pair.Key, pair.Value.DuplicateAction());
                }
            }

            foreach (KeyValuePair<string, StickMapAction> pair in stickActionDict)
            {
                if (!secondLayer.stickActionDict.TryGetValue(pair.Key, out StickMapAction _))
                {
                    secondLayer.stickActionDict.Add(pair.Key, pair.Value.DuplicateAction());
                }
            }

            foreach (KeyValuePair<string, TriggerMapAction> pair in triggerActionDict)
            {
                if (!secondLayer.triggerActionDict.TryGetValue(pair.Key, out TriggerMapAction _))
                {
                    //secondLayer.triggerActionDict.Add(pair.Key, pair.Value.DuplicateAction());
                }
            }

            foreach (KeyValuePair<string, TouchpadMapAction> pair in touchpadActionDict)
            {
                if (!secondLayer.touchpadActionDict.TryGetValue(pair.Key, out TouchpadMapAction _))
                {
                    //secondLayer.touchpadActionDict.Add(pair.Key, pair.Value.DuplicateAction());
                }
            }

            foreach (KeyValuePair<string, GyroMapAction> pair in gyroActionDict)
            {
                if (!secondLayer.gyroActionDict.TryGetValue(pair.Key, out GyroMapAction _))
                {
                    secondLayer.gyroActionDict.Add(pair.Key, pair.Value.DuplicateAction());
                }
            }
        }

        public void CopyLayerReferences(ActionLayer secondLayer)
        {
            foreach (KeyValuePair<string, ButtonMapAction> pair in actionSetActionDict)
            {
                //if (!secondLayer.actionSetActionDict.TryGetValue(pair.Key, out ButtonMapAction _))
                {
                    secondLayer.actionSetActionDict.Add(pair.Key, pair.Value);
                }
            }

            foreach (KeyValuePair<string, ButtonMapAction> pair in buttonActionDict)
            {
                //if (!secondLayer.buttonActionDict.TryGetValue(pair.Key, out ButtonMapAction _))
                {
                    secondLayer.buttonActionDict.Add(pair.Key, pair.Value);
                }
            }

            foreach (KeyValuePair<string, DPadMapAction> pair in dpadActionDict)
            {
                //if (!secondLayer.dpadActionDict.TryGetValue(pair.Key, out DPadMapAction _))
                {
                    secondLayer.dpadActionDict.Add(pair.Key, pair.Value);
                }
            }

            foreach (KeyValuePair<string, StickMapAction> pair in stickActionDict)
            {
                //if (!secondLayer.stickActionDict.TryGetValue(pair.Key, out StickMapAction _))
                {
                    secondLayer.stickActionDict.Add(pair.Key, pair.Value);
                }
            }

            foreach (KeyValuePair<string, TriggerMapAction> pair in triggerActionDict)
            {
                //if (!secondLayer.stickActionDict.TryGetValue(pair.Key, out TriggerMapAction _))
                {
                    secondLayer.triggerActionDict.Add(pair.Key, pair.Value);
                }
            }

            foreach (KeyValuePair<string, TouchpadMapAction> pair in touchpadActionDict)
            {
                //if (!secondLayer.touchpadActionDict.TryGetValue(pair.Key, out TouchpadMapAction _))
                {
                    secondLayer.touchpadActionDict.Add(pair.Key, pair.Value);
                }
            }

            foreach (KeyValuePair<string, GyroMapAction> pair in gyroActionDict)
            {
                //if (!secondLayer.gyroActionDict.TryGetValue(pair.Key, out GyroMapAction _))
                {
                    secondLayer.gyroActionDict.Add(pair.Key, pair.Value);
                }
            }
        }

        public void ClearActions()
        {
            mappedActions.Clear();
            reverseActionDict.Clear();
            normalActionDict.Clear();

            actionSetActionDict.Clear();
            buttonActionDict.Clear();
            dpadActionDict.Clear();
            stickActionDict.Clear();
            triggerActionDict.Clear();
            touchpadActionDict.Clear();
            gyroActionDict.Clear();
        }

        public void ReplaceButtonAction(ButtonMapAction oldAction, ButtonMapAction action)
        {
            string mapId = oldAction.MappingId;
            int ind = layerActions.FindIndex((item) => item == oldAction);
            int mappedInd = -1;
            if (ind >= 0)
            {
                ButtonMapAction tempAction = layerActions[ind] as ButtonMapAction;
                layerActions.RemoveAt(ind);
                layerActions.Insert(ind, action);

                normalActionDict.Remove(mapId);
                reverseActionDict.Remove(tempAction);

                mappedInd = mappedActions.FindIndex((item) => (item == tempAction));
                if (mappedInd != -1)
                {
                    mappedActions.RemoveAt(mappedInd);
                    mappedActions.Insert(mappedInd, action);
                }
                else
                {
                    mappedActions.Add(action);
                }
            }
            else
            {
                layerActions.Add(action);
                mappedActions.Add(action);
            }

            buttonActionDict[mapId] = action;

            normalActionDict.Add(mapId, action);
            reverseActionDict.Add(action, mapId);
        }

        public void AddButtonMapAction(ButtonMapAction action)
        {
            layerActions.Add(action);
            //mappedActions.Add(action);
            buttonActionDict[action.MappingId] = action;

            normalActionDict[action.MappingId] = action;
            reverseActionDict[action] = action.MappingId;
        }

        public void ReplaceTriggerAction(TriggerMapAction oldAction, TriggerMapAction action)
        {
            string mapId = oldAction.MappingId;
            int ind = layerActions.FindIndex((item) => item == oldAction);
            int mappedInd = -1;
            if (ind >= 0)
            {
                TriggerMapAction tempAction = layerActions[ind] as TriggerMapAction;
                layerActions.RemoveAt(ind);
                layerActions.Insert(ind, action);

                normalActionDict.Remove(mapId);
                reverseActionDict.Remove(tempAction);

                mappedInd = mappedActions.FindIndex((item) => (item == tempAction));
                if (mappedInd != -1)
                {
                    mappedActions.RemoveAt(mappedInd);
                    mappedActions.Insert(mappedInd, action);
                }
                else
                {
                    mappedActions.Add(action);
                }
            }
            else
            {
                layerActions.Add(action);
                mappedActions.Add(action);
            }

            triggerActionDict[mapId] = action;

            normalActionDict.Add(mapId, action);
            reverseActionDict.Add(action, mapId);
        }

        public void AddTriggerAction(TriggerMapAction action)
        {
            layerActions.Add(action);
            //mappedActions.Add(action);
            triggerActionDict[action.MappingId] = action;

            normalActionDict[action.MappingId] = action;
            reverseActionDict[action] = action.MappingId;
        }

        public void ReplaceStickAction(StickMapAction oldAction, StickMapAction action)
        {
            string mapId = oldAction.MappingId;
            int ind = layerActions.FindIndex((item) => item == oldAction);
            int mappedInd = -1;
            if (ind >= 0)
            {
                StickMapAction tempAction = layerActions[ind] as StickMapAction;
                layerActions.RemoveAt(ind);
                layerActions.Insert(ind, action);

                normalActionDict.Remove(mapId);
                reverseActionDict.Remove(tempAction);

                mappedInd = mappedActions.FindIndex((item) => (item == tempAction));
                if (mappedInd != -1)
                {
                    mappedActions.RemoveAt(mappedInd);
                    mappedActions.Insert(mappedInd, action);
                }
                else
                {
                    mappedActions.Add(action);
                }
            }
            else
            {
                layerActions.Add(action);
                mappedActions.Add(action);
            }

            stickActionDict[mapId] = action;

            normalActionDict.Add(mapId, action);
            reverseActionDict.Add(action, mapId);
        }

        public void AddStickAction(StickMapAction action)
        {
            layerActions.Add(action);
            //mappedActions.Add(action);
            stickActionDict[action.MappingId] = action;

            normalActionDict[action.MappingId] = action;
            reverseActionDict[action] = action.MappingId;
        }

        public void ReplaceDPadAction(DPadMapAction oldAction, DPadMapAction action)
        {
            string mapId = oldAction.MappingId;
            int ind = layerActions.FindIndex((item) => item == oldAction);
            int mappedInd = -1;
            if (ind >= 0)
            {
                DPadMapAction tempAction = layerActions[ind] as DPadMapAction;
                layerActions.RemoveAt(ind);
                layerActions.Insert(ind, action);

                normalActionDict.Remove(mapId);
                reverseActionDict.Remove(tempAction);

                mappedInd = mappedActions.FindIndex((item) => (item == tempAction));
                if (mappedInd != -1)
                {
                    mappedActions.RemoveAt(mappedInd);
                    mappedActions.Insert(mappedInd, action);
                }
                else
                {
                    mappedActions.Add(action);
                }
            }
            else
            {
                layerActions.Add(action);
                mappedActions.Add(action);
            }

            dpadActionDict[mapId] = action;

            normalActionDict.Add(mapId, action);
            reverseActionDict.Add(action, mapId);
        }

        public void AddDPadAction(DPadMapAction action)
        {
            layerActions.Add(action);
            //mappedActions.Add(action);
            dpadActionDict[action.MappingId] = action;

            normalActionDict[action.MappingId] = action;
            reverseActionDict[action] = action.MappingId;
        }

        public void ReplaceGyroAction(GyroMapAction oldAction, GyroMapAction action)
        {
            string mapId = oldAction.MappingId;
            int ind = layerActions.FindIndex((item) => item == oldAction);
            int mappedInd = -1;
            if (ind >= 0)
            {
                GyroMapAction tempAction = layerActions[ind] as GyroMapAction;
                layerActions.RemoveAt(ind);
                layerActions.Insert(ind, action);

                normalActionDict.Remove(mapId);
                reverseActionDict.Remove(tempAction);

                mappedInd = mappedActions.FindIndex((item) => (item == tempAction));
                if (mappedInd != -1)
                {
                    mappedActions.RemoveAt(mappedInd);
                    mappedActions.Insert(mappedInd, action);
                }
                else
                {
                    mappedActions.Add(action);
                }
            }
            else
            {
                layerActions.Add(action);
                mappedActions.Add(action);
            }

            gyroActionDict[mapId] = action;

            normalActionDict.Add(mapId, action);
            reverseActionDict.Add(action, mapId);
        }

        public void AddGyroAction(GyroMapAction action)
        {
            layerActions.Add(action);
            //mappedActions.Add(action);
            gyroActionDict[action.MappingId] = action;

            normalActionDict[action.MappingId] = action;
            reverseActionDict[action] = action.MappingId;
        }

        public void ReplaceTouchpadAction(TouchpadMapAction oldAction, TouchpadMapAction action)
        {
            string mapId = oldAction.MappingId;
            int ind = layerActions.FindIndex((item) => item == oldAction);
            int mappedInd = -1;
            if (ind >= 0)
            {
                TouchpadMapAction tempAction = layerActions[ind] as TouchpadMapAction;
                layerActions.RemoveAt(ind);
                layerActions.Insert(ind, action);

                normalActionDict.Remove(mapId);
                reverseActionDict.Remove(tempAction);

                touchpadActionDict[mapId] = action;
                mappedInd = mappedActions.FindIndex((item) => (item == tempAction));

                if (mappedInd != -1)
                {
                    mappedActions.RemoveAt(mappedInd);
                    mappedActions.Insert(mappedInd, action);
                }
                else
                {
                    mappedActions.Add(action);
                }

                normalActionDict.Add(mapId, action);
                reverseActionDict.Add(action, mapId);
            }
        }

        public void AddTouchpadAction(TouchpadMapAction action)
        {
            layerActions.Add(action);
            //mappedActions.Add(action);
            touchpadActionDict[action.MappingId] = action;

            normalActionDict[action.MappingId] = action;
            reverseActionDict[action] = action.MappingId;
        }

        public int FindNextAvailableId()
        {
            int result = 0;
            HashSet<int> currentIds = layerActions.Select((item) => item.Id).ToHashSet();
            bool unique = false;
            for (int i = 0; i < 1000 && !unique; i++)
            {
                if (!currentIds.Contains(i))
                {
                    result = i;
                    unique = true;
                }
            }

            return result;
        }
    }
}
