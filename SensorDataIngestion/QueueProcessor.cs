using System.Threading.Channels;

public class QueueProcessor : BackgroundService
{
    private readonly ChannelReader<SensorData> _reader;
    private readonly ISensorDataPipeline _pipeline;
    private readonly ILogger<QueueProcessor> _logger;

    public QueueProcessor(Channel<SensorData> channel, ISensorDataPipeline pipeline, ILogger<QueueProcessor> logger)
    {
        _reader = channel.Reader;
        _pipeline = pipeline;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QueueProcessor started");

        await foreach (var item in _reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _pipeline.ProcessAsync(item, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process sensor {SensorId}", item.SensorId);
            }
        }

        _logger.LogInformation("QueueProcessor stopping");
    }
}