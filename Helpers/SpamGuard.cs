namespace AmuraWebsite.Helpers;

/// <summary>
/// Shared anti-spam check for public forms. A real visitor never sees or fills the
/// honeypot field (it's visually hidden and skipped in tab order); bots that auto-fill
/// every input on a page will fill it, so any non-empty value means "reject silently".
/// Used consistently across every Get Quote form and Contact.
/// </summary>
public static class SpamGuard
{
    public static bool IsHoneypotTripped(string? honeypotValue)
        => !string.IsNullOrWhiteSpace(honeypotValue);
}
