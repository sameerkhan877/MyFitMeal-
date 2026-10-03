using System.ComponentModel.DataAnnotations;

namespace MyFitMeal.Models
{
    /* Purpose: A master list of Indian meals.
       Prevents: AI inventing meals that don't exist.
    */
    public class Meal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // e.g., "Paneer Bhurji & Roti"

        public string Category { get; set; } // Breakfast, Lunch, Dinner, Snack

        public string Preference { get; set; } // Veg, Non-Veg, Eggetarian

        // Macros per standard serving
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }

        public string Description { get; set; } // e.g., "High protein, easy for office tiffin"

        public bool IsTiffinFriendly { get; set; } // Logic for Office lifestyle
        public bool IsBudgetFriendly { get; set; } // Logic for College lifestyle
    }
}