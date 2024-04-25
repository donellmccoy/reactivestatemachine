using System;
using ReactiveStateMachine.Triggers;

namespace ReactiveStateMachine.Configuration
{
    public class TriggeredTransitionConfiguration<T, TTrigger>
    {
        public TriggeredTransitionConfiguration(Trigger<TTrigger> trigger)
        {
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        }

        public TriggeredTransitionConfiguration<T,TTrigger> From(T fromState)
        {
            FromState = fromState;
            return this;
        }

        public TriggeredTransitionConfiguration<T, TTrigger> To(T toState)
        {
            ToState = toState;
            return this;
        }

        public TriggeredTransitionConfiguration<T, TTrigger> Where(Func<TTrigger, bool> condition)
        {
            var existingCondition = Condition;

            Condition = existingCondition != null ? trigger => existingCondition(trigger) && condition(trigger) : condition;

            return this;
        }

        public TriggeredTransitionConfiguration<T, TTrigger> Do(Action<TTrigger> transitionAction)
        {
            TransitionAction = transitionAction;
            return this;
        }

        private T _fromState;
        internal T FromState
        {
            get => _fromState;
            private set
            {
                _fromState = value;
                IsFromStateSet = true;
            }
        }

        internal bool IsFromStateSet { get; private set; }

        private T _toState;
        internal T ToState
        {
            get => _toState;
            private set
            {
                _toState = value;
                IsToStateSet = true;
            }
        }

        internal bool IsToStateSet { get; private set; }
        
        internal Func<TTrigger, bool> Condition { get; private set; }
        internal Action<TTrigger> TransitionAction { get; private set; }
        internal Trigger<TTrigger> Trigger { get; private set; }
    }
}
