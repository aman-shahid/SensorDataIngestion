using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class QueueProcessorTests
{
    [Fact]
    public async Task ExecuteAsync_DrainsChannelAndInvokesPipeline()
    {
        var channel = Channel.CreateUnbounded<SensorData>();
        var pipeline = new TestPipeline();
        var processor = new QueueProcessor(channel, pipeline, NullLogger<QueueProcessor>.Instance);

        await processor.StartAsync(CancellationToken.None);

        var sensor = new SensorData
        {
            SensorId = "sensor-queue",
            Timestamp = DateTime.UtcNow,
            Data = "payload"
        };

        await channel.Writer.WriteAsync(sensor, CancellationToken.None);

        await pipeline.WaitForCountAsync(1, TimeSpan.FromSeconds(5));

        channel.Writer.Complete();
        await processor.StopAsync(CancellationToken.None);
    }

    private sealed class TestPipeline : ISensorDataPipeline
    {
        private TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _targetCount;
        public List<SensorData> Processed { get; } = new();

        public Task ProcessAsync(SensorData data, CancellationToken cancellationToken)
        {
            Processed.Add(data);
            if (Processed.Count >= _targetCount)
            {
                _tcs.TrySetResult(true);
            }

            return Task.CompletedTask;
        }

        public async Task WaitForCountAsync(int expectedCount, TimeSpan timeout)
        {
            if (Processed.Count >= expectedCount)
            {
                return;
            }

            _targetCount = expectedCount;
            var delayTask = Task.Delay(timeout);
            var completed = await Task.WhenAny(_tcs.Task, delayTask);
            if (completed != _tcs.Task)
            {
                throw new TimeoutException("Pipeline did not process expected count in time.");
            }

            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
