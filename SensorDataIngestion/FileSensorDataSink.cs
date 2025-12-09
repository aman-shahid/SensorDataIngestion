using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Options;

public class FileSensorDataSink : ISensorDataSink, IDisposable
{
    private readonly string _outputPath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    public FileSensorDataSink(IOptions<FileSensorDataSinkOptions> options)
    {
        var configuredPath = options.Value.OutputPath;
        _outputPath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(AppContext.BaseDirectory, "ingested.jsonl")
            : configuredPath!;

        var directory = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task PersistAsync(SensorData data, CancellationToken cancellationToken)
    {
        var line = JsonSerializer.Serialize(data);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(_outputPath, line + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _gate.Dispose();
        _disposed = true;
    }
}
