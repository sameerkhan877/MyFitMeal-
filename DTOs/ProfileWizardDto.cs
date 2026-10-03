namespace MyFitMeal.DTOs
{
    /* Purpose: Holds temporary data during the 7-step wizard.
       Prevents: Partial data corruption in the main UserProfile table.
    */
    public class ProfileWizardDto
    {
        // We group these to match your Wizard Steps
        public int Age { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public string Goal { get; set; } // Options: Fat Control, Muscle Support, Energy
        public string ActivityLevel { get; set; }
        public string Budget { get; set; } // Options: Low (College), Mid, High
        public string LifestyleType { get; set; } // College, Office, Home
        public string FoodPreference { get; set; }
    }
}