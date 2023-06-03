using System.Data;

namespace MenuApi.Factory;

public interface ITransactionFactory
{
    IDbTransaction BeginTransaction();

    IDbTransaction BeginTransaction(IsolationLevel isolationLevel);
}
