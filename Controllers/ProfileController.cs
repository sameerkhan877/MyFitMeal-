using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFitMeal.Data;
using MyFitMeal.Models;
using System.Security.Claims;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Start the Wizard
    public IActionResult Wizard()
    {
        // Check if user already has a profile
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == userId);

        if (profile != null && profile.IsProfileComplete)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteProfile(UserProfile model)
    {
        // 1. Force remove the 'User' and 'UserId' from validation 
        // because we assign them manually below.
        ModelState.Remove("User");
        ModelState.Remove("UserId");

        if (!ModelState.IsValid)
        {
            // DEBUG: This will list all errors in your "Output" window in Visual Studio
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine("VALIDATION ERROR: " + error.ErrorMessage);
            }

            // If invalid, go back to step 1
            return View("Wizard", model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        model.UserId = userId;
        model.IsProfileComplete = true;

        _context.UserProfiles.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Dashboard");
    }
    //--------------------------------- Logic to Check Profile Status on Login ------------------//
    public async Task<IActionResult> CheckProfileStatus()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null || !profile.IsProfileComplete)
        {
            return RedirectToAction("Wizard");
        }

        return RedirectToAction("Index", "Dashboard");
    }
}