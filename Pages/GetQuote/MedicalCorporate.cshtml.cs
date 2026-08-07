using AmuraWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.GetQuote;

public class MedicalCorporateModel : PageModel
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
        // TODO: once the backend exists, call it here (and route to Amura's back
        // office / trigger the client acknowledgement email); QuoteSubmissionStore
        // can stay as a local fallback log if that's useful, or be retired.
        QuoteSubmissionStore.Save("MedicalCorporate", Input);

        Submitted = true;
        return Page();
    }

    public class FormInput
    {
        public string? Website { get; set; }

        [Required, StringLength(120)]
        public string ContactName { get; set; } = string.Empty;

        [Required, StringLength(160)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required, Range(1, 100000)]
        public int EmployeeCount { get; set; }

        // PENDING from Amura: confirmed field list for this product (benefit tiers, etc.)
        [StringLength(2000)]
        public string? AdditionalDetails { get; set; }
    }
}
