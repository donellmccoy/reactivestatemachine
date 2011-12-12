using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NUnit.Framework;

namespace Tests
{
    [TestFixture, RequiresSTA]
    public class TriggeredTransitionOnDispatcherTests : AbstractReactiveStateMachineTest
    {
        IDisposable _stateChangedSubscription;

        [Test]
        public void TriggeredTransitionIsMade()
        {
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);

            var transitionMade = false;

            var trigger = new Subject<Object>();
            
            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(null);

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(transitionMade);
        }

        [Test]
        public void TriggeredTransitionWithConditionIsMade()
        {
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);

            var transitionMade = false;

            var trigger = new Subject<Object>();

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, args => true);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(null);

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(transitionMade);
        }

        [Test]
        public void TriggeredTransitionWithConditionIsNotMade()
        {
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);

            var transitionMade = false;

            var trigger = new Subject<Object>();

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, args => false);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(null);

            evt.WaitOne(2000);

            DispatcherHelper.DoEvents();

            Assert.False(transitionMade);
            Assert.AreEqual(StateMachine.CurrentState, TestStates.Collapsed);
        }

        [Test]
        public void TransitionActionOfTriggeredTransitionIsCalled()
        {
            var trigger = new Subject<object>();
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);
            var transitionActionCalled = false;

            Action<object> transitionAction = o => transitionActionCalled = true;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(null);

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(transitionActionCalled);
        }

        [Test]
        public void TransitionActionOfTriggeredTransitionWithConditionIsCalled()
        {
            var trigger = new Subject<object>();
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);
            var transitionActionCalled = false;

            Action<object> transitionAction = o => transitionActionCalled = true;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, o => true, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(null);

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(transitionActionCalled);
        }

        [Test]
        public void TransitionActionOfTriggeredTransitionWithConditionIsNotCalled()
        {
            var trigger = new Subject<object>();
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);
            var transitionActionCalled = false;

            Action<object> transitionAction = o => transitionActionCalled = true;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, o => false, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(null);

            evt.WaitOne(2000);

            DispatcherHelper.DoEvents();

            Assert.False(transitionActionCalled);
            Assert.AreEqual(StateMachine.CurrentState, TestStates.Collapsed);
        }

        [Test]
        public void ExceptionInTransitionActionOfTriggeredTransitionIsHandledAndReported()
        {
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);
            var trigger = new Subject<Object>();
            var exceptionHandledAndReported = false;

            var transitionAction = new Action<object>(o => { throw new Exception(); });

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, transitionAction);

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.StateMachineException += (sender, args) => exceptionHandledAndReported = true;

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(null);

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(exceptionHandledAndReported);
        }

        [Test]
        public void ExceptionInConditionOfTriggeredTransitionIsHandledAndReported()
        {
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);
            var trigger = new Subject<Object>();
            var exceptionHandledAndReported = false;

            var condition = new Func<object,bool>(o => { throw new Exception(); });

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, condition);

            StateMachine.StateMachineException += (sender, args) =>
            {
                exceptionHandledAndReported = true;
                evt.Set();
            };

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(null);

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(exceptionHandledAndReported);
            Assert.AreEqual(StateMachine.CurrentState, TestStates.Collapsed);
        }

        [Test]
        public void ActionOfTriggeredTransitionCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);

            var startedEvt = new ManualResetEvent(false);
            
            var trigger = new Subject<Object>();

            var dispatcherObject = new Window();

            var transitionAction = new Action<object>(o => Assert.DoesNotThrow(() => dispatcherObject.Dispatcher.VerifyAccess()));

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, transitionAction);

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(null);

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();
        }

        [Test]
        public void ConditionOfTriggeredTransitionCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);
            var trigger = new Subject<Object>();
            
            var dispatcherObject = new Window();

            var condition = new Func<object, bool>(o =>
            {
                Assert.DoesNotThrow(() => dispatcherObject.Dispatcher.VerifyAccess());
                return true;
            });

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, condition);

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            while (!startedEvt.WaitOne(50))
                DispatcherHelper.DoEvents();

            trigger.OnNext(null);

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();
        }
    }
}
