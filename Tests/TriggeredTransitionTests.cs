using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TriggeredTransitionTests : AbstractReactiveStateMachineTest
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

            startedEvt.WaitOne();

            trigger.OnNext(null);
            
            evt.WaitOne();

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

            startedEvt.WaitOne();

            trigger.OnNext(null);

            evt.WaitOne();

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

            startedEvt.WaitOne();

            trigger.OnNext(null);

            evt.WaitOne(2000);

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

            startedEvt.WaitOne();

            trigger.OnNext(null);
            
            evt.WaitOne();

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

            startedEvt.WaitOne();

            trigger.OnNext(null);

            evt.WaitOne();

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

            startedEvt.WaitOne();

            trigger.OnNext(null);

            evt.WaitOne(2000);

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

            startedEvt.WaitOne();

            trigger.OnNext(null);

            evt.WaitOne();

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

            StateMachine.StateMachineException += (sender, args) => exceptionHandledAndReported = true;

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();

            StateMachine.Start();

            startedEvt.WaitOne();

            trigger.OnNext(null);

            evt.WaitOne(2000);

            Assert.True(exceptionHandledAndReported);
            Assert.AreEqual(StateMachine.CurrentState, TestStates.Collapsed);
        }


    }
}
