using ATMAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMAPI.Database;

public class AccountRepository : IAccountRepository
{
    private readonly APIDbContext _dbContext;
    private readonly ILogger<AccountRepository> _logger;

    public AccountRepository(APIDbContext dbContext, ILogger<AccountRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public Task InitAccountAsync(int userId)
    {
        try
        {
            _dbContext.Accounts.AddRange([
                new AccountEntity
                {
                    IdAlias = 1000000 + userId,
                    Type = AccountType.Checking,
                    UserId = userId
                },
                new AccountEntity
                {
                    IdAlias = 2000000 + userId,
                    Type = AccountType.Saving,
                    UserId = userId
                }
            ]);
            return _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public Task<List<AccountEntity>> GetAccountsByUserIdAsync(int userId)
    {
        return _dbContext
            .Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public Task UpdateAccountAsync(AccountEntity account)
    {
        try {
            _dbContext.Accounts.Update(account);
            return _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public Task<AccountEntity?> GetAccountByIdAliasAsync(int idAlias)
    {
        return _dbContext.Accounts.FirstOrDefaultAsync(a => a.IdAlias == idAlias);
    }
}