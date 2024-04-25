using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ReactiveStateMachine
{
    public class ReactiveVisualStateManager : VisualStateManager
    {
        #region private fields

        private IDisposable _currentStateChangedSubscription;
        private IDisposable _transitionStoryboardCompletedSubscription;

        private Dispatcher _currentDispatcher;

        /// <summary>
        /// A dictionary that maps the Group name of the VisualStateGroup to the associated ReactiveStateMachine
        /// </summary>
        private readonly Dictionary<string, IReactiveStateMachine> _mappings = new Dictionary<string, IReactiveStateMachine>();

        #endregion

        #region ctor

        public ReactiveVisualStateManager(FrameworkElement targetControl)
        {
            TargetControl = targetControl ?? throw new ArgumentNullException(nameof(targetControl));

            _currentDispatcher = Dispatcher.CurrentDispatcher;
        }

        #endregion

        #region public properties

        public FrameworkElement TargetControl { get; private set; }

        #endregion

        #region internal methods

        internal VisualStateGroup GetVisualStateGroup(string groupName)
        {
            return GetVisualStateGroups(TargetControl).OfType<VisualStateGroup>().Where(g => g.Name == groupName).Single();
        }

        internal VisualState GetVisualState(VisualStateGroup group, string state)
        {
            return group.States.OfType<VisualState>().Where(s => s.Name == state).Single();
        }

        internal VisualTransition GetVisualTransition(VisualStateGroup group, string fromState, string toState)
        {
            return group.Transitions.OfType<VisualTransition>().Where(t => t.From == fromState && t.To == toState).SingleOrDefault();
        }

        internal Task<bool> TransitionState(string groupName, string fromState, string toState)
        {
            return Task.Factory.StartNew(() => TransitionStateInternal(groupName, fromState, toState));
        }

        internal bool TransitionStateInternal(string groupName, string fromState, string toState)
        {
            var waitHandle = new ManualResetEventSlim();
#if DEBUG
            Console.WriteLine("VSM: Transitioning " + groupName + " from " + fromState + " to " + toState);
#endif

            if (TargetControl == null)
            {
                return false;
            }

            var result = _currentDispatcher.Invoke(() =>
            {
                _currentStateChangedSubscription?.Dispose();

                _transitionStoryboardCompletedSubscription?.Dispose();

                var group = GetVisualStateGroup(groupName);
                var targetState = GetVisualState(group, toState);
                var transition = GetVisualTransition(group, fromState, toState);

                if (transition?.Storyboard != null)
                {
                    _transitionStoryboardCompletedSubscription = Observable.FromEventPattern<EventArgs>(transition.Storyboard, "Completed").Subscribe(evt =>
                    {
                        _transitionStoryboardCompletedSubscription.Dispose();
#if DEBUG
                        Console.WriteLine("Storyboard.Completed (" + fromState + " --> " + toState + ")\t");
#endif
                        waitHandle.Set();
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
                        waitHandle.Set();
                    });
                }

                return base.GoToStateCore(null, TargetControl, toState, group, targetState, true);
            });

            waitHandle.Wait();

            return result;
        }

        internal void AddMapping(string groupName, IReactiveStateMachine stateMachine)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            if (stateMachine == null)
            {
                throw new ArgumentNullException(nameof(stateMachine));
            }

            _mappings.Add(groupName, stateMachine);
            stateMachine.AssociatedVisualStateManager = this;
        }

        internal void RemoveMapping(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw new ArgumentNullException(nameof(groupName));
            }

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
            
            //have to check if any of state, stateGroupsRoot or group is null, as GoToStateCore likes to throw exceptions
            return state != null && 
                   stateGroupsRoot != null && 
                   group != null && base.GoToStateCore(control, stateGroupsRoot, stateName, group, state, useTransitions);
        }

        #endregion

    }
}
