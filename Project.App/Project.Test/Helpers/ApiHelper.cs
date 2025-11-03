using System.Text.Json;
using Project.Api.Utilities.Constants;

namespace Project.Test.Helpers;

public static class ApiHelper
{
    public static T? Deserialize<T>(string data)
    {
        return JsonSerializer.Deserialize<T>(data, ApiJsonSerializerOptions.DefaultOptions);
    }
}
