using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestaurantsAPIManagement.Data;
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

            return CreatedAtAction(nameof(GetById), new { id = restaurant.RestaurantId }, restaurant);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMealsInARestaurant(int id)
        {
            var restaurant = (await _restaurantService.GetAllRestaurantsAsync()).FirstOrDefault(r => r.RestaurantId == id);

            if (restaurant == null || restaurant.RestaurantMeals.Count == 0)
            { 
                return NotFound(); 
            }

            List<Object> meals = new List<Object>();

            
            foreach (var mealEntry in restaurant.RestaurantMeals)
            {
                var mealId = mealEntry.Key;
                try
                {
                    var response = await _httpClient.GetAsync($"https://www.themealdb.com/api/json/v1/1/lookup.php?i={mealId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var mealResponse = JsonConvert.DeserializeObject<MealsResponse>(content);

                        if (mealResponse?.Meals != null && mealResponse.Meals.Any())
                        {
                            var mealDb = mealResponse.Meals.FirstOrDefault();
                            if (mealDb == null)
                            {
                                // return NotFound("Meal not found."); 
                                continue;// If a meal is not found, continue to the next meal in the loop
                            }
                            var selectedMeal = new
                            {
                                mealDb.StrMeal,
                                mealDb.StrMealThumb,
                                Quantity = mealEntry.Value // Including the quantity in the response
                            };

                            meals.Add(selectedMeal);
                        }
                    }
                    else
                    {
                        // return NotFound("Meal not found.");
                        continue;// Skip this meal and continue with the next one
                    }
                }
                catch (HttpRequestException e)
                {

                    //return StatusCode(500, "An error occurred while fetching data from TheMealDB API.");
                    continue;
                }
            }
            if (!meals.Any())
            {
                return NotFound("No meals found for the restaurant.");
            }
            return Ok(meals);
        }

        [HttpPost("{restaurantId}")]
        [AllowAnonymous]
        public async Task<IActionResult> AddMealToRestaurant(int restaurantId, string mealId = null, int mealQuantity = 0)
        {
            //var randomMeal = await _httpClient.GetAsync($"https://www.themealdb.com/api/json/v1/1/random.php");
            if (mealId == null)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"https://www.themealdb.com/api/json/v1/1/random.php");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var mealsResponse = JsonConvert.DeserializeObject<MealsResponse>(content);

                        if (mealsResponse?.Meals != null && mealsResponse.Meals.Any())
                        {

                            var meal = mealsResponse.Meals.FirstOrDefault();

                            if (mealsResponse.Meals.Count() == 0) { return Ok(new MealsResponse()); }

                            mealId = mealsResponse.Meals.FirstOrDefault().IdMeal;
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

            Restaurant getRestaurant = await _restaurantService.GetRestaurantByIdAsync(restaurantId);
            if (getRestaurant == null) { return NotFound($"The restaurant with the ID:{restaurantId} does NOT exist!"); }
            try
            {
                getRestaurant.RestaurantMeals.Add(mealId, mealQuantity);
                return Ok($"Meal {mealId} added successfully.");
            }
            catch (Exception ex)
            {

                // Log the exception details
                return StatusCode(500, $"An error occurred while adding the meal: {mealId} to the menu of the restaurant: {restaurantId}/{getRestaurant.RestaurantName}.");

            }
        }
    }
}
