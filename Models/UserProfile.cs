using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MyFitMeal.Models
{
    /* Purpose: Stores comprehensive fitness data for a user.
       Relationships: Linked 1:1 with IdentityUser via UserId.
    */
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        // Add the '?' to make these nullable for the form submission
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        //[Required]
        //public string Gender { get; set; }

        [Required]
        public int Age { get; set; }
        public double Height { get; set; } // in cm
        public double Weight { get; set; } // in kg
        public string Gender { get; set; } // Male/Female (MEN focused but logic stays robust)

        // Step 2-5: Goals & Lifestyle
        public string Goal { get; set; } // Fat Loss, Muscle Gain, Maintenance
        public string ActivityLevel { get; set; } // Sedentary, Moderate, Active
        public string LifestyleType { get; set; } // College, Office, Home
        public string Budget { get; set; } // Budget, Standard, Premium
        public string FoodPreference { get; set; } // Veg, Non-Veg, Eggetarian

        // Logic Flag
        public bool IsProfileComplete { get; set; } = false;

        // Theme Preference (UI Fix)
        public bool IsDarkMode { get; set; } = false;
    }
}