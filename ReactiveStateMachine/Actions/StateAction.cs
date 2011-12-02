using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStateMachine.Actions
{
    internal class StateAction<T>
    {
        public StateAction(Action action)
        {
            Action = action;
            IsReferenceStateSet = false;
        }

        public StateAction(Action action, Func<bool> condition)
            : this(action)
        {
            Condition = condition;
        }

        public StateAction(Action action, T referenceState)
            : this(action)
        {
            IsReferenceStateSet = true;
            ReferenceState = referenceState;
        }

        public StateAction(Action action, T referenceState, Func<bool> condition)
            : this(action, referenceState)
        {
            Condition = condition;
        }

        public bool IsReferenceStateSet { get; private set; }
        public Action Action { get; private set; }
        public T ReferenceState { get; private set; }
        public Func<bool> Condition { get; private set; }
    }
}
