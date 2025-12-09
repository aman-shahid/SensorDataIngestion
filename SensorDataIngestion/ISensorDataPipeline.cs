public interface ISensorDataPipeline
{
    Task ProcessAsync(SensorData data, CancellationToken cancellationToken);
}
