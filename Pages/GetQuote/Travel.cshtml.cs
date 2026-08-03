using AmuraWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.GetQuote;

public class TravelModel : PageModel
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
        QuoteSubmissionStore.Save("Travel", Input);

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

        [Required]
        public string TripType { get; set; } = string.Empty; // Single trip / Frequent traveller

        [Required, StringLength(120)]
        public string Destination { get; set; } = string.Empty;

        [Required, DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        // PENDING from Amura: confirmed field list for this product.
        [StringLength(2000)]
        public string? AdditionalDetails { get; set; }
    }
}
