using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ReactiveStateMachine.Transitions;
using ReactiveStateMachine.Triggers;

namespace ReactiveStateMachine
{
    /// <summary>
    /// TODO: Add error handling to all observables !
    /// </summary>
    /// <typeparam name="T"></typeparam>


    public class ReactiveStateMachine<T> : IReactiveStateMachine, INotifyPropertyChanged, IDisposable
    {
        #region private fields

        private BlockingCollection<Action> _queue = new BlockingCollection<Action>();

        private readonly Dictionary<T, State<T>> _states = new Dictionary<T, State<T>>();

        private bool _running;

        internal Dispatcher CurrentDispatcher { get; private set; }

        #endregion

        #region ctor

        public ReactiveStateMachine(String name, T startState)
        {
            Name = name;
            StartState = startState;
            
            if(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                CurrentDispatcher = Dispatcher.CurrentDispatcher;
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

        #region StartState

        private T _startState;

        /// <summary>
        /// Gets or sets the StartState property. This observable property 
        /// indicates ....
        /// </summary>
        public T StartState
        {
            get { return _startState; }
            set
            {
                if (!_currentState.Equals(value))
                {
                    _startState = value;
                    RaisePropertyChanged("StartState");
                }
            }
        }

        #endregion

        #region AssociatedVisualStateManager

        public ReactiveVisualStateManager AssociatedVisualStateManager
        {
            get;
            set;
        }

        #endregion

        #region Name

        public String Name { get; private set; }

        #endregion

        #endregion

        #region public events

        #region State Changed

        public event EventHandler<StateChangedEventArgs<T>> StateChanged;

        private void RaiseStateChanged(StateChangedEventArgs<T> e)
        {
            EventHandler<StateChangedEventArgs<T>> handler = StateChanged;
            if (handler != null) handler(this, e);
        }

        private void RaiseStateChanged(T fromState, T toState)
        {
            RaiseStateChanged(new StateChangedEventArgs<T>(fromState, toState));
        }

        #endregion

        #region State Machine Started

        public event EventHandler StateMachineStarted;

        private void RaiseStateMachineStarted()
        {
            EventHandler handler = StateMachineStarted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region State Machine Stopped

        public event EventHandler StateMachineStopped;

        private void RaiseStateMachineStopped()
        {
            EventHandler handler = StateMachineStopped;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region State Machine Exception

        public event EventHandler<StateMachineExceptionEventArgs> StateMachineException;

        private void RaiseStateMachineException(Exception e)
        {
            RaiseStateMachineException(new StateMachineExceptionEventArgs(e));
        }

        private void RaiseStateMachineException(StateMachineExceptionEventArgs e)
        {
            EventHandler<StateMachineExceptionEventArgs> handler = StateMachineException;
            if (handler != null) handler(this, e);
        }

        #endregion

        #endregion

        #region public methods

        #region State Machine Management

        public void Start()
        {
            if (_running)
                throw new InvalidOperationException("State machine is already running");
            _running = true;

            _queue = new BlockingCollection<Action>();

            Task.Factory.StartNew(() =>
            {
                foreach (Action transition in _queue.GetConsumingEnumerable())
                {
                    transition();
                }
            });

            _queue.Add(StartStateMachineInternal);
        }

        public void Stop()
        {
            if (!_running)
                return;

            _running = false;

            _queue.CompleteAdding();

            while (!_queue.IsCompleted)
                Thread.Sleep(10);

            RaiseStateMachineStopped();
        }

        #endregion

        #region Configuration API

        #region Entry Actions

        /// <summary>
        /// Adds an entry action to the given state, which will be executed whenever the state is entered
        /// </summary>
        /// <param name="enteredState"></param>
        /// <param name="entryAction"></param>
        public void AddEntryAction(T enteredState, Action entryAction)
        {
            AddEntryAction(enteredState, entryAction, null);
        }

        /// <summary>
        /// Adds an entry action to the given state, which will be executed whenever the state is entered and the given condition evaluates to true
        /// </summary>
        /// <param name="enteredState"></param>
        /// <param name="entryAction"></param>
        /// <param name="condition"></param>
        public void AddEntryAction(T enteredState, Action entryAction, Func<bool> condition)
        {
            var state = GetState(enteredState);
            state.AddEntryAction(entryAction, condition);
        }

        /// <summary>
        /// Adds an entry action to the given state, which will be executed whenever the state is entered from the given previous state
        /// </summary>
        /// <param name="enteredState"></param>
        /// <param name="fromState"></param>
        /// <param name="entryAction"></param>
        public void AddEntryAction(T enteredState, T fromState, Action entryAction)
        {
            AddEntryAction(enteredState, fromState, entryAction, null);
        }

        /// <summary>
        /// Adds an entry action to the given state, which will be executed whenever the state is entered from the given previous state and the given condition evaluates to true
        /// </summary>
        /// <param name="enteredState"></param>
        /// <param name="fromState"></param>
        /// <param name="entryAction"></param>
        /// <param name="condition"></param>
        public void AddEntryAction(T enteredState, T fromState, Action entryAction, Func<bool> condition)
        {
            if (enteredState.Equals(fromState))
                throw new InvalidOperationException("entry actions are not allowed/executed for internal transitions, i.e. transitions that start and end in the same state");

            var state = GetState(enteredState);
            state.AddEntryAction(fromState, entryAction, condition);
        }

        #endregion

        #region Exit Actions

        /// <summary>
        /// Adds an exit action to the given state, which will be executed whenever the state is exited
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="exitAction"></param>
        public void AddExitAction(T currentState, Action exitAction)
        {
            AddExitAction(currentState, exitAction, null);
        }

        /// <summary>
        /// Adds an exit action to the given state, which will be executed whenever the state is exited and the given condition evaluates to true
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="exitAction"></param>
        /// <param name="condition"></param>
        public void AddExitAction(T currentState, Action exitAction, Func<bool> condition)
        {
            var state = GetState(currentState);
            state.AddExitAction(exitAction, condition);
        }

        /// <summary>
        /// Adds an exit action to the given state, which will be executed whenever the state is exited to the given next state
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="toState"></param>
        /// <param name="exitAction"></param>
        public void AddExitAction(T currentState, T toState, Action exitAction)
        {
            AddExitAction(currentState, toState, exitAction, null);
        }

        /// <summary>
        /// Adds an exit action to the given state, which will be executed whenever the state is exited to the given next state and the given condition evaluates to true
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="toState"></param>
        /// <param name="exitAction"></param>
        /// <param name="condition"></param>
        public void AddExitAction(T currentState, T toState, Action exitAction, Func<bool> condition)
        {
            if (currentState.Equals(toState))
                throw new InvalidOperationException("exit actions are not allowed/executed for internal transitions, i.e. transitions that start and end in the same state");

            var state = GetState(currentState);
            state.AddExitAction(toState, exitAction, condition);
        }

        #endregion

        #region Triggered Transitions

        public void AddTransition<TTrigger>(T fromState, T toState, IObservable<TTrigger> trigger) where TTrigger : class
        {
            AddTransition(fromState, toState, trigger, null, null);
        }

        public void AddTransition<TTrigger>(T fromState, T toState, IObservable<TTrigger> trigger, Func<TTrigger, bool> condition) where TTrigger : class
        {
            AddTransition(fromState, toState, trigger, condition, null);
        }

        public void AddTransition<TTrigger>(T fromState, T toState, IObservable<TTrigger> trigger, Action<TTrigger> transitionAction) where TTrigger : class
        {
            AddTransition(fromState, toState, trigger, null, transitionAction);
        }

        public void AddTransition<TTrigger>(T fromState, T toState, IObservable<TTrigger> trigger, Func<TTrigger, bool> condition, Action<TTrigger> transitionAction) where TTrigger : class
        {
            AddTransition(fromState, toState, new Trigger<TTrigger>(trigger), condition, transitionAction);
        }

        public void AddTransition<TTrigger>(T fromState, T toState, Trigger<TTrigger> trigger) where TTrigger : class
        {
            AddTransition(fromState, toState, trigger, null, null);
        }

        public void AddTransition<TTrigger>(T fromState, T toState, Trigger<TTrigger> trigger, Func<TTrigger, bool> condition) where TTrigger : class
        {
            AddTransition(fromState, toState, trigger, condition, null);
        }

        public void AddTransition<TTrigger>(T fromState, T toState, Trigger<TTrigger> trigger, Action<TTrigger> transitionAction) where TTrigger : class
        {
            AddTransition(fromState, toState, trigger, null, transitionAction);
        }

        public void AddTransition<TTrigger>(T fromState, T toState, Trigger<TTrigger> trigger, Func<TTrigger, bool> condition, Action<TTrigger> transitionAction) where TTrigger:class
        {
            var stateObject = GetState(fromState);

            var transition = new TriggeredTransition<T, TTrigger>(fromState, toState, trigger, transitionAction, condition);

            stateObject.AddTriggeredTransition(transition);
        }

        #endregion

        #region Timed Transitions

        public void AddTimedTransition(T fromState, T toState, TimeSpan after)
        {
            AddTimedTransition(fromState, toState, after, null, null);
        }

        public void AddTimedTransition(T fromState, T toState, TimeSpan after, Func<bool> condition)
        {
            AddTimedTransition(fromState, toState, after, condition, null);
        }
        
        /// <summary>
        /// Adds a transition from <para>fromState</para> to <para>toState</para>, which will be made when the given time span has elapsed. The given transition action is executed during the transition
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="toState"></param>
        /// <param name="after"></param>
        /// <param name="transitionAction"></param>
        public void AddTimedTransition(T fromState, T toState, TimeSpan after, Action transitionAction)
        {
            AddTimedTransition(fromState, toState, after, null, transitionAction);
        }

        public void AddTimedTransition(T fromState, T toState, TimeSpan after, Func<bool> condition, Action transitionAction)
        {
            var stateObject = GetState(fromState);

            Func<object, bool> realCondition = null;
            if (condition != null)
                realCondition = o => condition();

            Action<object> realAction = null;
            if (transitionAction != null)
                realAction = o => transitionAction();


            stateObject.AddTimedTransition(new TimedTransition<T, object>(fromState, toState, after, realCondition, realAction));
        }

        #endregion

        #region Automatic Transitions

        public void AddAutomaticTransition(T fromState, T toState)
        {
            AddAutomaticTransition(fromState, toState, null, null);
        }

        public void AddAutomaticTransition(T fromState, T toState, Func<bool> condition)
        {
            AddAutomaticTransition(fromState, toState, condition, null);
        }

        /// <summary>
        /// Adds an automatic state transition from <para>fromState</para> to <para>toState</para>. This transition is initiated as soon as <para>fromState</para> is active.
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="toState"></param>
        /// <param name="transitionAction"></param>
        public void AddAutomaticTransition(T fromState, T toState, Action transitionAction)
        {
            AddAutomaticTransition(fromState, toState, null, transitionAction);
        }

        public void AddAutomaticTransition(T fromState, T toState, Func<bool> condition, Action transitionAction)
        {
            var stateObject = GetState(fromState);

            Func<object, bool> realCondition = null;
            if (condition != null)
                realCondition = o => condition();

            Action<object> realAction = null;
            if (transitionAction != null)
                realAction = o => transitionAction();

            stateObject.AddAutomaticTransition(new Transition<T, object>(fromState, toState, realCondition, realAction));
        }

        #endregion

        #region State Toggles

        /// <summary>
        /// Adds a state toggle between <para>stateOne</para> and <para>stateTwo</para>, which will be triggered by the given trigger. The given transition actions are executed during the respective state transition.
        /// </summary>
        /// <typeparam name="TTrigger"></typeparam>
        /// <param name="stateOne"></param>
        /// <param name="stateTwo"></param>
        /// <param name="trigger"></param>
        /// <param name="transitionActionOne"></param>
        /// <param name="transitionActionTwo"></param>
        public void AddStateToggle<TTrigger>(T stateOne, T stateTwo, IObservable<TTrigger> trigger, Action<TTrigger> transitionActionOne, Action<TTrigger> transitionActionTwo) where TTrigger : class
        {
            AddTransition(stateOne, stateTwo, trigger, transitionActionOne);
            AddTransition(stateTwo, stateOne, trigger, transitionActionTwo);
        }

        #endregion

        #endregion

        #endregion

        #region private methods

        private State<T> GetState(T state)
        {
            State<T> stateObject;
            if (!_states.TryGetValue(state, out stateObject))
            {
                stateObject = new State<T>(state, this);
                _states.Add(state, stateObject);
            }
            return stateObject;
        }

        private void StartStateMachineInternal()
        {
            var state = GetState(StartState);

            state.Enter(StartState);
            
            CurrentState = StartState;

            RaiseStateMachineStarted();

            if (state.TryAutomaticTransition())
                return;

            state.ResumeTransitions();
        }

        #endregion

        #region internal methods
        
        internal void TransitionStateInternal<TTrigger>(T fromState, T toState, TTrigger trigger, Action<TTrigger> transitionAction)
        {
            TransitionOverride(trigger);

            if (!CurrentState.Equals(fromState))
                return;

            var isInternalTransition = fromState.Equals(toState);

            Task<bool> vsmTransition = null;

            if (!isInternalTransition && AssociatedVisualStateManager != null)
            {
                vsmTransition = AssociatedVisualStateManager.TransitionState(Name, fromState.ToString(), toState.ToString());
            }

            var currentState = GetState(fromState);
            var futureState = GetState(toState);

            //exit the current state
            if (!isInternalTransition)
            {
                currentState.IgnoreTransitions();
                currentState.Exit(toState);
            }

            //transition from old state to new
            if (transitionAction != null)
            {
                try
                {
                    if (CurrentDispatcher != null)
                        CurrentDispatcher.Invoke(transitionAction, trigger);
                    else
                        transitionAction(trigger);
                }
                catch (Exception e)
                {
                    RaiseStateMachineException(e);
                }
            }

            //enter the next state
            if (!isInternalTransition)
            {
                futureState.Enter(fromState);
                futureState.ResumeTransitions();
            }

            CurrentState = toState;

            RaiseStateChanged(fromState, toState);

            if (vsmTransition == null || vsmTransition.IsCompleted)
                futureState.TryAutomaticTransition();
            else
                vsmTransition.ContinueWith(result => futureState.TryAutomaticTransition());
        }

        internal void EnqueueTransition(Action transition)
        {
            _queue.Add(transition);
        }

        #endregion

        #region protected methods

        protected virtual void TransitionOverride<TTrigger>(TTrigger trigger)
        {
            
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

        #region IDisposable Members
        //TODO: proper IDisposable implementation
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
