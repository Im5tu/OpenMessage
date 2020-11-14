using OpenMessage.Pipelines.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpenMessage.Tests.Pipelines
{
    public class BatcherTests
    {
        private readonly BatcherBase<string> _batcher;
        private readonly int _batchSize = 3;
        private readonly IList<IReadOnlyCollection<string>> _history = new List<IReadOnlyCollection<string>>();
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(3);

        public BatcherTests(ITestOutputHelper testOutputHelper) => _batcher = new TestBatcher(testOutputHelper, _history, _timeout, _batchSize);

        [Fact]
        public async Task WhenBatchDoesNotFillUpBeforeTimeout_ThenASmallBatchIsProcessedAfterTheTimeout()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            await Task.WhenAll(Enumerable.Range(0, _batchSize - 1)
                                         .Select(i => _batcher.BatchAsync($"{i}")));
            stopwatch.Stop();

            Assert.Equal(1, _history.Count);

            Assert.Equal(_batchSize - 1, _history.Single().Count);

            Assert.True(stopwatch.Elapsed.Add(TimeSpan.FromMilliseconds(5)) >= _timeout);
        }

        [Fact]
        public async Task WhenBatchFillsUpBeforeTimeout_ThenItIsProcessedEarly()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            await Task.WhenAll(Enumerable.Range(0, _batchSize)
                                         .Select(i => _batcher.BatchAsync($"{i}")));
            stopwatch.Stop();

            Assert.Equal(1, _history.Count);

            Assert.Equal(_batchSize, _history.Single().Count);

            Assert.True(stopwatch.Elapsed < _timeout);
        }

        private class TestBatcher : BatcherBase<string>
        {
            private readonly IList<IReadOnlyCollection<string>> _batches;
            private readonly ITestOutputHelper _testOutputHelper;

            public TestBatcher(ITestOutputHelper testOutputHelper, IList<IReadOnlyCollection<string>> batches, TimeSpan timeout, int batchSize)
                : base(batchSize, timeout)
            {
                _testOutputHelper = testOutputHelper;
                _batches = batches;
            }

            protected override Task OnBatchAsync(IReadOnlyCollection<string> batch)
            {
                _batches.Add(batch);
                _testOutputHelper.WriteLine($"Received batch of {batch.Count} items: {string.Join(", ", batch)}");

                return Task.CompletedTask;
            }
        }
    }
}