namespace MyFitMeal.DTOs
{
    /* Purpose: Data transfer between Gemini API and Dashboard.
       Ensures: AI response structure matches our C# logic.
    */
    public class DailyDietDto
    {
        public int DayNumber { get; set; }
        public string Breakfast { get; set; }
        public string Lunch { get; set; }
        public string Dinner { get; set; }
        public string Snack { get; set; }
    }
}