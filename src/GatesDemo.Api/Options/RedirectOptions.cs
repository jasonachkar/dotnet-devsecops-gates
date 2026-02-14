// =============================================================================
// RedirectOptions.cs — Configuration for the /api/redirect endpoint
// =============================================================================
// Binds to the "RedirectOptions" section in appsettings.json.
// Controls which hosts the redirect endpoint will accept as valid targets.
//
// Example appsettings.json:
//   "RedirectOptions": {
//     "AllowedHosts": [ "example.com", "learn.microsoft.com" ]
//   }
//
// Why an allowlist? Without it, the redirect endpoint would be an open redirect
// vulnerability (CWE-601), allowing attackers to use your domain as a redirect
// proxy for phishing. The demo/vulnerable-codeql branch removes this check
// to demonstrate CodeQL detection.
// =============================================================================

namespace GatesDemo.Api.Options;

// Sealed: this class should not be inherited — it's a plain configuration POCO.
public sealed class RedirectOptions
{
    // SectionName must match the JSON key in appsettings.json exactly.
    // Used in Program.cs: builder.Configuration.GetSection(RedirectOptions.SectionName)
    public const string SectionName = "RedirectOptions";

    // List of allowed target hosts (e.g., "example.com", "learn.microsoft.com").
    // Compared case-insensitively against the host portion of redirect target URLs.
    public List<string> AllowedHosts { get; set; } = new();
}
