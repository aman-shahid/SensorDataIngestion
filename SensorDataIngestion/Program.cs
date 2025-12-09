using System.Threading.Channels;
using System.Text.Json;

// A simple worker service demonstrating basic ingestion of sensor data.
// assumption: data flow is something like: sensor -> edge device/gateway/ -> IoT Broker (fx. Azure Event Hub) -> background processing -> storage (sink)

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.Configure<FileSensorDataSinkOptions>(builder.Configuration.GetSection("FileSink"));
builder.Services.AddSingleton(Channel.CreateUnbounded<SensorData>());
builder.Services.AddSingleton<ISensorDataSink, FileSensorDataSink>();
builder.Services.AddSingleton<ISensorDataPipeline, SensorDataPipeline>();
builder.Services.AddHostedService<QueueProcessor>();

var app = builder.Build();


// Configure endpoints
app.MapGet("/", () => Results.Ok(new { Service = "SensorDataIngestion", Version = "1.0" }));

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapPost("/api/ingest", async (HttpRequest request, Channel<SensorData> channel) =>
{
    try
    {
        var sensor = await JsonSerializer.DeserializeAsync<SensorData>(request.Body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (sensor == null)
            return Results.BadRequest(new { error = "Invalid payload" });

        var validationError = Helpers.Validate(sensor);
        if (validationError != null)
            return Results.BadRequest(new { error = validationError });

        // Enqueue without blocking the HTTP thread
        await channel.Writer.WriteAsync(sensor); // channel  will be replaced by an event hub when/if in production.

        return Results.Accepted(null, new { status = "queued" });
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { error = "Malformed JSON" });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("SensorDataIngestion started. POST sensor data to /api/ingest as JSON.");
});

// Configure Kestrel URL so we can test locally
app.Urls.Clear();
app.Urls.Add("http://localhost:5000");

await app.RunAsync();
