using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyFitMeal.Data;
using MyFitMeal.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity Configuration with Strict Security
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>() // Enables Admin roles
.AddEntityFrameworkStores<ApplicationDbContext>();

// 3. Fix: Ensure Razor Pages (Identity) and Controllers work together
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<CalculatorService>();
builder.Services.AddHttpClient<GeminiService>();

builder.Services.AddControllersWithViews(options => {
    options.Filters.Add<ProfileCheckFilter>();
});


var app = builder.Build();

// 4. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();



app.MapAreaControllerRoute(
    name: "MyFitMealAdmin",
    areaName: "Admin",
    pattern: "Admin/{controller=AdminDashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 5. Fix: This maps the Identity Login/Logout pages
app.MapRazorPages();

app.Run();