
namespace Application.Services.Event.Worker
{
    public interface IWorkItemsQueue
    {
        void Enqueue(Func<CancellationToken, Task> workItem, string? id = null);
        Task<(Func<CancellationToken, Task>, string?)> Dequeue(CancellationToken cancellationToken);
    }
}
