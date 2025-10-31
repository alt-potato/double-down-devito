using System.Collections.Concurrent;

namespace Project.Test.Helpers;

/// <summary>
/// Custom Fact attribute that skips tests if a live API is not reachable.
/// Caches the reachability status for each unique URL per test run.
/// </summary>
public class LiveApiFactAttribute : FactAttribute
{
    private static readonly ConcurrentDictionary<string, bool> _apiReachableCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveApiFactAttribute"/> class,
    /// checking for the specified API URL.
    /// </summary>
    /// <param name="apiBaseUrl">The base URL of the API to check for reachability.</param>
    /// <param name="skipMessage">An optional custom message to display when the test is skipped.</param>
    public LiveApiFactAttribute(string apiBaseUrl, string? skipMessage = null)
    {
        bool isApiReachable = _apiReachableCache.GetOrAdd(
            apiBaseUrl,
            url =>
            {
                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                    var request = new HttpRequestMessage(HttpMethod.Head, url);
                    var response = client.Send(request);
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }
        );

        if (!isApiReachable)
        {
            Skip =
                skipMessage
                ?? $"API at {apiBaseUrl} is not reachable. Skipping live integration test.";
        }
    }
}
