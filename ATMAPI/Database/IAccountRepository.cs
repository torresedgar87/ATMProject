using ATMAPI.Database.Entities;

namespace ATMAPI.Database;

public interface IAccountRepository
{
    Task InitAccountAsync(int userId);
    
    Task<List<AccountEntity>> GetAccountsByUserIdAsync(int userId);

    Task UpdateAccountAsync(AccountEntity account);
    
    Task <AccountEntity?> GetAccountByIdAliasAsync(int idAlias);
}