using AmuraWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.Quote;

public class ProfessionalIndemnityModel : PageModel
{
    private readonly IQuoteSubmissionService _submissions;

    public ProfessionalIndemnityModel(IQuoteSubmissionService submissions)
    {
        _submissions = submissions;
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    [BindProperty]
    public string FormToken { get; set; } = string.Empty;

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
            // Looks automated — don't advance to the proposal form or persist.
            return RedirectToPage("/Index");
        }

        var submission = new QuoteSubmission
        {
            Type = SubmissionType.ProfessionalIndemnity,
            ContactName = Input.FullName,
            ContactEmail = Input.Email,
            ContactPhone = Input.Phone,
            Details = new()
            {
                ["Profession"] = Input.Profession,
                ["YearsInPractice"] = Input.YearsInPractice.ToString()
            }
        };

        var reference = await _submissions.SubmitAsync(submission);

        // Per the project plan: Professional Indemnity capture redirects
        // into a separate proposal form rather than ending here.
        return RedirectToPage("Proposal", new { reference });
    }

    // PENDING: confirm against Amura's actual per-product field spec.
    public class FormInput
    {
        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(160)]
        public string Profession { get; set; } = string.Empty;

        [Range(0, 60)]
        public int YearsInPractice { get; set; }

        // Honeypot — real users never see or fill this in.
        public string? Website { get; set; }
    }
}
