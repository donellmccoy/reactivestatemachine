using System;
using ReactiveStateMachine;

namespace TouchStateMachine
{
    public class TouchStateMachine<T> : TrackingStateMachine<T>
    {
        public TouchStateMachine(String name, T startState) : this(name, startState, new WindowsTouchTracker())
        {
            
        }

        public TouchStateMachine(String name, T startState, ITouchTracker inputTracker) : base(name, startState, inputTracker)
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

        public bool ContainsContact(object contact)
        {
            return (InputTracker as ITouchTracker).ContainsContact(contact);
        }
    }
}
