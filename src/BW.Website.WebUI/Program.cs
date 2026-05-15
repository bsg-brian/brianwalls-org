using BW.Website.Infrastructure.Data.Contexts;
using BW.Website.WebUI.WebInfrastructure.CompositionRoot;
using BW.Website.WebUI.WebInfrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Centralized composition
builder.Services.AddWebUiServices(builder.Configuration);

var app = builder.Build();

// Sample SQLite database creation.
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	db.Database.EnsureCreated();
}

// Standard ASP.NET Core middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();

app.UseRouting();

//app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
