namespace MyFitMeal.Services
{
    /* Purpose: Pure mathematical engine for Macros.
       Logic: Weight (kg), Height (cm), Age.
    */
    public class CalculatorService
    {
        public (double Calories, double Protein, double Carbs, double Fats) CalculateMacros(Models.UserProfile profile)
        {
            // 1. Calculate BMR (Basal Metabolic Rate) - Male Formula
            double bmr = (10 * profile.Weight) + (6.25 * profile.Height) - (5 * profile.Age) + 5;

            // 2. Adjust for Activity Level
            double multiplier = profile.ActivityLevel switch
            {
                "Sedentary" => 1.2,
                "Moderate" => 1.55,
                "Active" => 1.725,
                _ => 1.2
            };

            double tdee = bmr * multiplier;

            // 3. Goal Adjustment
            double targetCalories = profile.Goal switch
            {
                "Fat Control" => tdee - 500, // Deficit
                "Muscle Support" => tdee + 300, // Surplus
                _ => tdee // Maintenance
            };

            // 4. Macro Splits (Standard for Indian bodies)
            // Protein: 1.8g per kg weight (if muscle goal) or 1.2g (standard)
            double protein = profile.Goal == "Muscle Support" ? profile.Weight * 1.8 : profile.Weight * 1.2;

            // Fats: 25% of total calories (1g fat = 9 cal)
            double fats = (targetCalories * 0.25) / 9;

            // Carbs: Remainder (1g carb = 4 cal)
            double carbs = (targetCalories - (protein * 4) - (fats * 9)) / 4;

            return (Math.Round(targetCalories), Math.Round(protein), Math.Round(carbs), Math.Round(fats));
        }
    }
}