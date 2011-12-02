using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStateMachine
{
    public class TrackingStateMachine<T> : ReactiveStateMachine<T>
    {
        protected IInputPointTracker InputTracker {get; private set;}

        public TrackingStateMachine(T startState, IInputPointTracker inputTracker) : base(startState)
        {
            if (inputTracker == null)
                throw new ArgumentNullException("inputTracker");

            InputTracker = inputTracker;
        }

        protected override void TransitionOverride<TTrigger>(TTrigger trigger)
        {
            if (trigger is EventArgs)
                InputTracker.Track(trigger as EventArgs);
        }
    }
}
