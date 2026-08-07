using AmuraWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.GetQuote;

public class MotorModel : PageModel
{
    [BindProperty]
    public FormInput Input { get; set; } = new();

    public bool Submitted { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (SpamGuard.IsHoneypotTripped(Input.Website))
        {
            Submitted = true;
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Real backend/API (Task #4) doesn't exist yet — persist locally so the
        // submission isn't lost, instead of just showing "received" and discarding it.
        // This is a lead-capture form only: the real premium calculation engine
        // (Task #13, Client Portal) and DMVIC/insurer API integration (Task #15)
        // aren't built yet, so no rate is quoted here — we just collect the
        // details Amura's team needs to come back with a manual quote.
        // TODO: once the backend exists, call it here; QuoteSubmissionStore can
        // stay as a local fallback log if that's useful, or be retired.
        QuoteSubmissionStore.Save("Motor", Input);

        Submitted = true;
        return Page();
    }

    public class FormInput : IValidatableObject
    {
        public string? Website { get; set; }

        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        // PENDING confirmation from Amura: "Private" is included here as the
        // typical 4th category alongside the three named — flag with Amura
        // whether private vehicles should route through this website form at
        // all, or only through the Client Portal once it exists.
        [Required]
        public string VehicleCategory { get; set; } = string.Empty; // Private / PSV / Commercial / Institution

        [Required]
        public string CoverType { get; set; } = string.Empty; // ThirdParty / Comprehensive

        [StringLength(20)]
        public string? RegistrationNumber { get; set; }

        [Required, StringLength(80)]
        public string MakeModel { get; set; } = string.Empty;

        [Required, Range(1980, 2100)]
        public int YearOfManufacture { get; set; }

        // Required only for PSV (bus/matatu).
        [Range(1, 200)]
        public int? SittingCapacity { get; set; }

        // Required for Commercial and Institution.
        [Range(0.1, 100)]
        public decimal? Tonnage { get; set; }

        // Required only when CoverType = Comprehensive.
        [Range(0, 1_000_000_000)]
        public decimal? EstimatedValue { get; set; }

        [Required]
        public string Period { get; set; } = string.Empty; // Annual / Short Term

        // PENDING from Amura: the actual extra questions needed for
        // Comprehensive cover (e.g. excess protection, windscreen, PVT,
        // named/any driver) — collected as free text until that's confirmed.
        [StringLength(2000)]
        public string? ComprehensiveDetails { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (VehicleCategory == "PSV" && SittingCapacity is null)
            {
                yield return new ValidationResult(
                    "Sitting capacity is required for PSV vehicles.",
                    new[] { nameof(SittingCapacity) });
            }

            if ((VehicleCategory == "Commercial" || VehicleCategory == "Institution") && Tonnage is null)
            {
                yield return new ValidationResult(
                    "Tonnage is required for commercial and institution vehicles.",
                    new[] { nameof(Tonnage) });
            }

            if (CoverType == "Comprehensive" && EstimatedValue is null)
            {
                yield return new ValidationResult(
                    "Estimated value is required for comprehensive cover.",
                    new[] { nameof(EstimatedValue) });
            }
        }
    }
}
