using Vogen;

[assembly: VogenDefaults(
    conversions: Conversions.Default | Conversions.DapperTypeHandler,
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateSwashbuckleMappingExtensionMethod
)]