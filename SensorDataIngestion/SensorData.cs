public class SensorData
{
    public string SensorId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Data { get; set; } = string.Empty;
}