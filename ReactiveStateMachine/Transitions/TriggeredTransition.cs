using System;
using ReactiveStateMachine.Triggers;

namespace ReactiveStateMachine.Transitions
{
    internal class TriggeredTransition<T, TTrigger> : Transition<T, TTrigger>
    {
        public TriggeredTransition(T fromState, T toState, Trigger<TTrigger> trigger, Action<TTrigger> transitionAction, Func<TTrigger, bool> condition)
            : this(fromState, toState, trigger, transitionAction, condition, false)
        {
        }

        public TriggeredTransition(T fromState, T toState, Trigger<TTrigger> trigger, Action<TTrigger> transitionAction, Func<TTrigger, bool> condition, bool oneTime)
            : base(fromState, toState, condition, transitionAction, oneTime)
        {
            Trigger = trigger;
        }

        public Trigger<TTrigger> Trigger { get; private set; }
    }
}
