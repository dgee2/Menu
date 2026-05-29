using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Menu.ApiServiceDefaults;

public static class Extensions
{
    public static TBuilder AddApiServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var assemblyName = Assembly.GetCallingAssembly()?.GetName().Name;
        builder.AddServiceDefaults();

        // Configure Open API
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
            o.InferSecuritySchemes();
            o.SupportNonNullableReferenceTypes();
            RegisterDocumentation(o, assemblyName);
        });

        // Problem details
        builder.Services.AddProblemDetails();

        // Response compression (gzip + brotli for JSON API payloads)
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/problem+json"]);
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        return builder;
    }

    private static void RegisterDocumentation(SwaggerGenOptions o, string? assemblyName)
    {
        if (assemblyName is not null)
        {
            var xmlFilename = $"{assemblyName}.xml";
            o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        }
    }

    public static WebApplication MapDefaultApiEndpoints(this WebApplication app)
    {
        app.UseResponseCompression();

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
