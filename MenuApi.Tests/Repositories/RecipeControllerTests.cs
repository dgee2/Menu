using AutoFixture.NUnit3;
using AutoMapper;
using FakeItEasy;
using MenuApi.Controllers;
using MenuApi.Repositories;
using MenuApi.Tests.Factory;
using NUnit.Framework;
using Shouldly;
using System;
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

        [Test]
        public void Constructor_Should_Throw_Exception_For_null_recipeRepository()
        {
            Func<RecipeController> fun = () => new RecipeController(null, mapper);
            var ex = fun.ShouldThrow<ArgumentNullException>();
            ex.ParamName.ShouldBe("recipeRepository");
        }

        [Test]
        public void Constructor_Should_Throw_Exception_For_null_mapper()
        {
            Func<RecipeController> fun = () => new RecipeController(recipeRepository, null);
            var ex = fun.ShouldThrow<ArgumentNullException>();
            ex.ParamName.ShouldBe("mapper");
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