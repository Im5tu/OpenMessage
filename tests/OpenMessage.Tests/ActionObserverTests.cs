using FluentAssertions;
using System;
using Xunit;

namespace OpenMessage.Tests
{
    public class ActionObserverTests
    {
        public class Constructor
        {
            [Fact]
            public void GivenANullActionThrowsArgumentNullException()
            {
                Action act = () => new ActionObserver<string>(null);

                act.ShouldThrow<ArgumentNullException>();
            }
        }

        public class OnNext
        {
            [Fact]
            public void GivenANullEntityThrowArgumentNullException()
            {
                Action<string> callback = str => { };
                var target = new ActionObserver<string>(callback);

                Action act = () => target.OnNext(null);
            }

            [Fact]
            public void GivenOnCompleteHasBeenCalledThenCallbackIsNotInvoked()
            {
                var callbackInvoked = false;
                Action<string> callback = str => { callbackInvoked = true; };
                var target = new ActionObserver<string>(callback);

                target.OnCompleted();
                target.OnNext("");

                callbackInvoked.Should().BeFalse();
            }

            [Fact]
            public void GivenAnEntityIsSuppliedThenCallbackIsInvoked()
            {
                var callbackInvoked = false;
                Action<string> callback = str => { callbackInvoked = true; };
                var target = new ActionObserver<string>(callback);

                target.OnNext("");

                callbackInvoked.Should().BeTrue();
            }
        }
    }
}
