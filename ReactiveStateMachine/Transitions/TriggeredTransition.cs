using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveStateMachine.Triggers;

namespace ReactiveStateMachine.Transitions
{
    internal class TriggeredTransition<T, TTrigger> : Transition<T, TTrigger> where TTrigger:class
    {
        public TriggeredTransition(T fromState, T toState, Trigger<TTrigger> trigger, Action<TTrigger> transitionAction, Func<TTrigger, bool> condition) : base(fromState, toState, condition, transitionAction)
        {
            Trigger = trigger;
        }

        public Trigger<TTrigger> Trigger { get; private set; }
    }
}
