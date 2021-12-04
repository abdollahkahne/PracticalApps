using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ContosoWorker
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, ValueTask>> _queue;
        public BackgroundTaskQueue(int capacity)
        {
            // To intitialize the channel (which is used to synchronize work between producer-consumer components in concurrent processing)
            // we may need to enable some option like its capacity and its FullMode (What should it do when it is full )
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
            };
            _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);// initialize the channel
        }
        public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken stoppingToken)
        {
            var workItem = await _queue.Reader.ReadAsync(stoppingToken);// This can be used without asyncc-await since the return type is similar to return of function
            return workItem;
        }

        public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
        {
            if (workItem == null)
                throw new ArgumentNullException(nameof(workItem));
            await _queue.Writer.WriteAsync(workItem); // Here we should use await since we want assure that the item added to channel
        }
    }
    public interface IBackgroundTaskQueue
    {
        // We use ValueTask instead of Task since we only enqueue or dequeue tasks once
        // Value Task is a Task that implemented using struct as value type. It has some limitation but it is performant
        // only one time we can use await for it and the second time we get undefined
        // we can not use it as Task and only we should use it as await since it is not defined until the work completed
        // If we want To use it As Task we can use its AsTask() method but only once! after that it is undefined
        ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
        ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken stoppingToken);
    }
}