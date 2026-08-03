using AmuraWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.GetQuote;

public class MedicalIndividualModel : PageModel
{
    [BindProperty]
    public FormInput Input { get; set; } = new();

    public bool Submitted { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // Honeypot check first — bail out before touching ModelState/validation
        // so a bot submission never even reaches real processing.
        if (SpamGuard.IsHoneypotTripped(Input.Website))
        {
            // Pretend it worked so the bot doesn't learn anything from the response.
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
        QuoteSubmissionStore.Save("MedicalIndividual", Input);

        Submitted = true;
        return Page();
    }

    public class FormInput
    {
        // Honeypot — must stay empty. Not shown to real users.
        public string? Website { get; set; }

        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string PlanType { get; set; } = string.Empty; // Individual / Family

        [Range(0, 15)]
        public int Dependents { get; set; }

        // PENDING from Amura: confirmed field list for this product (per project plan).
        [StringLength(2000)]
        public string? AdditionalDetails { get; set; }
    }
}
