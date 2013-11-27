using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
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

            var touchDevice = args.TouchDevice;

            TouchAction contactAction;
            try
            {
                contactAction = touchDevice.GetTouchPoint(null).Action;
            }
            catch (Exception)
            {
                contactAction = (TouchAction)GetInstanceField(touchDevice.GetType(), touchDevice, "_lastAction");
            }

            if (contactAction == TouchAction.Down)
                AddPoint(args.TouchDevice);
            else if (contactAction == TouchAction.Up)
                RemovePoint(args.TouchDevice);
        }

        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        ///
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        ///
        /// <returns>The field value from the object.</returns>
        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
    }
}
