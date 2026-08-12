using AmuraWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AmuraWebsite.Pages.Quote;

public class MedicalIndividualModel : PageModel
{
    private readonly IQuoteSubmissionService _submissions;

    public MedicalIndividualModel(IQuoteSubmissionService submissions)
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
        // Server-side rule from the platform: at most one Spouse among the
        // dependant rows (any number of Children is fine).
        var rows = new[] { Input.Dependent1, Input.Dependent2, Input.Dependent3, Input.Dependent4, Input.Dependent5 };
        var spouseCount = rows.Count(d => d.Relationship == "Spouse" && !string.IsNullOrWhiteSpace(d.FullName));
        if (spouseCount > 1)
        {
            ModelState.AddModelError(string.Empty, "Only one spouse can be listed per quote.");
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

        var familyMembers = rows
            .Where(d => !string.IsNullOrWhiteSpace(d.FullName) && d.Relationship != "None" && d.DateOfBirth.HasValue)
            .Select(d => new { relationship = d.Relationship, fullName = d.FullName!, dateOfBirth = d.DateOfBirth!.Value.ToString("yyyy-MM-dd") })
            .ToList();

        var submission = new QuoteSubmission
        {
            Type = SubmissionType.MedicalIndividual,
            ContactName = Input.FullName,
            ContactEmail = Input.Email,
            ContactPhone = Input.Phone,
            Details = new()
            {
                ["IdNo"] = Input.IdNo,
<<<<<<< HEAD
                ["DateOfBirth"] = Input.DateOfBirth!.Value.ToString("yyyy-MM-dd"),
=======
                ["DateOfBirth"] = Input.DateOfBirth.ToString("yyyy-MM-dd"),
>>>>>>> cff447d3d43ed2aaef6127c261952d345a8fab76
                ["FamilyMembersJson"] = JsonSerializer.Serialize(familyMembers)
            }
        };

        ReferenceNumber = await _submissions.SubmitAsync(submission);
        Submitted = true;
        return Page();
    }

    public class DependentInput
    {
        public string Relationship { get; set; } = "None";
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    // Field set confirmed against the real platform's
    // POST /api/quoterequests/medical-individual contract.
    public class FormInput
    {
        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(40)]
        [Display(Name = "National ID / Passport number")]
        public string IdNo { get; set; } = string.Empty;

<<<<<<< HEAD
        [Required(ErrorMessage = "Date of birth is required."), DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
=======
        [Required, DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
>>>>>>> cff447d3d43ed2aaef6127c261952d345a8fab76

        // Up to 5 dependant rows — matches the platform's familyMembers[]
        // array (relationship must be "Spouse", at most one, or "Child",
        // any number). Blank rows are simply not sent.
        public DependentInput Dependent1 { get; set; } = new();
        public DependentInput Dependent2 { get; set; } = new();
        public DependentInput Dependent3 { get; set; } = new();
        public DependentInput Dependent4 { get; set; } = new();
        public DependentInput Dependent5 { get; set; } = new();

        // Honeypot — real users never see or fill this in.
        public string? Website { get; set; }
    }
}
