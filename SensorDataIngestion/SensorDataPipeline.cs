using Microsoft.Extensions.Logging;

public class SensorDataPipeline : ISensorDataPipeline
{
    private readonly ISensorDataSink _sink;
    private readonly ILogger<SensorDataPipeline> _logger;

    public SensorDataPipeline(ISensorDataSink sink, ILogger<SensorDataPipeline> logger)
    {
        _sink = sink;
        _logger = logger;
    }

    public async Task ProcessAsync(SensorData data, CancellationToken cancellationToken)
    {
        await _sink.PersistAsync(data, cancellationToken);
        _logger.LogInformation("Persisted sensor {SensorId} at {Timestamp:o}", data.SensorId, data.Timestamp);
    }
}
