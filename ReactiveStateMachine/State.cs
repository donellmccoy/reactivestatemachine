using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReactiveStateMachine.Actions;
using ReactiveStateMachine.Transitions;
using ReactiveStateMachine.Triggers;

namespace ReactiveStateMachine
{
    internal class State<T>
    {
        private readonly List<StateAction<T>> _entryActions = new List<StateAction<T>>();
        private readonly List<StateAction<T>> _exitActions = new List<StateAction<T>>();
        private readonly List<IIgnoringObservable<object>> _transitions = new List<IIgnoringObservable<object>>();

        private readonly ReactiveStateMachine<T> _stateMachine;

        private IDisposable _timeBasedTransitionSubscription;

        public T StateRepresentation { get; private set; }

        public State(T stateRepresentation, ReactiveStateMachine<T> stateMachine)
        {
            _stateMachine = stateMachine;
            StateRepresentation = stateRepresentation;
        }

        #region entry actions

        public void AddEntryAction(Action entryAction)
        {
            _entryActions.Add(new StateAction<T>(entryAction));
        }

        public void AddEntryAction(Action entryAction, Func<bool> condition)
        {
            _entryActions.Add(new StateAction<T>(entryAction, condition));
        }

        public void AddEntryAction(T fromState, Action entryAction)
        {
            _entryActions.Add(new StateAction<T>(entryAction, fromState));
        }

        public void AddEntryAction(T fromState, Action entryAction, Func<bool> condition)
        {
            _entryActions.Add(new StateAction<T>(entryAction, fromState, condition));
        }

        public IEnumerable<Action> GetValidEntryActions(T fromState)
        {
            return _entryActions.
                Where(tuple => (tuple.Condition != null && tuple.Condition()) || tuple.Condition == null).
                Where(tuple => (tuple.IsReferenceStateSet && tuple.ReferenceState.Equals(fromState)) || !tuple.IsReferenceStateSet)
                .Select(tuple => tuple.Action).ToArray();
        }

        #endregion

        #region exit actions

        public void AddExitAction(Action exitAction)
        {
            _exitActions.Add(new StateAction<T>(exitAction));
        }

        public void AddExitAction(Action exitAction, Func<bool> condition)
        {
            _exitActions.Add(new StateAction<T>(exitAction, condition));
        }

        public void AddExitAction(T toState, Action exitAction)
        {
            _exitActions.Add(new StateAction<T>(exitAction, toState));
        }

        public void AddExitAction(T toState, Action exitAction, Func<bool> condition)
        {
            _exitActions.Add(new StateAction<T>(exitAction, toState, condition));
        }

        public IEnumerable<Action> GetValidExitActions(T toState)
        {
            return _exitActions.
                Where(tuple => (tuple.Condition != null && tuple.Condition()) || tuple.Condition == null).
                Where(tuple => (tuple.IsReferenceStateSet && tuple.ReferenceState.Equals(toState)) || !tuple.IsReferenceStateSet)
                .Select(tuple => tuple.Action).ToArray();
        }

        #endregion

        public Transition<T, object> AutomaticTransition { get; set; }
        public TimedTransition<T, object> TimedTransition { get; set; }

        public bool TryAutomaticTransition()
        {
            if(AutomaticTransition != null)
            {
                _stateMachine.EnqueueTransition(() => _stateMachine.TransitionStateInternal(StateRepresentation, TimedTransition.ToState, null, TimedTransition.TransitionAction));
                return true;
            }
            return false;
        }

        public void StartTimeBasedTransition()
        {
            if (TimedTransition != null)
            {
                _timeBasedTransitionSubscription = Observable.Return<object>(null).Delay(TimedTransition.After).Where(args => _stateMachine.CurrentState.Equals(StateRepresentation)).Subscribe(args => _stateMachine.EnqueueTransition(() => _stateMachine.TransitionStateInternal(StateRepresentation, TimedTransition.ToState, args, TimedTransition.TransitionAction)));
            }
        }

        public void AddTriggeredTransition<TTrigger>(TriggeredTransition<T, TTrigger> transition) where TTrigger:class
        {
            _transitions.Add(transition.Trigger.Sequence);

            transition.Trigger.Sequence.Subscribe(args =>
            {
                if (transition.Condition != null && !transition.Condition(args)) return;
                
                Action action = () => _stateMachine.TransitionStateInternal(transition.FromState, transition.ToState, args, transition.TransitionAction);

                _stateMachine.EnqueueTransition(action);
            });
        }

        public void Enter(T fromState)
        {
            foreach (var entryAction in GetValidEntryActions(fromState))
                entryAction();
        }

        public void Exit(T toState)
        {
            //dispose of a potential time-based transition
            if (_timeBasedTransitionSubscription != null)
                _timeBasedTransitionSubscription.Dispose();

            foreach (var exitAction in GetValidExitActions(toState))
                exitAction();
        }

        public void ResumeTransitions()
        {
            foreach (var transition in _transitions)
                transition.Resume();
        }

        internal void IgnoreTransitions()
        {
            foreach (var transition in _transitions)
                transition.Ignore();
        }
    }
}
