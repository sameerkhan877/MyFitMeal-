namespace MyFitMeal.DTOs
{
    public class DietDayDto
    {
        public int DayNumber { get; set; }
        public string Breakfast { get; set; }
        public string Lunch { get; set; }
        public string Dinner { get; set; }
        public string Snack { get; set; }
        public string NutritionTip { get; set; } // Why this meal fits the lifestyle
    }
}