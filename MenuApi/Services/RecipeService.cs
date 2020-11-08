using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MenuApi.Factory;
using MenuApi.Repositories;
using MenuApi.ViewModel;

namespace MenuApi.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository recipeRepository;
        private readonly IMapper mapper;
        private readonly ITransactionFactory transactionFactory;

        public RecipeService(IRecipeRepository recipeRepository, IMapper mapper, ITransactionFactory transactionFactory)
        {
            this.recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.transactionFactory = transactionFactory ?? throw new ArgumentNullException(nameof(transactionFactory));
        }

        public async Task<IEnumerable<Recipe>> GetRecipesAsync()
        {
            var recipes = await recipeRepository.GetRecipesAsync().ConfigureAwait(false);
            return recipes.Select(mapper.Map<Recipe>);
        }

        public async Task<FullRecipe> GetRecipeAsync(int recipeId)
        {
            var dbRecipe = await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false);
            var dbIngredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);

            var recipe = mapper.Map<FullRecipe>(dbRecipe);
            recipe.Ingredients = dbIngredients.Select(mapper.Map<RecipeIngredient>);

            return recipe;
        }

        public async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId)
        {
            var ingredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);
            return ingredients.Select(mapper.Map<RecipeIngredient>);
        }

        public async Task<int> CreateRecipeAsync(NewRecipe newRecipe)
        {
            if (newRecipe is null)
            {
                throw new ArgumentNullException(nameof(newRecipe));
            }

            var ingredients = newRecipe.Ingredients.Select(mapper.Map<DBModel.RecipeIngredient>);

            using var tran = transactionFactory.BeginTransaction();
            var recipeId = await recipeRepository.CreateRecipeAsync(newRecipe.Name, tran).ConfigureAwait(false);

            await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ingredients, tran).ConfigureAwait(false);

            tran.Commit();
            return recipeId;
        }

        public async Task UpdateRecipeAsync(int recipeId, NewRecipe newRecipe)
        {
            if (newRecipe is null)
            {
                throw new ArgumentNullException(nameof(newRecipe));
            }

            var ingredients = newRecipe.Ingredients.Select(mapper.Map<DBModel.RecipeIngredient>);

            using var tran = transactionFactory.BeginTransaction();
            await recipeRepository.UpdateRecipeAsync(recipeId, newRecipe.Name, tran).ConfigureAwait(false);

            await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ingredients, tran).ConfigureAwait(false);
            tran.Commit();
        }
    }
}
