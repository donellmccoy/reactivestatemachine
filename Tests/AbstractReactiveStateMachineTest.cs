using System;
using System.Collections.Generic;
using System.Linq;
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


        [SetUp]
        public void SetUpReactiveStateMachineTest()
        {
            
        }

        [TearDown]
        public void TearDownReactiveStateMachineTest()
        {
            
        }
    }
}
