using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFitMeal.Data;
using MyFitMeal.Models;
using MyFitMeal.Services;
using System.Security.Claims;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CalculatorService _calcService;

    public DashboardController(ApplicationDbContext context, CalculatorService calcService)
    {
        _context = context;
        _calcService = calcService;
    }

    // --- 1. MAIN DASHBOARD (Updated with Expiry & Sync Logic) ---
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Profile check
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null || !profile.IsProfileComplete)
            return RedirectToAction("Wizard", "Profile");

        // Fetch Diet Plan
        var plan = await _context.WeeklyPlans
                            .Include(p => p.Days)
                            .FirstOrDefaultAsync(p => p.UserId == userId);

        // --- THE UPDATED AUTO-DAY & EXPIRY LOGIC ---
        if (plan != null)
        {
            var timeSpan = DateTime.Now.Date - plan.CreatedDate.Date;
            int dayCounter = (int)timeSpan.TotalDays + 1;

            // Check if plan is older than 7 days
            if (dayCounter > 7)
            {
                ViewBag.IsExpired = true; // For UI to show "Generate New Plan" button
                ViewBag.ActiveDay = 1;
            }
            else
            {
                ViewBag.IsExpired = false;
                ViewBag.ActiveDay = ((dayCounter - 1) % 7) + 1;
            }
        }
        else
        {
            ViewBag.ActiveDay = 1;
            ViewBag.IsExpired = false;
            ViewBag.Lifestyle = profile.LifestyleType;
            return View("NoPlan");
        }

        // Fetch Weight History for Chart
        var weightLogs = await _context.WeightLogs
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.DateLogged)
            .Take(7)
            .OrderBy(w => w.DateLogged)
            .ToListAsync();

        ViewBag.WeightData = weightLogs.Select(w => w.Value).ToList();
        ViewBag.WeightLabels = weightLogs.Select(w => w.DateLogged.ToString("dd MMM")).ToList();

        // UI Widget Data (Automatically uses synced profile weight)
        var macros = _calcService.CalculateMacros(profile);
        ViewBag.TargetCals = macros.Calories;
        ViewBag.Protein = macros.Protein;
        ViewBag.CurrentWeight = profile.Weight;

        // Health Tracking Widgets
        ViewBag.Steps = 5500;
        ViewBag.WaterGlasses = 12;
        ViewBag.UserDisplayName = profile.Gender == "Male" ? "Mr. " + User.Identity.Name.Split('@')[0] : "Ms. " + User.Identity.Name.Split('@')[0];

        return View(plan);
    }

    // --- 2. LIVE WEIGHT UPDATE (Updated with Auto-Profile Sync) ---
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateWeight(double weight)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (weight <= 0) return RedirectToAction("Index");

        // 1. Add to Weight Logs history
        var log = new WeightLog { UserId = userId, Value = weight, DateLogged = DateTime.Now };
        _context.WeightLogs.Add(log);

        // 2. SYNC LOGIC: Update the main User Profile immediately
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null)
        {
            profile.Weight = weight; // This ensures future AI generations use this weight
            _context.UserProfiles.Update(profile);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // --- 3. STATISTICS PAGE ---
    public async Task<IActionResult> Statistics()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile != null)
        {
            var macros = _calcService.CalculateMacros(profile);
            ViewBag.TargetCals = macros.Calories;
            ViewBag.Protein = macros.Protein;
            ViewBag.CurrentWeight = profile.Weight;

            var history = await _context.WeightLogs
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.DateLogged)
                .ToListAsync();

            ViewBag.HistoryData = history.Select(h => h.Value).ToList();
            ViewBag.HistoryLabels = history.Select(h => h.DateLogged.ToString("MMM dd")).ToList();
        }

        return View();
    }

    // --- 4. AI GENERATION & MEALS (Uses updated profile weight) ---
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> GenerateFullPlan([FromServices] GeminiService geminiService)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        // This now uses the synced weight from UpdateWeight
        var result = _calcService.CalculateMacros(profile);

        string prompt = $@"Generate a 7-day Indian diet plan in JSON format.
            Target: {result.Calories} kcal. Protein: {result.Protein}g.
            Lifestyle: {profile.LifestyleType}. Food: {profile.FoodPreference}. Budget: {profile.Budget}.
            Return ONLY a JSON array with: DayNumber, Breakfast, Lunch, Dinner, Snack.";

        var aiResponse = await geminiService.GetDietPlan(prompt);

        if (aiResponse != null)
        {
            var oldPlan = await _context.WeeklyPlans.Include(p => p.Days).FirstOrDefaultAsync(p => p.UserId == userId);
            if (oldPlan != null) _context.WeeklyPlans.Remove(oldPlan);

            var newPlan = new WeeklyPlan { UserId = userId, CreatedDate = DateTime.Now };
            foreach (var day in aiResponse)
            {
                newPlan.Days.Add(new DailyDiet
                {
                    DayNumber = day.DayNumber,
                    Breakfast = day.Breakfast,
                    Lunch = day.Lunch,
                    Snack = day.Snack,
                    Dinner = day.Dinner
                });
            }
            _context.WeeklyPlans.Add(newPlan);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    // --- 5. REMAINDER OF EXISTING LOGIC (Details, Favorites, etc.) ---
    public async Task<IActionResult> Details(int id)
    {
        var day = await _context.DailyDiets.Include(d => d.WeeklyPlan).FirstOrDefaultAsync(d => d.Id == id);
        if (day == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null)
        {
            var result = _calcService.CalculateMacros(profile);
            ViewBag.TargetCals = result.Calories; ViewBag.Protein = result.Protein;
        }
        return View(day);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleFavorite(string mealName, string mealType)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existing = await _context.FavoriteMeals.FirstOrDefaultAsync(f => f.UserId == userId && f.MealName == mealName);

        if (existing != null)
        {
            _context.FavoriteMeals.Remove(existing);
            await _context.SaveChangesAsync();
            return Json(new { success = true, status = "removed" });
        }

        var fav = new FavoriteMeal { UserId = userId, MealName = mealName, MealType = mealType, DateSaved = DateTime.Now };
        _context.FavoriteMeals.Add(fav);
        await _context.SaveChangesAsync();
        return Json(new { success = true, status = "added" });
    }

    public async Task<IActionResult> Favorites()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return View(await _context.FavoriteMeals.Where(f => f.UserId == userId).OrderByDescending(f => f.DateSaved).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var fav = await _context.FavoriteMeals.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        if (fav != null)
        {
            _context.FavoriteMeals.Remove(fav);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false });
    }
}