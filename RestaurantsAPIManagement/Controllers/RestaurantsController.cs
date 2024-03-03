using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestaurantsAPIManagement.Models;
using System.Data.Common;
using System.Net.Http;

namespace RestaurantsAPIManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class RestaurantsController : Controller
    {
        private readonly MockRestaurantService _restaurantService;
        private readonly HttpClient _httpClient;

        public RestaurantsController(MockRestaurantService restaurantService, IHttpClientFactory httpClientFactory)
        {
            _restaurantService = restaurantService;
            _httpClient = httpClientFactory.CreateClient();
        }


        //public RestaurantsController(IHttpClientFactory httpClientFactory)
        //{
        //    this._httpClient = httpClientFactory.CreateClient();
        //}

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRestaurants()
        {
            var restaurants = await _restaurantService.GetAllRestaurantsAsync();
            return Ok(restaurants);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }
            return Ok(restaurant);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddRestaurant([FromBody] Restaurant restaurant)
        {
            await _restaurantService.AddRestaurantAsync(restaurant);
            // Return a response indicating the restaurant was created along with a '201 Created' status code 
            return CreatedAtAction(nameof(GetById), new { id = restaurant.RestaurantId }, restaurant);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMealsInARestaurant(int id)
        {
            var restaurantMeals = (await _restaurantService.GetAllRestaurantsAsync()).Where(r => r.RestaurantId == id).Select(m => m.RestaurantMeals).FirstOrDefault();

            if (restaurantMeals == null || restaurantMeals.Count == 0) { return NotFound(); }

            List<Object> meals = new List<Object>();



            //I am using a loop just because I am simulating a db for restaurants and also because we don't have an available endpoint to fetch all the meals one time
            //It is not really good to do this as it is heavy! If we had a db on the cloud for example, a simple LINQ would pull all the meals in a list from the db
            foreach (var meal in restaurantMeals)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"https://www.themealdb.com/api/json/v1/1/lookup.php?i={meal}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var mealResponse = JsonConvert.DeserializeObject<MealsResponse>(content);

                        if (mealResponse?.Meals != null && mealResponse.Meals.Any())
                        {
                            var mealDb = mealResponse.Meals.FirstOrDefault();
                            if (mealDb == null) { return NotFound("Meal not found."); }

                            // Create a response model that only includes the attributes we are interested in
                            var selectedMeal = new
                            {
                                mealDb.StrMeal,
                                mealDb.StrMealThumb
                            };

                            meals.Add(selectedMeal);
                        }
                        else
                        {
                            return NotFound("Meal not found.");
                        }
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve data from TheMealDB.");
                    }

                }
                catch (HttpRequestException e)
                {
                    // Log the exception details
                    return StatusCode(500, "An error occurred while fetching data from TheMealDB API.");
                }
            }

            return Ok(meals);
        }
    }
}
