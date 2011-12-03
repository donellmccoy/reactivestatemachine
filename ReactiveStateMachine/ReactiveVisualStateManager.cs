using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace ReactiveStateMachine
{
    public class ReactiveVisualStateManager : VisualStateManager
    {
        #region private fields

        private IDisposable _currentStateChangedSubscription;
        private IDisposable _transitionStoryboardCompletedSubscription;

        /// <summary>
        /// A dictionary that maps the Group name of the VisualStateGroup to the associated ReactiveStateMachine
        /// </summary>
        private readonly Dictionary<String, IReactiveStateMachine> _mappings = new Dictionary<String, IReactiveStateMachine>();

        #endregion

        #region ctor

        public ReactiveVisualStateManager(FrameworkElement targetControl)
        {
            if (targetControl == null)
                throw new ArgumentNullException("targetControl");

            TargetControl = targetControl;
        }

        #endregion

        #region public properties

        public FrameworkElement TargetControl { get; private set; }

        #endregion

        #region internal methods

        internal VisualStateGroup GetVisualStateGroup(String groupName)
        {
            return GetVisualStateGroups(TargetControl).OfType<VisualStateGroup>().Where(g => g.Name == groupName).Single();
        }

        internal VisualState GetVisualState(VisualStateGroup group, String state)
        {
            return group.States.OfType<VisualState>().Where(s => s.Name == state).Single();
        }

        internal VisualTransition GetVisualTransition(VisualStateGroup group, String fromState, String toState)
        {
            return group.Transitions.OfType<VisualTransition>().Where(t => t.From == fromState && t.To == toState).SingleOrDefault();
        }

        internal bool TriggerStateChange(String groupName, String fromState, String toState)
        {
            #if DEBUG
            Console.WriteLine("VSM: Transitioning " + groupName + " from " + fromState + " to " + toState);
            #endif

            if (TargetControl == null)
                return false;

            IReactiveStateMachine currentMachine = null;

            if (!_mappings.TryGetValue(groupName, out currentMachine))
                return false;

            if (_currentStateChangedSubscription != null)
                _currentStateChangedSubscription.Dispose();

            if (_transitionStoryboardCompletedSubscription != null)
                _transitionStoryboardCompletedSubscription.Dispose();

            var group = GetVisualStateGroup(groupName);
            var targetState = GetVisualState(group, toState);
            var transition = GetVisualTransition(group, fromState, toState);
                
            if (transition != null && transition.Storyboard != null)
            {
                _transitionStoryboardCompletedSubscription = Observable.FromEventPattern<EventArgs>(transition.Storyboard, "Completed").Subscribe(evt =>
                {
                    _transitionStoryboardCompletedSubscription.Dispose();
                    #if DEBUG
                    Console.WriteLine("Storyboard.Completed (" + fromState + " --> " + toState + ")\t");
                    #endif
                    currentMachine.ExternalStateChanged(fromState, toState);
                });
            }
            else
            {
                _currentStateChangedSubscription = Observable.FromEventPattern<VisualStateChangedEventArgs>(group, "CurrentStateChanged").Subscribe(evt =>
                {
                    _currentStateChangedSubscription.Dispose();
                    #if DEBUG
                    Console.WriteLine("VisualStateGroup.CurrentStateChanged (" + fromState + " --> " + toState + ")\t");
                    #endif
                    currentMachine.ExternalStateChanged((evt.EventArgs.OldState != null) ? evt.EventArgs.OldState.Name : "", evt.EventArgs.NewState.Name);
                });
            }

            return base.GoToStateCore(null, TargetControl, toState, group, targetState, true);
        }

        internal void AddMapping(String groupName, IReactiveStateMachine stateMachine)
        {
            if (String.IsNullOrEmpty(groupName))
                throw new ArgumentNullException("groupName");

            if (stateMachine == null)
                throw new ArgumentNullException("stateMachine");

            _mappings.Add(groupName, stateMachine);
            stateMachine.AssociatedVisualStateManager = this;
        }

        internal void RemoveMapping(String groupName)
        {
            if (String.IsNullOrEmpty(groupName))
                throw new ArgumentNullException("groupName");

            _mappings[groupName].AssociatedVisualStateManager = null;
            _mappings.Remove(groupName);
        }

        #endregion

        #region overrides

        protected override bool GoToStateCore(FrameworkElement control, FrameworkElement stateGroupsRoot, string stateName, VisualStateGroup group, VisualState state, bool useTransitions)
        {
            ////find out which StateMachine is affected and start transition 
            //IReactiveStateMachine targetMachine = null;

            //if (_mappings.TryGetValue(group.Name, out targetMachine))
            //{
            //    String currentState = group.CurrentState.Name;

            //    targetMachine.TransitionStateInternal(currentState, stateName);
            //    return true;
            //}

            //if no StateMachine is registered with this name, just use the default VSM mechanism
            return base.GoToStateCore(control, stateGroupsRoot, stateName, group, state, useTransitions);
        }

        #endregion

    }
}
