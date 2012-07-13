using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Microsoft.Surface.Presentation;
using ReactiveStateMachine;

namespace TouchStateMachine
{
    public class SurfaceTouchTracker : InputPointTrackerBase
    {
        public SurfaceTouchTracker()
        {
        }

        public override void Track(EventArgs e)
        {
            if (!(e is TouchEventArgs))
                return;

            var args = e as TouchEventArgs;

            var type = args.TouchDevice.GetType();
            var property = type.GetProperty("Action", BindingFlags.NonPublic | BindingFlags.Instance);

            var contactAction = property.GetValue(args.TouchDevice, null);

            if (TouchAction.Down.Equals(contactAction) && !Contains(args.TouchDevice))
                AddPoint(args.TouchDevice);
            else if (TouchAction.Up.Equals(contactAction) && Contains(args.TouchDevice))
                RemovePoint(args.TouchDevice);
        }
    }
}
