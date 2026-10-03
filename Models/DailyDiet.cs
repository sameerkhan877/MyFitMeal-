
namespace MyFitMeal.Models
{
    public class DailyDiet
    {

        public bool IsAdminOverride { get; set; } = false; // Flag to identify manual changes
        public string? AdminNote { get; set; } // Notes from the admin to the user
        public int Id { get; set; }
        public int DayNumber { get; set; } // 1 to 7
        public string Breakfast { get; set; }
        public string Lunch { get; set; }
        public string Dinner { get; set; }
        public string Snack { get; set; }

        public int WeeklyPlanId { get; set; }
        public WeeklyPlan WeeklyPlan { get; set; }
    }
}