using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;

namespace DS4MapperUnitTests
{
    public abstract class BindingHelperBase
    {
        protected TestMapper mapper;

        protected void FillMappingProfileInitialData(Profile tempProfile,
            List<ProfileActionsMapping> tempMappings)
        {
            // Populate ActionLayer dicts with default no action elements
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                int layerIndex = 0;
                foreach (ActionLayer layer in set.ActionLayers)
                {
                    if (layerIndex == 0)
                    {
                        foreach (KeyValuePair<string, InputBindingMeta> tempMeta in mapper.BindingDict)
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
                                        if (mapper.KnownStickDefinitions.TryGetValue(tempMeta.Key,
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
                                        if (mapper.KnownTriggerDefinitions.TryGetValue(tempMeta.Key,
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
                                        if (mapper.KnownTouchpadDefinitions.TryGetValue(tempMeta.Key,
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
                                        if (mapper.KnownGyroSensDefinitions.TryGetValue(tempMeta.Key,
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
                                if (mapper.BindingDict.TryGetValue(layerMapping.InputBinding, out InputBindingMeta tempBind))
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
                                                if (mapper.KnownStickDefinitions.TryGetValue(tempBind.id,
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
                                                if (mapper.KnownTriggerDefinitions.TryGetValue(tempBind.id, out TriggerDefinition tempDef))
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
                                                if (mapper.KnownTouchpadDefinitions.TryGetValue(tempBind.id, out TouchpadDefinition tempDef))
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
                                                if (mapper.KnownGyroSensDefinitions.TryGetValue(tempBind.id, out GyroSensDefinition tempDef))
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
        }

        protected void SyncActionData(Profile tempProfile)
        {
            // Compile convenience List for MapActions instances in layers
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                ActionLayer parentLayer = set.DefaultActionLayer;
                foreach (ActionLayer layer in set.ActionLayers)
                {
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
        }
    }
}
