using System.Text.Json;

namespace AmuraWebsite.Helpers;

/// <summary>
/// Stopgap persistence for Get Quote / Contact submissions while the real backend
/// (Task #4 — quote submission &amp; routing) doesn't exist yet. Appends one JSON
/// line per submission to App_Data/quote-submissions.jsonl, which lives outside
/// wwwroot so it is never served as a static file.
///
/// This is a placeholder, not a real store: no de-duplication, no querying, no
/// encryption at rest, single flat file. Once the backend/API exists, replace the
/// call site (SpamGuard-checked, ModelState-valid branch of each OnPost) with the
/// real submission call — and either migrate anything queued up in this file, or
/// keep this as a local fallback log if that's useful operationally.
///
/// Holds real customer PII (names, emails, phone numbers) — App_Data/ is excluded
/// via .gitignore so it never gets committed.
/// </summary>
public static class QuoteSubmissionStore
{
    private static readonly object WriteLock = new();
    private static string? _rootPath;

    /// <summary>Call once at startup with app.Environment.ContentRootPath.</summary>
    public static void Initialize(string contentRootPath)
    {
        _rootPath = contentRootPath;
    }

    public static void Save(string product, object input)
    {
        if (_rootPath is null)
        {
            throw new InvalidOperationException(
                "QuoteSubmissionStore.Initialize(contentRootPath) must be called at startup, before any form submission.");
        }

        var record = new
        {
            SubmittedAtUtc = DateTime.UtcNow,
            Product = product,
            Data = input
        };

        var line = JsonSerializer.Serialize(record);

        var dir = Path.Combine(_rootPath, "App_Data");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "quote-submissions.jsonl");

        lock (WriteLock)
        {
            File.AppendAllText(path, line + Environment.NewLine);
        }
    }
}
