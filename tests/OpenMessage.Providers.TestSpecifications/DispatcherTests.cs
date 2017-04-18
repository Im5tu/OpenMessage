using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Xunit;

namespace OpenMessage.Providers.TestSpecifications
{
    public abstract class DispatcherTests<T> : TestBase
            where T : class
    {
        private Mock<IDispatchInterceptor<T>> _interceptor;
        protected abstract T InterceptEntity { get; }

        [Fact]
        public void GivenANullEntityThrowArgumentNullException()
        {
            var target = Services.GetRequiredService<IDispatcher<T>>();

            Action act = () => target.DispatchAsync(default(T));
            act.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("entity");
        }

        [Fact]
        public void GivenANullEntityWhenSuppliedWithAScheduleThrowArgumentNullException()
        {
            var target = Services.GetRequiredService<IDispatcher<T>>();

            Action act = () => target.DispatchAsync(default(T), TimeSpan.Zero);
            act.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("entity");
        }

        [Fact]
        public void GivenAnInvalidScheduleTimeThrowArgumentException()
        {
            var target = Services.GetRequiredService<IDispatcher<T>>();

            T entity;
            if (typeof(T).Equals(typeof(string)))
                entity = (T)(object)string.Empty;
            else
                entity = (T)Activator.CreateInstance(typeof(T));

            Action act = () => target.DispatchAsync(entity, TimeSpan.MinValue);
            act.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void GivenAnEntityWhenInterceptorReturnsFalseThenTaskFaultsWithException()
        {
            var target = Services.GetRequiredService<IDispatcher<T>>();

            var tsk = target.DispatchAsync(InterceptEntity);
            try
            {
                tsk.Wait();
            }
            catch { }

            tsk.IsFaulted.Should().BeTrue();
            tsk.Exception.Should().NotBeNull();
            _interceptor.Verify(x => x.Intercept(It.IsAny<T>()), Times.Once);
        }

        protected override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            _interceptor = new Mock<IDispatchInterceptor<T>>();
            _interceptor.Setup(x => x.Intercept(It.IsAny<T>())).Returns((T val) => !ReferenceEquals(val, InterceptEntity));

            return services.AddSingleton(_interceptor.Object);
        }
    }
}
