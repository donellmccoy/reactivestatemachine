using System;
using System.Windows;

namespace ReactiveStateMachine.Transitions
{
    internal class TimedTransition<T, TTrigger> : Transition<T, TTrigger>
    {
        public TimedTransition(T fromState, T toState, TimeSpan after, Func<TTrigger, bool> condition, Action<TTrigger> transitionAction)
            : this(fromState, toState, after, condition, transitionAction, false)
        {
        }

        public TimedTransition(T fromState, T toState, TimeSpan after, Func<TTrigger, bool> condition, Action<TTrigger> transitionAction, bool oneTime)
            : base(fromState, toState, condition, transitionAction, oneTime)
        {
            After = after;
        }

        public TimeSpan After { get; private set; }
    }
}
