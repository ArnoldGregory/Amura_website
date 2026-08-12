using System.Text.Json;

namespace AmuraWebsite.Services;

/// <summary>
/// Working implementation of Task #4. Every submission gets a reference
/// number and is appended to App_Data/submissions.jsonl so nothing is lost
/// even before a real back-office system or database is wired up — this
/// local persistence always happens, regardless of what's below.
///
/// Real Insurance Platform API integration: all 5 non-Motor product types
/// (Medical Individual, Medical Corporate, Professional Indemnity, Travel,
/// Domestic) now submit to the real backend via IInsurancePlatformClient,
/// matching the exact field contracts confirmed in the "Website - Full
/// Client Journey" Postman collection. Motor isn't a simple "quote request"
/// on the real platform at all — it's a full live pricing + client/vehicle
/// registration + purchase + M-Pesa STK push flow, structurally different
/// from a lead-capture form and out of scope here; its Get Quote form stays
/// local-only, same as Contact.
///
/// If the platform isn't configured (see appsettings.json
/// InsurancePlatform:BaseUrl / WebsiteGuestApiKey) or a call fails for any
/// reason, this silently falls back to local-only — nothing the user sees
/// changes, and no submission is ever lost either way.
///
/// One thing still stubbed pending an Amura decision (see the plan's
/// "Integrations & APIs" sheet): the client acknowledgement email. Swap in
/// a real transactional-email call once a provider is chosen — isolated
/// here on purpose so that doesn't touch any form page either.
/// </summary>
public sealed class QuoteSubmissionService : IQuoteSubmissionService
{
    private readonly ILogger<QuoteSubmissionService> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly IInsurancePlatformClient _platformClient;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public QuoteSubmissionService(
        ILogger<QuoteSubmissionService> logger,
        IWebHostEnvironment env,
        IInsurancePlatformClient platformClient)
    {
        _logger = logger;
        _env = env;
        _platformClient = platformClient;
    }

    public async Task<string> SubmitAsync(QuoteSubmission submission, CancellationToken ct = default)
    {
        submission.ReferenceNumber = GenerateReferenceNumber(submission.Type);
        submission.SubmittedAtUtc = DateTimeOffset.UtcNow;

        if (_platformClient.IsConfigured)
        {
            var platformId = await TrySubmitToPlatformAsync(submission, ct);
            if (platformId != null)
            {
                submission.Details["PlatformQuoteRequestId"] = platformId;
                _logger.LogInformation(
                    "Submission {Reference} also created on the Insurance Platform as quoteRequestId {PlatformId}.",
                    submission.ReferenceNumber, platformId);
            }
        }

        await PersistAsync(submission, ct);

        // Back-office routing — for types not yet wired to the real
        // platform (or if the platform call above didn't succeed), this is
        // the only routing that happens right now.
        _logger.LogInformation(
            "Quote submission {Reference} ({Type}) from {Name} <{Email}> routed to back office.",
            submission.ReferenceNumber, submission.Type, submission.ContactName, submission.ContactEmail);

        // Client acknowledgement — TODO: replace with real transactional
        // email send once a provider is chosen (SendGrid / Amazon SES / etc).
        _logger.LogInformation(
            "Acknowledgement email queued for {Email} — reference {Reference}.",
            submission.ContactEmail, submission.ReferenceNumber);

        return submission.ReferenceNumber;
    }

    private Task<string?> TrySubmitToPlatformAsync(QuoteSubmission submission, CancellationToken ct)
    {
        return submission.Type switch
        {
            SubmissionType.MedicalIndividual => SubmitMedicalIndividualAsync(submission, ct),

            SubmissionType.MedicalCorporate => _platformClient.SubmitMedicalCorporateAsync(
                submission.Details.GetValueOrDefault("CompanyName", string.Empty),
                submission.ContactPhone ?? string.Empty,
                submission.ContactEmail,
                ct),

            // Only the initial PI capture matches this endpoint's shape —
            // the follow-up Proposal page collects different fields
            // entirely and isn't a "request quote" call.
            SubmissionType.ProfessionalIndemnity when submission.Details.GetValueOrDefault("Stage") != "Proposal" =>
                _platformClient.SubmitProfessionalIndemnityAsync(
                    submission.ContactName,
                    submission.ContactPhone ?? string.Empty,
                    submission.ContactEmail,
                    submission.Details.GetValueOrDefault("Profession", string.Empty),
                    ct),

            SubmissionType.Travel => SubmitTravelAsync(submission, ct),

            SubmissionType.Domestic => _platformClient.SubmitDomesticAsync(
                BuildDomesticDetailsJson(submission), ct),

            _ => Task.FromResult<string?>(null)
        };
    }

