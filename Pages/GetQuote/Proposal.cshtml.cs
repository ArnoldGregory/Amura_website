using AmuraWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.GetQuote;

public class ProposalModel : PageModel
{
    [BindProperty]
    public FormInput Input { get; set; } = new();

    public bool Submitted { get; set; }

    public void OnGet()
    {
        // Prefill from the Professional Indemnity capture step, if this request came
        // through that redirect. TempData survives exactly one redirect by design.
        if (TempData["ProposalFullName"] is string name) Input.FullName = name;
        if (TempData["ProposalEmail"] is string email) Input.Email = email;
        if (TempData["ProposalPhone"] is string phone) Input.Phone = phone;
        if (TempData["ProposalProfession"] is string profession) Input.Profession = profession;
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
        // submission isn't lost. TODO: once the backend exists, call it here
        // (route to Amura's back office, send the client acknowledgement email).
        QuoteSubmissionStore.Save("ProfessionalIndemnity-Proposal", Input);

        Submitted = true;
        return Page();
    }

    public class FormInput
    {
        public string? Website { get; set; }

        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(160)]
        public string Profession { get; set; } = string.Empty;

        // PENDING from Amura: confirmed proposal-form field list for this product.
        [StringLength(2000)]
        public string? AdditionalDetails { get; set; }
    }
}
