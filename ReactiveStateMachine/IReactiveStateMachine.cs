using System;

namespace ReactiveStateMachine
{
    public interface IReactiveStateMachine
    {
        ReactiveVisualStateManager AssociatedVisualStateManager { get; set; }

        void ExternalStateChanged(String fromState, String toState);
    }
}
