using System;

namespace ReactiveStateMachine.Transitions
{
    internal class TimedTransition<T, TTrigger> : Transition<T, TTrigger>
    {
        public TimedTransition(T fromState, T toState, TimeSpan after, Func<TTrigger, bool> condition, Action<TTrigger> transitionAction) : base(fromState, toState, condition, transitionAction)
        {
            After = after;
        }

        public TimeSpan After { get; private set; }
    }
}
