using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using NUnit.Framework;
using ReactiveStateMachine;

namespace Tests
{

    public enum TestStates
    {
        NotStarted,
        Collapsed,
        FadingIn,
        Visible,
        FadingOut
    }


    [TestFixture]
    public abstract class AbstractReactiveStateMachineTest
    {

        protected ReactiveStateMachine<TestStates> StateMachine { get; set; }

        protected IObservable<StateChangedEventArgs<TestStates>> StateChanged;

        protected AbstractReactiveStateMachineTest()
        {
            
        }

        [SetUp]
        public void SetUpReactiveStateMachineTest()
        {
            StateMachine = new ReactiveStateMachine<TestStates>("TestStateMachine", TestStates.Collapsed);
            StateChanged = Observable.FromEventPattern<StateChangedEventArgs<TestStates>>(StateMachine, "StateChanged").Select(evt => evt.EventArgs);
        }

        [TearDown]
        public void TearDownReactiveStateMachineTest()
        {
            
        }
    }
}
