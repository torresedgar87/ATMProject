using ATMAPI.Database;
using ATMAPI.Database.Entities;
using ATMAPI.DTO;

namespace ATMAPI.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    
    public AccountService(IAccountRepository accountRepository,  ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }
    
    public async Task<Account> GetAccountByIdAliasAsync(int idAlias)
    {
        var accountEntity = await _accountRepository.GetAccountByIdAliasAsync(idAlias);
        var transactions = await _transactionRepository.GetTransactionResultCombinedByAccountIdAsync(idAlias);

        if (accountEntity == null)
        {
            return null;
        }
        
        return new Account
        {
            Id = accountEntity.IdAlias,
            Type =  accountEntity.Type,
            Balance =  accountEntity.Balance,
            Transactions = transactions
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new Transaction
                {
                    Id = t.IdAlias,
                    Type = t.Type,
                    CreatedAt = t.CreatedAt,
                    ToAccountId = t.ToAccountId,
                    FromAccountId = t.FromAccountId,
                    Amount = t.Amount,
                    Message = t.Message,
                    Status = t.Status,
                    ProcessedAt = t.ProcessedAt
                }).ToList(),
        }; 
    }
}