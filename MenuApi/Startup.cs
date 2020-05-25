using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AutoMapper;
using MenuApi.Configuration;
using MenuApi.Repositories;
using MenuApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MenuApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddHostedService<CosmosSetupService>();

            services.Configure<CosmosConfig>(Configuration.GetSection("Cosmos"));
            services.Configure<SearchConfig>(Configuration.GetSection("Search"));

            services.AddTransient<IIngredientRepository, IngredientRepository>();
            services.AddTransient<IRecipeRepository, RecipeRepository>();
            services.AddTransient<ISearchFactory, SearchFactory>();

            services.AddSingleton(sp => new CosmosClient(sp.GetRequiredService<IOptions<CosmosConfig>>().Value.ConnectionString));
            services.AddTransient<IIngredientRepository, IngredientRepository>();

            services.AddSingleton<IValidatable>(resolver => resolver.GetRequiredService<IOptions<CosmosConfig>>().Value);
            services.AddSingleton<IValidatable>(resolver => resolver.GetRequiredService<IOptions<SearchConfig>>().Value);
            services.AddTransient<IStartupFilter, SettingValidationStartupFilter>();

            services.AddControllers();
            services.AddSwaggerDocument();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Startup file")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Register the Swagger generator and the Swagger UI middlewares
                app.UseOpenApi();
                app.UseSwaggerUi3();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
