namespace GatesDemo.Api.Options;

public sealed class RedirectOptions
{
    public const string SectionName = "RedirectOptions";

    public List<string> AllowedHosts { get; set; } = new();
}
