using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NUnit.Framework;

namespace Tests
{
    [TestFixture, RequiresSTA]
    public class DispatcherTests : AbstractReactiveStateMachineTest
    {
        [Test]
        public void StateMachineStartedEventHandlerCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);

            var dispatcherObject = new Window();

            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>("TestMachine", TestStates.Collapsed);

            StateMachine.StateMachineStarted += (sender, args) => 
            { 
                Assert.DoesNotThrow( () => dispatcherObject.Dispatcher.VerifyAccess());
                evt.Set();
            };

            StateMachine.Start();

            while (!evt.WaitOne(50))
            {
                DispatcherHelper.DoEvents();
            }
        }

        [Test]
        public void StateChangedEventHandlerCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);

            var dispatcherObject = new Window();

            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>("TestMachine", TestStates.Collapsed);

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);

            StateMachine.StateChanged += (sender, args) =>
            {
                Assert.DoesNotThrow(() => dispatcherObject.Dispatcher.VerifyAccess());
                evt.Set();
            };

            StateMachine.Start();

            while (!evt.WaitOne(50))
            {
                DispatcherHelper.DoEvents();
            }
        }

        [Test]
        public void StateMachineStoppedEventHandlerCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);

            var dispatcherObject = new Window();

            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>("TestMachine", TestStates.Collapsed);

            StateMachine.StateMachineStopped += (sender, args) =>
            {
                Assert.DoesNotThrow(() => dispatcherObject.Dispatcher.VerifyAccess());
                evt.Set();
            };

            StateMachine.Start();

            StateMachine.Stop();

            while (!evt.WaitOne(50))
            {
                DispatcherHelper.DoEvents();
            }
        }

        [Test]
        public void StateMachineExceptionEventHandlerCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);

            var dispatcherObject = new Window();

            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>("TestMachine", TestStates.Collapsed);

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, new Action(() => { throw new Exception(); }));

            StateMachine.StateMachineException += (sender, args) =>
            {
                Assert.DoesNotThrow(() => dispatcherObject.Dispatcher.VerifyAccess());
                evt.Set();
            };

            StateMachine.Start();

            while (!evt.WaitOne(50))
            {
                DispatcherHelper.DoEvents();
            }
        }
    }
}
