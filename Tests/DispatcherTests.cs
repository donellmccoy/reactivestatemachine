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
            var evt = new ManualResetEvent(false);

            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>("TestMachine", TestStates.Collapsed);
            var dispatcherObject = new Window();

            var exception = false;

            var condition = new Func<bool>(() =>
            {
                dispatcherObject.Dispatcher.VerifyAccess();
                return true;
            });

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, condition);

            StateMachine.StateMachineException += (sender, args) => { exception = true; };

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.Start();

            while (!evt.WaitOne(50))
            {
                DispatcherHelper.DoEvents();
            }

            Assert.False(exception);
        }

        [Test]
        public void ActionOfTriggeredTransitionCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);

            var trigger = new Subject<object>();

            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>("TestMachine", TestStates.Collapsed);
            var dispatcherObject = new Window();

            var exception = false;

            var transitionAction = new Action<object>(o => dispatcherObject.Dispatcher.VerifyAccess());

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn,trigger, transitionAction);

            StateMachine.StateMachineException += (sender, args) => { exception = true; };

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(new object());

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();
            

            Assert.False(exception);
        }

        [Test]
        public void ConditionOfTriggeredTransitionCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);

            var trigger = new Subject<object>();

            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>("TestMachine", TestStates.Collapsed);
            var dispatcherObject = new Window();

            var exception = false;

            var condition = new Func<object, bool>(o =>
            {
                dispatcherObject.Dispatcher.VerifyAccess();
                return true;
            });

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, condition);

            StateMachine.StateMachineException += (sender, args) => { exception = true;
                                                                        evt.Set();
            };

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Task.Factory.StartNew(() => trigger.OnNext(new object()));

            while (!evt.WaitOne(50))
            {
                DispatcherHelper.DoEvents();
            }

            Assert.False(exception);
        }

        [Test]
        public void ActionOfTimedTransitionCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);

            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>("TestMachine", TestStates.Collapsed);
            var dispatcherObject = new Window();

            var exception = false;

            var transitionAction = new Action(() => dispatcherObject.Dispatcher.VerifyAccess());

            StateMachine.AddTimedTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), transitionAction);

            StateMachine.StateMachineException += (sender, args) => { exception = true; };

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();
            
            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.False(exception);
        }

        [Test]
        public void ConditionOfTimedTransitionCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);

            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>("TestMachine", TestStates.Collapsed);
            var dispatcherObject = new Window();

            var exception = false;

            var condition = new Func<bool>(() =>
            {
                dispatcherObject.Dispatcher.VerifyAccess();
                return true;
            });

            StateMachine.AddTimedTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), condition);

            StateMachine.StateMachineException += (sender, args) =>
            {
                exception = true;
                evt.Set();
            };

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            while (!evt.WaitOne(50))
            {
                DispatcherHelper.DoEvents();
            }

            Assert.False(exception);
        }

        

        

        
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
