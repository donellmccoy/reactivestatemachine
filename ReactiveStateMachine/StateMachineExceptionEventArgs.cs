using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStateMachine
{
    public class StateMachineExceptionEventArgs : EventArgs
    {
        public StateMachineExceptionEventArgs(Exception e)
        {
            Exception = e;
        }

        public Exception Exception { get; private set; }
    }
}
