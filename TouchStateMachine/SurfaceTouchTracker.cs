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
        private static PropertyInfo _inputArgsProperty;
        private static PropertyInfo _inputRecordProperty;
        private static PropertyInfo _contactActionProperty;
        private static object _contactAdd;
        private static object _contactRemove;

        public SurfaceTouchTracker()
        {
            var contactType = typeof (Touch);
            _inputArgsProperty = contactType.GetProperty("InputArgs", BindingFlags.NonPublic | BindingFlags.Instance);
            _inputRecordProperty = contactType.Assembly.GetType("Microsoft.Surface.Presentation.RawInputSurfaceEventArgs", true).GetProperty("InputRecord");
            _contactActionProperty = contactType.Assembly.GetType("Microsoft.Surface.Presentation.RawInputContactRecord").GetProperty("Action");

            var enumValues = contactType.Assembly.GetType("Microsoft.Surface.Presentation.RawContactAction").GetEnumValues();

            _contactAdd = enumValues.GetValue(0);
            _contactRemove = enumValues.GetValue(2);
        }

        public override void Track(EventArgs e)
        {
            if (!(e is TouchEventArgs))
                return;

            var args = e as TouchEventArgs;

            var rawInputEventArgs = _inputArgsProperty.GetValue(args.TouchDevice, null);
            var inputRecord = _inputRecordProperty.GetValue(rawInputEventArgs, null);
            var contactAction = _contactActionProperty.GetValue(inputRecord, null);

            if (contactAction.Equals(_contactAdd) && !Contains(args.TouchDevice))
                AddPoint(args.TouchDevice);
            else if (contactAction.Equals(_contactRemove) && Contains(args.TouchDevice))
                RemovePoint(args.TouchDevice);
        }
    }
}
