using Vogen;

[assembly: VogenDefaults(
    conversions: Conversions.Default | Conversions.EfCoreValueConverter,
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateSwashbuckleMappingExtensionMethod
)]