using AmuraWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages;

public class ContactModel : PageModel
{
    [BindProperty]
    public ContactFormInput Input { get; set; } = new();

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
        // message isn't lost. TODO: once the backend exists, wire this into the
        // same routing/notification pipeline used for Get Quote submissions
        // (route to back office, send acknowledgement email).
        QuoteSubmissionStore.Save("Contact", Input);

        Submitted = true;
        return Page();
    }

    public class ContactFormInput
    {
        public string? Website { get; set; }

        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [Required, StringLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}
