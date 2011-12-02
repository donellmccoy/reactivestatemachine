using System;

namespace OldReactiveStateMachine
{
    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(String fromState, String currentState)
        {
            CurrentState = currentState;
            FromState = fromState;
        }

        public String CurrentState { get; set; }
        public String FromState { get; set; }
    }

    public class StateChangedEventArgs<T> : StateChangedEventArgs
    {

        public StateChangedEventArgs(T fromState, T currentState) : base(fromState.ToString(), currentState.ToString())
        {
            CurrentState = currentState;
            FromState = fromState;
        }

        public new T CurrentState { get; set; }
        public new T FromState { get; set; }

    }
}
