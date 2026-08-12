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
<<<<<<< HEAD
        if (Input.DepartureDate.HasValue && Input.ReturnDate.HasValue && Input.ReturnDate < Input.DepartureDate)
=======
        if (Input.ReturnDate < Input.DepartureDate)
>>>>>>> cff447d3d43ed2aaef6127c261952d345a8fab76
        {
            ModelState.AddModelError(nameof(Input.ReturnDate), "Return date can't be before the departure date.");
        }

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
            Type = SubmissionType.Travel,
            ContactName = Input.FullName,
            ContactEmail = Input.Email,
            ContactPhone = Input.Phone,
            Details = new()
            {
<<<<<<< HEAD
                ["DateOfBirth"] = Input.DateOfBirth!.Value.ToString("yyyy-MM-dd"),
                ["KraPin"] = Input.KraPin ?? string.Empty,
                ["Destination"] = Input.Destination,
                ["DepartureDate"] = Input.DepartureDate!.Value.ToString("yyyy-MM-dd"),
                ["ReturnDate"] = Input.ReturnDate!.Value.ToString("yyyy-MM-dd"),
=======
                ["DateOfBirth"] = Input.DateOfBirth.ToString("yyyy-MM-dd"),
                ["KraPin"] = Input.KraPin ?? string.Empty,
                ["Destination"] = Input.Destination,
                ["DepartureDate"] = Input.DepartureDate.ToString("yyyy-MM-dd"),
                ["ReturnDate"] = Input.ReturnDate.ToString("yyyy-MM-dd"),
>>>>>>> cff447d3d43ed2aaef6127c261952d345a8fab76
                ["TravellingWithFamily"] = Input.TravellingWithFamily.ToString(),
                ["TripType"] = Input.TripType
            }
        };

        ReferenceNumber = await _submissions.SubmitAsync(submission);
        Submitted = true;
        return Page();
    }

    // Field set confirmed against the real platform's
    // POST /api/quoterequests/travel contract. tripType must be
    // VACATION, BUSINESS, or SPORTS. Email/phone aren't part of the
    // platform's schema for this endpoint but are kept here for our own
    // follow-up records.
    public class FormInput
    {
        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

<<<<<<< HEAD
        [Required(ErrorMessage = "Date of birth is required."), DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime? DateOfBirth { get; set; }
=======
        [Required, DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime DateOfBirth { get; set; }
>>>>>>> cff447d3d43ed2aaef6127c261952d345a8fab76

        [StringLength(20)]
        [Display(Name = "KRA PIN (optional)")]
        public string? KraPin { get; set; }

        [Required, StringLength(120)]
        public string Destination { get; set; } = string.Empty;

<<<<<<< HEAD
        [Required(ErrorMessage = "Departure date is required."), DataType(DataType.Date)]
        public DateTime? DepartureDate { get; set; }

        [Required(ErrorMessage = "Return date is required."), DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }
=======
        [Required, DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }
>>>>>>> cff447d3d43ed2aaef6127c261952d345a8fab76

        [Display(Name = "Travelling with family?")]
        public bool TravellingWithFamily { get; set; }

        [Required]
        public string TripType { get; set; } = "VACATION";

        // Honeypot — real users never see or fill this in.
        public string? Website { get; set; }
    }
}
