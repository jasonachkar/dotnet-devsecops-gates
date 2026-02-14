namespace GatesDemo.Api.Options;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimitOptions";

    public int PermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 0;
}
