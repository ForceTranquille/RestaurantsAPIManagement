namespace RestaurantsAPIManagement.Models
{
    public class MockRestaurantService
    {
        private List<Restaurant> _restaurants;

        public MockRestaurantService()
        {
            _restaurants = new List<Restaurant>
            {
                new Restaurant { RestaurantId = 1, RestaurantName = "Restaurant A", RestaurantAddress = "1517 Shattuck Ave, Berkeley, CA 94709", RestaurantMeals = new List<string>(){ "52842", "52960", "52953" } },
                new Restaurant { RestaurantId = 2, RestaurantName = "Restaurant B", RestaurantAddress = "1234 Jean Talon, Montreal, CA 94709", RestaurantMeals = new List<string>(){ "52945", "52863", "53018" } },
            };
        }
        public async Task<List<Restaurant>> GetAllRestaurantsAsync()
        {
            return await Task.FromResult(_restaurants);
        }
        public async Task<Restaurant> GetRestaurantByIdAsync(int id)
        {
            return await Task.FromResult(_restaurants.FirstOrDefault(r =>
                                                     r.RestaurantId == id));
        }
        public async Task AddRestaurantAsync(Restaurant restaurant)
        {
            var newId = _restaurants.Max(r => r.RestaurantId) + 1;
            restaurant.RestaurantId = newId;
            _restaurants.Add(restaurant);

            await Task.CompletedTask; 
        }
    }
}
