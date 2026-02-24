using Menu.ApiServiceDefaults;
using MenuApi;
using MenuApi.Recipes;
using MenuApi.Repositories;
using MenuApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.AddApiServiceDefaults();

// Recipe specific stuff (needs putting in extension methods)
builder.Services.ConfigureSwaggerGen(o => o.MapVogenTypesInMenuApi());

builder.Services.AddTransient<IUnitRepository, UnitRepository>();
builder.Services.AddTransient<IIngredientRepository, IngredientRepository>();
builder.Services.AddTransient<IIngredientService, IngredientService>();

builder.Services.AddTransient<IRecipeRepository, RecipeRepository>();
builder.Services.AddTransient<IRecipeService, RecipeService>();

builder.Services.AddDbContext<MenuDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("menu");
    options.UseSqlServer(!string.IsNullOrWhiteSpace(connectionString)
        ? connectionString
        : "Server=.;Database=menu;Trusted_Connection=True;");
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0Domain"]}/";
        options.Audience = builder.Configuration["Auth0Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });
builder.Services.AddAuthorizationBuilder();

// Add CORS policy before building the app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (!string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("menu")))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MenuDbContext>();
    await dbContext.Database.MigrateAsync().ConfigureAwait(false);
}

// Use CORS middleware before authentication/authorization
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultApiEndpoints();

// Configure the APIs
var api = app.MapGroup("/api")
    .RequireAuthorization();

api.MapRecipes();
api.MapIngredients();

await app.RunAsync();

#pragma warning disable S1118 // Utility classes should not have public constructors
public partial class Program { }
#pragma warning restore S1118 // Utility classes should not have public constructors