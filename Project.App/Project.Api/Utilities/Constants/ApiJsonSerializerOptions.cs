using System.Text.Json;

namespace Project.Api.Utilities.Constants;

public static class ApiJsonSerializerOptions
{
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        // use js convention instead of C#
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };
}
