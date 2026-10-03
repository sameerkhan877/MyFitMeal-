namespace MyFitMeal.Models
{
    public class FavoriteMeal
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Link to the user
        public string MealName { get; set; } // Name of the dish
        public string MealType { get; set; } // Breakfast/Lunch/Dinner
        public DateTime DateSaved { get; set; } = DateTime.Now;
    }
}
