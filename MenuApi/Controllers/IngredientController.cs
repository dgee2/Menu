using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MenuApi.Repositories;
using MenuApi.Search;
using MenuApi.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Search;

namespace MenuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientController : ControllerBase
    {
        private readonly IIngredientRepository ingredientRepository;

        public IngredientController(IIngredientRepository ingredientRepository)
        {
            this.ingredientRepository = ingredientRepository ?? throw new ArgumentNullException(nameof(ingredientRepository));
        }

        [HttpGet]
        public IAsyncEnumerable<Ingredient> GetIngredients() => ingredientRepository.GetIngredientsAsync();

        [HttpPost]
        public async Task PostIngredient(NewIngredient ingredient) => await ingredientRepository.CreateIngredientAsync(ingredient).ConfigureAwait(false);

        [HttpPut]
        public async Task PutIngredient(Ingredient ingredient) => await ingredientRepository.UpdateIngredientAsync(ingredient).ConfigureAwait(false);

        [HttpGet("search")]
        public async Task<IEnumerable<Ingredient>> SearchIngredients([FromQuery] string q) => await ingredientRepository.SearchIngredientsAsync(q).ConfigureAwait(false);
    }
}