using AmuraWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.Quote;

public class MotorModel : PageModel
{
    private readonly IQuoteSubmissionService _submissions;

    public MotorModel(IQuoteSubmissionService submissions)
    {
        _submissions = submissions;
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    [BindProperty]
    public string FormToken { get; set; } = string.Empty;

    public bool Submitted { get; set; }
    public string? ReferenceNumber { get; set; }

    public void OnGet()
    {
        FormToken = SpamGuard.GenerateFormToken();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (SpamGuard.LooksLikeSpam(Input.Website, FormToken))
        {
            Submitted = true;
            return Page();
        }

        var submission = new QuoteSubmission
        {
            Type = SubmissionType.Motor,
            ContactName = Input.FullName,
            ContactEmail = Input.Email,
            ContactPhone = Input.Phone,
            Details = new()
            {
                ["VehicleMake"] = Input.VehicleMake,
                ["VehicleModel"] = Input.VehicleModel,
                ["YearOfManufacture"] = Input.YearOfManufacture.ToString(),
                ["CoverType"] = Input.CoverType
            }
        };

        ReferenceNumber = await _submissions.SubmitAsync(submission);
        Submitted = true;
        return Page();
    }

    // PENDING: confirm against Amura's actual per-product field spec.
    // Note: this form only captures a quote request — it does not touch
    // DMVIC or issue a certificate. That's real-time insurer/DMVIC API
    // integration and belongs to the Client Portal phase, not here.
    public class FormInput
    {
        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string VehicleMake { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string VehicleModel { get; set; } = string.Empty;

        [Range(1980, 2027)]
        public int YearOfManufacture { get; set; } = DateTime.UtcNow.Year;

        [Required]
        public string CoverType { get; set; } = "Comprehensive";

        // Honeypot — real users never see or fill this in.
        public string? Website { get; set; }
    }
}
