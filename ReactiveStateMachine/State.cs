using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReactiveStateMachine.Actions;
using ReactiveStateMachine.Transitions;

namespace ReactiveStateMachine
{
    internal class State<T>
    {
        #region private fields

        private readonly List<StateAction<T>> _entryActions = new List<StateAction<T>>();
        private readonly List<StateAction<T>> _exitActions = new List<StateAction<T>>();
        private readonly List<IIgnoringObservable<object>> _transitions = new List<IIgnoringObservable<object>>();
        private readonly List<IDisposable> _currentSubscriptions = new List<IDisposable>();
        private readonly List<TimedTransition<T, object>> _timedTransitions = new List<TimedTransition<T, object>>();
        private readonly List<Transition<T, object>> _automaticTransitions = new List<Transition<T, object>>();
        private readonly ReactiveStateMachine<T> _stateMachine;

        #endregion

        #region ctor

        public State(T stateRepresentation, ReactiveStateMachine<T> stateMachine)
        {
            StateRepresentation = stateRepresentation;
            _stateMachine = stateMachine;
        }

        #endregion

        #region public properties

        public T StateRepresentation { get; private set; }

        #endregion

        #region entry actions

        public void AddEntryAction(Action entryAction, Func<bool> condition)
        {
            _entryActions.Add(new StateAction<T>(entryAction, condition));
        }

        public void AddEntryAction(T fromState, Action entryAction, Func<bool> condition)
        {
            _entryActions.Add(new StateAction<T>(entryAction, fromState, condition));
        }

        private IEnumerable<Action> GetValidEntryActions(T fromState)
        {
            return _entryActions.
                Where(tuple => (!tuple.IsReferenceStateSet || tuple.ReferenceState.Equals(fromState))).
                Where(tuple =>
                {
                    if (tuple.Condition == null)
                    {
                        return true;
                    }

                    Func<bool> safeCondition = () =>
                    {
                        try
                        {
                            return tuple.Condition();
                        }
                        catch (Exception e)
                        {
                            _stateMachine.RaiseStateMachineException(e);
                        }
                        return false;
                    };

                    if (_stateMachine.CurrentDispatcher != null)
                    {
                        return (bool)_stateMachine.CurrentDispatcher.Invoke(safeCondition, null);
                    }

                    return safeCondition();
                })
                .Select(tuple => tuple.Action).ToArray();
        }

        #endregion

        #region exit actions

        public void AddExitAction(Action exitAction, Func<bool> condition)
        {
            _exitActions.Add(new StateAction<T>(exitAction, condition));
        }

        public void AddExitAction(T toState, Action exitAction, Func<bool> condition)
        {
            _exitActions.Add(new StateAction<T>(exitAction, toState, condition));
        }

        public IEnumerable<Action> GetValidExitActions(T toState)
        {
            return _exitActions
                .Where(tuple => (!tuple.IsReferenceStateSet || tuple.ReferenceState.Equals(toState)))
                .Where(tuple =>
                {
                    if (tuple.Condition == null)
                    {
                        return true;
                    }

                    Func<bool> safeCondition = () =>
                    {
                        try
                        {
                            return tuple.Condition();
                        }
                        catch (Exception e)
                        {
                            _stateMachine.RaiseStateMachineException(e);
                        }
                        return false;
                    };

                    if (_stateMachine.CurrentDispatcher != null)
                    {
                        return (bool)_stateMachine.CurrentDispatcher.Invoke(safeCondition, null);
                    }

                    return safeCondition();
                })
                .Select(tuple => tuple.Action).ToArray();
        }

        #endregion

        #region transitions

