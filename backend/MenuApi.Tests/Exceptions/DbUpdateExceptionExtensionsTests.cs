#nullable enable

using AwesomeAssertions;
using MenuApi.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Tests.Exceptions;

/// <summary>
/// The duplicate-title 409 reaches these overloads by two different routes - creation surfaces a
/// <see cref="DbUpdateException"/> wrapping the SQL error, while <c>ExecuteUpdateAsync</c> throws
/// the <c>SqlException</c> raw - and both funnel into the same set of error numbers.
/// </summary>
/// <remarks>
/// <c>SqlException</c> has no constructible surface outside the driver, so the number check is
/// tested directly here and the two routes end to end by the duplicate-title integration tests
/// (<c>Create_Recipe_With_Duplicate_Title_Returns_Conflict</c> and
/// <c>Update_Recipe_To_Duplicate_Title_Returns_Conflict</c>).
/// </remarks>
public class DbUpdateExceptionExtensionsTests
{
    [Theory]
    [InlineData(2627)] // Unique constraint violation
    [InlineData(2601)] // Unique index violation
    public void UniquenessErrorNumbers_AreRecognised(int errorNumber)
    {
        DbUpdateExceptionExtensions.IsUniqueConstraintViolationError(errorNumber).Should().BeTrue();
    }

    [Theory]
    [InlineData(1205)] // Deadlock victim
    [InlineData(547)] // Foreign key violation
    [InlineData(0)]
    public void OtherErrorNumbers_AreNotRecognised(int errorNumber)
    {
        DbUpdateExceptionExtensions.IsUniqueConstraintViolationError(errorNumber).Should().BeFalse();
    }

    [Fact]
    public void DbUpdateException_WithNoInnerException_IsNotAConflict()
    {
        new DbUpdateException("failed").IsUniqueConstraintViolation().Should().BeFalse();
    }

    [Fact]
    public void DbUpdateException_WrappingSomethingOtherThanASqlException_IsNotAConflict()
    {
        var exception = new DbUpdateException("failed", new TimeoutException());

        exception.IsUniqueConstraintViolation().Should().BeFalse();
    }
}
