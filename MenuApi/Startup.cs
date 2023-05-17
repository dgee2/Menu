using System.Data;
using MenuApi.Configuration;
using MenuApi.Factory;
using MenuApi.Repositories;
using MenuApi.Services;
using Microsoft.Data.SqlClient;

namespace MenuApi;

public class Startup
{
    const string CorsPolicyName = "Cors";
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Startup).Assembly);

        services.AddTransient<IIngredientRepository, IngredientRepository>();
        services.AddTransient<IRecipeRepository, RecipeRepository>();
        services.AddTransient<IRecipeService, RecipeService>();

        services.AddTransient<IStartupFilter, SettingValidationStartupFilter>();

        services.AddControllers();
        services.AddSwaggerGen();

        ConfigureDatabase(services);
        services.AddApplicationInsightsTelemetry(o =>
        {
            o.ConnectionString = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
        });

        services.AddCors(options =>
        {
            options.AddPolicy(name: CorsPolicyName,
                builder =>
                {
                    builder.AllowAnyOrigin();
                });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseCors(CorsPolicyName);

            app.UseDeveloperExceptionPage();

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Menu API");
            });
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }

    public void ConfigureDatabase(IServiceCollection services)
    {
        services.AddScoped<IDbConnection>(_ => new SqlConnection(Configuration.GetConnectionString("menudb")));
        services.AddScoped<ITransactionFactory, TransactionFactory>();
    }
}
