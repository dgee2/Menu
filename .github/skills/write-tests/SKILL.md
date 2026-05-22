---
name: write-tests
description: Patterns for writing unit and integration tests in the Menu repository — xUnit, AutoFixture, FakeItEasy, AwesomeAssertions, CustomAutoData, and Aspire integration test conventions.
user-invokable: true
context: inline
---

# Writing Tests

## Purpose

Use this skill when writing or reviewing tests in `MenuApi.Tests` (unit) or `MenuApi.Integration.Tests` (integration). The two projects use different fixture patterns and have different infrastructure requirements.

## Unit tests (`MenuApi.Tests`)

### Stack

- **xUnit** — test runner
- **AutoFixture** — anonymous test data generation
- **FakeItEasy** — fakes / mocks
- **AwesomeAssertions** — fluent assertions (use `.Should()`)

### The `[CustomAutoData]` attribute

Use `[CustomAutoData]` on every test method that needs AutoFixture-generated parameters. This attribute wires up a custom `ValueObjectSpecimenBuilder` (defined in `CustomGenerator.cs`) that knows how to construct Vogen value objects via reflection. Without it, AutoFixture cannot create Vogen types and will throw.

```csharp
[Theory]
[CustomAutoData]
public async Task CreateRecipe_ReturnsCreatedRecipe(
    NewRecipe newRecipe,
    [Frozen] IRecipeRepository repository,
    RecipeService sut)
{
    // Arrange
    A.CallTo(() => repository.InsertAsync(A<DBModel.Recipe>._))
        .Returns(Task.CompletedTask);

    // Act
    var result = await sut.CreateAsync(newRecipe).ConfigureAwait(false);

    // Assert
    result.Should().NotBeNull();
}
```

### Assertion style

Always use **AwesomeAssertions** (`.Should()`). Do **not** use FluentAssertions — these are two different packages and the APIs diverge.

```csharp
// Correct
result.Should().NotBeNull();
result.Name.Should().Be(expected.Name);

// Wrong — do not use xUnit raw assertions where AwesomeAssertions can be used
Assert.NotNull(result);
```

### ConfigureAwait

Add `ConfigureAwait(false)` on every `await` in test helpers and non-test-method code. Test methods themselves do not need it.

## Integration tests (`MenuApi.Integration.Tests`)

### Infrastructure

- Uses **Aspire Testing** to spin up the full `AppHost` including a containerised SQL Server.
- Requires Docker to be running.
- Requires Auth0 secrets (set via environment variables or user secrets):
  - `parameters__Auth0TestClientId`
  - `parameters__Auth0TestClientSecret`
  - `parameters__Auth0Domain`

### Mandatory collection attribute

Every integration test class **must** use:

```csharp
[Collection("API Host Collection")]
```

This ensures all integration test classes run sequentially against the single shared `AppHost` instance. Omitting it causes parallelism issues and flaky tests.

### The `[ShortStringAutoData]` attribute

Use `[ShortStringAutoData]` (defined in the integration test project) instead of `[CustomAutoData]` for integration tests. It:

- Limits generated strings to lengths that fit `varchar(50)` database columns.
- Empties collection properties to avoid constraint violations on first insert.

```csharp
[Theory]
[ShortStringAutoData]
public async Task PostRecipe_ReturnsCreated(NewRecipe newRecipe, HttpClient client)
{
    var response = await client.PostAsJsonAsync("/recipes", newRecipe).ConfigureAwait(false);
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### Running integration tests

```bash
cd backend
dotnet test --project MenuApi.Integration.Tests/MenuApi.Integration.Tests.csproj --configuration Release
```

Integration tests are slower (Docker startup + SQL container). Run unit tests first to catch logic errors before running integration tests.
