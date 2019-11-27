using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage.Pipelines.Builders
{
    /// <summary>
    /// A bad implementation of batching:
    ///
    /// 1. Message 1 waits for the batch to fill up or the timeout to trigger (whichever is first)
    /// 2. Messages 2 to N-1 requests add themselves to the batch and wait for the batch to be processed
    /// 3. Message N  add themselves to the batch, alerts Message 1 that the batch is full and waits for the batch to be processed
    /// 4. Message 1 processes the batch and alerts Messages 1 to N that the batch has been processed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ShittyBatcher<T>
    {
        private readonly ILogger<ShittyBatcher<T>> _logger;
        private readonly int _batchSize = 100;
        private readonly TimeSpan _batchTimeout = TimeSpan.FromMilliseconds(500);
        private IList<T> _list;
        private TaskCompletionSource<bool> _batchFullSource;
        private TaskCompletionSource<bool> _batchProcessedSource;

        public ShittyBatcher(ILogger<ShittyBatcher<T>> logger)
        {
            _logger = logger;
        }

        public async Task Add(T t, Func<IReadOnlyCollection<T>, Task> action)
        {
            if (_list == null)
            {
                _list = new List<T>();
                _batchFullSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _batchProcessedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            var list = _list;
            var batchFullSource = _batchFullSource;
            var batchProcessedSource = _batchProcessedSource;

            list.Add(t);

            if (list.Count == 1)
            {
                await Task.WhenAny(Task.Delay(_batchTimeout), batchFullSource.Task);

                _list = null;
                _batchFullSource = null;
                _batchProcessedSource = null;

                var batch = new ReadOnlyCollection<T>(list);

                _logger.LogInformation($"Collected batch of {batch.Count} items");

                await action(batch);

                batchProcessedSource.TrySetResult(true);
            }
            else if (list.Count >= _batchSize)
            {
                batchFullSource.TrySetResult(true);
            }

            await batchProcessedSource.Task;
        }
    }
}
