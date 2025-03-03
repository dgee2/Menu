using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Menu.ApiServiceDefaults;

public static class Extensions
{
    public static TBuilder AddApiServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.AddServiceDefaults();

        // Configure Open API
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
            o.InferSecuritySchemes();
            o.SupportNonNullableReferenceTypes();
            RegisterDocumentation(o);
        });

        // Problem details
        builder.Services.AddProblemDetails();

        return builder;
    }

    private static void RegisterDocumentation(SwaggerGenOptions o)
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        if (assemblyName is not null)
        {
            var xmlFilename = $"{assemblyName}.xml";
            o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        }
    }

    public static WebApplication MapDefaultApiEndpoints(this WebApplication app)
    {

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

        return app;
    }
}
