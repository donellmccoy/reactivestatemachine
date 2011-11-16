using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Reactive.Linq;

namespace ReactiveStateMachine
{
    public class StateMachineAwareVisualStateManager : VisualStateManager
    {
        #region private fields

        /// <summary>
        /// A dictionary that maps the Group name of the VisualStateGroup to the associated ReactiveStateMachine
        /// </summary>
        private readonly Dictionary<String, ReactiveStateMachine> _mappings = new Dictionary<string, ReactiveStateMachine>();

        private IDisposable _currentStateChangedSubscription;
        private IDisposable _transitionStoryboardCompletedSubscription;

        #endregion

        #region ctor

        public StateMachineAwareVisualStateManager(FrameworkElement targetControl)
        {
            if (targetControl == null)
                throw new ArgumentNullException("targetControl");

            TargetControl = targetControl;
        }

        #endregion

        #region public properties

        public FrameworkElement TargetControl { get; set; }

        #endregion

        #region internal methods

        internal void TriggerStateChange(String groupName, String fromState, String toState)
        {
            #if DEBUG
            Console.WriteLine("VSM: Transitioning " + groupName + " from " + fromState + " to " + toState);
            #endif

            if (TargetControl == null)
                return;

            if (_currentStateChangedSubscription != null)
                _currentStateChangedSubscription.Dispose();

            if (_transitionStoryboardCompletedSubscription != null)
                _transitionStoryboardCompletedSubscription.Dispose();

            try
            {
                VisualStateGroup group = VisualStateManager.GetVisualStateGroups(TargetControl).OfType<VisualStateGroup>().Where(g => g.Name == groupName).Single();

                VisualState state = group.States.OfType<VisualState>().Where(s => s.Name == toState).Single();

                VisualTransition transition = null;

                try
                {
                    transition = group.Transitions.OfType<VisualTransition>().Where(t => t.From == fromState && t.To == toState).Single();
                }
                catch { }


                if (transition != null && transition.Storyboard != null)
                {
                    _transitionStoryboardCompletedSubscription = Observable.FromEventPattern<EventArgs>(transition.Storyboard, "Completed").Subscribe(evt =>
                    {

                        ReactiveStateMachine machine = null;

                        if (_mappings.TryGetValue(group.Name, out machine))
                        {
                            #if DEBUG
                            Console.WriteLine("Storyboard.Completed (" + fromState + " --> " + toState + ")\t");
                            #endif
                            machine.ExternalStateChanged(fromState, toState);
                        }

                    });
                }
                else
                {
                    _currentStateChangedSubscription = Observable.FromEventPattern<VisualStateChangedEventArgs>(group, "CurrentStateChanged").Subscribe(evt =>
                    {

                        _currentStateChangedSubscription.Dispose();

                        ReactiveStateMachine machine = null;

                        if (_mappings.TryGetValue(group.Name, out machine))
                        {
                            #if DEBUG
                            Console.WriteLine("VisualStateGroup.CurrentStateChanged (" + fromState + " --> " + toState + ")\t");
                            #endif
                            machine.ExternalStateChanged((evt.EventArgs.OldState != null) ? evt.EventArgs.OldState.Name : "", evt.EventArgs.NewState.Name);
                        }

                    });
                }

                bool success = base.GoToStateCore(null, TargetControl, toState, group, state, true);
            }
            catch { }
        }

        internal void AddMapping(String groupName, ReactiveStateMachine stateMachine)
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

        /// <summary>
        /// This method is called whenever a call to GoToState is made to the VSM
        /// We intercept the call to the base method of the VSM by calling into our StateMachine, which ultimately will call TriggerStateChange (see above)
        /// </summary>
        /// <param name="control"></param>
        /// <param name="stateGroupsRoot"></param>
        /// <param name="stateName"></param>
        /// <param name="group"></param>
        /// <param name="state"></param>
        /// <param name="useTransitions"></param>
        /// <returns></returns>
        protected override bool GoToStateCore(FrameworkElement control, FrameworkElement stateGroupsRoot, string stateName, VisualStateGroup group, VisualState state, bool useTransitions)
        {
            //find out which StateMachine is affected and start transition 
            ReactiveStateMachine targetMachine = null;

            if (_mappings.TryGetValue(group.Name, out targetMachine))
            {
                String currentState = group.CurrentState.Name;

                targetMachine.TransitionStateInternal(currentState, stateName);
                return true;
            }
            
            //if no StateMachine is registered with this name, just use the default VSM mechanism
            return base.GoToStateCore(control, stateGroupsRoot, stateName, group, state, useTransitions);
        }

        #endregion
    }
}
