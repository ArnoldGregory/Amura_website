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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO (Task #4 — Backend quote submission & routing):
        // wire this into the same routing/notification pipeline used for
        // Get Quote submissions once it exists (route to back office,
        // send acknowledgement email).

        Submitted = true;
        return Page();
    }

    public class ContactFormInput
    {
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
