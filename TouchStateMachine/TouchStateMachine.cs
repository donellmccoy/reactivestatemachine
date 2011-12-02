using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ReactiveStateMachine;

namespace TouchStateMachine
{
    public class TouchStateMachine<T> : TrackingStateMachine<T>
    {

        public TouchStateMachine(T startState) : this(startState, new WindowsTouchTracker())
        {
            
        }

        public TouchStateMachine(T startState, IInputPointTracker inputTracker) : base(startState, inputTracker)
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
