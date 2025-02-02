using System.Collections.Concurrent;

namespace Application.Services.Event.Worker
{
    internal class WorkItemsQueue : IWorkItemsQueue
    {
        private ConcurrentQueue<(Func<CancellationToken, Task>, string?)> queue = new();
        private SemaphoreSlim signal = new(0);


        public void Enqueue(Func<CancellationToken, Task> workItem, string? id)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            queue.Enqueue((workItem, id));
            signal.Release();
        }

        public async Task<(Func<CancellationToken, Task>, string?)> Dequeue(CancellationToken cancellationToken)
        {
            await signal.WaitAsync(cancellationToken);
            queue.TryDequeue(out var workItem);

            return workItem;
        }
    }
}