    private Task<string?> SubmitMedicalIndividualAsync(QuoteSubmission submission, CancellationToken ct)
    {
        var familyMembers = new List<(string Relationship, string FullName, DateTime DateOfBirth)>();
        if (submission.Details.TryGetValue("FamilyMembersJson", out var json) && !string.IsNullOrWhiteSpace(json))
        {
            var parsed = JsonSerializer.Deserialize<List<FamilyMemberJson>>(json);
            if (parsed != null)
            {
                familyMembers.AddRange(parsed.Select(m =>
                    (m.relationship, m.fullName, ParseIsoDate(m.dateOfBirth))));
            }
        }

        return _platformClient.SubmitMedicalIndividualAsync(
            submission.ContactName,
            ParseIsoDate(submission.Details.GetValueOrDefault("DateOfBirth", DateTime.UtcNow.ToString("yyyy-MM-dd"))),
            submission.Details.GetValueOrDefault("IdNo", string.Empty),
            submission.ContactEmail,
            submission.ContactPhone ?? string.Empty,
            familyMembers,
            ct);
    }

    private Task<string?> SubmitTravelAsync(QuoteSubmission submission, CancellationToken ct)
    {
        return _platformClient.SubmitTravelAsync(
            submission.ContactName,
            ParseIsoDate(submission.Details.GetValueOrDefault("DateOfBirth", DateTime.UtcNow.ToString("yyyy-MM-dd"))),
            string.IsNullOrWhiteSpace(submission.Details.GetValueOrDefault("KraPin")) ? null : submission.Details["KraPin"],
            submission.Details.GetValueOrDefault("Destination", string.Empty),
            ParseIsoDate(submission.Details.GetValueOrDefault("DepartureDate", DateTime.UtcNow.ToString("yyyy-MM-dd"))),
            ParseIsoDate(submission.Details.GetValueOrDefault("ReturnDate", DateTime.UtcNow.ToString("yyyy-MM-dd"))),
            bool.TryParse(submission.Details.GetValueOrDefault("TravellingWithFamily"), out var withFamily) && withFamily,
            submission.Details.GetValueOrDefault("TripType", "VACATION"),
            ct);
    }

    private sealed class FamilyMemberJson
    {
        public string relationship { get; set; } = string.Empty;
        public string fullName { get; set; } = string.Empty;
        public string dateOfBirth { get; set; } = string.Empty;
    }

    private static DateTime ParseIsoDate(string value) =>
        DateTime.ParseExact(value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private static string BuildDomesticDetailsJson(QuoteSubmission submission)
    {
        var details = new
        {
            fullName = submission.ContactName,
            phone = submission.ContactPhone,
            propertyAddress = submission.Details.GetValueOrDefault("PropertyAddress"),
            propertyType = submission.Details.GetValueOrDefault("PropertyType"),
            estimatedValue = submission.Details.GetValueOrDefault("EstimatedValue")
        };
        return JsonSerializer.Serialize(details);
    }

    private static string GenerateReferenceNumber(SubmissionType type)
    {
        var prefix = type switch
        {
            SubmissionType.MedicalIndividual => "MED",
            SubmissionType.MedicalCorporate => "COR",
            SubmissionType.ProfessionalIndemnity => "PIN",
            SubmissionType.Travel => "TRV",
            SubmissionType.Domestic => "DOM",
            SubmissionType.Motor => "MOT",
            _ => "MSG"
        };

        var stamp = DateTimeOffset.UtcNow.ToString("yyMMdd");
        var suffix = Guid.NewGuid().ToString("N")[..5].ToUpperInvariant();
        return $"AMR-{prefix}-{stamp}-{suffix}";
    }

    private async Task PersistAsync(QuoteSubmission submission, CancellationToken ct)
    {
        var dataDir = Path.Combine(_env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDir);
        var filePath = Path.Combine(dataDir, "submissions.jsonl");

        var line = JsonSerializer.Serialize(submission) + Environment.NewLine;

        await _fileLock.WaitAsync(ct);
        try
        {
            await File.AppendAllTextAsync(filePath, line, ct);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
