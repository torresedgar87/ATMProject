using System.Runtime.InteropServices.JavaScript;
using ATMAPI.Database;
using ATMAPI.Database.Entities;
using ATMAPI.DTO;
using ATMAPI.DTO.Request;
using ATMAPI.Errors.Services;

namespace ATMAPI.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionProcessingService _transactionProcessingService;

    public TransactionService(ITransactionRepository transactionRepository,  IAccountRepository accountRepository,
        ITransactionProcessingService transactionProcessingService)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _transactionProcessingService = transactionProcessingService;
    }

    private List<Transaction> MapTransactionEntitiesToTransactions(List<TransactionEntity> transactionEntities)
    {
        return transactionEntities
            .Select(t => new Transaction
            {
                Id = t.IdAlias,
                Type = t.Type,
                Amount = t.Amount,
                FromAccountId = t.FromAccountId,
                ToAccountId = t.ToAccountId,
                CreatedAt =  t.CreatedAt,
            })
            .ToList();
    }
    
    private List<Transaction> MapTransactionResultCombinedEntitiesToTransactions(List<TransactionResultCombinedEntity> transactionEntities)
    {
        return transactionEntities
            .Select(t => new Transaction
            {
                Id = t.IdAlias,
                Type = t.Type,
                Amount = t.Amount,
                FromAccountId = t.FromAccountId,
                ToAccountId = t.ToAccountId,
                CreatedAt =  t.CreatedAt,
                Message = t.Message,
                Status = t.Status,
                ProcessedAt = t.ProcessedAt,
            })
            .ToList();
    }

    // we validate this at minimum because we want to maintain some data integrity in the transaction table
    private bool ValidateCreateTransactionRequest(CreateTransaction createTransaction)
    {
        return createTransaction.Amount > 0
            || (createTransaction.Type != TransactionType.Transfer && createTransaction.FromAccountId >  1000000)
            || (createTransaction.Type == TransactionType.Transfer && createTransaction.ToAccountId > 1000000
                && createTransaction.FromAccountId > 1000000);
    }
    
    public async Task<Transaction> CreateTransactionAsync(CreateTransaction createTransaction)
    {
        var isTransactionValid = ValidateCreateTransactionRequest(createTransaction);

        if (!isTransactionValid)
        {
            throw new InvalidTransactionRequestException("Transaction is not valid");
        }
        
        // we want to store every valid transaction and process them later, mainly because
        // we want to process them in order of when they were received
        await _transactionRepository.CreateTransactionAsync(new TransactionEntity
        {
            IdAlias = createTransaction.Id,
            FromAccountId = createTransaction.FromAccountId,
            ToAccountId = createTransaction.ToAccountId,
            Type =  createTransaction.Type,
            Amount = createTransaction.Amount,
            CreatedAt = DateTime.UtcNow,
        });
        
        var transactionEntity = await _transactionRepository.GetTransactionByIdAliasAsync(createTransaction.Id);

        if (transactionEntity == null)
        {
            throw new TransactionCreationException($"Transaction creation failed for {createTransaction.Id}");
        }
        
        // we don't await so we can process async
        _transactionProcessingService.ProcessTransactionAsync(new TransactionResultCombinedEntity
        {
            Id = transactionEntity.Id,
            CreatedAt = transactionEntity.CreatedAt,
            IdAlias =  transactionEntity.IdAlias,
            Type = transactionEntity.Type,
            Amount = transactionEntity.Amount,
            ToAccountId = transactionEntity.ToAccountId,
            FromAccountId = transactionEntity.FromAccountId,
        });

        var mappedTransactions = MapTransactionEntitiesToTransactions([transactionEntity]);
        return mappedTransactions[0];
    }

    public async Task<List<Transaction>> GetTransactionsByAccountIdAsync(int accountId)
    {
        var transactionEntities = await _transactionRepository.GetTransactionByAccountIdAsync(accountId);
        
        return MapTransactionEntitiesToTransactions(transactionEntities);
    }

    public async Task<List<Transaction>> GetTransactionsResultCombinedByAccountIdAsync(int accountId)
    {
        var transactionEntities = await _transactionRepository.GetTransactionResultCombinedByAccountIdAsync(accountId);
        return MapTransactionResultCombinedEntitiesToTransactions(transactionEntities);
    }
}