using ReactiveStateMachine;

namespace TouchStateMachine
{
    public class TouchStateMachine<T> : TrackingStateMachine<T>
    {
        public TouchStateMachine(T startState) : this(startState, new WindowsTouchTracker())
        {
            
        }

        public TouchStateMachine(T startState, ITouchTracker inputTracker) : base(startState, inputTracker)
        {
            
        }

        public int TouchCount
        {
            get { return InputTracker.Count; }
        }

        public object OldestContact
        {
            get { return InputTracker.OldestPoint; }
        }

        public object NewestContact
        {
            get { return InputTracker.NewestPoint; }
        }
    }
}
