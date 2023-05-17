using System.Data;

namespace MenuApi.Factory;

public class TransactionFactory : ITransactionFactory
{
    private readonly IDbConnection dbConnection;

    public TransactionFactory(IDbConnection dbConnection)
    {
        this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }

    private IDbConnection EnsureOpenConnection
    {
        get
        {
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            return dbConnection;
        }
    }

    public IDbTransaction BeginTransaction() => EnsureOpenConnection.BeginTransaction();

    public IDbTransaction BeginTransaction(IsolationLevel isolationLevel) => EnsureOpenConnection.BeginTransaction(isolationLevel);
}
