using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;

namespace OldReactiveStateMachine
{
    public abstract class OldReactiveStateMachine
    {
        #region abstract methods

        internal abstract void TransitionStateInternal(String fromState, String toState);

        internal abstract void ExternalStateChanged(String fromState, String toState);

        #endregion

        #region public properties

        public String Name { get; set; }

        public StateMachineAwareVisualStateManager AssociatedVisualStateManager { get; internal set; }

        #endregion

        #region events

        internal event EventHandler<StateChangedEventArgs> StateChanged;
        internal event EventHandler<StateChangingEventArgs> StateChanging;

        #endregion

        #region internal methods

        internal void RaiseStateChangingEvent(String fromState, String toState)
        {
            if (StateChanging != null)
                StateChanging(this, new StateChangingEventArgs(fromState, toState));
        }

        internal void RaiseStateChangedEvent(String fromState, String toState)
        {
            if (StateChanged != null)
                StateChanged(this, new StateChangedEventArgs(fromState, toState));
        }

        #endregion
    }

    public class OldReactiveStateMachine<T> : OldReactiveStateMachine, INotifyPropertyChanged where T : struct
    {
        #region private fields

        private readonly Dictionary<T, Action> _globalEnterActions = new Dictionary<T, Action>();
        private readonly Dictionary<T, Action> _globalExitActions = new Dictionary<T, Action>();
        //private readonly Dictionary<Tuple<T, T>, Action<Object>> _transitions = new Dictionary<Tuple<T, T>, Action<Object>>();
        //private readonly Dictionary<T, ITimeBasedTransition<T>> _timedTransitions = new Dictionary<T, ITimeBasedTransition<T>>();
        //private readonly Dictionary<Tuple<T, T>, ITransition<T>> _transitions = new Dictionary<Tuple<T, T>, ITransition<T>>();


        private readonly object _currentStateLock;

        private bool _running;

        #endregion

        #region ctor

        public OldReactiveStateMachine(String name, T startState)
        {
            Name = name;
            CurrentState = startState;
        }

        #endregion

        #region public properties

        #region CurrentState

        private T _currentState;

        /// <summary>
        /// Gets or sets the CurrentState property. This observable property 
        /// indicates ....
        /// </summary>
        public T CurrentState
        {
            get { return _currentState; }
            private set
            {
                if (!_currentState.Equals(value))
                {
                    _currentState = value;
                    RaisePropertyChanged("CurrentState");
                }
            }
        }

        #endregion

        #endregion

        #region public events

        public new event EventHandler<StateChangingEventArgs<T>> StateChanging;
        public new event EventHandler<StateChangedEventArgs<T>> StateChanged;

        public event EventHandler<StateMachineExceptionEventArgs> StateMachineException;

        #endregion

        #region public methods

        public void AddTransition<TTrigger>(T fromState, T toState, Action<TTrigger> transitionAction, IObservable<TTrigger> trigger)
        {
            trigger.Where(e => CurrentState.Equals(fromState)).Subscribe(e => TransitionState(fromState, toState, e, transitionAction), OnStateMachineError);
        }

        public void AddTransition(T fromState, T toState, Action<object> transitionAction, TimeSpan after)
        {
            Observable.FromEventPattern<StateChangedEventArgs<T>>(this, "StateChanged").
                Where(evt => evt.EventArgs.CurrentState.Equals(fromState)).
                Delay(after).
                Where(evt => CurrentState.Equals(fromState)).
                ObserveOnDispatcher().
                Subscribe(evt => TransitionState(fromState, toState, null, transitionAction), OnStateMachineError);
        }

        //public void AddStateChangeTrigger(T fromState, T toState, TimeSpan after)
        //{
        //    _timedTransitions.Add(fromState, new Tuple<T, TimeSpan>(toState, after));
        //}

        public void SetGlobalEnterStateAction(T state, Action enterStateAction)
        {
            if (enterStateAction == null)
                throw new ArgumentNullException("enterStateAction");

            _globalEnterActions[state] = enterStateAction;
        }

        public void SetGlobalExitStateAction(T state, Action exitStateAction)
        {
            if (exitStateAction == null)
                throw new ArgumentNullException("exitStateAction");

            _globalExitActions[state] = exitStateAction;
        }

        //public IDisposable AddStateChangeTrigger(T fromState, T toState, IObservable<Object> trigger)
        //{
        //    return trigger.Where(unit => CurrentState.Equals(fromState)).Subscribe((unit) => TransitionState(fromState, toState, unit));
        //}

