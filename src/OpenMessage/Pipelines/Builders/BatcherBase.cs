using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMessage.Pipelines.Builders
{
    internal abstract class BatcherBase<T>
    {
        private Batch _currentBatch;

        protected BatcherBase(int batchSize, TimeSpan timeout)
        {
            _currentBatch = new Batch(batchSize);

            _ = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var nextBatch = new Batch(batchSize);
                    var batch = Interlocked.Exchange(ref _currentBatch, nextBatch);

                    if (batch is {})
                    {
                        // General flow:
                        //--------------------
                        // 1. Complete the batch to prevent further additions
                        // 2. Check to see whether we should trigger the OnBatchAsync method
                        // 3. Set the task completion source
                        var currentBatch = batch.Flush();

                        if (currentBatch.Count > 0)
                            _ = Task.Factory.StartNew(async () =>
                            {
                                //"Fire and forget"; lets not block up the batcher while waiting for it to process
                                try
                                {
                                    await OnBatchAsync(currentBatch);
                                    batch.BatchProcessedTaskCompletionSource.SetResult(true);
                                }
                                catch (Exception ex)
                                {
                                    // Leave the logging of the exception to the consumer
                                    batch.BatchProcessedTaskCompletionSource.SetException(ex);
                                }
                            });
                    }

                    //Wait for a timeout, or the next batch to complete
                    await Task.WhenAny(Task.Delay(timeout), nextBatch.BatchFullTaskCompletionSource.Task);
                }
            }, default, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public Task BatchAsync(T entity)
        {
            var successful = false;

            while (!successful)
            {
                // Take a reference to the current instance
                // This prevents a race condition between adding and returning the task
                var current = _currentBatch;

                if (successful = current.AddToBatch(entity))
                    return current.BatchProcessedTaskCompletionSource.Task;
            }

            throw new InvalidOperationException("Batch failed to add successfully. This should never happen...");
        }

        /// <summary>
        ///     The action to execute when a batch is created
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        protected abstract Task OnBatchAsync(IReadOnlyCollection<T> batch);

        private class Batch
        {
            private readonly List<T> _batch;
            private readonly int _batchSize;
            private bool _batchCompleted;
            internal TaskCompletionSource<bool> BatchFullTaskCompletionSource { get; } = new TaskCompletionSource<bool>(TaskContinuationOptions.RunContinuationsAsynchronously);

            internal TaskCompletionSource<bool> BatchProcessedTaskCompletionSource { get; } = new TaskCompletionSource<bool>(TaskContinuationOptions.RunContinuationsAsynchronously);

            public Batch(int batchSize)
            {
                _batchSize = batchSize;
                _batch = new List<T>(_batchSize);
            }

            internal bool AddToBatch(T entity)
            {
                if (_batchCompleted)
                    // Quicker exit if we've already finished
                    return false;

                lock (_batch)
                {
                    // Double check that we haven't already completed this batch
                    if (!_batchCompleted)
                    {
                        _batch.Add(entity);

                        //If the batch is full, trigger the batch to process early
                        if (_batch.Count >= _batchSize)
                        {
                            BatchFullTaskCompletionSource.TrySetResult(true);
                            _batchCompleted = true;

                            return true;
                        }
                    }

                    return !_batchCompleted;
                }
            }

            internal List<T> Flush()
            {
                lock (_batch)
                {
                    _batchCompleted = true;

                    return _batch;
                }
            }
        }
    }
}