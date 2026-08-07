using System.Text.Json;

namespace AmuraWebsite.Services;

/// <summary>
/// Working implementation of Task #4. Every submission gets a reference
/// number and is appended to App_Data/submissions.jsonl so nothing is lost
/// even before a real back-office system or database is wired up.
///
/// Two things are intentionally stubbed pending decisions that are outside
/// dev control (see the plan's "Integrations & APIs" sheet):
///   - Back-office routing: currently just logged. Swap in the real
///     integration once Amura confirms how they want submissions delivered
///     (email inbox, CRM webhook, etc.).
///   - Client acknowledgement email: currently just logged. Swap in a real
///     ISendGridClient/SES call once a transactional email provider is
///     chosen — the Integrations sheet notes this has no client-side
///     dependency and can be picked by the dev team.
/// Both integration points are isolated in this one class on purpose, so
/// wiring in the real providers later doesn't touch any of the form pages.
/// </summary>
public sealed class QuoteSubmissionService : IQuoteSubmissionService
{
    private readonly ILogger<QuoteSubmissionService> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public QuoteSubmissionService(ILogger<QuoteSubmissionService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task<string> SubmitAsync(QuoteSubmission submission, CancellationToken ct = default)
    {
        submission.ReferenceNumber = GenerateReferenceNumber(submission.Type);
        submission.SubmittedAtUtc = DateTimeOffset.UtcNow;

        await PersistAsync(submission, ct);

        // Back-office routing — TODO: replace with the real integration once
        // Amura confirms how they want submissions delivered.
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
