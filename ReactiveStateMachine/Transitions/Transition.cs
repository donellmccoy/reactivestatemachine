using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStateMachine.Transitions
{
    internal class Transition<T, TTrigger>
    {
        public Transition(T fromState, T toState, Func<TTrigger, bool> condition, Action<TTrigger> transitionAction)
        {
            FromState = fromState;
            ToState = toState;
            Condition = condition;
            TransitionAction = transitionAction;
        }

        public T FromState { get; private set; }
        public T ToState { get; private set; }

        public Action<TTrigger> TransitionAction { get; private set; }
        public Func<TTrigger, bool> Condition { get; private set; }
    }
}
