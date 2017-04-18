using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace OpenMessage.Providers.TestSpecifications
{
    public abstract class ObservableTests<T> : TestBase
            where T : class
    {
        [Fact]
        public void GivenANullObserverThrowArgumentNullException()
        {
            var target = Services.GetRequiredService<IObservable<T>>();

            Action act = () => target.Subscribe(null);

            act.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("observer");
        }

        [Fact]
        public void GivenAnObserverThenShouldReturnNonNullObserver()
        {
            var target = Services.GetRequiredService<IObservable<T>>();

            target.Subscribe(new ActionObserver<T>(entity => { })).Should().NotBeNull();
        }
    }
}
