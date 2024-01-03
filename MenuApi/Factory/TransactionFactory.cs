using System.Data;

namespace MenuApi.Factory;

public class TransactionFactory(IDbConnection dbConnection) : ITransactionFactory
{
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
