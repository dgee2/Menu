using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuDB.Migrations
{
    /// <inheritdoc />
    public partial class BackfillRecipeSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO dbo.RecipeSearchIndex
                    (RecipeId, Title, SummaryText, IngredientsText, StepsText, OwnerUserId,
                     AccessScope, IsPublishedExternally, UpdatedAtUtc)
                SELECT recipe.Id, recipe.Title, recipe.Summary, ingredients.SearchText, steps.SearchText,
                       recipe.OwnerUserId, scope.Name, 0, recipe.UpdatedAtUtc
                FROM dbo.Recipe AS recipe
                INNER JOIN dbo.RecipeAccessScope AS scope ON scope.Id = recipe.AccessScopeId
                OUTER APPLY (
                    SELECT STRING_AGG(CAST(ingredient.IngredientText AS nvarchar(max)), N' ')
                        WITHIN GROUP (ORDER BY ingredient.SortOrder, ingredient.Id) AS SearchText
                    FROM dbo.RecipeIngredient AS ingredient
                    WHERE ingredient.RecipeId = recipe.Id
                        AND NULLIF(LTRIM(RTRIM(ingredient.IngredientText)), N'') IS NOT NULL
                ) AS ingredients
                OUTER APPLY (
                    SELECT STRING_AGG(CAST(step.InstructionText AS nvarchar(max)), N' ')
                        WITHIN GROUP (ORDER BY step.SortOrder, step.Id) AS SearchText
                    FROM dbo.RecipeStep AS step
                    WHERE step.RecipeId = recipe.Id
                        AND NULLIF(LTRIM(RTRIM(step.InstructionText)), N'') IS NOT NULL
                ) AS steps
                WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.RecipeSearchIndex AS searchIndex
                    WHERE searchIndex.RecipeId = recipe.Id
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Backfilled rows are indistinguishable from rows maintained by the consumer.
            // Preserve them when rolling back this data-only migration.
        }
    }
}
