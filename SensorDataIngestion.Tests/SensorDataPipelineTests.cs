using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class SensorDataPipelineTests
{
    [Fact]
    public async Task ProcessAsync_PersistsThroughSink()
    {
        var sink = new TestSink();
        var pipeline = new SensorDataPipeline(sink, NullLogger<SensorDataPipeline>.Instance);
        var sensor = new SensorData
        {
            SensorId = "sensor-1",
            Timestamp = DateTime.UtcNow,
            Data = "payload"
        };

        await pipeline.ProcessAsync(sensor, CancellationToken.None);

        Assert.Single(sink.Items);
        Assert.Same(sensor, sink.Items[0]);
    }

    private sealed class TestSink : ISensorDataSink
    {
        public List<SensorData> Items { get; } = new();

        public Task PersistAsync(SensorData data, CancellationToken cancellationToken)
        {
            Items.Add(data);
            return Task.CompletedTask;
        }
    }
}
