using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.ButtonActions;

namespace DS4MapperTest.ViewModels
{
    public class AlwaysOnButtonFuncEditViewModel : ButtonFuncEditViewModel
    {
        public new string InputControlName
        {
            get
            {
                string result = "";
                result = !string.IsNullOrEmpty(action.Name) ? action.Name :
                    $"Action Set {mapper.ActionProfile.CurrentActionSetIndex+1}";
                return result;
            }
        }

        public AlwaysOnButtonFuncEditViewModel(Mapper mapper, ButtonMapAction action) :
            base(mapper, action)
        {
        }

        public new void SwitchLayerAction(ButtonMapAction oldAction, ButtonMapAction newAction, bool copyProps = true)
        {
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            mapper.QueueEvent(() =>
            {
                oldAction.Release(mapper, ignoreReleaseActions: true);
                //int tempInd = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.LayerActions.FindIndex((item) => item == tempAction);
                //if (tempInd >= 0)
                {
                    //mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.LayerActions.RemoveAt(tempInd);
                    //mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.LayerActions.Insert(tempInd, newAction);

                    //oldAction.Release(mapper, ignoreReleaseActions: true);

                    //mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddTouchpadAction(this.action);
                    //newAction.MappingId = oldAction.MappingId;
                    //if (oldAction.Id != MapAction.DEFAULT_UNBOUND_ID)
                    bool exists = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(oldAction);
                    if (exists)
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.ReplaceActionSetButtonAction(oldAction, newAction);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddActionSetButtonMapAction(newAction);
                    }

                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        if (copyProps)
                        {
                            MapAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[oldAction.MappingId];
                            if (MapAction.IsSameType(baseLayerAction, newAction))
                            {
                                newAction.SoftCopyFromParent(baseLayerAction as ButtonMapAction);
                            }
                        }

                        mapper.ActionProfile.CurrentActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.SyncActions();
                        mapper.ActionProfile.CurrentActionSet.ClearCompositeLayerActions();
                        mapper.ActionProfile.CurrentActionSet.PrepareCompositeLayer();
                    }
                }

                resetEvent.Set();
            });

            resetEvent.Wait();
        }
    }
}
