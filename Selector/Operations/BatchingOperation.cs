using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Selector.Operations
{
    public class BatchingOperation<T> where T : IOperation
    {
        protected ILogger<BatchingOperation<T>> logger;
        protected CancellationToken _token;

        public ConcurrentQueue<T> WaitingRequests { get; private set; } = new();
        public ConcurrentQueue<T> DoneRequests { get; private set; } = new();

        private TimeSpan interRequestDelay;
        private TimeSpan timeout;
        private int simultaneousRequests;

        public BatchingOperation(TimeSpan _interRequestDelay, TimeSpan _timeout, int _simultaneous,
            IEnumerable<T> requests, ILogger<BatchingOperation<T>>? _logger = null)
        {
            interRequestDelay = _interRequestDelay;
            timeout = _timeout;
            simultaneousRequests = _simultaneous;
            logger = _logger ?? NullLogger<BatchingOperation<T>>.Instance;

            foreach (var request in requests)
            {
                WaitingRequests.Enqueue(request);
            }
        }

        public bool TimedOut { get; private set; } = false;

        private async void HandleSuccessfulRequest(object? o, EventArgs e)
        {
            using var span = Trace.Tracer.StartActivity();
            await Task.Delay(interRequestDelay, _token);
            TransitionRequest();
        }

        private void TransitionRequest()
        {
            using var span = Trace.Tracer.StartActivity();
            if (WaitingRequests.TryDequeue(out var request))
            {
                request.Success += HandleSuccessfulRequest;
                _ = request.Execute();
                DoneRequests.Enqueue(request);

                logger.LogInformation("Executing request {} of {}", DoneRequests.Count,
                    WaitingRequests.Count + DoneRequests.Count);
            }
        }

        public async Task TriggerRequests(CancellationToken token)
        {
            using var span = Trace.Tracer.StartActivity();
            foreach (var _ in Enumerable.Range(1, simultaneousRequests))
            {
                TransitionRequest();
            }

            var timeoutTask = Task.Delay(timeout, token);
            var allTasks = WaitingRequests.Union(DoneRequests).Select(r => r.Task);

            var firstToFinish = await Task.WhenAny(timeoutTask, Task.WhenAll(allTasks));

            TimedOut = firstToFinish == timeoutTask;
        }
    }
}