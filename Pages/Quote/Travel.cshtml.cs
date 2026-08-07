using AmuraWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.Quote;

public class TravelModel : PageModel
{
    private readonly IQuoteSubmissionService _submissions;

    public TravelModel(IQuoteSubmissionService submissions)
    {
        _submissions = submissions;
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    public bool Submitted { get; set; }
    public string? ReferenceNumber { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var submission = new QuoteSubmission
        {
            Type = SubmissionType.Travel,
            ContactName = Input.FullName,
            ContactEmail = Input.Email,
            ContactPhone = Input.Phone,
            Details = new()
            {
                ["Destination"] = Input.Destination,
                ["DepartureDate"] = Input.DepartureDate.ToString("yyyy-MM-dd"),
                ["ReturnDate"] = Input.ReturnDate.ToString("yyyy-MM-dd"),
                ["Travelers"] = Input.Travelers.ToString()
            }
        };

        ReferenceNumber = await _submissions.SubmitAsync(submission);
        Submitted = true;
        return Page();
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

        [Required, StringLength(120)]
        public string Destination { get; set; } = string.Empty;

        [Required, DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        [Range(1, 20)]
        public int Travelers { get; set; } = 1;
    }
}
