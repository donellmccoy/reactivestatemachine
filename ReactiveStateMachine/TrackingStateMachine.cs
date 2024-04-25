using System;

namespace ReactiveStateMachine
{
    public class TrackingStateMachine<T> : ReactiveStateMachine<T>
    {
        protected IInputPointTracker InputTracker {get; private set;}

        public TrackingStateMachine(string name, T startState, IInputPointTracker inputTracker) : base(name, startState)
        {
            InputTracker = inputTracker ?? throw new ArgumentNullException(nameof(inputTracker));
        }

        protected override void TransitionOverride<T, TTrigger>(T fromState, T toState, TTrigger trigger)
        {
            if (trigger is EventArgs args)
            {
                InputTracker.Track(args);
            }
        }
    }
}
