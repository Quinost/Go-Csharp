namespace API.Shared.Entities;

public class JobResult
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid JobId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime FinishedAt { get; set; } = DateTime.UtcNow;
}
