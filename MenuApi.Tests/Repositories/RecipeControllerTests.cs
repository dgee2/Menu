using AutoFixture.NUnit3;
using AutoMapper;
using FakeItEasy;
using MenuApi.Controllers;
using MenuApi.Repositories;
using MenuApi.Tests.Factory;
using NUnit.Framework;
using Shouldly;
using System.Threading.Tasks;

namespace MenuApi.Tests
{
    public class RecipeControllerTests
    {
        RecipeController sut;
        IMapper mapper;
        IRecipeRepository recipeRepository;

        [SetUp]
        public void Setup()
        {
            mapper = AutoMapperFactory.CreateMapper();
            recipeRepository = A.Fake<IRecipeRepository>();

            sut = new RecipeController(recipeRepository, mapper);
        }

        [Test, AutoData]
        public async Task GetRecipeSuccess(int recipeId, string name)
        {
            var dbResult = new DBModel.Recipe
            {
                Id = recipeId,
                Name = name
            };
            A.CallTo(() => recipeRepository.GetRecipeAsync(recipeId)).Returns(Task.FromResult(dbResult));

            var result = await sut.GetRecipeAsync(recipeId);
            result.Name.ShouldBe(name);
            result.Id.ShouldBe(recipeId);
        }
    }
}