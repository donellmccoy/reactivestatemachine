using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ConfigurationTests : AbstractReactiveStateMachineTest
    {
        [Test]
        public void StartStateIsSetCorrectly()
        {
            Assert.AreEqual(StateMachine.StartState, TestStates.Collapsed);
        }

        [Test]
        public void EntryActionIsNotAllowedForInternalTransitions()
        {
            Assert.Throws<InvalidOperationException>(() => StateMachine.AddEntryAction(TestStates.Collapsed, TestStates.Collapsed, ()=> {}));
        }

    }
}
