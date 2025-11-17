using ATMAPI.Database;
using ATMAPI.Database.Entities;

namespace ATMAPI.Services;

public class TransactionProcessingService : ITransactionProcessingService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<TransactionProcessingService> _logger;
    
    public TransactionProcessingService(ITransactionRepository transactionRepository, IAccountRepository accountRepository,
        ILogger<TransactionProcessingService> logger)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }
    
    public async Task ProcessDepositAsync(AccountEntity accountEntity, TransactionResultCombinedEntity  transactionEntity)
    {
        accountEntity.Balance += transactionEntity.Amount;
        accountEntity.LastProcessedTransactionId = transactionEntity.Id;

        await _transactionRepository.CreateTransactionResultAsync(new TransactionResultEntity
        {
            TransactionId = transactionEntity.Id,
            CreatedAt = DateTime.UtcNow,
            Status = TransactionStatus.Processed,
            AccountId = accountEntity.IdAlias
        });

        await _accountRepository.UpdateAccountAsync(accountEntity);
    }

    public async Task ProcessWithdrawalAsync(AccountEntity accountEntity, TransactionResultCombinedEntity transactionEntity)
    {
        if (accountEntity.Balance < transactionEntity.Amount)
        {
            await _transactionRepository.CreateTransactionResultAsync(new TransactionResultEntity
            {
                TransactionId = transactionEntity.Id,
                CreatedAt = DateTime.UtcNow,
                Status = TransactionStatus.Failed,
                Message =  $"Insufficient balance {accountEntity.Balance} for withdraw of ${transactionEntity.Amount}",
                AccountId = accountEntity.IdAlias
            });
            
            accountEntity.LastProcessedTransactionId = transactionEntity.Id;
            await _accountRepository.UpdateAccountAsync(accountEntity);
            return;
        }
        
        accountEntity.Balance -= transactionEntity.Amount;
        accountEntity.LastProcessedTransactionId = transactionEntity.Id;
        
        await _transactionRepository.CreateTransactionResultAsync(new TransactionResultEntity
        {
            TransactionId = transactionEntity.Id,
            CreatedAt = DateTime.UtcNow,
            Status = TransactionStatus.Processed,
            AccountId = accountEntity.IdAlias
        });

        await _accountRepository.UpdateAccountAsync(accountEntity);
    }

    public async Task ProcessTransferAsync(AccountEntity accountEntity, TransactionResultCombinedEntity transactionEntity)
    {
        if (accountEntity.IdAlias == transactionEntity.FromAccountId)
        {
            if (accountEntity.Balance < transactionEntity.Amount)
            {
                if (accountEntity.Balance < transactionEntity.Amount)
                {
                    // hacky check to keep duplicate TransactionResults from being created for the same transaction
                    if (transactionEntity.ToAccountId != accountEntity.IdAlias)
                    {
                        await _transactionRepository.CreateTransactionResultAsync(new TransactionResultEntity
                        {
                            TransactionId = transactionEntity.Id,
                            CreatedAt = DateTime.UtcNow,
                            Status = TransactionStatus.Failed,
                            Message =  $"Insufficient balance {accountEntity.Balance} for transfer of ${transactionEntity.Amount}",
                            AccountId = accountEntity.IdAlias
                        });
                    }
                    
                    accountEntity.LastProcessedTransactionId = transactionEntity.Id;
                    await _accountRepository.UpdateAccountAsync(accountEntity);
                    return;
                }
            }
            
            accountEntity.Balance -= transactionEntity.Amount;
            accountEntity.LastProcessedTransactionId = transactionEntity.Id;
        
            // hacky check to keep duplicate TransactionResults from being created for the same transaction
            if (transactionEntity.ToAccountId != accountEntity.IdAlias)
            {
                await _transactionRepository.CreateTransactionResultAsync(new TransactionResultEntity
                {
                    TransactionId = transactionEntity.Id,
                    CreatedAt = DateTime.UtcNow,
                    Status = TransactionStatus.Processed,
                    AccountId = accountEntity.IdAlias
                });
            }

            await _accountRepository.UpdateAccountAsync(accountEntity);
            return;
        }

        accountEntity.Balance += transactionEntity.Amount;
        accountEntity.LastProcessedTransactionId = transactionEntity.Id;

        // hacky check to keep duplicate TransactionResults from being created for the same transaction
        if (transactionEntity.ToAccountId != accountEntity.IdAlias)
        {
            await _transactionRepository.CreateTransactionResultAsync(new TransactionResultEntity
            {
                TransactionId = transactionEntity.Id,
                CreatedAt = DateTime.UtcNow,
                Status = TransactionStatus.Processed,
                AccountId = accountEntity.IdAlias
            });
        }

        await _accountRepository.UpdateAccountAsync(accountEntity);
    }

    public async Task ProcessTransactionsPerAccountAsync(AccountEntity accountEntity)
    {
        if (accountEntity == null)
        {
            return;
        }
        
        var transactionEntities = await _transactionRepository
            .GetTransactionResultCombinedByAccountIdAsync(accountEntity.IdAlias, accountEntity.LastProcessedTransactionId);
        
        var orderedTransactionEntities = transactionEntities
            .OrderBy(t => t.CreatedAt);

        foreach (var transaction in orderedTransactionEntities)
        {
            if (transaction.Type == TransactionType.Deposit)
            {
                await ProcessDepositAsync(accountEntity, transaction);
            }
            else if (transaction.Type == TransactionType.Withdrawal)
            {
                await ProcessWithdrawalAsync(accountEntity, transaction);
            }
            else if (transaction.Type == TransactionType.Transfer)
            {
                await ProcessTransferAsync(accountEntity, transaction);
            }
        }
    }
    
    // can we do in background?
    public async Task ProcessTransactionAsync(TransactionResultCombinedEntity transactionEntity)
    {
        _logger.LogInformation("Processing transaction {transactionEntity}", transactionEntity);
        
        var fromAccountEntity = await _accountRepository.GetAccountByIdAliasAsync(transactionEntity.FromAccountId);
        var toAccountEntity = await _accountRepository.GetAccountByIdAliasAsync(transactionEntity.ToAccountId);
    
        // transfers need two valid accounts
        if ((fromAccountEntity == null || toAccountEntity == null)
            && transactionEntity.Type == TransactionType.Transfer)
        {
            await _transactionRepository.CreateTransactionResultAsync(new TransactionResultEntity
            {
                TransactionId = transactionEntity.Id,
                Message = $"Invalid transfer between accounts ${transactionEntity.FromAccountId} and {transactionEntity.ToAccountId}",
                CreatedAt =  DateTime.UtcNow,
                Status = TransactionStatus.Failed,
            });
            return;
        }
        // withdraw and deposits need from account to be valid
        else if ((transactionEntity.Type == TransactionType.Withdrawal ||
                  transactionEntity.Type == TransactionType.Deposit)
                 && fromAccountEntity == null)
        {
            await _transactionRepository.CreateTransactionResultAsync(new TransactionResultEntity
            {
                TransactionId = transactionEntity.Id,
                Message = $"Invalid transaction of ${transactionEntity.Type} account ${transactionEntity.FromAccountId} does not exist",
                CreatedAt =  DateTime.UtcNow,
                Status = TransactionStatus.Failed,
            });
            return;
        }

        // we can parallelize here
        await ProcessTransactionsPerAccountAsync(fromAccountEntity);
        await ProcessTransactionsPerAccountAsync(toAccountEntity);
    }
}