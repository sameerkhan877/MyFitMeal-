using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyFitMeal.Data;
using System.Security.Claims;

public class ProfileCheckFilter : IActionFilter
{
    private readonly ApplicationDbContext _context;
    public ProfileCheckFilter(ApplicationDbContext context) => _context = context;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Check if user is logged in
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == userId);
            var controller = context.RouteData.Values["controller"]?.ToString();

            // Agar profile incomplete hai aur user Dashboard par hai, toh Wizard par bhejo
            if ((profile == null || !profile.IsProfileComplete) && controller == "Dashboard")
            {
                context.Result = new RedirectToActionResult("Wizard", "Profile", null);
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}