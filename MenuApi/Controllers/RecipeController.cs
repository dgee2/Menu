using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper mapper;

        public RecipeController(IRecipeRepository recipeRepository, IMapper mapper)
        {
            this.recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<IEnumerable<Recipe>> GetRecipesAsync()
        {
            var recipes = await recipeRepository.GetRecipesAsync().ConfigureAwait(false);
            return recipes.Select(mapper.Map<Recipe>);
        }

        [HttpGet("{recipeId}")]
        public async Task<Recipe> GetRecipeAsync(int recipeId)
        {
            var recipe = await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false);
            return mapper.Map<Recipe>(recipe);
        }

        [HttpGet("{recipeId}/Ingredient")]
        public async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId)
        {
            var ingredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);
            return ingredients.Select(mapper.Map<RecipeIngredient>);
        }

        [HttpPost]
        public async Task<Recipe> CreateRecipeAsync([FromBody] NewRecipe newRecipe)
        {
            if (newRecipe is null)
            {
                throw new ArgumentNullException(nameof(newRecipe));
            }

            var recipe = await recipeRepository.CreateRecipeAsync(newRecipe.Name, newRecipe.Ingredients).ConfigureAwait(false);
            return mapper.Map<Recipe>(recipe);
        }
    }
}
