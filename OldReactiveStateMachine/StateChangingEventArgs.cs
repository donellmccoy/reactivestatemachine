using System;

namespace OldReactiveStateMachine
{

    public class StateChangingEventArgs : EventArgs
    {
        public StateChangingEventArgs(String currentState, String targetState)
        {
            CurrentState = currentState;
            TargetState = targetState;
        }

        public String CurrentState { get; set; }
        public String TargetState { get; set; }
    }

    public class StateChangingEventArgs<T> : StateChangingEventArgs
    {
        public StateChangingEventArgs(T currentState, T targetState) : base(currentState.ToString(), targetState.ToString())
        {
            CurrentState = currentState;
            TargetState = targetState;
        }

        public new T CurrentState { get; set; }
        public new T TargetState { get; set; }
    }
}
