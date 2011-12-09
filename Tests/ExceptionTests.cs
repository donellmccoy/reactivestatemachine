using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ReactiveStateMachine;

namespace Tests
{
    [TestFixture]
    public class ExceptionTests : AbstractReactiveStateMachineTest
    {

        [Test]
        public void ExceptionInTransitionAction()
        {
            var evt = new ManualResetEvent(false);

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, new Action(() => { throw new Exception(); }));
            StateMachine.StateMachineException += (sender,args) => evt.Set();
            
            StateMachine.Start();

            evt.WaitOne();
        }
    }
}
