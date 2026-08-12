using AmuraWebsite.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IQuoteSubmissionService, QuoteSubmissionService>();

builder.Services.Configure<InsurancePlatformOptions>(
    builder.Configuration.GetSection(InsurancePlatformOptions.SectionName));
builder.Services.AddSingleton<PlatformTokenCache>();
builder.Services.AddHttpClient<IInsurancePlatformClient, InsurancePlatformClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<InsurancePlatformOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Behind nginx (reverse proxy) in production — trust the X-Forwarded-*
// headers so HTTPS redirection, HSTS, and client IP all work correctly.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
