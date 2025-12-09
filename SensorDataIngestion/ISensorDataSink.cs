public interface ISensorDataSink
{
    Task PersistAsync(SensorData data, CancellationToken cancellationToken);
}
