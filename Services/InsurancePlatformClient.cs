using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace AmuraWebsite.Services;

public sealed class InsurancePlatformClient : IInsurancePlatformClient
{
    private readonly HttpClient _http;
    private readonly InsurancePlatformOptions _options;
    private readonly PlatformTokenCache _tokenCache;
    private readonly ILogger<InsurancePlatformClient> _logger;

    private static readonly TimeSpan AssumedTokenLifetime = TimeSpan.FromMinutes(45);

<<<<<<< HEAD
    // The real API returns lowercase JSON keys (success/data/token) while
    // our DTOs are PascalCase for normal C# style — System.Text.Json is
    // case-sensitive by default, so without this every response would
    // silently fail to deserialize and everything would quietly fall back
    // to local-only with no visible error.
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

=======
>>>>>>> cff447d3d43ed2aaef6127c261952d345a8fab76
    public InsurancePlatformClient(
        HttpClient http,
        IOptions<InsurancePlatformOptions> options,
        PlatformTokenCache tokenCache,
        ILogger<InsurancePlatformClient> logger)
    {
        _http = http;
        _options = options.Value;
        _tokenCache = tokenCache;
        _logger = logger;
    }

    public bool IsConfigured => _options.IsConfigured;

    public Task<string?> SubmitMedicalIndividualAsync(
        string clientName, DateTime clientDob, string idNo, string email, string phone,
        IReadOnlyList<(string Relationship, string FullName, DateTime DateOfBirth)> familyMembers,
        CancellationToken ct = default)
    {
        var payload = new
        {
            clientId = (int?)null,
            channel = "WEBSITE",
            clientName,
            clientDob = clientDob.ToString("yyyy-MM-dd"),
            familyMembers = familyMembers.Select(m => new
            {
                relationship = m.Relationship,
                fullName = m.FullName,
                dateOfBirth = m.DateOfBirth.ToString("yyyy-MM-dd")
            }),
            idNo,
            email,
            phone
        };
        return PostQuoteRequestAsync("/api/quoterequests/medical-individual", payload, ct);
    }

    public Task<string?> SubmitMedicalCorporateAsync(
        string companyName, string phone, string email, CancellationToken ct = default)
    {
        var payload = new
        {
            clientId = (int?)null,
            channel = "WEBSITE",
            companyName,
            phone,
            email
        };
        return PostQuoteRequestAsync("/api/quoterequests/medical-corporate", payload, ct);
    }

    public Task<string?> SubmitProfessionalIndemnityAsync(
        string clientOrCompanyName, string phone, string email, string profession, CancellationToken ct = default)
    {
        var payload = new
        {
            clientId = (int?)null,
            channel = "WEBSITE",
            clientOrCompanyName,
            phone,
            email,
            profession
        };
        return PostQuoteRequestAsync("/api/quoterequests/professional-indemnity", payload, ct);
    }

    public Task<string?> SubmitTravelAsync(
        string clientName, DateTime dob, string? kraPin, string destination,
        DateTime travelDateFrom, DateTime travelDateTo, bool travellingWithFamily, string tripType,
        CancellationToken ct = default)
    {
        var payload = new
        {
            clientId = (int?)null,
            channel = "WEBSITE",
            clientName,
            dob = dob.ToString("yyyy-MM-dd"),
            kraPin,
            destination,
            travelDateFrom = travelDateFrom.ToString("yyyy-MM-ddTHH:mm:ss"),
            travelDateTo = travelDateTo.ToString("yyyy-MM-ddTHH:mm:ss"),
            travellingWithFamily,
            tripType
        };
        return PostQuoteRequestAsync("/api/quoterequests/travel", payload, ct);
    }

    public Task<string?> SubmitDomesticAsync(string detailsJson, CancellationToken ct = default)
    {
        var payload = new
        {
            clientId = (int?)null,
            channel = "WEBSITE",
            detailsJson
        };
        return PostQuoteRequestAsync("/api/quoterequests/domestic", payload, ct);
    }

    private async Task<string?> PostQuoteRequestAsync(string path, object payload, CancellationToken ct)
    {
        if (!IsConfigured)
        {
            // Not configured yet — expected state until real credentials are
            // supplied. Callers fall back to local persistence for this.
            return null;
        }

        var token = await EnsureTokenAsync(ct);
        if (token is null)
        {
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("X-Channel", "WEBSITE_GUEST");

            using var response = await _http.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token may have expired server-side before our cache thought
                // it would — clear and let the caller retry once naturally
                // on the next submission rather than looping here.
                _tokenCache.Clear();
                _logger.LogWarning("Insurance Platform API returned 401 on {Path}; token cleared.", path);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Insurance Platform API {Path} returned {Status}.", path, response.StatusCode);
                return null;
            }

<<<<<<< HEAD
            var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<QuoteRequestResponseData>>(JsonOptions, ct);
=======
            var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<QuoteRequestResponseData>>(cancellationToken: ct);
>>>>>>> cff447d3d43ed2aaef6127c261952d345a8fab76
            if (envelope is { Success: true } && envelope.Data?.QuoteRequestId is { } id)
            {
                return id;
            }

            _logger.LogWarning("Insurance Platform API {Path} responded without a usable quoteRequestId.", path);
            return null;
        }
        catch (Exception ex)
        {
            // Network issues, timeouts, unexpected response shape — none of
            // this should ever bubble up and block a form submission.
            _logger.LogWarning(ex, "Insurance Platform API call to {Path} failed.", path);
            return null;
        }
    }

    private async Task<string?> EnsureTokenAsync(CancellationToken ct)
    {
        if (_tokenCache.HasValidToken)
        {
            return _tokenCache.Token;
        }

        using var releaser = await _tokenCache.LockAsync(ct);

        // Re-check — another request may have logged in while we waited.
        if (_tokenCache.HasValidToken)
        {
            return _tokenCache.Token;
        }

        try
        {
            var loginPayload = new { channel = "WEBSITE_GUEST", apiKey = _options.WebsiteGuestApiKey };
            using var response = await _http.PostAsJsonAsync("/api/auth/channel-login", loginPayload, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Insurance Platform channel-login returned {Status}.", response.StatusCode);
                return null;
            }

<<<<<<< HEAD
            var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<LoginResponseData>>(JsonOptions, ct);
=======
            var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<LoginResponseData>>(cancellationToken: ct);
>>>>>>> cff447d3d43ed2aaef6127c261952d345a8fab76
            if (envelope is { Success: true } && !string.IsNullOrEmpty(envelope.Data?.Token))
            {
                _tokenCache.SetToken(envelope.Data.Token, AssumedTokenLifetime);
                return envelope.Data.Token;
            }

            _logger.LogWarning("Insurance Platform channel-login succeeded but returned no token.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Insurance Platform channel-login failed.");
            return null;
        }
    }
}
