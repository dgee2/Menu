using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.Factory;
using Microsoft.Data.SqlClient;
using System.Data;
using MenuApi.Recipes;
using System.Reflection;
using MenuApi.ValueObjects;
using Menu.ApiServiceDefaults;
using System.Runtime.CompilerServices;

ValueObject.ConfigureDapperTypeHandlers();

var builder = WebApplication.CreateBuilder(args);
builder.AddApiServiceDefaults();

// Recipe specific stuff (needs putting in extension methods)
builder.Services.ConfigureSwaggerGen(o => o.MapVogenTypesInMenuApi());
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddTransient<IUnitRepository, UnitRepository>();
builder.Services.AddTransient<IIngredientRepository, IngredientRepository>();
builder.Services.AddTransient<IIngredientService, IngredientService>();

builder.Services.AddTransient<IRecipeRepository, RecipeRepository>();
builder.Services.AddTransient<IRecipeService, RecipeService>();

builder.AddSqlServerClient(connectionName: "menu");
builder.Services.AddTransient<IDbConnection, SqlConnection>();
builder.Services.AddScoped<ITransactionFactory, TransactionFactory>();

var app = builder.Build();

app.MapDefaultApiEndpoints();

// Configure the APIs
var api = app.MapGroup("/api");
api.MapRecipes();
api.MapIngredients();

await app.RunAsync();

#pragma warning disable S1118 // Utility classes should not have public constructors
public partial class Program { }
#pragma warning restore S1118 // Utility classes should not have public constructors