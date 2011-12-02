using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStateMachine
{
    public interface IReactiveStateMachine
    {
        ReactiveVisualStateManager AssociatedVisualStateManager { get; set; }

        void ExternalStateChanged(String fromState, String toState);
    }
}
