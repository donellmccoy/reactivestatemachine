using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveStateMachine.Triggers;

namespace ReactiveStateMachine.Configuration
{
    public class TransitionConfiguration<T, TTrigger>
    {
        public TransitionConfiguration(Trigger<TTrigger> trigger)
        {
            if (trigger == null)
                throw new ArgumentNullException("trigger");

            Trigger = trigger;
        }

        public TransitionConfiguration<T,TTrigger> From(T fromState)
        {
            FromState = fromState;
            return this;
        }

        public TransitionConfiguration<T, TTrigger> To(T toState)
        {
            ToState = toState;
            return this;
        }

        public TransitionConfiguration<T, TTrigger> Where(Func<TTrigger, bool> condition)
        {
            Condition = condition;
            return this;
        }

        public TransitionConfiguration<T, TTrigger> Do(Action<TTrigger> transitionAction)
        {
            TransitionAction = transitionAction;
            return this;
        }

        private T _fromState;
        internal T FromState
        {
            get
            {
                return _fromState;
            }
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
            get { return _toState; }
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
