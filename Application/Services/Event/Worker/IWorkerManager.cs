
namespace Application.Services.Event.Worker
{
    public interface IWorkerManager
    {
        void AddSingleRunJob(Func<CancellationToken, Task> workerJob);
        void AddLoopJob(string id, Func<CancellationToken, Task> workerJob);
        void CancelRunningJob(string id);
        Task<Task> DequeueAndRunJob(CancellationToken token);
    }
}
