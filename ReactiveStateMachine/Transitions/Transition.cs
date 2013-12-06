using System;

namespace ReactiveStateMachine.Transitions
{
    internal class Transition<T, TTrigger>
    {
        public Transition(T fromState, T toState, Func<TTrigger, bool> condition, Action<TTrigger> transitionAction)
            : this(fromState, toState, condition, transitionAction, false)
        {
            FromState = fromState;
            ToState = toState;
            Condition = condition;
            TransitionAction = transitionAction;
        }

        public Transition(T fromState, T toState, Func<TTrigger, bool> condition, Action<TTrigger> transitionAction, bool oneTime)
        {
            FromState = fromState;
            ToState = toState;
            Condition = condition;
            TransitionAction = transitionAction;
            OneTime = oneTime;
        }

        public T FromState { get; private set; }
        public T ToState { get; private set; }

        public Action<TTrigger> TransitionAction { get; private set; }
        public Func<TTrigger, bool> Condition { get; private set; }

        public bool OneTime { get; private set; }
    }
}
