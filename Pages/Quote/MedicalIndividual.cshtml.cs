using AmuraWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.Quote;

public class MedicalIndividualModel : PageModel
{
    private readonly IQuoteSubmissionService _submissions;

    public MedicalIndividualModel(IQuoteSubmissionService submissions)
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
            Type = SubmissionType.MedicalIndividual,
            ContactName = Input.FullName,
            ContactEmail = Input.Email,
            ContactPhone = Input.Phone,
            Details = new()
            {
                ["DateOfBirth"] = Input.DateOfBirth.ToString("yyyy-MM-dd"),
                ["Dependents"] = Input.Dependents.ToString(),
                ["CoverLevel"] = Input.CoverLevel,
                ["ExistingConditions"] = Input.ExistingConditions ?? string.Empty
            }
        };

        ReferenceNumber = await _submissions.SubmitAsync(submission);
        Submitted = true;
        return Page();
    }

    // PENDING: confirm this field list against Amura's actual per-product
    // data spec (Key Assumptions: "per-product Get Quote data fields ...
    // CONFIRMED from Amura" — spec not yet attached at time of writing).
    public class FormInput
    {
        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required, DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Range(0, 10)]
        public int Dependents { get; set; }

        [Required]
        public string CoverLevel { get; set; } = "Standard";

        [StringLength(1000)]
        public string? ExistingConditions { get; set; }

        // Honeypot — real users never see or fill this in.
        public string? Website { get; set; }
    }
}
