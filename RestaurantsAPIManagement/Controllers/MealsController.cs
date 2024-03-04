using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestaurantsAPIManagement.Models;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace RestaurantsAPIManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class MealsController : Controller
    {
        private readonly HttpClient _httpClient;

        public MealsController(IHttpClientFactory httpClientFactory)
        {
            this._httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetRandomMeal()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://www.themealdb.com/api/json/v1/1/random.php");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var mealsResponse = JsonConvert.DeserializeObject<MealsResponse>(content);

                    if (mealsResponse?.Meals != null && mealsResponse.Meals.Any())
                    {

                        var meal = mealsResponse.Meals.FirstOrDefault();

                        if (mealsResponse.Meals.Count() == 0) 
                        { 
                            return Ok(new MealsResponse()); 
                        }

                        return Ok(meal); 
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
                
                return StatusCode(500, "An error occurred while fetching data from TheMealDB API.");
            }
        }

        [HttpGet("specific/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSpecificMeal(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://www.themealdb.com/api/json/v1/1/lookup.php?i={id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var mealResponse = JsonConvert.DeserializeObject<MealsResponse>(content);

                    if (mealResponse?.Meals != null && mealResponse.Meals.Any())
                    {
                        var meal = mealResponse.Meals.FirstOrDefault(); 
                        if (meal == null) { return NotFound("Meal not found."); }

                  
                        var selectedMeal = new
                        {
                            meal.StrMeal,
                            meal.StrMealThumb,
                            meal.StrCategory,
                            meal.StrIngredient1,
                            meal.StrIngredient2,
                            meal.StrIngredient3,
                            meal.StrIngredient4,
                            meal.StrIngredient5,
                            meal.StrIngredient6,
                            meal.StrIngredient7,
                            meal.StrIngredient8,
                            meal.StrIngredient9,
                            meal.StrIngredient10,
                            meal.StrIngredient11,
                            meal.StrIngredient12,
                            meal.StrIngredient13,
                            meal.StrIngredient14,
                            meal.StrIngredient15,
                            meal.StrIngredient16,
                            meal.StrIngredient17,
                            meal.StrIngredient18,
                            meal.StrIngredient19,
                            meal.StrIngredient20
                        };

                        return Ok(selectedMeal);
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
               
                return StatusCode(500, "An error occurred while fetching data from TheMealDB API.");
            }
        }
    }
}