        public void AddTriggeredTransition<TTrigger>(TriggeredTransition<T, TTrigger> transition) where TTrigger : class
        {
            _transitions.Add(transition.Trigger.Sequence);

            transition.Trigger.Sequence.Subscribe(args =>
            {
                if (transition.Condition != null)
                {
                    try
                    {
                        var success = false;

                        if (_stateMachine.CurrentDispatcher != null)
                        {
                            success = (bool)_stateMachine.CurrentDispatcher.Invoke(transition.Condition, args);
                        }
                        else
                        {
                            success = transition.Condition(args);
                        }

                        if(!success)
                        {
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        _stateMachine.RaiseStateMachineException(e);
                        return;
                    }
                }

                Action action = () => _stateMachine.TransitionStateInternal(transition.FromState, transition.ToState, args, transition.TransitionAction);

                _stateMachine.EnqueueTransition(action);
            });
        }

        public void AddTimedTransition(TimedTransition<T, object> timedTransition)
        {
            _timedTransitions.Add(timedTransition);
        }

        public void AddAutomaticTransition(Transition<T, object> transition)
        {
            _automaticTransitions.Add(transition);
        }

        #endregion

        public bool TryAutomaticTransition()
        {
            foreach (var automaticTransition in _automaticTransitions)
            {
                bool success;

                if (automaticTransition.Condition == null)
                {
                    success = true;
                }
                else
                {
                    var automaticTransitionCopy = automaticTransition;

                    Func<bool> safeCondition = () =>
                    {
                        try
                        {
                            return automaticTransitionCopy.Condition(null);
                        }
                        catch (Exception e)
                        {
                            _stateMachine.RaiseStateMachineException(e);
                            return false;
                        }
                    };

                    if (_stateMachine.CurrentDispatcher != null)
                    {
                        success = (bool)_stateMachine.CurrentDispatcher.Invoke(safeCondition, null);
                    }
                    else
                    {
                        success = safeCondition();
                    }
                }

                if (success)
                {
                    _stateMachine.EnqueueTransition(() => _stateMachine.TransitionStateInternal(StateRepresentation, automaticTransition.ToState, null, automaticTransition.TransitionAction));

                    // Remove one time transitions
                    _automaticTransitions.RemoveAll(t => t.OneTime);
                    
                    return true;
                }
            }

            return false;
        }

        public void Enter(T fromState)
        {
            foreach (var entryAction in GetValidEntryActions(fromState))
            {
                var entryActionCopy = entryAction;
                Action safeEntryAction = () =>
                {
                    try
                    {
                        entryActionCopy();
                    }
                    catch (Exception e)
                    {
                        _stateMachine.RaiseStateMachineException(e);
                    }
                };

                if (_stateMachine.CurrentDispatcher != null)
                {
                    _stateMachine.CurrentDispatcher.Invoke(safeEntryAction);
                }
                else
                {
                    safeEntryAction();
                }
            }
        }

        public void Exit(T toState)
        {
            //dispose of all timed transition
            foreach (var subscription in _currentSubscriptions)
            {
                subscription.Dispose();
            }

            //prune _currentSubscriptions
            _currentSubscriptions.Clear();

            foreach (var exitAction in GetValidExitActions(toState))
            {
                var exitActionCopy = exitAction;

                Action safeExitAction = () =>
                {
                    try
                    {
                        exitActionCopy();
                    }
                    catch (Exception e)
                    {
                        _stateMachine.RaiseStateMachineException(e);
                    }
                };

                if (_stateMachine.CurrentDispatcher != null)
                {
                    _stateMachine.CurrentDispatcher.Invoke(safeExitAction);
                }
                else
                {
                    safeExitAction();
                }
            }
        }

        public void ResumeTransitions()
        {
            foreach (var transition in _transitions)
            {
                transition.Resume();
            }

            foreach (var timedTransition in _timedTransitions)
            {
                var timedTransitionCopy = timedTransition;
                
                var subscription = Observable.Return<object>(null).Delay(timedTransition.After).
                    Where(args => _stateMachine.CurrentState.Equals(StateRepresentation)).
                    Where(args =>
                    {
                        if (timedTransitionCopy.Condition == null)
                        {
                            return true;
                        }

                        var safeCondition = new Func<bool>(() =>
                        {
                            try
                            {
                                return timedTransitionCopy.Condition(args);
                            }
                            catch (Exception e)
                            {
                                _stateMachine.RaiseStateMachineException(e);
                                return false;
                            }
                        });
                        
                        if (_stateMachine.CurrentDispatcher != null)
                        {
                            return (bool)_stateMachine.CurrentDispatcher.Invoke(safeCondition);
                        }

                        return safeCondition();
                    }).                    
                    Subscribe(args => _stateMachine.EnqueueTransition(() => _stateMachine.TransitionStateInternal(StateRepresentation, timedTransitionCopy.ToState, args, timedTransitionCopy.TransitionAction)));
                _currentSubscriptions.Add(subscription);
            }
        }

        public void IgnoreTransitions()
        {
            foreach (var transition in _transitions)
            {
                transition.Ignore();
            }
        }
    }
}
