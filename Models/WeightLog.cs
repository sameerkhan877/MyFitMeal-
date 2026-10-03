namespace MyFitMeal.Models
{
    public class WeightLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public double Value { get; set; }
        public DateTime DateLogged { get; set; }
    }
}
