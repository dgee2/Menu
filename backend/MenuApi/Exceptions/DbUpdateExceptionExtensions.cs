using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Exceptions;

public static class DbUpdateExceptionExtensions
{
    private const int SqlServerUniqueConstraintViolationError = 2627;
    private const int SqlServerUniqueIndexViolationError = 2601;

    public static bool IsUniqueConstraintViolation(this DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlEx &&
               (sqlEx.Number == SqlServerUniqueConstraintViolationError ||
                sqlEx.Number == SqlServerUniqueIndexViolationError);
    }
}
