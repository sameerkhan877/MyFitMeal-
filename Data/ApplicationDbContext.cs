using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyFitMeal.Models;

namespace MyFitMeal.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<WeeklyPlan> WeeklyPlans { get; set; }
        public DbSet<DailyDiet> DailyDiets { get; set; }
        public DbSet<FavoriteMeal> FavoriteMeals { get; set; }
        public DbSet<WeightLog> WeightLogs { get; set; }
    }
}
