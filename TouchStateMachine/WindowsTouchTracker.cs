using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveStateMachine;

namespace TouchStateMachine
{
    public class WindowsTouchTracker : IInputPointTracker
    {
        public void Track(EventArgs e)
        {
            
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        object IInputPointTracker.OldestPoint
        {
            get { throw new NotImplementedException(); }
        }

        object IInputPointTracker.NewestPoint
        {
            get { throw new NotImplementedException(); }
        }

        public object[] ActivePoints
        {
            get { throw new NotImplementedException(); }
        }
    }
}
