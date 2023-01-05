using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.ButtonActions
{
    public class ButtonAction : ButtonMapAction
    {
        public const string ACTION_TYPE_NAME = "ButtonAction";

        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string FUNCTIONS = "Functions";
        }

        private class PrivateStateData
        {
        }

        //private List<OutputActionData> outputActions =
        //    new List<OutputActionData>();
        //private OutputActionDataEnumerator outputActionEnumerator;

        protected ButtonAction parentButtonAct;
        protected bool useParentActions;
        public bool UseParentActions => useParentActions;
        // Hold reference to usable PrivateStateData instance
        private PrivateStateData privateState;

        //private NormalPressFunc pressFunc;
        private List<OutputActionData> activeActions = new List<OutputActionData>();
        protected List<ActionFunc> actionFuncs = new List<ActionFunc>();
        protected List<ActionFunc> actionFuncCandidates = new List<ActionFunc>();

        // Cleared at start of event loop. Populated during event loop
        private List<ActionFunc> activeFuns = new List<ActionFunc>();
        private List<ActionFunc> releaseFuns = new List<ActionFunc>();
        private List<ActionFunc> distanceFuns = new List<ActionFunc>();
        private List<int> removeFuncsCandiates = new List<int>();
        //private List<ActionFunc> tempReleaseFuncs = new List<ActionFunc>();

        private List<ActionFunc> usedFuncList;

        protected event EventHandler ActionFuncsUpdated;
        private event EventHandler<NotifyPropertyChangeArgs> NotifyPropertyChanged;

        public List<ActionFunc> ReleaseFuns { get => releaseFuns; }
        public List<ActionFunc> ActionFuncs { get => actionFuncs; }

        public override double ButtonDistance
        {
            get
            {
                double result = 0.0;
                if (status) result = 1.0;
                return result;
            }
        }

        public override double AxisUnit
        {
            get
            {
                double result = 0.0;
                if (status) result = 1.0;
                return result;
            }
        }

        // Specify the input state of the button
        protected bool status;

        public ButtonAction()
        {
            privateState = new PrivateStateData();
            actionTypeName = ACTION_TYPE_NAME;
        }

        protected ButtonAction(ButtonAction parentAction)
        {
            if (parentAction != null)
            {
                this.CopyBaseProps(parentAction);
                //parentAction.CopyBaseProps(this);

                actionFuncs.AddRange(parentAction.actionFuncs);
                parentAction.hasLayeredAction = true;
                mappingId = parentAction.mappingId;

                this.parentAction = parentAction;
                this.parentButtonAct = parentAction;
                useParentActions = true;

                privateState = parentAction.privateState;
            }

            actionTypeName = ACTION_TYPE_NAME;
        }

        public ButtonAction(ActionFunc actionFunc)
        {
            actionFuncs.Add(actionFunc);
            privateState = new PrivateStateData();

            actionTypeName = ACTION_TYPE_NAME;
        }

        public ButtonAction(OutputActionData outputAction)
        {
            //outputActions.Add(outputAction);
            //outputActionEnumerator = new OutputActionDataEnumerator(outputActions);

            NormalPressFunc pressFunc = new NormalPressFunc(outputAction);
            actionFuncs.Add(pressFunc);
            privateState = new PrivateStateData();
            //actionFuncCandidates.Add(pressFunc);

            actionTypeName = ACTION_TYPE_NAME;
        }

        public ButtonAction(IEnumerable<OutputActionData> outputActions)
        {
            //this.outputActions.AddRange(outputActions);
            //outputActionEnumerator =
            //    new OutputActionDataEnumerator(this.outputActions);

            NormalPressFunc pressFunc = new NormalPressFunc(outputActions);
            actionFuncs.Add(pressFunc);
            privateState = new PrivateStateData();
            //actionFuncCandidates.Add(pressFunc);

            actionTypeName = ACTION_TYPE_NAME;
        }

        public override void Prepare(Mapper mapper, bool status, bool alterState = true)
        {
            //if (status != active)
            if (this.status != status)
            {
                if (status)
                {
                    //if (useParentActions)
                    //{
                    //    usedFuncList = parentButtonAct.actionFuncs;
                    //}
                    //else
                    {
                        usedFuncList = actionFuncs;
                    }

                    actionFuncCandidates.AddRange(usedFuncList);
                    //actionFuncCandidates.AddRange(actionFuncs);
                    if (alterState)
                    {
                        stateData.elapsed.Restart();
                    }
                }

                this.status = status;
                if (alterState)
                {
                    stateData.wasActive = stateData.state;
                    stateData.state = status;
                    stateData.axisNormValue = status ? 1.0 : 0.0;
                }
                /*if (alterState)
                {
                    stateData.wasActive = stateData.state;
                    stateData.state = status;
                    stateData.axisValue = status ? 1.0 : 0.0;
                }
                */

                active = true;
                activeEvent = true;
            }
        }

        private bool interruptFound = false;
        public override void Event(Mapper mapper)
        {
            if (active)
            {
                if (status)
                {
                    // Activate current actions
                    /*foreach(ActionFunc func in activeFuns)
                    {
                        foreach (OutputActionData action in func.OutputActions)
                        {
                            mapper.RunEventFromButton(action, status);
                        }
                    }
                    */

                    /*foreach(OutputActionData action in activeActions)
                    {
                        mapper.RunEventFromButton(action, status);
                    }
                    */

                    // Check for new function candidates to add
                    int i = 0;
                    bool removed = false;
                    bool distanceNonInterruptable = false;
                    bool pressNonInterruptable = false;
                    distanceFuns.Clear();
                    double distancePercent = 0.0;
                    //ActionFuncEnumerator funcEnumerator =
                    //        new ActionFuncEnumerator(actionFuncCandidates);
                    //funcEnumerator.MoveToEnd();
                    //while (funcEnumerator.MovePrevious())
                    foreach (ActionFunc func in actionFuncCandidates)
                    {
                        //ActionFunc func = funcEnumerator.Current;
                        func.Prepare(mapper, true, stateData);
                        if (func.onRelease)
                        {
                            releaseFuns.Add(func);
                            removeFuncsCandiates.Add(i);
                            removed = true;
                        }
                        else if (func.onDistance)
                        {
                            distanceFuns.Add(func);
                            if (!func.interruptable && func.active)
                            {
                                distanceNonInterruptable = true;
                            }

                            if (func.distance > distancePercent)
                            {
                                distancePercent = func.distance;
                            }
                        }
                        else if (func.active)
                        {
                            if (func.canPressInterrupt)
                            {
                                pressNonInterruptable = true;
                            }

                            activeFuns.Add(func);
                            removeFuncsCandiates.Add(i);
                            removed = true;
                        }

                        //if (func.canPressInterrupt)
                        //{
                        //    interruptFound = true;
                        //}

                        i++;
                    }

                    if (removed)
                    {
                        removeFuncsCandiates.Reverse();
                        foreach (int index in removeFuncsCandiates)
                        {
                            actionFuncCandidates.RemoveAt(index);
                        }

                        removeFuncsCandiates.Clear();
                    }

                    i = 0;
                    removed = false;

                    // Press Interrupt func found in sequence. Check current steps
                    //if (interruptFound)
                    //{
                    //    ActionFuncEnumerator funcEnumerator =
                    //            new ActionFuncEnumerator(actionFuncCandidates);
                    //    funcEnumerator.MoveToEnd();
                    //    while (funcEnumerator.MovePrevious())
                    //    {
                    //        ActionFunc func = funcEnumerator.Current;
                    //        func.Prepare(mapper, status);
                    //        if (func.active && func.canPressInterrupt)
                    //        {
                    //            pressNonInterruptable = true;
                    //        }
                    //    }
                    //}

                    // Change DistanceFunc action states before other ActionFunc instances
                    if (distanceFuns.Count > 0)
                    {
                        foreach (ActionFunc func in distanceFuns)
                        {
                            bool shouldInterrupt = func.interruptable && func.distance < distancePercent;
                            if (func.active && !shouldInterrupt)
                            {
                                foreach (OutputActionData action in func.OutputActions)
                                {
                                    //Console.WriteLine("JAMIES CRYING");
                                    if (!action.checkTick)
                                    {
                                        if (action.processOutput)
                                        {
                                            action.ProcessAction();
                                        }

                                        if (action.breakSequence) break;
                                        mapper.RunEventFromButton(action, status);
                                        action.firstRun = false;
                                    }
                                    else
                                    {
                                        WrapTickProcess(action);
                                        if (action.ProcessTick())
                                        {
                                            mapper.RunEventFromButton(action, status);
                                            action.firstRun = false;
                                            action.EffectiveDurationMs = (int)(action.DurationMs / ButtonDistance);
                                        }
                                        else if (action.breakSequence)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else if ((func.active && shouldInterrupt) ||
                                func.finished)
                            {
                                OutputActionDataEnumerator activeActionsEnumerator =
                                    new OutputActionDataEnumerator(func.OutputActions);
                                activeActionsEnumerator.MoveToEnd();
                                while (activeActionsEnumerator.MovePrevious())
                                {
                                    OutputActionData action = activeActionsEnumerator.Current;
                                    if (action.activatedEvent)
                                    {
                                        mapper.RunEventFromButton(action, status);
                                        action.Release();
                                        //if (action.checkTick) action.Release();
                                    }

                                    action.firstRun = true;
                                }
                                /*foreach (OutputActionData action in func.OutputActions)
                                {
                                    activeActions.Remove(action);
                                    if (action.activatedEvent)
                                    {
                                        mapper.RunEventFromButton(action, false);
                                    }

                                    action.firstRun = false;
                                }
                                */


                                func.Release(mapper);
                                removed = true;
                            }
                        }
                    }

                    i = 0;
                    removed = false;
                    foreach (ActionFunc func in activeFuns)
                    {
                        //func.Prepare(mapper, true, stateData);
                        func.Event(mapper, stateData);
                        bool shouldInterrupt = func.interruptable && pressNonInterruptable;
                        bool outputStatus = func.outputActive;
                        if (func.active && !shouldInterrupt)
                        {
                            foreach (OutputActionData action in func.OutputActions)
                            {
                                //Console.WriteLine("JAMIES CRYING");

                                //if (useNotches)
                                //{
                                //    WrapNotchesProcess(action);
                                //    if (!action.activatedEvent && action.currentNotches >= 1.0)
                                //    {
                                //        OutputActionData.NotchResultData notchData = action.ProcessNotches();
                                //        if (notchData.useAnalog)
                                //        {
                                //            mapper.RunEventFromAnalog(action, true, notchData.notches, 1.0);
                                //        }
                                //        else
                                //        {
                                //            mapper.RunEventFromButton(action, true);
                                //        }
                                //    }
                                //    else if (action.activatedEvent)
                                //    {
                                //        mapper.RunEventFromButton(action, false);
                                //    }

                                //    action.firstRun = false;
                                //    //action.activatedEvent = action.currentNotches != 0.0;
                                //}
                                if (!action.checkTick)
                                {
                                    if (action.processOutput) action.ProcessAction();
                                    if (action.breakSequence) break;

                                    if (processAction)
                                    {
                                        ProcessAction(mapper, outputStatus, action);
                                    }
                                    else if (analog)
                                    {
                                        mapper.RunEventFromAnalog(action, outputStatus, ButtonDistance, AxisUnit);
                                    }
                                    else
                                    {
                                        mapper.RunEventFromButton(action, outputStatus);
                                    }

                                    action.firstRun = false;
                                }
                                else if (action.checkTick)
                                {
                                    WrapTickProcess(action);
                                    if (action.ProcessTick())
                                    {
                                        if (processAction)
                                        {
                                            ProcessAction(mapper, outputStatus, action);
                                        }
                                        else if (analog)
                                        {
                                            mapper.RunEventFromAnalog(action, outputStatus, ButtonDistance, AxisUnit);
                                        }
                                        else
                                        {
                                            mapper.RunEventFromButton(action, outputStatus);
                                        }

                                        action.firstRun = false;
                                        action.EffectiveDurationMs = (int)(action.DurationMs / ButtonDistance);
                                    }
                                    else if (action.breakSequence)
                                    {
                                        break;
                                    }
                                }
                                /*else if (action.ProcessTick())
                                {
                                    mapper.RunEventFromButton(action, status);
                                    WrapTickProcess(action);
                                    //mapper.RunEventFromButton(action, status);
                                    //action.firstRun = false;
                                    //action.EffectiveDurationMs = (int)(action.DurationMs / ButtonDistance);
                                }
                                else if (analog)
                                {
                                    WrapTickProcess(action);
                                }
                                */
                            }
                        }
                        else if (func.active && shouldInterrupt)
                        {
                            foreach (OutputActionData action in func.OutputActions)
                            {
                                activeActions.Remove(action);
                                if (action.activatedEvent)
                                {
                                    mapper.RunEventFromButton(action, false);
                                }

                                action.firstRun = false;
                                action.Release();
                            }

                            func.Release(mapper);
                            removeFuncsCandiates.Add(i);
                            removed = true;
                        }
                        else if (func.onChorded && func.finished)
                        {
                            OutputActionDataEnumerator activeActionsEnumerator =
                                new OutputActionDataEnumerator(func.OutputActions);
                            activeActionsEnumerator.MoveToEnd();
                            while (activeActionsEnumerator.MovePrevious())
                            {
                                OutputActionData action = activeActionsEnumerator.Current;
                                if (action.activatedEvent)
                                {
                                    //mapper.RunEventFromButton(action, status);
                                    mapper.RunEventFromButton(action, false);
                                    action.Release();
                                    //if (action.checkTick) action.Release();
                                }

                                action.firstRun = true;
                            }

                            func.Release(mapper);

                            // Need to add func back to candiates list for future
                            // evaluation
                            actionFuncCandidates.Add(func);

                            // Remove func from currently active func list
                            removeFuncsCandiates.Add(i);
                            removed = true;

                        }
                        else if (func.finished)
                        {
                            OutputActionDataEnumerator activeActionsEnumerator =
                                new OutputActionDataEnumerator(func.OutputActions);
                            activeActionsEnumerator.MoveToEnd();
                            while (activeActionsEnumerator.MovePrevious())
                            {
                                OutputActionData action = activeActionsEnumerator.Current;
                                if (action.activatedEvent)
                                {
                                    //mapper.RunEventFromButton(action, status);
                                    mapper.RunEventFromButton(action, false);
                                    action.Release();
                                    //if (action.checkTick) action.Release();
                                }

                                action.firstRun = true;
                            }

                            /*foreach (OutputActionData action in func.OutputActions)
                            {
                                activeActions.Remove(action);
                                if (action.activatedEvent)
                                {
                                    mapper.RunEventFromButton(action, false);
                                }

                                action.firstRun = false;
                            }
                            */

                            func.Release(mapper);
                            removeFuncsCandiates.Add(i);
                            removed = true;
                        }

                        i++;
                    }

                    if (removed)
                    {
                        removeFuncsCandiates.Reverse();
                        foreach (int index in removeFuncsCandiates)
                        {
                            activeFuns.RemoveAt(index);
                        }

                        removeFuncsCandiates.Clear();
                    }

                    /*pressFunc.Prepare(mapper, active);
                    foreach(OutputActionData action in pressFunc.OutputActions)
                    {
                        mapper.RunEventFromButton(action, status);
                        activeActions.Add(action);
                    }
                    */
                }
                else if (!status)
                {
                    activeActions.Clear();
                    actionFuncCandidates.Clear();

                    bool stillActiveFun = false;
                    foreach (ActionFunc func in activeFuns)
                    {
                        func.Prepare(mapper, false, stateData);
                        if (!func.active)
                        {
                            activeActions.AddRange(func.OutputActions);
                        }
                        else
                        {
                            actionFuncCandidates.Add(func);
                            stillActiveFun = true;
                        }
                        //func.Release(mapper);
                    }

                    activeFuns.Clear();
                    if (stillActiveFun)
                    {
                        activeFuns.AddRange(actionFuncCandidates);
                        actionFuncCandidates.Clear();
                    }

                    OutputActionDataEnumerator activeActionsEnumerator =
                        new OutputActionDataEnumerator(activeActions);
                    activeActionsEnumerator.MoveToEnd();
                    while (activeActionsEnumerator.MovePrevious())
                    {
                        OutputActionData action = activeActionsEnumerator.Current;
                        if (action.activatedEvent)
                        {
                            mapper.RunEventFromButton(action, status);
                            action.Release();
                            //if (action.checkTick) action.Release();
                        }

                        action.firstRun = true;
                    }

                    activeActions.Clear();

                    if (distanceFuns.Count > 0)
                    {
                        ActionFuncEnumerator distanceFuncsEnumerator =
                            new ActionFuncEnumerator(distanceFuns);
                        distanceFuncsEnumerator.MoveToEnd();
                        while (distanceFuncsEnumerator.MovePrevious())
                        {
                            ActionFunc func = distanceFuncsEnumerator.Current;
                            if (!func.active) continue;

                            foreach(OutputActionData action in func.OutputActions)
                            {
                                if (action.activatedEvent)
                                {
                                    mapper.RunEventFromButton(action, status);
                                    action.Release();
                                    //if (action.checkTick) action.Release();
                                }

                                action.firstRun = true;
                            }
                        }
                    }

                    // Only bother if a ReleaseFunc instance was found
                    if (releaseFuns.Count > 0)
                    {
                        bool releaseNonInterrupt = false;
                        ActionFuncEnumerator funcEnumerator =
                            new ActionFuncEnumerator(releaseFuns);
                        //funcEnumerator.MoveToEnd();
                        //int releaseIdx = releaseFuns.Count - 1;
                        long maxTime = 0;
                        while (funcEnumerator.MoveNext())
                        {
                            ReleaseFunc func = funcEnumerator.Current as ReleaseFunc;
                            //func.Prepare(mapper, new ActionFuncStateData()
                            //{
                            //    state = true,
                            //    axisValue = 0.0
                            //});
                            func.Prepare(mapper, true, stateData);
                            Trace.WriteLine($"FUNC ACTIVE {func.active} {func}");
                            if (func.active)
                            {
                                if (!func.interruptable)
                                {
                                    releaseNonInterrupt = true;
                                }

                                if (func.DurationMs >= maxTime)
                                {
                                    maxTime = func.DurationMs;
                                }
                                //tempReleaseFuncs.Add(cunc);
                            }
                            else
                            {
                                func.Prepare(mapper, false, stateData);
                                //func.Prepare(mapper, new ActionFuncStateData()
                                //{
                                //    state = false,
                                //    axisValue = 0.0,
                                //});
                                //releaseFuns.RemoveAt(releaseIdx);
                            }

                            //releaseIdx -= 1;
                        }

                        // Release funcs found. Activate events
                        Trace.WriteLine(maxTime);
                        funcEnumerator = new ActionFuncEnumerator(releaseFuns);
                        while (funcEnumerator.MoveNext())
                        {
                            ReleaseFunc func = funcEnumerator.Current as ReleaseFunc;
                            //func.distance < distancePercent
                            bool shouldInterrupt = func.active && func.interruptable && func.DurationMs < maxTime;
                            if (!shouldInterrupt && func.active)
                            //if (func.active)
                            {
                                Trace.WriteLine("MADE IT HERE");
                                foreach (OutputActionData action in func.OutputActions)
                                {
                                    if (action.processOutput) action.ProcessAction();
                                    if (action.breakSequence) break;

                                    if (processAction)
                                    {
                                        ProcessAction(mapper, true, action);
                                    }
                                    else if (analog)
                                    {
                                        mapper.RunEventFromAnalog(action, true, ButtonDistance, AxisUnit);
                                    }
                                    else
                                    {
                                        mapper.RunEventFromButton(action, true);
                                    }

                                    //mapper.PendingReleaseActions.Add(action);
                                    action.firstRun = true;
                                    //if (action.checkTick) action.Release();
                                    //else if (action.breakSequence) break;
                                    bool currentBreakSequence = action.breakSequence;
                                    if (action.OutputType != OutputActionData.ActionType.Keyboard)
                                    {
                                        action.Release();
                                    }

                                    if (currentBreakSequence) break;
                                    //activeActions.Add(action);
                                }

                                mapper.PendingReleaseFuns.Add(func);
                            }
                            else
                            {
                                func.Prepare(mapper, false, stateData);
                            }

                            //func.Prepare(mapper, false, stateData);
                            //func.Prepare(mapper, new ActionFuncStateData()
                            //{
                            //    state = false,
                            //    axisValue = 0.0,
                            //});
                        }

                        //tempReleaseFuncs.Clear();
                        releaseFuns.Clear();
                    }

                    /*ActionFunc tempRelease = null;
                    foreach (ActionFunc func in releaseFuns)
                    {
                        func.Prepare(mapper, true);

                        if (func.active)
                        {
                            tempRelease = func;
                        }

                        //func.Release(mapper);
                    }

                    releaseFuns.Remove(tempRelease);
                    */

                    /*foreach(ActionFunc func in releaseFuns)
                    {
                        func.Release(mapper);
                    }

                    releaseFuns.Clear();
                    */

                    /*if (tempRelease != null)
                    {
                        foreach (OutputActionData action in tempRelease.OutputActions)
                        {
                            mapper.RunEventFromButton(action, true);
                            //mapper.PendingReleaseActions.Add(action);
                            action.firstRun = true;
                            if (action.checkTick) action.Release();
                            else if (action.breakSequence) break;
                            //activeActions.Add(action);
                        }

                        mapper.PendingReleaseFuns.Add(tempRelease);
                        tempRelease.Prepare(mapper, false);
                    }
                    */

                    /*foreach (ActionFunc func in actionFuncCandidates)
                    {
                        func.Prepare(mapper, false, stateData);
                        //func.Prepare(mapper, new ActionFuncStateData()
                        //{
                        //    state = false,
                        //    axisValue = 0.0,
                        //});
                        //func.Release(mapper);
                    }
                    */

                    //actionFuncCandidates.Clear();
                    interruptFound = false;

                    //pressFunc.Release(mapper);
                }
                /*outputActionEnumerator.Reset();
                while (outputActionEnumerator.MoveNext())
                {
                    OutputActionData action = outputActionEnumerator.Current;
                    mapper.RunEventFromButton(action, status);
                }
                */

                /*foreach(OutputActionData action in outputActions)
                {
                    mapper.RunEventFromButton(action, status);
                }
                */
            }

            active = status || activeFuns.Count > 0;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState=true, bool ignoreReleaseActions=false)
        {
            //if (active)
            if (active || activeFuns.Count > 0)
            {
                if (resetState)
                {
                    stateData.Reset(true);
                }

                foreach (ActionFunc func in activeFuns)
                {
                    activeActions.AddRange(func.OutputActions);
                    func.Release(mapper);
                }

                activeFuns.Clear();
                actionFuncCandidates.Clear();

                OutputActionDataEnumerator activeActionsEnumerator =
                    new OutputActionDataEnumerator(activeActions);
                activeActionsEnumerator.MoveToEnd();
                while (activeActionsEnumerator.MovePrevious())
                {
                    OutputActionData action = activeActionsEnumerator.Current;
                    if (action.activatedEvent)
                    {
                        mapper.RunEventFromButton(action, false);
                        action.Release();
                        //if (action.checkTick) action.Release();
                    }
                }

                activeActions.Clear();

                if (distanceFuns.Count > 0)
                {
                    ActionFuncEnumerator distanceFuncsEnumerator =
                        new ActionFuncEnumerator(distanceFuns);
                    distanceFuncsEnumerator.MoveToEnd();
                    while (distanceFuncsEnumerator.MovePrevious())
                    {
                        ActionFunc func = distanceFuncsEnumerator.Current;
                        if (!func.active) continue;
                        foreach (OutputActionData action in func.OutputActions)
                        {
                            if (action.activatedEvent)
                            {
                                mapper.RunEventFromButton(action, false);
                                action.Release();
                                //if (action.checkTick) action.Release();
                            }

                            action.firstRun = true;
                        }
                    }

                    distanceFuns.Clear();
                }

                /*foreach (ActionFunc func in activeFuns)
                {
                    func.Release(mapper);
                }
                */

                if (!ignoreReleaseActions)
                {
                    // Only bother if a ReleaseFunc instance was found
                    if (releaseFuns.Count > 0)
                    {
                        bool releaseNonInterrupt = false;
                        ActionFuncEnumerator funcEnumerator =
                            new ActionFuncEnumerator(releaseFuns);
                        //funcEnumerator.MoveToEnd();
                        //int releaseIdx = releaseFuns.Count - 1;
                        long maxTime = 0;
                        while (funcEnumerator.MoveNext())
                        {
                            ReleaseFunc func = funcEnumerator.Current as ReleaseFunc;
                            //stateData.state = true;
                            func.Prepare(mapper, true, stateData);
                            //func.Prepare(mapper, new ActionFuncStateData()
                            //{
                            //    state = true,
                            //    axisValue = 1.0,
                            //});
                            if (func.active)
                            {
                                if (!func.interruptable)
                                {
                                    releaseNonInterrupt = true;
                                }

                                if (func.DurationMs >= maxTime)
                                {
                                    maxTime = func.DurationMs;
                                }
                                //tempReleaseFuncs.Add(cunc);
                            }
                            else
                            {
                                func.Prepare(mapper, false, stateData);
                                //func.Prepare(mapper, new ActionFuncStateData()
                                //{
                                //    state = false,
                                //    axisValue = 0.0,
                                //});
                                //releaseFuns.RemoveAt(releaseIdx);
                            }

                            //releaseIdx -= 1;
                            stateData.state = false;
                        }

                        // Release funcs found. Activate events
                        funcEnumerator = new ActionFuncEnumerator(releaseFuns);
                        while (funcEnumerator.MoveNext())
                        {
                            ReleaseFunc func = funcEnumerator.Current as ReleaseFunc;
                            //func.distance < distancePercent
                            bool shouldInterrupt = func.active && func.interruptable && func.DurationMs < maxTime;
                            if (!shouldInterrupt && func.active)
                            {
                                foreach (OutputActionData action in func.OutputActions)
                                {
                                    mapper.RunEventFromButton(action, true);
                                    //mapper.PendingReleaseActions.Add(action);
                                    action.firstRun = true;
                                    if (action.checkTick) action.Release();
                                    else if (action.breakSequence) break;
                                    //activeActions.Add(action);
                                }

                                mapper.PendingReleaseFuns.Add(func);
                            }

                            stateData.state = false;
                            func.Prepare(mapper, false, stateData);
                            //func.Prepare(mapper, new ActionFuncStateData()
                            //{
                            //    state = false,
                            //    axisValue = 0.0,
                            //});
                        }

                        //tempReleaseFuncs.Clear();
                        releaseFuns.Clear();
                    }
                }

                //pressFunc.Release(mapper);
                /*outputActionEnumerator.MoveToEnd();
                while (outputActionEnumerator.MovePrevious())
                {
                    OutputActionData action = outputActionEnumerator.Current;
                    mapper.RunEventFromButton(action, false);
                }
                */
                /*foreach (OutputActionData action in outputActions)
                {
                    mapper.RunEventFromButton(action, false);
                }
                */
            }

            active = false;
            activeEvent = false;
            interruptFound = false;
            status = false;
            //if (resetState)
            //{
            //    stateData.Reset(true);
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseActions(Mapper mapper, bool resetState = true)
        {
            if (resetState)
            {
                stateData.Reset(true);
            }

            foreach (ActionFunc func in activeFuns)
            {
                activeActions.AddRange(func.OutputActions);
                func.Release(mapper);
            }

            activeFuns.Clear();
            actionFuncCandidates.Clear();

            OutputActionDataEnumerator activeActionsEnumerator =
                new OutputActionDataEnumerator(activeActions);
            activeActionsEnumerator.MoveToEnd();
            while (activeActionsEnumerator.MovePrevious())
            {
                OutputActionData action = activeActionsEnumerator.Current;
                if (action.activatedEvent)
                {
                    mapper.RunEventFromButton(action, false);
                    if (action.checkTick) action.Release();
                }
            }

            activeActions.Clear();

            if (distanceFuns.Count > 0)
            {
                ActionFuncEnumerator distanceFuncsEnumerator =
                    new ActionFuncEnumerator(distanceFuns);
                distanceFuncsEnumerator.MoveToEnd();
                while (distanceFuncsEnumerator.MovePrevious())
                {
                    ActionFunc func = distanceFuncsEnumerator.Current;
                    if (!func.active) continue;
                    foreach (OutputActionData action in func.OutputActions)
                    {
                        if (action.activatedEvent)
                        {
                            mapper.RunEventFromButton(action, false);
                            if (action.checkTick) action.Release();
                        }

                        action.firstRun = true;
                    }
                }

                distanceFuns.Clear();
            }

            /*foreach (ActionFunc func in activeFuns)
            {
                func.Release(mapper);
            }
            */

            if (!mapper.Quit)
            {
                // Only bother if a ReleaseFunc instance was found
                if (releaseFuns.Count > 0)
                {
                    bool releaseNonInterrupt = false;
                    ActionFuncEnumerator funcEnumerator =
                        new ActionFuncEnumerator(releaseFuns);
                    //funcEnumerator.MoveToEnd();
                    //int releaseIdx = releaseFuns.Count - 1;
                    long maxTime = 0;
                    while (funcEnumerator.MoveNext())
                    {
                        ReleaseFunc func = funcEnumerator.Current as ReleaseFunc;
                        //stateData.state = true;
                        func.Prepare(mapper, true, stateData);
                        //func.Prepare(mapper, new ActionFuncStateData()
                        //{
                        //    state = true,
                        //    axisValue = 1.0,
                        //});
                        if (func.active)
                        {
                            if (!func.interruptable)
                            {
                                releaseNonInterrupt = true;
                            }

                            if (func.DurationMs >= maxTime)
                            {
                                maxTime = func.DurationMs;
                            }
                            //tempReleaseFuncs.Add(cunc);
                        }
                        else
                        {
                            func.Prepare(mapper, false, stateData);
                            //func.Prepare(mapper, new ActionFuncStateData()
                            //{
                            //    state = false,
                            //    axisValue = 0.0,
                            //});
                            //releaseFuns.RemoveAt(releaseIdx);
                        }

                        //releaseIdx -= 1;
                        stateData.state = false;
                    }

                    // Release funcs found. Activate events
                    funcEnumerator = new ActionFuncEnumerator(releaseFuns);
                    while (funcEnumerator.MoveNext())
                    {
                        ReleaseFunc func = funcEnumerator.Current as ReleaseFunc;
                        //func.distance < distancePercent
                        bool shouldInterrupt = func.active && func.interruptable && func.DurationMs < maxTime;
                        if (!shouldInterrupt && func.active)
                        {
                            foreach (OutputActionData action in func.OutputActions)
                            {
                                mapper.RunEventFromButton(action, true);
                                //mapper.PendingReleaseActions.Add(action);
                                action.firstRun = true;
                                if (action.checkTick) action.Release();
                                else if (action.breakSequence) break;
                                //activeActions.Add(action);
                            }

                            mapper.PendingReleaseFuns.Add(func);
                        }

                        stateData.state = false;
                        //func.Prepare(mapper, false, stateData);

                        //func.Prepare(mapper, new ActionFuncStateData()
                        //{
                        //    state = false,
                        //    axisValue = 0.0,
                        //});
                    }

                    //tempReleaseFuncs.Clear();
                    releaseFuns.Clear();
                }
            }

            //pressFunc.Release(mapper);
            /*outputActionEnumerator.MoveToEnd();
            while (outputActionEnumerator.MovePrevious())
            {
                OutputActionData action = outputActionEnumerator.Current;
                mapper.RunEventFromButton(action, false);
            }
            */
            /*foreach (OutputActionData action in outputActions)
            {
                mapper.RunEventFromButton(action, false);
            }
            */
        }

        public override void SoftRelease(Mapper mapper, MapAction _,
            bool resetState = true)
        {
            if (active || activeFuns.Count > 0)
            {
                if (!useParentActions)
                {
                    ReleaseActions(mapper, resetState);
                }

                //if (resetState)
                //{
                //    stateData.Reset(true);
                //}

                //foreach (ActionFunc func in activeFuns)
                //{
                //    activeActions.AddRange(func.OutputActions);
                //    func.Release(mapper);
                //}

                //activeFuns.Clear();

                //OutputActionDataEnumerator activeActionsEnumerator =
                //    new OutputActionDataEnumerator(activeActions);
                //activeActionsEnumerator.MoveToEnd();
                //while (activeActionsEnumerator.MovePrevious())
                //{
                //    OutputActionData action = activeActionsEnumerator.Current;
                //    if (action.activatedEvent)
                //    {
                //        mapper.RunEventFromButton(action, false, false);
                //        if (action.checkTick) action.Release();
                //    }
                //}

                //activeActions.Clear();

                //if (distanceFuns.Count > 0)
                //{
                //    ActionFuncEnumerator distanceFuncsEnumerator =
                //        new ActionFuncEnumerator(distanceFuns);
                //    distanceFuncsEnumerator.MoveToEnd();
                //    while (distanceFuncsEnumerator.MovePrevious())
                //    {
                //        ActionFunc func = distanceFuncsEnumerator.Current;
                //        if (!func.active) continue;
                //        foreach (OutputActionData action in func.OutputActions)
                //        {
                //            if (action.activatedEvent)
                //            {
                //                mapper.RunEventFromButton(action, false, false);
                //                if (action.checkTick) action.Release();
                //            }

                //            action.firstRun = true;
                //        }
                //    }

                //    distanceFuns.Clear();
                //}

                /*foreach (ActionFunc func in activeFuns)
                {
                    func.Release(mapper);
                }
                */

                //distanceFuns.Clear();
                //releaseFuns.Clear();
                //actionFuncCandidates.Clear();

                //pressFunc.Release(mapper);
                /*outputActionEnumerator.MoveToEnd();
                while (outputActionEnumerator.MovePrevious())
                {
                    OutputActionData action = outputActionEnumerator.Current;
                    mapper.RunEventFromButton(action, false);
                }
                */
                /*foreach (OutputActionData action in outputActions)
                {
                    mapper.RunEventFromButton(action, false);
                }
                */


                actionFuncCandidates.Clear();
            }

            active = false;
            activeEvent = false;
            interruptFound = false;
            status = false;
            //if (resetState)
            //{
            //    stateData.Reset(true);
            //}
        }

        public override void PrepareAnalog(Mapper mapper, double axisValue, double axisUnit,
            bool alterState = true)
        {
        }

        public virtual void WrapTickProcess(OutputActionData action)
        {
        }

        public virtual void WrapNotchesProcess(OutputActionData action)
        {
        }

        public override ButtonMapAction DuplicateAction()
        {
            Trace.WriteLine("IN ButtonAction");
            return new ButtonAction(this);
        }

        private HashSet<string> fullPropertySet = new HashSet<string>(
            new string[] {
                PropertyKeyStrings.NAME,
                PropertyKeyStrings.FUNCTIONS,
            }
        );

        public override void SoftCopyFromParent(ButtonMapAction parentAction)
        {
            if (parentAction is ButtonAction parentBtnAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                this.parentButtonAct = parentBtnAction;
                privateState = parentBtnAction.privateState;
                parentBtnAction.hasLayeredAction = true;

                parentBtnAction.NotifyPropertyChanged += ParentBtnAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch(parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = parentBtnAction.name;
                            break;
                        case PropertyKeyStrings.FUNCTIONS:
                            actionFuncs.Clear();
                            actionFuncs.AddRange(parentBtnAction.actionFuncs);
                            useParentActions = true;
                            break;
                        default:
                            break;
                    }
                }

                //if (!changedProperties.Contains(PropertyKeyStrings.NAME))
                //{
                //    name = parentBtnAction.name;
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.FUNCTIONS))
                //{
                //    actionFuncs.AddRange(parentBtnAction.actionFuncs);
                //    useParentActions = true;
                //}

                /*if (!changedProperties.Contains(""))
                {

                }
                */
            }
        }

        public override void SoftCopy(ButtonMapAction parentAction)
        {
            if (parentAction is ButtonAction parentBtnAction)
            {
                base.SoftCopy(parentAction);

                parentBtnAction.NotifyPropertyChanged += ParentBtnAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch (parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = parentBtnAction.name;
                            break;
                        case PropertyKeyStrings.FUNCTIONS:
                            actionFuncs.Clear();
                            actionFuncs.AddRange(parentBtnAction.actionFuncs);
                            useParentActions = true;
                            break;
                        default:
                            break;
                    }
                }

                //if (!changedProperties.Contains(PropertyKeyStrings.NAME))
                //{
                //    name = parentBtnAction.name;
                //}

                //if (!changedProperties.Contains(PropertyKeyStrings.FUNCTIONS))
                //{
                //    actionFuncs.AddRange(parentBtnAction.actionFuncs);
                //    useParentActions = true;
                //}

                /*if (!changedProperties.Contains(""))
                {

                }
                */
            }
        }

        private void ParentBtnAction_NotifyPropertyChanged(object sender,
            NotifyPropertyChangeArgs e)
        {
            CascadePropertyChange(e.Mapper, e.PropertyName);
        }

        public override void CopyAction(ButtonMapAction sourceAction, bool addProps = true)
        {
            if (sourceAction is ButtonAction tempSrcBtnAction)
            {
                // Determine the set with properties that should inherit
                // from the parent action
                /*IEnumerable<string> useParentProList = tempSrcBtnAction.changedProperties;

                foreach (string parentPropType in useParentProList)
                {
                    switch (parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempSrcBtnAction.name;
                            break;
                        case PropertyKeyStrings.FUNCTIONS:
                            foreach (ActionFunc func in tempSrcBtnAction.actionFuncs)
                            {
                                actionFuncs.Add(ActionFuncCopyFactory.CopyFunc(func));
                            }
                            break;
                        default:
                            break;
                    }
                }
                */
                name = tempSrcBtnAction.name;
                foreach (ActionFunc func in tempSrcBtnAction.actionFuncs)
                {
                    actionFuncs.Add(ActionFuncCopyFactory.CopyFunc(func));
                }

                if (addProps)
                {
                    changedProperties.Add(PropertyKeyStrings.NAME);
                    changedProperties.Add(PropertyKeyStrings.FUNCTIONS);
                }
            }
        }

        protected override void CascadePropertyChange(Mapper mapper, string propertyName)
        {
            if (changedProperties.Contains(propertyName))
            {
                // Property already overrridden in action. Leave
                return;
            }
            else if (parentButtonAct == null)
            {
                // No parent action. Leave
                return;
            }

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = parentButtonAct.name;
                    break;
                case PropertyKeyStrings.FUNCTIONS:
                    if (active)
                    {
                        Release(mapper, ignoreReleaseActions: true);
                    }

                    actionFuncs.Clear();
                    actionFuncs.AddRange(parentButtonAct.actionFuncs);
                    useParentActions = true;
                    break;
                default:
                    break;
            }
        }

        public override void RaiseNotifyPropertyChange(Mapper mapper, string propertyName)
        {
            NotifyPropertyChanged?.Invoke(this,
                new NotifyPropertyChangeArgs(mapper, propertyName));
        }

        public override string Describe()
        {
            string result = "ButtonAction";
            return result;
        }

        public override string DescribeActions(Mapper mapper)
        {
            string result = "";
            List<string> tempList = new List<string>();
            foreach(ActionFunc func in actionFuncs)
            {
                tempList.Add(func.Describe(mapper));
            }

            if (tempList.Count > 0)
            {
                result = string.Join("|", tempList);
            }

            return result;
        }

        //public void CopyPropsTo(ButtonAction secondAction)
        //{
        //    base.CopyBaseProps(secondAction);
        //}

        //public void ShallowCopyBindingsTo(ButtonAction secondAction)
        //{
        //    foreach (ActionFunc func in actionFuncs)
        //    {
        //        secondAction.actionFuncs.Add(func);
        //        //secondAction.ActionFuncs.Add(ActionFuncCopyFactory.CopyFunc(func));
        //    }
        //}

        //public void CopyBindingsTo(ButtonAction secondAction)
        //{
        //    secondAction.ActionFuncs.Clear();

        //    foreach(ActionFunc func in actionFuncs)
        //    {
        //        secondAction.ActionFuncs.Add(ActionFuncCopyFactory.CopyFunc(func));
        //    }
        //}
    }
}
