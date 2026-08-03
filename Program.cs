using AmuraWebsite.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var app = builder.Build();

// Content root (project folder), not AppContext.BaseDirectory (build output) —
// the latter gets wiped on `dotnet clean` / rebuild, which would silently lose
// every queued quote submission. See QuoteSubmissionStore's own comments.
QuoteSubmissionStore.Initialize(app.Environment.ContentRootPath);

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
