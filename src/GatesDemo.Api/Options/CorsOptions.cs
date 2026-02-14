namespace GatesDemo.Api.Options;

public sealed class CorsOptions
{
    public const string SectionName = "CorsOptions";

    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
