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
        public async Task<FullRecipe> GetRecipeAsync(int recipeId)
        {
            var dbRecipe = await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false);
            var dbIngredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);

            var recipe = mapper.Map<FullRecipe>(dbRecipe);
            recipe.Ingredients = dbIngredients.Select(mapper.Map<RecipeIngredient>);

            return recipe;
        }

        [HttpGet("{recipeId}/Ingredient")]
        public async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId)
        {
            var ingredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);
            return ingredients.Select(mapper.Map<RecipeIngredient>);
        }

        [HttpPost]
        public async Task<FullRecipe> CreateRecipeAsync([FromBody] NewRecipe newRecipe)
        {
            if (newRecipe is null)
            {
                throw new ArgumentNullException(nameof(newRecipe));
            }

            var ingredients = newRecipe.Ingredients.Select(mapper.Map<DBModel.RecipeIngredient>);

            var recipeId = await recipeRepository.CreateRecipeAsync(newRecipe.Name).ConfigureAwait(false);

            await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ingredients).ConfigureAwait(false);

            var recipe = await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false);
            var result = mapper.Map<FullRecipe>(recipe);
            var recipeIngredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);
            result.Ingredients = recipeIngredients.Select(mapper.Map<RecipeIngredient>);

            return result;
        }
    }
}
