using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MenuApi.Repositories;
using MenuApi.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MenuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeRepository recipeRepository;

        public RecipeController(IRecipeRepository recipeRepository)
        {
            this.recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        }

        [HttpGet]
        public async Task<IEnumerable<Recipe>> GetRecipesAsync() => await recipeRepository.GetRecipesAsync().ConfigureAwait(false);

        [HttpGet("{recipeId}")]
        public async Task<Recipe> GetRecipeAsync(int recipeId) => await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false);

        [HttpGet("{recipeId}/Ingredient")]
        public async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId) => await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);

        [HttpPost]
        public async Task<Recipe> CreateRecipeAsync([FromBody]NewRecipe newRecipe)
        {
            if (newRecipe is null)
            {
                throw new ArgumentNullException(nameof(newRecipe));
            }

            return await recipeRepository.CreateRecipeAsync(newRecipe.Name, newRecipe.Ingredients).ConfigureAwait(false);
        }
    }
}
