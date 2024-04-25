using System;

namespace ReactiveStateMachine.Actions
{
    internal class StateAction<T>
    {
        public StateAction(Action action, Func<bool> condition)
        {
            Action = action;
            Condition = condition;
            IsReferenceStateSet = false;
        }

        public StateAction(Action action, T referenceState, Func<bool> condition) : this(action, condition)
        {
            ReferenceState = referenceState;
            IsReferenceStateSet = true;
        }

        public Action Action { get; private set; }
        public Func<bool> Condition { get; private set; }
        public T ReferenceState { get; private set; }
        public bool IsReferenceStateSet { get; private set; }
    }
}
