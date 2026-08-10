using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Exceptions;

public static class DbUpdateExceptionExtensions
{
    private const int SqlServerUniqueConstraintViolationError = 2627;
    private const int SqlServerUniqueIndexViolationError = 2601;

    /// <summary>
    /// Whether a SQL Server error number means "this violated a uniqueness rule".
    /// </summary>
    /// <remarks>
    /// Split out from the two overloads below because <see cref="SqlException"/> cannot be
    /// constructed outside the driver, so this is the only part of the decision a unit test can
    /// reach. The plumbing - creation surfacing a wrapped exception, <c>ExecuteUpdateAsync</c>
    /// throwing a raw one - is covered by the duplicate-title integration tests.
    /// </remarks>
    public static bool IsUniqueConstraintViolationError(int sqlErrorNumber) =>
        sqlErrorNumber is SqlServerUniqueConstraintViolationError or SqlServerUniqueIndexViolationError;

    /// <summary>Creation goes through EF's SaveChanges, which wraps the SQL error.</summary>
    public static bool IsUniqueConstraintViolation(this DbUpdateException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.InnerException is SqlException sqlEx
            && IsUniqueConstraintViolationError(sqlEx.Number);
    }

    /// <summary>Updates go through ExecuteUpdateAsync, which throws the SQL error unwrapped.</summary>
    public static bool IsUniqueConstraintViolation(this SqlException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return IsUniqueConstraintViolationError(exception.Number);
    }
}
