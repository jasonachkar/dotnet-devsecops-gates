// =============================================================================
// CorsOptions.cs — Configuration for Cross-Origin Resource Sharing (CORS)
// =============================================================================
// Binds to the "CorsOptions" section in appsettings.json.
// Controls which browser origins are allowed to make cross-origin requests.
//
// Example appsettings.json:
//   "CorsOptions": {
//     "AllowedOrigins": [ "https://example.com" ]
//   }
//
// SECURITY: This is NOT a wildcard (*) CORS policy. Only explicitly listed
// origins are permitted. Using a wildcard would allow any website to make
// authenticated cross-origin requests to this API.
// =============================================================================

namespace GatesDemo.Api.Options;

public sealed class CorsOptions
{
    public const string SectionName = "CorsOptions";

    // Array of allowed origin URLs (e.g., "https://example.com").
    // Must include scheme (https://) and port if non-standard.
    // Empty array = no cross-origin requests allowed (secure default).
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
