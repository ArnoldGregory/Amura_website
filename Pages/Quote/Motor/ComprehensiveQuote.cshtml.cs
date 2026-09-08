using AmuraWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AmuraWebsite.Pages.Quote.Motor;

public class ComprehensiveQuoteModel : PageModel
{
    private readonly IMotorPurchaseClient _client;

    public ComprehensiveQuoteModel(IMotorPurchaseClient client)
    {
        _client = client;
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    public ComprehensiveQuoteResponse? Quote { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsConfigured => _client.IsConfigured;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!_client.IsConfigured)
        {
            ErrorMessage = "Comprehensive quotes aren't available right now — please contact us directly.";
            return Page();
        }

        Quote = await _client.GetComprehensiveCompareAsync(Input.VehicleValue);
        if (Quote is null || Quote.Options.Count == 0)
        {
            ErrorMessage = "We couldn't get a comprehensive quote for that vehicle value right now. Please try again shortly, or contact us directly.";
        }

        return Page();
    }

    public class FormInput
    {
        [Required(ErrorMessage = "Please enter your vehicle's value.")]
        [Range(10000, 100000000, ErrorMessage = "Enter a vehicle value between 10,000 and 100,000,000.")]
        [Display(Name = "Vehicle value (KES)")]
        public decimal VehicleValue { get; set; }
    }
}