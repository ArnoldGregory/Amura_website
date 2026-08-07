using AmuraWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.GetQuote;

public class ProfessionalIndemnityModel : PageModel
{
    [BindProperty]
    public FormInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // Honeypot check happens before the redirect fires — this form has no inline
        // success state (it always redirects to Proposal), so this is the only place
        // a bot submission can be caught.
        if (SpamGuard.IsHoneypotTripped(Input.Website))
        {
            // Silently swallow it — don't send bots on to the Proposal form.
            return RedirectToPage("/GetQuote/ProfessionalIndemnity");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Carry the captured details forward so Proposal can continue the flow
        // without asking the person to repeat themselves.
        TempData["ProposalFullName"] = Input.FullName;
        TempData["ProposalEmail"] = Input.Email;
        TempData["ProposalPhone"] = Input.Phone;
        TempData["ProposalProfession"] = Input.Profession;

        return RedirectToPage("/GetQuote/Proposal");
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
    }
}
