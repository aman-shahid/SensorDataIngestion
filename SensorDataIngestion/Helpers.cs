public static class Helpers
{
    public static string? Validate(SensorData s)
    {
        if (string.IsNullOrWhiteSpace(s.SensorId))
            return "SensorId is required";

        // Allow some clock skew; reject timestamps far in the future or too old
        var now = DateTime.UtcNow;
        if (s.Timestamp.Kind != DateTimeKind.Utc)
        {
            // Try to treat as UTC
            s.Timestamp = DateTime.SpecifyKind(s.Timestamp, DateTimeKind.Utc);
        }

        if (s.Timestamp > now.AddMinutes(5))
            return "Timestamp is in the future";

        if (s.Timestamp < now.AddYears(-1))
            return "Timestamp is too old";

        return null;
    }
}