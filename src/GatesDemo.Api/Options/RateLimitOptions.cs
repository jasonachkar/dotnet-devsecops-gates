// =============================================================================
// RateLimitOptions.cs — Configuration for the fixed-window rate limiter
// =============================================================================
// Binds to the "RateLimitOptions" section in appsettings.json.
// Controls how many requests are allowed per time window.
//
// Example appsettings.json:
//   "RateLimitOptions": {
//     "PermitLimit": 10,
//     "WindowSeconds": 60,
//     "QueueLimit": 0
//   }
//
// The fixed-window algorithm divides time into fixed intervals and allows
// up to PermitLimit requests per interval. Simple and predictable.
// =============================================================================

namespace GatesDemo.Api.Options;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimitOptions";

    // Maximum number of requests allowed per time window.
    // Default: 10 requests per window.
    public int PermitLimit { get; set; } = 10;

    // Duration of each rate limit window in seconds.
    // Default: 60 seconds (1 minute).
    public int WindowSeconds { get; set; } = 60;

    // Number of requests to queue when the limit is reached.
    // 0 = reject immediately with 429; >0 = queue and process when window resets.
    // Default: 0 (no queuing — immediate rejection for clarity in demos).
    public int QueueLimit { get; set; } = 0;
}
