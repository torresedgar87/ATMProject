using ATMAPI.Database.Entities;
using ATMAPI.DTO;
using ATMAPI.DTO.Request;

namespace ATMAPI.Services;

public interface ITransactionService
{
    Task<Transaction> CreateTransactionAsync(CreateTransaction createTransaction);
    
    Task<List<Transaction>> GetTransactionsByAccountIdAsync(int accountId);

    public Task<List<Transaction>> GetTransactionsResultCombinedByAccountIdAsync(int accountId);
}