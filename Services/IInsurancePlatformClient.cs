namespace AmuraWebsite.Services;

/// <summary>
/// Talks to the real Insurance Platform backend (the one described in the
/// "Website - Full Client Journey" Postman collection) as the WEBSITE_GUEST
/// channel. Only covers the request-quote endpoints that this website's
/// forms currently collect complete data for — see QuoteSubmissionService
/// for which product types are actually wired to which methods, and why.
///
/// Every method returns null (never throws to the caller) if the platform
/// isn't configured or the call fails — callers are expected to fall back
/// to local persistence so a real-backend hiccup never loses a submission.
/// </summary>
public interface IInsurancePlatformClient
{
    bool IsConfigured { get; }

    Task<string?> SubmitMedicalIndividualAsync(
        string clientName, DateTime clientDob, string idNo, string email, string phone,
        IReadOnlyList<(string Relationship, string FullName, DateTime DateOfBirth)> familyMembers,
        CancellationToken ct = default);

    Task<string?> SubmitMedicalCorporateAsync(
        string companyName, string phone, string email, CancellationToken ct = default);

    Task<string?> SubmitProfessionalIndemnityAsync(
        string clientOrCompanyName, string phone, string email, string profession, CancellationToken ct = default);

    Task<string?> SubmitTravelAsync(
        string clientName, DateTime dob, string? kraPin, string destination,
        DateTime travelDateFrom, DateTime travelDateTo, bool travellingWithFamily, string tripType,
        CancellationToken ct = default);

    Task<string?> SubmitDomesticAsync(
        string detailsJson, CancellationToken ct = default);
}