        public void AddStateToggleTrigger<TTrigger>(T stateOne, T stateTwo, IObservable<TTrigger> trigger, Action<TTrigger> transitionActionOne, Action<TTrigger> transitionActionTwo)
        {
            trigger.Subscribe(unit =>
            {
                if (CurrentState.Equals(stateOne))
                    TransitionState(stateOne, stateTwo, unit, transitionActionOne);
                else
                    TransitionState(stateTwo, stateOne, unit, transitionActionTwo);

            }, OnStateMachineError);
        }

        public void AddAutomaticStateChange(T fromState, T toState, Action<object> transitionAction)
        {
            Observable.FromEventPattern<StateChangedEventArgs<T>>(this, "StateChanged").Where(evt => evt.EventArgs.CurrentState.Equals(fromState)).Subscribe(evt => TransitionState(fromState, toState, null, transitionAction), OnStateMachineError);
        }

        //public void SetTransitionAction(T fromState, T toState, Action<Object> transitionAction)
        //{
        //    Tuple<T, T> key = new Tuple<T, T>(fromState, toState);
        //    _transitions[key] = transitionAction;
        //}

        /// <summary>
        /// Starts the state machine by transitioning from an indeterminate state to the initial state
        /// </summary>
        public void Start()
        {
            if (_running)
                throw new InvalidOperationException("State machine is already running");

            //enter the target state
            Action enterAction = null;
            if (_globalEnterActions.TryGetValue(CurrentState, out enterAction))
                enterAction();

            if (AssociatedVisualStateManager != null)
                AssociatedVisualStateManager.TriggerStateChange(Name, "", CurrentState.ToString());

            _running = true;
        }

        #endregion

        
        public void TransitionState<TTrigger>(T fromState, T toState, TTrigger trigger, Action<TTrigger> transitionAction)
        {
#if DEBUG
            Console.WriteLine("\nTransitioning " + Name + " from " + fromState + " to " + toState);
#endif
            
            if (!fromState.Equals(toState))
            {
                //exit the current state
                Action exitAction = null;
                if (_globalExitActions.TryGetValue(fromState, out exitAction))
                    exitAction();
            }
            
            //do transition here
            if (transitionAction != null)
                transitionAction(trigger);

            if (!fromState.Equals(toState))
            {
                //enter the target state
                Action enterAction = null;
                if (_globalEnterActions.TryGetValue(toState, out enterAction))
                    enterAction();
            }
            
            //Raise an event indicating that we are about to change the state
            RaiseStateChangingEvent(fromState, toState);

            //Set the new state
            CurrentState = toState;

            //if we have an associated VSM we trigger its StateChange mechanism
            //the VSM will then trigger the ultimate StateChanged event via ExternalStateChanged (see below)
            if (AssociatedVisualStateManager != null)
            {
                AssociatedVisualStateManager.TriggerStateChange(Name, fromState.ToString(), toState.ToString());
            }
            //if there's no associated VSM, we throw the StateChanged event here
            else
            {
                RaiseStateChangedEvent(fromState, toState);
            }
        }

        #region private methods

        private void RaiseStateChangingEvent(T fromState, T toState)
        {
            RaiseStateChangingEvent(fromState.ToString(), toState.ToString());

            if (StateChanging != null)
                StateChanging(this, new StateChangingEventArgs<T>(fromState, toState));
        }

        private void RaiseStateChangedEvent(T fromState, T toState)
        {
            RaiseStateChangedEvent(fromState.ToString(), toState.ToString());

            if (StateChanged != null)
                StateChanged(this, new StateChangedEventArgs<T>(fromState, toState));
        }

        private void RaiseStateMachineExceptionEvent(StateMachineExceptionEventArgs e)
        {
            EventHandler<StateMachineExceptionEventArgs> handler = StateMachineException;
            if (handler != null) handler(this, e);
        }

        private void OnStateMachineError(Exception e)
        {
            RaiseStateMachineExceptionEvent(new StateMachineExceptionEventArgs(e));
        }

        #endregion

        #region internal methods

        internal override void TransitionStateInternal(string fromState, string toState)
        {
            T state1 = (T)Enum.Parse(typeof(T), fromState);
            T state2 = (T)Enum.Parse(typeof(T), toState);

            TransitionState<object>(state1, state2, null, null);
        }

        internal override void ExternalStateChanged(string fromState, string toState)
        {
            T state1 = default(T);

            if (!String.IsNullOrEmpty(fromState))
                state1 = (T)Enum.Parse(typeof(T), fromState);

            T state2 = (T)Enum.Parse(typeof(T), toState);

            RaiseStateChangedEvent(state1, state2);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }

        #endregion

    }
}
