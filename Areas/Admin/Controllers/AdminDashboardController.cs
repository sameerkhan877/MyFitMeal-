using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFitMeal.Data;
using MyFitMeal.Models;

namespace MyFitMeal.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Custom check: Admin role ya specific elite access
    [Authorize]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. MASTER USER LIST ---
        public async Task<IActionResult> Index()
        {
            // Security check: Sirf Admin role ya aapki specific email ko access milega
            if (!(User.IsInRole("Admin") || User.Identity.Name == "afzalkhan52404@gmail.com"))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var profiles = await _context.UserProfiles
                .Include(p => p.User)
                .AsNoTracking()
                .ToListAsync();
            return View(profiles);
        }

        // --- 2. SPECIFIC USER DIET VIEW ---
        [HttpGet]
        public async Task<IActionResult> UserDiet(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest("User ID is missing.");

            var plan = await _context.WeeklyPlans
                .Include(p => p.Days)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (plan == null)
            {
                TempData["Error"] = "Is user ka koi AI diet plan generate nahi hua hai.";
                return RedirectToAction(nameof(Index));
            }

            // Sync User Email for UI Header
            var profile = await _context.UserProfiles.Include(u => u.User).FirstOrDefaultAsync(p => p.UserId == userId);
            ViewBag.UserEmail = profile?.User?.Email ?? "Unknown User";
            ViewBag.UserId = userId;

            return View(plan);
        }

        // --- 3. EMERGENCY DIET RESET ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetDiet(string userId)
        {
            var plan = await _context.WeeklyPlans.FirstOrDefaultAsync(p => p.UserId == userId);
            if (plan != null)
            {
                _context.WeeklyPlans.Remove(plan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User architecture reset successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 4. MEAL OVERRIDE (GET) ---
        // Note: Humne EditMeal.cshtml delete kar di hai, isliye hum "EditUserDiet" view explicitly call karenge
        public async Task<IActionResult> EditMeal(int id)
        {
            var meal = await _context.DailyDiets
                .Include(d => d.WeeklyPlan)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (meal == null) return NotFound();

            // Explicitly calling the existing view file
            return View("EditUserDiet", meal);
        }

        // --- 5. UPDATE PROTOCOL (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserDiet(DailyDiet model)
        {
            // Database se fresh record nikalna zaroori hai include ke saath
            var meal = await _context.DailyDiets
                .Include(d => d.WeeklyPlan)
                .FirstOrDefaultAsync(d => d.Id == model.Id);

            if (meal == null) return NotFound();

            // Admin manual override logic
            meal.Breakfast = model.Breakfast;
            meal.Lunch = model.Lunch;
            meal.Snack = model.Snack;
            meal.Dinner = model.Dinner;
            meal.AdminNote = model.AdminNote;

            try
            {
                _context.Update(meal);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Day {meal.DayNumber} Protocol Synchronized.";

                // Redirecting back to the user's diet list
                return RedirectToAction(nameof(UserDiet), new { userId = meal.WeeklyPlan?.UserId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Sync Error: " + ex.Message);

                // Validation fail hone par wapas "EditUserDiet" view dikhana
                return View("EditUserDiet", model);
            }
        }
    }
}
