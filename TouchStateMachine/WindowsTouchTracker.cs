using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ReactiveStateMachine;

namespace TouchStateMachine
{
    public class WindowsTouchTracker : InputPointTrackerBase
    {
        public override void Track(EventArgs e)
        {
            if (!(e is TouchEventArgs))
                return;

            var args = e as TouchEventArgs;

            var contactAction = args.TouchDevice.GetTouchPoint(null).Action;

            if (contactAction == TouchAction.Down)
                AddPoint(args.TouchDevice);
            else if (contactAction == TouchAction.Up)
                RemovePoint(args.TouchDevice);
        }
    }
}
