using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NUnit.Framework;

namespace Tests
{
    [TestFixture, RequiresSTA]
    public class DispatcherTests : AbstractReactiveStateMachineTest
    {
        [Test]
        public void ActionOfAutomaticTransitionCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);

            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>("TestMachine", TestStates.Collapsed);
            var dispatcherObject = new Window();

            var exception = false;

            var transitionAction = new Action(() => dispatcherObject.Dispatcher.VerifyAccess());

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, transitionAction);

            StateMachine.StateMachineException += (sender, args) => { exception = true;};

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.Start();

            while(!evt.WaitOne(50))
            {
                DispatcherHelper.DoEvents();
            }

            Assert.False(exception);
        }

        [Test]
        public void ConditionOfAutomaticTransitionCanAccessDispatcher()
        {
            
        }

        [Test]
        public void ActionOfTriggeredTransitionCanAccessDispatcher()
        {
            
        }

        [Test]
        public void ConditionOfTriggeredTransitionCanAccessDispatcher()
        {
            
        }

        [Test]
        public void ActionOfTimedTransitionCanAccessDispatcher()
        {
            
        }

        [Test]
        public void ConditionOfTimedTransitionCanAccessDispatcher()
        {
            
        }

        [Test]
        public void EntryActionCanAccessDispatcher()
        {
            
        }

        [Test]
        public void ConditionOfEntryActionCanAccessDispatcher()
        {
            
        }

        [Test]
        public void ExitActionCanAccessDispatcher()
        {
            
        }

        [Test]
        public void ConditionOfExitActionCanAccessDispatcher()
        {
            
        }
    }
}
