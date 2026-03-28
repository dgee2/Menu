#nullable enable
#nullable enable

using AwesomeAssertions;
using FakeItEasy;
using FluentValidation;
using MenuApi.Validation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MenuApi.Tests.Validation;

public class ValidationFilterTests
{
    private static DefaultHttpContext CreateHttpContextWithValidator()
    {
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection();
        services.AddScoped<IValidator<NewRecipe>, NewRecipeValidator>();
        httpContext.RequestServices = services.BuildServiceProvider();
        return httpContext;
    }

    [Fact]
    public async Task InvalidRequest_Returns400()
    {
        var invalidRecipe = new NewRecipe
        {
            Name = RecipeName.From(new string('a', 501)),
            Ingredients = []
        };

        var httpContext = CreateHttpContextWithValidator();
        var context = A.Fake<EndpointFilterInvocationContext>();
        A.CallTo(() => context.HttpContext).Returns(httpContext);
        A.CallTo(() => context.Arguments).Returns(new List<object?> { invalidRecipe });

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(null);
        var filter = new ValidationFilter<NewRecipe>();

        var result = await filter.InvokeAsync(context, next);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ValidRequest_CallsNext()
    {
        var validRecipe = new NewRecipe
        {
            Name = RecipeName.From("Valid Recipe"),
            Ingredients =
            [
                new RecipeIngredient
                {
                    Name = IngredientName.From("Flour"),
                    Unit = IngredientUnitName.From("Grams"),
                    Amount = IngredientAmount.From(100m)
                }
            ]
        };

        var httpContext = CreateHttpContextWithValidator();
        var context = A.Fake<EndpointFilterInvocationContext>();
        A.CallTo(() => context.HttpContext).Returns(httpContext);
        A.CallTo(() => context.Arguments).Returns(new List<object?> { validRecipe });

        var nextCalled = false;
        var expectedResult = new object();
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(expectedResult);
        };

        var filter = new ValidationFilter<NewRecipe>();
        var result = await filter.InvokeAsync(context, next);

        nextCalled.Should().BeTrue();
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task NullBody_Returns400()
    {
        var httpContext = CreateHttpContextWithValidator();
        var context = A.Fake<EndpointFilterInvocationContext>();
        A.CallTo(() => context.HttpContext).Returns(httpContext);
        A.CallTo(() => context.Arguments).Returns(new List<object?> { });

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(null);
        var filter = new ValidationFilter<NewRecipe>();

        var result = await filter.InvokeAsync(context, next);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UninitializedVogenStruct_Returns400()
    {
        // Simulates when FluentValidation throws accessing uninitialized Vogen structs
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection();
        var throwingValidator = A.Fake<IValidator<NewRecipe>>();
        A.CallTo(() => throwingValidator.ValidateAsync(A<NewRecipe>._, A<CancellationToken>._))
            .ThrowsAsync(new Exception("Use of uninitialized Value Object."));
        services.AddScoped<IValidator<NewRecipe>>(_ => throwingValidator);
        httpContext.RequestServices = services.BuildServiceProvider();

        var recipe = new NewRecipe
        {
            Name = RecipeName.From("test"),
            Ingredients = []
        };
        var context = A.Fake<EndpointFilterInvocationContext>();
        A.CallTo(() => context.HttpContext).Returns(httpContext);
        A.CallTo(() => context.Arguments).Returns(new List<object?> { recipe });

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(null);
        var filter = new ValidationFilter<NewRecipe>();

        var result = await filter.InvokeAsync(context, next);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task NonVogenException_Propagates()
    {
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection();
        var throwingValidator = A.Fake<IValidator<NewRecipe>>();
        A.CallTo(() => throwingValidator.ValidateAsync(A<NewRecipe>._, A<CancellationToken>._))
            .ThrowsAsync(new InvalidOperationException("Some unrelated error"));
        services.AddScoped<IValidator<NewRecipe>>(_ => throwingValidator);
        httpContext.RequestServices = services.BuildServiceProvider();

        var recipe = new NewRecipe
        {
            Name = RecipeName.From("test"),
            Ingredients = []
        };
        var context = A.Fake<EndpointFilterInvocationContext>();
        A.CallTo(() => context.HttpContext).Returns(httpContext);
        A.CallTo(() => context.Arguments).Returns(new List<object?> { recipe });

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(null);
        var filter = new ValidationFilter<NewRecipe>();

        Func<Task> act = async () => await filter.InvokeAsync(context, next);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
