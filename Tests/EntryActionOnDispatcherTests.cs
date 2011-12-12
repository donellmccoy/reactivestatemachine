using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using NUnit.Framework;

namespace Tests
{
    [TestFixture, RequiresSTA]
    public class EntryActionOnDispatcherTests : AbstractReactiveStateMachineTest
    {
        IDisposable _stateChangedSubscription;

        #region single entry action

        [Test]
        public void SingleEntryActionIsCalled()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddEntryAction(TestStates.FadingIn, entryAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(entryActionCalled);
        }


        #endregion

        #region multiple entry actions

        [Test]
        public void MultipleEntryActionsAreCalled()
        {
            var evt = new ManualResetEvent(false);
            const int numEntryActionsToCall = 10;
            var numEntryActionsCalled = 0;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);

            Action entryAction = () => numEntryActionsCalled++;

            for (int i = 0; i < numEntryActionsToCall; i++)
            {
                StateMachine.AddEntryAction(TestStates.FadingIn, entryAction);
            }

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.AreEqual(numEntryActionsToCall, numEntryActionsCalled);
        }

        #endregion

        #region entry actions in series

        [Test]
        public void EntryActionsAreCalledInSeries()
        {
            var evt = new ManualResetEvent(false);
            const int numEntryActionsToCall = 5;
            var numEntryActionsCalled = 0;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddAutomaticTransition(TestStates.FadingIn, TestStates.Visible);
            StateMachine.AddAutomaticTransition(TestStates.Visible, TestStates.FadingOut);
            StateMachine.AddAutomaticTransition(TestStates.FadingOut, TestStates.NotStarted);

            Action entryAction = () => numEntryActionsCalled++;

            StateMachine.AddEntryAction(TestStates.Collapsed, entryAction);
            StateMachine.AddEntryAction(TestStates.FadingIn, entryAction);
            StateMachine.AddEntryAction(TestStates.Visible, entryAction);
            StateMachine.AddEntryAction(TestStates.FadingOut, entryAction);
            StateMachine.AddEntryAction(TestStates.NotStarted, entryAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.NotStarted).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.AreEqual(numEntryActionsToCall, numEntryActionsCalled);
        }

        #endregion

        #region conditional entry actions

        [Test]
        public void ConditionalEntryActionIsCalled()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddEntryAction(TestStates.FadingIn, entryAction, () => true);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(entryActionCalled);
        }

        [Test]
        public void ConditionalEntryActionIsNotCalled()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddEntryAction(TestStates.FadingIn, entryAction, () => false);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                evt.Set();
                _stateChangedSubscription.Dispose();
            });

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.False(entryActionCalled);
        }

        #endregion

        #region entry actions for specific transitions

        [Test]
        public void EntryActionIsCalledOnSpecificTransition()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddEntryAction(TestStates.FadingIn, TestStates.Collapsed, entryAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                evt.Set();
                _stateChangedSubscription.Dispose();
            });

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(entryActionCalled);
        }

        [Test]
        public void EntryActionIsNotCalledOnOtherTransitions()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddEntryAction(TestStates.FadingIn, TestStates.NotStarted, entryAction);
            StateMachine.AddEntryAction(TestStates.FadingIn, TestStates.FadingOut, entryAction);
            StateMachine.AddEntryAction(TestStates.FadingIn, TestStates.Visible, entryAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                evt.Set();
                _stateChangedSubscription.Dispose();
            });

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.False(entryActionCalled);
        }

        #endregion

        #region entry action on start state

        [Test]
        public void EntryActionIsCalledOnStartState()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddEntryAction(TestStates.Collapsed, entryAction);

            StateMachine.StateMachineStarted += (sender, args) => evt.Set();

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(entryActionCalled);
        }

        #endregion

        #region exception in entry action

        [Test]
        public void ExceptionInEntryActionIsHandledAndReported()
        {
            var evt = new ManualResetEvent(false);
            var exceptionHandledAndReported = false;

            Action entryAction = () => { throw new Exception(); };

            StateMachine.AddEntryAction(TestStates.Collapsed, entryAction);

            StateMachine.StateMachineStarted += (sender, args) => evt.Set();

            StateMachine.StateMachineException += (sender, args) => exceptionHandledAndReported = true;

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(exceptionHandledAndReported);
        }

        #endregion

        #region exception in condition of entry action

        [Test]
        public void ExceptionInConditionOfEntryActionIsHandledAndReported()
        {
            var evt = new ManualResetEvent(false);
            var exceptionHandledAndReported = false;

            Action entryAction = () => { };
            Func<bool> condition = () => { throw new Exception(); };

            StateMachine.AddEntryAction(TestStates.Collapsed, entryAction, condition);

            StateMachine.StateMachineStarted += (sender, args) => evt.Set();

            StateMachine.StateMachineException += (sender, args) => exceptionHandledAndReported = true;

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();

            Assert.True(exceptionHandledAndReported);
        }

        #endregion

        #region dispatcher access

        [Test]
        public void EntryActionCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);

            var dispatcherObject = new Window();

            var entryAction = new Action(() => Assert.DoesNotThrow(() => dispatcherObject.Dispatcher.VerifyAccess()));

            StateMachine.AddEntryAction(TestStates.Collapsed, entryAction);

            StateMachine.StateMachineStarted += (sender, args) => evt.Set();

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();
        }

        [Test]
        public void ConditionOfEntryActionCanAccessDispatcher()
        {
            var evt = new ManualResetEvent(false);

            var dispatcherObject = new Window();

            var entryAction = new Action(() => { });

            var condition = new Func<bool>(() =>
            {
                Assert.DoesNotThrow(() => dispatcherObject.Dispatcher.VerifyAccess());
                return true;
            });

            StateMachine.AddEntryAction(TestStates.Collapsed, entryAction, condition);

            StateMachine.StateMachineStarted += (sender, args) => evt.Set();

            StateMachine.Start();

            while (!evt.WaitOne(50))
                DispatcherHelper.DoEvents();
        }

        #endregion        
    }
}
