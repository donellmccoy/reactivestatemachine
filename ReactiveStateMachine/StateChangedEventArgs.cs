using System;

namespace ReactiveStateMachine
{
    public class StateChangedEventArgs<T> : EventArgs
    {
        public StateChangedEventArgs(T fromState, T toState)
        {
            FromState = fromState;
            ToState = toState;
        }

        public T FromState { get; private set; }
        public T ToState { get; private set; }
    }
}
