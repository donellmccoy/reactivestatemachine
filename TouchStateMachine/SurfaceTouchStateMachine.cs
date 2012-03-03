using System;
using System.Windows.Input;
using ReactiveStateMachine;

namespace TouchStateMachine
{
    public class SurfaceTouchStateMachine<T> : TrackingStateMachine<T>
    {
        public SurfaceTouchStateMachine(String name, T startState) : base(name, startState, new SurfaceTouchTracker())
        {
            
        }


    }
}
