using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStateMachine
{
    public class StateMachineConfigurationException : Exception
    {
        public StateMachineConfigurationException(String message) : base(message)
        {
            
        }
    }
}
