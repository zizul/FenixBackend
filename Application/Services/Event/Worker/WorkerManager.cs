using System.Collections.Concurrent;

namespace Application.Services.Event.Worker
{
    internal class WorkerManager : IWorkerManager
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource> loopJobsCancellationTokens = new();

        private readonly IWorkItemsQueue queue;


        public WorkerManager(IWorkItemsQueue queue)
        {
            this.queue = queue;
        }

        public void AddSingleRunJob(Func<CancellationToken, Task> workerJob)
        {
            queue.Enqueue(workerJob);
        }

        public void AddLoopJob(string id, Func<CancellationToken, Task> workerJob)
        {
            var tokenSource = new CancellationTokenSource();
            loopJobsCancellationTokens.AddOrUpdate(id, tokenSource, (key, oldValue) => oldValue);

            queue.Enqueue(workerJob, id);
        }

        public void CancelRunningJob(string id)
        {
            if (loopJobsCancellationTokens.TryGetValue(id, out var cancellationSource))
            {
                cancellationSource.Cancel();
                loopJobsCancellationTokens.Remove(id, out cancellationSource);
            }
        }

        public async Task<Task> DequeueAndRunJob(CancellationToken token)
        {
            var (job, id) = await queue.Dequeue(token);

            var relatedToken = GetCancellationToken(token, id);

            var runningJob = Task. Run(() => job(relatedToken), relatedToken);
            return runningJob;
        }
         
        private CancellationToken GetCancellationToken(CancellationToken token, string? id)
        {
            var relatedToken = token;
            if (id != null && loopJobsCancellationTokens.TryGetValue(id, out var cancellationSource))
            {
                relatedToken = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationSource.Token,
                    token).Token;
            }
            return relatedToken;
        }
    }
}
