namespace MyFitMeal.Models
{
    public class WeeklyPlan
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public List<DailyDiet> Days { get; set; } = new List<DailyDiet>();
    }
}