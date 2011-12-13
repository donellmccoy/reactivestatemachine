using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStateMachine.Configuration
{
    public class TimedTransitionConfiguration<T>
    {
        public TimedTransitionConfiguration(TimeSpan after)
        {
            After = after;
        }

        public TimedTransitionConfiguration<T> From(T fromState)
        {
            FromState = fromState;
            return this;
        }

        public TimedTransitionConfiguration<T> To(T toState)
        {
            ToState = toState;
            return this;
        }

        public TimedTransitionConfiguration<T> Where(Func<bool> condition)
        {
            Condition = condition;
            return this;
        }

        public TimedTransitionConfiguration<T> Do(Action transitionAction)
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
        
        internal Func<bool> Condition { get; private set; }
        internal Action TransitionAction { get; private set; }
        internal TimeSpan After { get; private set; }
    }
}
