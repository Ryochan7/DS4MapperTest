using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.MapperUtil
{
    public class OutputActionData
    {
        public enum ActionType : uint
        {
            Empty,
            Keyboard,
            RelativeMouse,
            AbsoluteMouse,
            MouseButton,
            MouseWheel,
            GamepadControl,
            SwitchSet,
            SwitchActionLayer,
            ApplyActionLayer,
            RemoveActionLayer,
            HoldActionLayer,
            Wait,
            CycleStep,
        }

        public enum SetChangeCondition : uint
        {
            None,
            Pressed,
            Released,
        }

        public enum ActionLayerChangeCondition : uint
        {
            None,
            Pressed,
            Released,
        }

        private ActionType outputType;

        public struct NotchResultData
        {
            public double notches;
            public bool useAnalog;
        }

        public enum RelativeMouseDir
        {
            MouseUp,
            MouseDown,
            MouseLeft,
            MouseRight,
        }

        public enum CycleStepActionType
        {
            None,
            Forward,
            Backward,
            MoveToFront,
            MoveToEnd,
            MoveToStep,
        }

        public struct CycleStepAction
        {
            public string cycleId;
            public CycleStepActionType stepActionType;
            public int stepNum;
        }

        // internal codes
        private int outputCode;
        // Native event code. Used primarily for keyboard translation
        private uint outputCodeAlias;
        // Hold string as stored in the profile
        private string outputCodeProfileStr;

        private JoypadActionCodes joypadCode;
        private bool negative;

        private JoypadAxesCodes axisCode;
        private StickActionCodes stickCode;
        private DPadActionCodes dpadCode;
        private JoypadActionCodeMapping outputPadMap = new JoypadActionCodeMapping();
        public bool repeat = true;
        public bool activatedEvent = false;
        public bool firstRun = true;
        // Used for HoldActionLayer to skip releasing an action on ActionLayer change
        public bool skipRelease = false;
        public bool waitForRelease = false;
        private int changeToSet = -1;
        private SetChangeCondition changeCondition;
        public bool checkTick;
        public RelativeMouseDir mouseDir;
        public CycleStepAction cycleStepAct;

        public bool useNotches;
        public double currentNotches;
        public bool processOutput;

        // Flag to have process stop processing output actions in an ActionFunc sequence
        public bool breakSequence;
        private bool tickTimerActive;
        private const int DEFAULT_CHANGE_LAYER_ID = -1;
        private int changeToLayer = DEFAULT_CHANGE_LAYER_ID;
        // TODO: Why is this here again? Possibly remove?
        private int fromProfileChangeLayer = DEFAULT_CHANGE_LAYER_ID;
        private ActionLayerChangeCondition layerChangeCondition;

        private Stopwatch elapsed = new Stopwatch();
        // Base line duration for an action
        public const int DEFAULT_TICK_DURATION_MS = 100; // express in ms
        private int durationMs = DEFAULT_TICK_DURATION_MS;
        public int DurationMs
        {
            get => durationMs;
            set
            {
                durationMs = value;
                effectiveDurationMs = value;
            }
        }

        // Adjustable duration to use for analog actions. Should mostly
        // be the same value as durationMs
        private int effectiveDurationMs = 100;
        public int EffectiveDurationMs
        {
            get => effectiveDurationMs;
            set => effectiveDurationMs = value;
        }

        public OutputActionDataBindSettings extraSettings = new OutputActionDataBindSettings();

        public ActionType OutputType { get => outputType; set => outputType = value; }
        public int OutputCode { get => outputCode; set => outputCode = value; }
        public uint OutputCodeAlias { get => outputCodeAlias; set => outputCodeAlias = value; }
        public string OutputCodeStr { get => outputCodeProfileStr; set => outputCodeProfileStr = value; }
        public JoypadActionCodes JoypadCode { get => joypadCode; set => joypadCode = value; }
        public bool Negative { get => negative; set => negative = value; }
        public JoypadAxesCodes AxisCode { get => axisCode; set => axisCode = value; }
        public StickActionCodes StickCode { get => stickCode; set => stickCode = value; }
        public DPadActionCodes DpadCode { get => dpadCode; set => dpadCode = value; }
        public JoypadActionCodeMapping OutputPadMap
        {
            get => outputPadMap; set => outputPadMap = value;
        }

        public int ChangeToSet { get => changeToSet; set => changeToSet = value; }
        public SetChangeCondition ChangeCondition { get => changeCondition; set => changeCondition = value; }
        public bool CheckTick { get => checkTick; set => checkTick = value; }
        public Stopwatch Elapsed { get => elapsed; }
        public int ChangeToLayer { get => changeToLayer; set => changeToLayer = value; }
        public int FromProfileChangeLayer
        {
            get => fromProfileChangeLayer;
            set
            {
                fromProfileChangeLayer = value;
                changeToLayer = value;
            }
        }

        public ActionLayerChangeCondition LayerChangeCondition
        {
            get => layerChangeCondition;
            set => layerChangeCondition = value;
        }

        public OutputActionData(ActionType type, int code, int codeAlias = 0)
        {
            outputType = type;
            outputCode = code;
            if (codeAlias == 0)
            {
                outputCodeAlias = (uint)code;
            }
            else
            {
                outputCodeAlias = (uint)codeAlias;
            }

            if (type == ActionType.Keyboard)
            {
                repeat = false;
            }
            else if (type == ActionType.RelativeMouse)
            {
                if (Enum.TryParse(code.ToString(), out RelativeMouseDir tempDir))
                {
                    mouseDir = tempDir;
                }
            }

            processOutput = UseProcessForAction();
        }

        public OutputActionData(ActionType type, JoypadActionCodes actionCode, bool negative = false)
        {
            outputType = type;
            if (type == ActionType.GamepadControl)
            {
                joypadCode = actionCode;
                this.negative = negative;
            }

            processOutput = UseProcessForAction();
        }

        public OutputActionData(ActionType type, JoypadAxesCodes axisCode)
        {
            outputType = type;
            if (type == ActionType.GamepadControl)
            {
                this.axisCode = axisCode;
                outputPadMap.type = JoypadActionCodeMapping.ControlType.Axis;
                outputPadMap.outputValue.axisCode = axisCode;
            }

            processOutput = UseProcessForAction();
        }

        public OutputActionData(ActionType type, StickActionCodes stickCode)
        {
            outputType = type;
            if (type == ActionType.GamepadControl)
            {
                this.stickCode = stickCode;
                outputPadMap.type = JoypadActionCodeMapping.ControlType.Stick;
                outputPadMap.outputValue.stickCode = stickCode;
            }

            processOutput = UseProcessForAction();
        }

        public OutputActionData(ActionType type, DPadActionCodes dpadCode)
        {
            outputType = type;
            if (type == ActionType.GamepadControl)
            {
                this.dpadCode = dpadCode;
                outputPadMap.type = JoypadActionCodeMapping.ControlType.DPad;
                outputPadMap.outputValue.dpadCode = dpadCode;
            }

            processOutput = UseProcessForAction();
        }

        public OutputActionData(OutputActionData sourceData)
        {
            outputType = sourceData.outputType;
            outputCode = sourceData.outputCode;
            outputCodeAlias = sourceData.outputCodeAlias;
            outputCodeProfileStr = sourceData.outputCodeProfileStr;
            dpadCode = sourceData.dpadCode;
            stickCode = sourceData.stickCode;
            joypadCode = sourceData.joypadCode;
            negative = sourceData.negative;
            repeat = sourceData.repeat;
            changeToSet = sourceData.changeToSet;
            ChangeCondition = sourceData.changeCondition;

            changeToLayer = sourceData.changeToLayer;
            layerChangeCondition = sourceData.layerChangeCondition;
            fromProfileChangeLayer = sourceData.fromProfileChangeLayer;

            durationMs = sourceData.durationMs;
            effectiveDurationMs = sourceData.effectiveDurationMs;
            outputPadMap = new JoypadActionCodeMapping(sourceData.outputPadMap);
            useNotches = sourceData.useNotches;
            processOutput = sourceData.processOutput;
        }

        public void Prepare(ActionType type, int code)
        {
            outputType = type;
            outputCode = code;

            if (type == ActionType.Keyboard)
            {
                repeat = false;
            }
            else if (type == ActionType.RelativeMouse)
            {
                if (Enum.TryParse(code.ToString(), out RelativeMouseDir tempDir))
                {
                    mouseDir = tempDir;
                }
            }

            processOutput = UseProcessForAction();
        }

        private bool UseProcessForAction()
        {
            bool process = false;
            switch(outputType)
            {
                case ActionType.Wait:
                    process = true;
                    break;
                default:
                    break;
            }
            return process;
        }

        public bool ProcessAction()
        {
            bool active = activatedEvent;
            switch(outputType)
            {
                case ActionType.Wait:
                    {
                        if (!tickTimerActive)
                        {
                            elapsed.Restart();
                            tickTimerActive = true;
                        }

                        if (activatedEvent)
                        {
                            breakSequence = false;
                        }
                        else if (elapsed.ElapsedMilliseconds >= effectiveDurationMs)
                        {
                            elapsed.Reset();
                            active = true;
                            activatedEvent = true;
                            breakSequence = false;
                        }
                        else
                        {
                            active = false;
                            activatedEvent = false;
                            breakSequence = true;
                        }
                    }

                    break;
                default:
                    break;
            }
            return active;
        }

        public bool ProcessTick()
        {
            bool active = activatedEvent;
            //bool result = false;
            switch(outputType)
            {
                case ActionType.MouseWheel:
                {
                    if (!tickTimerActive)
                    {
                        elapsed.Restart();
                        tickTimerActive = true;
                    }

                    if (!firstRun)
                    {
                        if (elapsed.ElapsedMilliseconds >= effectiveDurationMs)
                        {
                            elapsed.Restart();
                            active = true;
                        }
                        else
                        {
                            active = false;
                        }
                    }
                    else
                    {
                        // Process first event immediately
                        active = true;
                        effectiveDurationMs = durationMs;
                    }

                    break;
                }
                /*case ActionType.Wait:
                {
                    if (!tickTimerActive)
                    {
                        elapsed.Restart();
                        tickTimerActive = true;
                    }

                    if (elapsed.ElapsedMilliseconds >= effectiveDurationMs)
                    {
                        elapsed.Reset();
                        active = true;
                        breakSequence = false;
                    }
                    else
                    {
                        active = false;
                        breakSequence = true;
                    }

                    break;
                }
                */
                default: break;
            }

            activatedEvent = !active;
            return active;
        }

        public NotchResultData ProcessNotches()
        {
            NotchResultData result = new NotchResultData();
            switch (outputType)
            {
                case ActionType.Keyboard:
                case ActionType.MouseButton:
                case ActionType.GamepadControl:
                    result.notches = 1.0;
                    currentNotches -= 1.0;
                    break;
                case ActionType.MouseWheel:
                    result.notches = (int)currentNotches;
                    result.useAnalog = true;
                    currentNotches = currentNotches - result.notches;
                    break;
                default:
                    break;
            }

            return result;
        }

        public void ComputeActionFlags()
        {
            processOutput = UseProcessForAction();
        }

        public void Release()
        {
            if (elapsed.IsRunning)
            {
                elapsed.Reset();
            }

            tickTimerActive = false;
            activatedEvent = false;
            firstRun = true;
            breakSequence = false;
            effectiveDurationMs = durationMs;
            skipRelease = false;
            waitForRelease = false;
            currentNotches = 0.0;
        }

        public void Reset()
        {
            Release();

            outputType = ActionType.Empty;
            outputCode = 0;
            outputCodeAlias = 0;
            outputCodeProfileStr = "";
            changeToLayer = -1;
            checkTick = false;
        }

        public string Describe(Mapper mapper)
        {
            string result = "";
            switch (outputType)
            {
                case ActionType.Keyboard:
                    //result = OutputDataAliasUtil.GetStringForKeyboardKey(outputCodeAlias);
                    result = OutputDataAliasUtil.GetDisplayStringForKeyboardKey((uint)outputCode);
                    //result = OutputCodeStr;
                    break;
                case ActionType.MouseButton:
                    result = OutputDataAliasUtil.GetStringForMouseButton(outputCode);
                    break;
                case ActionType.MouseWheel:
                    result = OutputDataAliasUtil.GetStringForMouseWheelBtn(outputCode);
                    break;
                case ActionType.RelativeMouse:
                    result = mouseDir.ToString();
                    break;
                case ActionType.GamepadControl:
                    result = OutputDataAliasUtil.GetStringForX360GamepadCode(joypadCode);
                    break;
                case ActionType.SwitchSet:
                    {
                        string tempSetName = $"{ChangeToSet}";
                        if (changeToSet >= 0 && changeToSet < mapper.ActionProfile.ActionSets.Count)
                        {
                            ActionSet tempSet = mapper.ActionProfile.ActionSets[changeToSet];
                            if (tempSet != null)
                            {
                                tempSetName = tempSet.Name;
                            }
                        }

                        result = $"Switch Set ({tempSetName})";
                    }

                    break;
                case ActionType.HoldActionLayer:
                    {
                        string tempLayerName = $"{changeToLayer}";
                        if (changeToLayer >= 0 && changeToLayer < mapper.ActionProfile.CurrentActionSet.ActionLayers.Count)
                        {
                            ActionLayer tempLayer = mapper.ActionProfile.CurrentActionSet.ActionLayers[changeToLayer];
                            if (tempLayer != null)
                            {
                                tempLayerName = tempLayer.Name;
                            }
                        }

                        result = $"Hold Layer ({tempLayerName})";
                    }

                    break;
                case ActionType.ApplyActionLayer:
                    {
                        string tempLayerName = $"{changeToLayer}";
                        if (changeToLayer >= 0 && changeToLayer < mapper.ActionProfile.CurrentActionSet.ActionLayers.Count)
                        {
                            ActionLayer tempLayer = mapper.ActionProfile.CurrentActionSet.ActionLayers[changeToLayer];
                            if (tempLayer != null)
                            {
                                tempLayerName = tempLayer.Name;
                            }
                        }

                        result = $"Apply Layer ({tempLayerName})";
                    }

                    break;
                case ActionType.RemoveActionLayer:
                    {
                        /*string tempLayerName = $"{changeToLayer}";
                        if (changeToLayer >= 0 && changeToLayer < mapper.ActionProfile.CurrentActionSet.ActionLayers.Count)
                        {
                            ActionLayer tempLayer = mapper.ActionProfile.CurrentActionSet.ActionLayers[changeToLayer];
                            if (tempLayer != null)
                            {
                                tempLayerName = tempLayer.Name;
                            }
                        }

                        result = $"Remove Layer ({tempLayerName})";
                        */
                        result = $"Remove Layer";
                    }

                    break;
                case ActionType.SwitchActionLayer:
                    {
                        string tempLayerName = $"{changeToLayer}";
                        if (changeToLayer >= 0 && changeToLayer < mapper.ActionProfile.CurrentActionSet.ActionLayers.Count)
                        {
                            ActionLayer tempLayer = mapper.ActionProfile.CurrentActionSet.ActionLayers[changeToLayer];
                            if (tempLayer != null)
                            {
                                tempLayerName = tempLayer.Name;
                            }
                        }

                        result = $"Switch Layer ({tempLayerName})";
                    }

                    break;
                case ActionType.Wait:
                    result = $"Wait {durationMs}";
                    break;
                case ActionType.Empty:
                    result = "Empty";
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
