#nullable enable

using AwesomeAssertions;
using MenuApi.ValueObjects;
using MenuDB.Configuration;
using Xunit;

namespace MenuApi.Tests.ValueObjects;

/// <summary>
/// The <see cref="RecipeAccessScope"/> enum and the <c>RecipeAccessScope</c> lookup table are two
/// hand-maintained copies of the same closed set, in two projects that cannot reference each other's
/// definition. Nothing in the compiler or in EF Core notices when they diverge - this does.
/// </summary>
public class RecipeAccessScopeDriftTests
{
    private static readonly IReadOnlyDictionary<byte, string> SeededRows =
        RecipeAccessScopeEntityConfiguration.SeedRows;

    [Fact]
    public void SeededRowsAndEnumMembersMatchExactly()
    {
        var enumValues = Enum.GetValues<RecipeAccessScope>()
            .ToDictionary(v => (byte)v, v => v.ToString());

        SeededRows.Should().BeEquivalentTo(enumValues);
    }

    [Theory]
    [InlineData(RecipeAccessScope.Private)]
    [InlineData(RecipeAccessScope.AuthenticatedUsers)]
    public void EnumMemberMatchesItsSeededRow(RecipeAccessScope scope)
    {
        SeededRows.Should().ContainKey((byte)scope)
            .WhoseValue.Should().Be(scope.ToString());
    }

    [Fact]
    public void NoMemberUsesZero()
    {
        // Zero is default(RecipeAccessScope). Leaving it unassigned means an uninitialised value can
        // never be mistaken for a real - and in particular for a permissive - scope.
        Enum.GetValues<RecipeAccessScope>().Should().NotContain(default(RecipeAccessScope));
    }
}
