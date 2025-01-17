using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.Factory;
using Microsoft.Data.SqlClient;
using System.Data;
using MenuApi.Recipes;
using System.Reflection;
using MenuApi.StrongIds;
using Microsoft.OpenApi.Models;

StrongIdConfig.ConfigureStrongIds();

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Configure Open API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.InferSecuritySchemes();
    o.SupportNonNullableReferenceTypes();
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    o.MapType<RecipeId>(() => new OpenApiSchema { Type = "integer" });
    o.MapType<IngredientId>(() => new OpenApiSchema { Type = "integer" });
});

// Problem details
builder.Services.AddProblemDetails();

// Recipe specific stuff (needs putting in extension methods)

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddTransient<IIngredientRepository, IngredientRepository>();
builder.Services.AddTransient<IRecipeRepository, RecipeRepository>();
builder.Services.AddTransient<IRecipeService, RecipeService>();

builder.AddSqlServerClient(connectionName: "menu");
builder.Services.AddTransient<IDbConnection, SqlConnection>();
builder.Services.AddScoped<ITransactionFactory, TransactionFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI((o) =>
    {
        o.DisplayRequestDuration();
        o.EnablePersistAuthorization();
        o.EnableTryItOutByDefault();
    });
    app.Map("/", () => Results.Redirect("/swagger"));
}

// Configure the APIs
var api = app.MapGroup("/api");
api.MapRecipes();
api.MapIngredients();

app.MapDefaultEndpoints();

await app.RunAsync();

#pragma warning disable S1118 // Utility classes should not have public constructors
public partial class Program { }
#pragma warning restore S1118 // Utility classes should not have public constructors