using Menu.ApiServiceDefaults;
using MenuApi.Factory;
using MenuApi.Recipes;
using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.ValueObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Security.Claims;

ValueObject.ConfigureDapperTypeHandlers();

var builder = WebApplication.CreateBuilder(args);
builder.AddApiServiceDefaults();

// Recipe specific stuff (needs putting in extension methods)
builder.Services.ConfigureSwaggerGen(o => o.MapVogenTypesInMenuApi());

builder.Services.AddTransient<IUnitRepository, UnitRepository>();
builder.Services.AddTransient<IIngredientRepository, IngredientRepository>();
builder.Services.AddTransient<IIngredientService, IngredientService>();

builder.Services.AddTransient<IRecipeRepository, RecipeRepository>();
builder.Services.AddTransient<IRecipeService, RecipeService>();

builder.AddSqlServerClient(connectionName: "menu");
builder.Services.AddTransient<IDbConnection, SqlConnection>();
builder.Services.AddScoped<ITransactionFactory, TransactionFactory>();

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