namespace RestaurantsAPIManagement.Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantAddress { get; set; }
        public List<string> RestaurantMeals { get; set; }
    }
}
