using System;
using System.Security;
using System.Security.Permissions;
using System.Windows.Threading;

namespace Tests
{
    public static class DispatcherHelper
    {
        

        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new ExitFrameHandler(frm => frm.Continue = false), frame);
            Dispatcher.PushFrame(frame);
        }

        private delegate void ExitFrameHandler(DispatcherFrame frame);
    }
}
