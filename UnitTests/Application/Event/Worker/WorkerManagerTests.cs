using Application.Services.Event.Worker;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace UnitTests.Application.Event.Worker
{
    public class WorkerManagerTests
    {
        private readonly IWorkItemsQueue queue;


        public WorkerManagerTests()
        {
            queue = Substitute.For<IWorkItemsQueue>();
        }

        [Fact]
        public async Task Dequeue_Should_CallDequeueAndRunJob()
        {
            var worker = new WorkerManager(queue);
            SetupQueue("123");
            worker.AddLoopJob("123", LoopJob);

            var runningJob = await worker.DequeueAndRunJob(default);
            await Task.Delay(100);

            await queue.Received()
                .Dequeue(Arg.Any<CancellationToken>());
            Assert.True(TaskStatus.RanToCompletion != runningJob.Status &&
                        TaskStatus.Canceled != runningJob.Status);
            Assert.False(runningJob.IsCompleted);
        }

        [Fact]
        public async Task Cancel_Should_StopJob()
        {
            var worker = new WorkerManager(queue);
            SetupQueue("123");
            worker.AddLoopJob("123", LoopJob);
            var runningJob = await worker.DequeueAndRunJob(default);

            worker.CancelRunningJob("123");
            await Task.Delay(100);

            Assert.True(TaskStatus.RanToCompletion == runningJob.Status ||
                        TaskStatus.Canceled == runningJob.Status);
            Assert.True(runningJob.IsCompleted);
        }

        private void SetupQueue(string id)
        {
            queue.Dequeue(Arg.Any<CancellationToken>()).Returns((LoopJob, id));
        }

        private async Task LoopJob(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(50, token);
            }
        }
    }
}