namespace Common.Contracts.Base;

public abstract record Message
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string SourceService { get; init; } = "Unknown"; // Add default value
}