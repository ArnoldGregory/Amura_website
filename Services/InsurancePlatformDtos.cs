using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmuraWebsite.Services;

public sealed class ApiEnvelope<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

public sealed class LoginResponseData
{
    public string? Token { get; set; }
}

public sealed class QuoteRequestResponseData
{
    // The platform's docs don't pin down whether this comes back as a
    // string or a number, so accept either and normalize to string.
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }

    public string? QuoteRequestId =>
        Extra != null && Extra.TryGetValue("quoteRequestId", out var el)
            ? el.ToString()
            : null;
}
