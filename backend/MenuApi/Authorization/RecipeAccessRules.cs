using System.Linq.Expressions;
using MenuApi.ValueObjects;
using MenuDB.Data;

namespace MenuApi.Repositories;

/// <summary>
/// The single evaluation of who may do what to a recipe. Read filtering and the
/// <c>canEdit</c>/<c>canDelete</c> flags returned to the client are derived here together, so the
/// flags cannot promise something the query would refuse.
/// </summary>
/// <remarks>
/// When recipe sharing lands, this is the one place that has to learn about shares - and the
/// client keeps working, because it never inferred permissions from ownership itself.
/// </remarks>
public static class RecipeAccessRules
{
    /// <summary>Recipes <paramref name="callerId"/> is allowed to read.</summary>
    public static Expression<Func<RecipeEntity, bool>> ReadableBy(MenuUserId callerId)
    {
        var ownerUserId = callerId.Value;
        return r => r.OwnerUserId == ownerUserId
            || r.AccessScopeId == (byte)RecipeAccessScope.AuthenticatedUsers;
    }

    /// <summary>Whether <paramref name="callerId"/> may modify <paramref name="recipe"/>.</summary>
    public static bool CanEdit(DBModel.Recipe recipe, MenuUserId callerId)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        return recipe.OwnerUserId == callerId;
    }

    /// <summary>Whether <paramref name="callerId"/> may delete <paramref name="recipe"/>.</summary>
    public static bool CanDelete(DBModel.Recipe recipe, MenuUserId callerId) => CanEdit(recipe, callerId);
}
