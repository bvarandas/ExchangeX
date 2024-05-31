namespace SharedX.Core.ValueObjects;
public record ActivityOutbox
{
    public string Activity { get; set; } = string.Empty;
    public string NextActivity { get; set; } = string.Empty;
}