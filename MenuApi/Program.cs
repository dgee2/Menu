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

const string CorsPolicyName = "CorsAll";

// Configure auth
//builder.AddAuthentication();
//builder.Services.AddAuthorizationBuilder().AddCurrentUserHandler();


// Add the service to generate JWT tokens
//builder.Services.AddTokenService();

// Configure the database
//var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=.db/Todos.db";
//builder.Services.AddSqlite<TodoDbContext>(connectionString);

// Configure identity
//builder.Services.AddIdentityCore<TodoUser>()
//                .AddEntityFrameworkStores<TodoDbContext>();

// State that represents the current user from the database *and* the request
//builder.Services.AddCurrentUser();

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

// Configure rate limiting
//builder.Services.AddRateLimiting();

// Configure OpenTelemetry
//builder.AddOpenTelemetry();

// Recipe specific stuff (needs putting in extension methods)

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddTransient<IIngredientRepository, IngredientRepository>();
builder.Services.AddTransient<IRecipeRepository, RecipeRepository>();
builder.Services.AddTransient<IRecipeService, RecipeService>();

builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(builder.Configuration.GetConnectionString("menudb")));
builder.Services.AddScoped<ITransactionFactory, TransactionFactory>();

builder.Services.AddApplicationInsightsTelemetry(o =>
{
    o.ConnectionString = builder.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
});


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsPolicyName,
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(CorsPolicyName);
    app.UseSwagger();
    app.UseSwaggerUI((o) =>
    {
        o.DisplayRequestDuration();
        o.EnablePersistAuthorization();
        o.EnableTryItOutByDefault();
    });
    app.Map("/", () => Results.Redirect("/swagger"));
}

//app.UseRateLimiter();

// Configure the APIs
var api = app.MapGroup("/api");
api.MapRecipes();
api.MapIngredients();

// Configure the prometheus endpoint for scraping metrics
//app.MapPrometheusScrapingEndpoint();
// NOTE: This should only be exposed on an internal port!
// .RequireHost("*:9100");

await app.RunAsync();

#pragma warning disable S1118 // Utility classes should not have public constructors
public partial class Program { }
#pragma warning restore S1118 // Utility classes should not have public constructors