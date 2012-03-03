using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStateMachine.Configuration
{
    public class TimedTransitionConfiguration<T> : TransitionConfiguration<T>
    {
        public TimedTransitionConfiguration(TimeSpan after)
        {
            After = after;
        }

        internal TimeSpan After { get; private set; }
    }
}
