using ATMAPI.Database.Entities;

namespace ATMAPI.Services;

public interface ITransactionProcessingService
{
    Task ProcessTransactionAsync(TransactionResultCombinedEntity transactionEntity);

    Task ProcessTransactionsPerAccountAsync(AccountEntity accountEntity);

    Task ProcessTransferAsync(AccountEntity accountEntity, TransactionResultCombinedEntity transactionEntity);

    Task ProcessWithdrawalAsync(AccountEntity accountEntity, TransactionResultCombinedEntity transactionEntity);

    Task ProcessDepositAsync(AccountEntity accountEntity, TransactionResultCombinedEntity transactionEntity);
}