namespace RestaurantsAPIManagement.Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantAddress { get; set; }
        public Dictionary<string, int> RestaurantMeals { get; set; }
    }
}
