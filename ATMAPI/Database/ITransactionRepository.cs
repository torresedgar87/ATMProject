using ATMAPI.Database.Entities;

namespace ATMAPI.Database;

public interface ITransactionRepository
{
    Task CreateTransactionAsync(TransactionEntity transactionEntity);
    
    Task<TransactionEntity?> GetTransactionByIdAliasAsync(Guid idAlias);
    
    Task<List<TransactionEntity>> GetTransactionByAccountIdAsync(int accountId, int lastProcessedTransactionId = 0);
    
    Task<List<TransactionResultCombinedEntity>> GetTransactionResultCombinedByAccountIdAsync(int accountId, int lastProcessedTransactionId = 0);

    public Task CreateTransactionResultAsync(TransactionResultEntity transactionResultEntity);
}